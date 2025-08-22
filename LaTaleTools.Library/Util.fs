module LaTaleTools.Library.Util

open System
open System.IO
open System.Numerics
open System.Runtime.CompilerServices
open System.Text
open FSharpPlus

let trimPath path =
    String.trimStart "/" path
    |> String.trimEnd "/"

let suffixMatchCi (string: string) (suffix: string)=
    string.EndsWith(suffix, StringComparison.InvariantCultureIgnoreCase)

[<IsReadOnly; Struct>]
type Vec2<'t when 't :> INumber<'t>> =
    | Vec2 of 't * 't
    static member (-) (Vec2(x2, y2): Vec2<'t>, Vec2(x1, y1): Vec2<'t>): Vec2<'t> =
        Vec2(x2 - x1, y2 - y1)

type UnmanagedMemoryAccessor with
  member this.ReadVec2<
          't when 't :> INumber<'t>
          and 't: (new: unit -> 't)
          and 't: struct
          and 't:> ValueType
      >(startingPos: int64): Vec2<'t> =
      let x = this.Read<'t>(startingPos)
      let y = this.Read<'t>(startingPos + int64(sizeof<'t>))
      Vec2(x, y)

  member this.ReadNullTerminatedString (startPos: int64) (maxLength: int) (encoding: Encoding): string =
    let byteArray = Array.zeroCreate<byte>(maxLength)
    this.ReadArray(startPos, byteArray, 0, maxLength) |> ignore

    let stringSize = byteArray |> Array.tryFindIndex (fun b -> b = 0uy)
                               |> Option.defaultValue maxLength

    encoding.GetString(byteArray[0..stringSize-1])