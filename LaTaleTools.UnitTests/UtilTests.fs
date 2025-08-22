namespace LaTaleTools.UnitTests

open Faqt
open LaTaleTools.Library.Util
open Microsoft.VisualStudio.TestTools.UnitTesting

[<TestClass>]
type UtilTests() =
    [<TestMethod>]
    member this.TestVecSubstraction() =
        let vec1 = Vec2(10, 11)
        let vec2 = Vec2(3, 5)

        let (Vec2(x, y)) = vec1 - vec2
        x.Should().Be(7) |> ignore
        y.Should().Be(6) |> ignore