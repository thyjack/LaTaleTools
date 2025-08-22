module LaTaleTools.WebApp.Views

open System
open Giraffe.ViewEngine
open LaTaleTools.Library.Util
open LaTaleTools.Library.VirtualFs
open LaTaleTools.WebApp.Route

let pageHead (titleString: string) =
    head [] [
        meta [ _charset "UTF-8" ]
        title [] [ str titleString ]
    ]

let pathView (isFile: bool) label path extraInfo =
    [
        a [
            yield _href (pathToBrowseLink path)
            if isFile then
                yield _onclick $"window.open(this.href,'{label}','width=600,height=600'); return false;"
        ] [ str label ]
        str " "
        i [] <|
            if String.IsNullOrEmpty extraInfo
            then []
            else [ str $"({extraInfo})" ]
    ]

let pageTemplate (fullPath: string) (name: string) (path: string) pageContent =
    html [] [
            pageHead $"{name} ({fullPath})"

            body []
            <| div [ _style "position:sticky;top:0;background-color:white;padding-top:10px" ] [
                   span [ _style "font-size:200%" ] [
                       if not (String.IsNullOrEmpty path) then
                           yield i [] [ str path ]
                           if not (path.EndsWith "/") then
                               yield str "/"
                       yield b [] [ str name ]
                   ]
                   hr []
               ]
               :: pageContent
        ]

let public dirView (fullPath: string) (name: string) (path: string) (children: FsNode list) =
    let isRoot = String.IsNullOrEmpty path
    let mapChild =
        function
        | FsNode (name, _) ->
            li []
            <| pathView true name
                   (if isRoot then $"/{name}" else $"{fullPath}/{name}") ""
        | FsDirNode (name, _, _) ->
            li []
            <| pathView false name
                   (if isRoot then $"/{name}" else $"{fullPath}/{name}") ""

    pageTemplate fullPath name path
    <| [ ul []
         <| [
                 if not isRoot then yield li [] (pathView false ".." path path)
                 yield! children |> List.map mapChild
         ]
       ]

let maybeRenderImage fullPath name = [
    if suffixMatchCi name ".png"
    then yield img [ _src (pathToDownloadLink fullPath) ]
]

let maybeRenderAudioPlayer fullPath name = [
    if suffixMatchCi name ".mp3"
       || suffixMatchCi name ".wav"
    then yield audio [ _controls; _autoplay; _loop ] [
        source [ _src (pathToDownloadLink fullPath) ]
    ]
]

let renderContentNativeHtml fullPath name =
    [ yield! maybeRenderImage fullPath name
      yield! maybeRenderAudioPlayer fullPath name
    ]

let public codeViewComponent (content: string) =
    pre [] [ str content ]
    |> List.singleton

let public fileView (fullPath: string) (name: string) (path: string) (view: XmlNode list option) =
    pageTemplate fullPath name path
    <| [ div [] [
             yield a [ _href (pathToDownloadLink fullPath) ] [ str "Download" ]
             yield br []
             yield! view |> Option.defaultWith (fun () -> renderContentNativeHtml fullPath name)
         ]
       ]