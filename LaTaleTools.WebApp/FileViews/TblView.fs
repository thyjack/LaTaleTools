module LaTaleTools.WebApp.FileViews.TblView

open System
open System.Threading.Tasks
open FSharpPlus
open Giraffe.ViewEngine
open LaTaleTools.Library.Spf
open LaTaleTools.Library.Tbl
open LaTaleTools.WebApp.Views

let public tblView (groups: SpriteGroup list) (getImageBase64: Sprite -> string option Task): XmlNode list Task =
    let titleRow =
        tr [] [
            th [] [ str "TBL Group Name" ]
            th [] [ str "Image Path" ]
            th [] [ str "Unknown" ]
            th [] [ str "Unknown" ]
            th [] [ str "Unknown" ]
            th [] [ str "Pos TopLeft" ]
            th [] [ str "Size" ]
            th [] [ str "Preview" ]
        ]
    let renderSpriteRow sprite =
        let (ArchivePath ap) = sprite.File
        task {
            let! base64Img = getImageBase64 sprite

            let preview =
                match base64Img with
                | None -> []
                | Some base64Img ->
                    [ img [ _src $"data:image/png;base64,{base64Img}" ] ]

            return [
                td [] (pathView true ap ap String.Empty)
                td [] [ str (sprite.Unknown1.ToString()) ]
                td [] [ str (sprite.UnknownVec1.ToString()) ]
                td [] [ str (sprite.UnknownVec2.ToString()) ]
                td [] [ str (sprite.PosTopLeft.ToString()) ]
                td [] [ str ((sprite.PosBottomRight - sprite.PosTopLeft).ToString()) ]
                td [] preview
            ]
        }
    let attachGroupInfo (group: SpriteGroup) rows =
        let groupRow =
            td [
                _rowspan (group.Sprites.Length.ToString())
            ] [
                str $"{group.Name} ({group.Sprites.Length})"
            ]
        match rows with
        | head :: tail -> ((groupRow :: head) :: tail)
        | _ -> [ groupRow :: List.replicate 7 (td [] []) ]
    let sprites =
        [
            for group in groups ->
                List.map renderSpriteRow group.Sprites
                |> sequence
                |> Task.map (attachGroupInfo group)
                |> Task.map (List.map (tr []))
        ]
        |> sequence

    task {
        let! rows = sprites
        let rows = List.collect id rows
        return [ table [ _border "1" ] (titleRow :: rows) ]
    }