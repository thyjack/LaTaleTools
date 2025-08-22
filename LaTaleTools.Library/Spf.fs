module LaTaleTools.Library.Spf

open System.IO
open LaTaleTools.Library.Encoding
open LaTaleTools.Library.Util

type public ArchivePath =
    | ArchivePath of string

    static member Join(path1, path2) =
        let path1 = trimPath path1
        let path2 = trimPath path2
        ArchivePath $"{path1}/{path2}"

type public FileEntryMetadata =
    { Path: ArchivePath

      ContentPosition: int
      ContentSize: int
      Index: int
    }

let readFixedString (view: UnmanagedMemoryAccessor) pos =
    let buffer = Array.zeroCreate<byte> 128
    view.ReadArray(pos, buffer, 0, 128)
        |> fun n -> assert (n = 128)

    let cutoff =
        Array.tryFindIndex (fun x -> x = 0uy) buffer
        |> Option.defaultValue 128
    filePathEncoding.GetString(System.ReadOnlySpan(buffer, 0, cutoff))

let public readMetadata (view: UnmanagedMemoryAccessor): FileEntryMetadata list =
    seq {
        let mutable ptr = view.Capacity - 140L
        let mutable contentPos = -1

        while contentPos <> 0 do
            ptr <- ptr - 140L

            let path = readFixedString view ptr
            contentPos <- view.ReadInt32 (ptr + 128L)
            let contentSize = view.ReadInt32 (ptr + 132L)
            let index = view.ReadInt32 (ptr + 136L)
            yield {
                Path = ArchivePath path
                ContentPosition = contentPos
                ContentSize = contentSize
                Index = index
            }
    } |> List.ofSeq