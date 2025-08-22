module LaTaleTools.WebApp.Logging

open System.Diagnostics
open Microsoft.Extensions.Logging

let public logged (logger: ILogger) (actionName: string) (action: unit -> 'a, additionalInfoOnComplete: 'a -> 'b): 'a =
    logger.LogInformation("Starting action {action}", actionName)
    let stopWatch = Stopwatch.StartNew()
    let result = action()
    stopWatch.Stop()
    let additionalInfo = additionalInfoOnComplete result
    logger.LogInformation(
        "Finished action {action} (in {duration}) with additional info: {info}",
        actionName,
        stopWatch.Elapsed,
        additionalInfo)

    result