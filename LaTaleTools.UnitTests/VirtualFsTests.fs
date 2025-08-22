namespace LaTaleTools.UnitTests

open Faqt
open System
open LaTaleTools.Library.VirtualFs
open Microsoft.VisualStudio.TestTools.UnitTesting

[<TestClass>]
type VirtualFsTests() =
    let paths = [
        "DATA/CHAR/A.PNG";
        "DATA/CHAR/B.PNG";
        "DATA/CHAR/C.PNG";
        "DATA/CHAR/MONSTER/MON.PNG";
        "DATA2/LIST.LDT";
    ]
    let pathsSegments =
        paths
        |> List.sort
        |> List.map (fun p -> p.Split ('/', StringSplitOptions.RemoveEmptyEntries) |> List.ofArray)

    [<TestMethod>]
    member this.TestBuildTree() =
        let tree = buildTree paths

        let expected =
            FsDirNode (
                "/", "", [
                    FsDirNode ("DATA", "/", [
                        FsDirNode ("CHAR", "/DATA", [
                            FsNode ("A.PNG", "/DATA/CHAR")
                            FsNode ("B.PNG", "/DATA/CHAR")
                            FsNode ("C.PNG", "/DATA/CHAR")
                            FsDirNode ("MONSTER", "/DATA/CHAR", [
                                FsNode ("MON.PNG", "/DATA/CHAR/MONSTER")
                            ])
                        ])
                    ])
                    FsDirNode ("DATA2", "/", [
                        FsNode ("LIST.LDT", "/DATA2")
                    ])
                ]
            )

        tree.Should()
            .Be(expected) |> ignore

    [<TestMethod>]
    member this.TestVisit() =
        let tree = buildTree paths

        let root = fsVisit "/" tree
        let node1 = fsVisit "/DATA/CHAR/B.PNG" tree
        let node2 = fsVisit "/DATA2" tree
        let notFound = fsVisit "/DATA2/LIST.LDT/2" tree

        root.Value.Name
            .Should().Be("/") |> ignore
        node1.Value.Name
            .Should().Be("B.PNG") |> ignore
        node2.Value.Name
            .Should().Be("DATA2") |> ignore
        notFound.IsNone.Should().BeTrue() |> ignore