module LaTaleTools.Library.Tbl

open System
open System.IO
open System.Text
open FSharpPlus
open LaTaleTools.Library.Spf
open LaTaleTools.Library.Util
open SkiaSharp

type Sprite =
    { Unknown1: uint32
      UnknownVec1: Vec2<int32>
      UnknownVec2: Vec2<single>

      PosTopLeft: Vec2<int32>
      PosBottomRight: Vec2<int32>
      File: ArchivePath
    }

type SpriteGroup =
    { Name: string
      Sprites: Sprite list
    }

let public readSpriteStream (sprite: Sprite) (baseFile: byte ReadOnlySpan) : byte array =
    let (Vec2(width, height)) = sprite.PosBottomRight - sprite.PosTopLeft
    use sourceImage = SKBitmap.Decode(baseFile)

    use outputBitMap = new SKBitmap(width, height)
    let outputRect = SKRectI.Create(0, 0, width, height)
    use canvas = new SKCanvas(outputBitMap)
    canvas.Clear()

    let (Vec2(x, y)) = sprite.PosTopLeft
    let cropRect = SKRectI.Create(x, y, width, height)
    canvas.DrawBitmap(sourceImage, cropRect, outputRect)

    use outputImage = SKImage.FromBitmap(outputBitMap)
    use imageData = outputImage.Encode(SKEncodedImageFormat.Png, 100)
    imageData.ToArray()

let public readSpriteGroups (pathToTbl: string) (view: UnmanagedMemoryAccessor): SpriteGroup list =
    let groupCount = view.ReadInt32 4L

    let readSprites pos count =
        let spriteSize = 164L
        let readSingleSprite n =
            { Unknown1 = view.ReadUInt32 (pos + spriteSize * n)
              UnknownVec1 = view.ReadVec2<int32> (pos + spriteSize * n + 4L)
              UnknownVec2 = view.ReadVec2<single> (pos + spriteSize * n + 12L)
              PosTopLeft = view.ReadVec2<int32> (pos + spriteSize * n + 20L)
              PosBottomRight = view.ReadVec2<int32> (pos + spriteSize * n + 28L)
              File =
                  let file = view.ReadNullTerminatedString (pos + spriteSize * n + 36L) 128 Encoding.ASCII
                  ArchivePath.Join(pathToTbl, String.toUpper file)
            }
        let sprites =
            [ 0L .. count - 1L ]
            |> List.map readSingleSprite
        (pos + spriteSize * int64(count), sprites)
    let _, revGroups =
        let groupSize = 136L
        let foldFunc (pos, revGroups) _ =
            let spriteCount = view.ReadInt32 pos
            let name = view.ReadNullTerminatedString (pos + 4L) 16 Encoding.ASCII

            let newPos, sprites = readSprites (pos + groupSize) spriteCount
            let newGroup = { Name = name; Sprites = sprites }
            (newPos, newGroup :: revGroups)
        List.fold foldFunc (8L, []) [ 0 .. groupCount - 1 ]

    List.rev revGroups