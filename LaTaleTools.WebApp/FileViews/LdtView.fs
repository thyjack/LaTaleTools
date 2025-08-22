module LaTaleTools.WebApp.FileViews.LdtView

open Giraffe.ViewEngine
open LaTaleTools.Library.Ldt

let public ldtView (ldtTable: LdtTable)  =
    let titleRow =
        tr []
        <| ( ldtTable.Columns
             |> List.map (fun (name, type_) -> th [] [
                 str name
                 str $" ({type_})"
             ])
           )

    let cellToStr =
        function
        | UnsignedCell u -> u.ToString()
        | StringCell s   -> s
        | BoolCell b     -> b.ToString()
        | SignedCell s   -> s.ToString()
        | FloatCell f    -> f.ToString()

    let rows =
        ldtTable.Rows
        |> Seq.map ( fun cells ->
                        tr []
                        <| ( cells
                             |> List.map ( fun cell ->
                                               td [] [ str (cellToStr cell) ]
                                         )
                           )
                   )
        |> List.ofSeq

    [ table [ _border "1" ] (titleRow :: rows) ]