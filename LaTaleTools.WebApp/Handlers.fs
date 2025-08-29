module LaTaleTools.WebApp.Handlers

open System
open System.IO
open System.Threading.Tasks
open FSharpPlus
open Giraffe
open LaTaleTools.Library.Encoding
open LaTaleTools.Library.Ldt
open LaTaleTools.Library.Spf
open LaTaleTools.Library.Tbl
open LaTaleTools.Library.Util
open LaTaleTools.Library.VirtualFs
open LaTaleTools.WebApp.AppState
open LaTaleTools.WebApp.FileViews.AudioView
open LaTaleTools.WebApp.FileViews.LdtView
open LaTaleTools.WebApp.FileViews.TblView
open LaTaleTools.WebApp.Route
open LaTaleTools.WebApp.Views
open Microsoft.Extensions.Logging
open TagLib

let normalisePath (path: string) =
    path.Substring(browseBasePath.Length)
        .TrimStart('/')
        .TrimEnd('/')

let genericFileHandler streamOrView handler: HttpHandler =
    fun next ctx ->
        let path = normalisePath ctx.Request.Path.Value
        let appState = ctx.GetService<AppState>()

        match fsVisit path appState.VirtualFs.Root with
        | None
        | Some (FsDirNode _) -> RequestErrors.notFound (text $"No such file: {path}") next ctx
        | Some (FsNode (name, _)) ->
            match streamOrView path appState with
            | None -> RequestErrors.notFound (text $"Could not find source file for file: {path}") next ctx
            | Some streamOrView -> handler name streamOrView next ctx

let renderTblHandler (fullPath: string) (name: string) (path: string): HttpHandler =
    let viewAction _name view: HttpHandler =
        fun next ctx ->
            let appState = ctx.GetService<AppState>()
            let logger = ctx.GetService<ILogger<Sprite>>()

            let spriteGroups = readSpriteGroups path view
            let allSprites = Seq.collect _.Sprites spriteGroups
            let uniqueFileContents =
                allSprites
                |> Seq.distinctBy _.File
                |> Seq.map
                    (
                        fun { File = ArchivePath ap as file } ->
                            monad' {
                                let! stream = openStreamInArchive ap appState
                                let buffer = Array.zeroCreate(int(stream.Length))
                                return task {
                                    let! _ =
                                        Logging.logged logger $"ReadInArchiveFile[{ap}]"
                                            (
                                                (fun () -> stream.ReadAsync(Memory(buffer))),
                                                (fun _ -> {| FileLength = buffer.Length |})
                                            )
                                    return (file, buffer)
                                }
                            }
                    )
                |> Seq.choose id
                |> sequence
                |> Task.map Map.ofSeq
            let getCroppedImageBase64 (sprite: Sprite): string option Task =
                let _getImageContent files =
                    monad' {
                        let! (file: byte array) = Map.tryFind sprite.File files

                        return task {
                            let byteArray = readSpriteStream sprite (ReadOnlySpan<byte>(file))
                            return Convert.ToBase64String(ReadOnlySpan(byteArray))
                        }
                    } |> sequence
                uniqueFileContents
                |> Task.map _getImageContent
                |> join

            task {
                let! subView = tblView spriteGroups getCroppedImageBase64
                return!
                    ( fileView fullPath name path subView
                      |> htmlView
                    ) next ctx
            }
    genericFileHandler openViewInArchive viewAction

type InMemoryTagLibFile(name: string, stream: Stream) =
    interface File.IFileAbstraction with
        member this.Name = name
        member this.ReadStream = stream
        member this.WriteStream = null
        member this.CloseStream(streamToClose: Stream) =
            if streamToClose <> null then
                streamToClose.Dispose()

let renderHandler (fullPath: string) fsNode: HttpHandler =
    match fsNode with
    | FsNode (name, path) when suffixMatchCi name ".ldt" ->
        let viewAction _name view =
            let ldtTable = readLdtTable view
            htmlView (fileView fullPath name path (ldtView ldtTable))
        genericFileHandler openViewInArchive viewAction
    | FsNode (name, path) when suffixMatchCi name ".tbl" ->
        renderTblHandler fullPath name path
    | FsNode (name, path) when suffixMatchCi name ".xml" ->
        let viewAction _name (stream: Stream) =
            fun next ctx ->
                task {
                    let length = stream.Length
                    let data = Array.zeroCreate<byte>(int(length))
                    do! stream.ReadAsync data |> map ignore
                    let dataString =
                        xmlStringEncoding.GetString(ReadOnlySpan<byte>(data))
                        |> String.trimStart ['\uFEFF'; '\u200B']
                    return!
                       ( htmlView
                         <| fileView fullPath name path (codeViewComponent dataString)
                       ) next ctx
                }
        genericFileHandler openStreamInArchive viewAction
    | FsNode (name, path) when [ ".mp3"; ".wav" ] |> List.exists (suffixMatchCi name) ->
        let viewAction name (stream: Stream) =
            use tagLibFile = File.Create(InMemoryTagLibFile(name, stream))
            
            let description = tagLibFile.Properties.Description
            let tags = tagLibFile.Tag
            
            let audioPlayerView = audioPlayerComponent fullPath description tags
            htmlView (fileView fullPath name path audioPlayerView)
        genericFileHandler openStreamInArchive viewAction
    | FsNode (name, path) when suffixMatchCi name ".png" ->
        let imageView = imageComponent fullPath name
        htmlView (fileView fullPath name path imageView)
    | FsNode (name, path) ->
        htmlView (fileView fullPath name path [])
    | FsDirNode (name, path, children) ->
        htmlView (dirView fullPath name path children)

let virtualFsHandler: HttpHandler =
    fun next ctx ->
        let fullPath = normalisePath ctx.Request.Path.Value
        let appState = ctx.GetService<AppState>()

        match fsVisit fullPath appState.VirtualFs.Root with
        | None -> RequestErrors.notFound (text $"No such path: {fullPath}") next ctx
        | Some node -> renderHandler fullPath node next ctx

let fileDownloadHandler: HttpHandler =
    let streamAction name stream =
        setHttpHeader "Content-Type" "application/octet-stream"
        >=> setHttpHeader "Content-Disposition" $"attachment; filename=\"{name}\""
        >=> streamData false stream None None
    genericFileHandler openStreamInArchive streamAction