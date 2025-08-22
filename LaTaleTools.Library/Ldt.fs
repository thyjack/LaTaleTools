module LaTaleTools.Library.Ldt

open System.IO
open System.Text
open LaTaleTools.Library.Encoding
open LaTaleTools.Library.Util

type Cell =
  | UnsignedCell of uint32
  | StringCell of string
  | BoolCell of bool
  | SignedCell of int32
  | FloatCell of float32

type CellDataType =
  | Unsigned = 0
  | String = 1
  | Bool = 2
  | Signed = 3
  | Float = 4

type LdtTable =
  { Columns: (string * CellDataType) list
    Rows: Cell list seq
  }

let public readLdtTable (view: UnmanagedMemoryAccessor): LdtTable =
  let rowCount = view.ReadInt32 8L
  let columnCount = view.ReadInt32 4L + 1

  let columnNames =
    "_RowId" :: ( seq { 0 .. columnCount - 2 }
                  |> Seq.map (fun n -> view.ReadNullTerminatedString (12L + int64(n * 64)) 64 Encoding.UTF8)
                  |> List.ofSeq
                )
  let columnDataTypes =
    CellDataType.Signed ::
      ( seq { 0 .. columnCount - 2 }
        |> Seq.map (fun n -> view.ReadInt32 (8204L + int64(n * 4)))
        |> Seq.map enum<CellDataType>
        |> List.ofSeq
      )

  let readSingleRow pos =
    let step (pos, row) dataType =
      match dataType with
      | CellDataType.Unsigned ->
        (pos + 4L, (UnsignedCell <| view.ReadUInt32 pos) :: row)
      | CellDataType.Signed ->
        (pos + 4L, (SignedCell <| view.ReadInt32 pos) :: row)
      | CellDataType.Bool ->
        (pos + 4L, (BoolCell <| (view.ReadInt32 pos = 1)) :: row)
      | CellDataType.Float ->
        (pos + 4L, (FloatCell <| view.ReadSingle pos) :: row)
      | CellDataType.String ->
        let strLen = int32(view.ReadInt16 pos)
        let cell = StringCell <| view.ReadNullTerminatedString (pos + 2L) strLen ldtStringEncoding
        (pos + 2L + int64(strLen), cell :: row)
      | t -> failwith $"unexpected data type {t}"

    let newPos, revRow = List.fold step (pos, []) columnDataTypes
    (newPos, List.rev revRow)

  let rec readRow pos count =
    if count = rowCount
    then Seq.empty
    else
      let newPos, row = readSingleRow pos
      seq {
        yield row
        yield! readRow newPos (count + 1)
      }

  let rows =
    readRow 8716L 0

  { Columns = List.zip columnNames columnDataTypes
    Rows = rows
  }