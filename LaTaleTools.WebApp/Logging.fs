module LaTaleTools.WebApp.Logging

open System
open System.Diagnostics
open System.Threading.Tasks
open Microsoft.Extensions.Logging

[<Struct>]
type private ActionLogger(logger: ILogger, actionName: string, startTimeTicks: int64) =
    interface IAsyncDisposable with
        member _.DisposeAsync() =
            let elapsed = Stopwatch.GetElapsedTime(startTimeTicks)
            logger.LogInformation("Completed {action} in {duration}", actionName, elapsed)
            ValueTask.CompletedTask

    interface IDisposable with
        member _.Dispose() =
            let elapsed = Stopwatch.GetElapsedTime(startTimeTicks)
            logger.LogInformation("Completed {action} in {duration}", actionName, elapsed)

let public prepareActionLogger (logger: ILogger) (actionName: string) : IDisposable =
    let startTimeTicks = Stopwatch.GetTimestamp()
    new ActionLogger(logger, actionName, startTimeTicks)
