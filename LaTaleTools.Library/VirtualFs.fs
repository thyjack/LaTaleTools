module LaTaleTools.Library.VirtualFs

open System
open FSharpPlus
open LaTaleTools.Library.Spf

type public SourceFile =
    SourceFile of Path: string

type public FileEntry =
    FileEntry of Source: SourceFile * Metadata: FileEntryMetadata

type public FsNode =
    | FsNode of Name: string * Path: string
    | FsDirNode of Name: string * Path: string * Children: FsNode list

    member x.Name =
        match x with
        | FsNode (name, _) -> name
        | FsDirNode (name, _, _) -> name
    member x.Path =
        match x with
        | FsNode (_, path) -> path
        | FsDirNode (_, path, _) -> path

type public VirtualFsState =
    { FileMapping: Map<ArchivePath, FileEntry>
      Root: FsNode
    }

let public fsVisit (path: string) (root: FsNode): FsNode option =
    let segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries) |> List.ofArray
    let rec _visit node =
        function
        | [] -> Some node
        | head :: rest ->
            match node with
            | FsNode _ -> None
            | FsDirNode (_, _, children) ->
                children
                |> List.tryFind (fun c -> c.Name = head)
                >>= (flip _visit rest)

    _visit root segments

let internal buildTree (paths: string seq): FsNode =
    let separator = '/'
    // paths broken down by segments (dirs)
    let pathSegments =
        paths
        |> Seq.sort
        |> Seq.map (fun p -> p.Split(separator, StringSplitOptions.RemoveEmptyEntries) |> List.ofArray)
        |> List.ofSeq

    let rec buildDir (revPath: string list) (pathSegments: string list list) =
        let fileName, path =
            match revPath with
            | []               -> "/", ""
            | fileName :: path -> fileName, "/" + String.Join(separator, List.rev path)

        match pathSegments with
        | [] | [[]] -> FsNode (fileName, path)
        | [ p::r ] -> // single path quick pass; optimisation only
            let node = buildDir (p :: revPath) [r]
            FsDirNode (fileName, path, [node])
        | _ ->
            let children =
                pathSegments
                |> List.groupBy List.head
                |> List.map (fun (seg, paths) ->
                    paths
                    |> List.map List.tail
                    |> buildDir (seg :: revPath))

            FsDirNode (fileName, path, children)

    buildDir [] pathSegments

let public buildFromFileEntries (entries: FileEntry seq): VirtualFsState =
    let mapping =
        entries
        |> Seq.map (fun (FileEntry (_, m) as e) -> (m.Path, e))
        |> Map.ofSeq
    let paths =
        entries
        |> Seq.map (fun (FileEntry (_, { Path = ArchivePath ap })) -> ap)

    { FileMapping = mapping
      Root = buildTree paths
    }