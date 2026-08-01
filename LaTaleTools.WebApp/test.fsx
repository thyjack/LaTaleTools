open System
open System.Threading.Tasks

type Test() =
    interface IAsyncDisposable with
        member _.DisposeAsync() =
            printfn "Disposing"
            ValueTask()
