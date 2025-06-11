module WebhookService.State

open System.Collections.Concurrent

let private processed = ConcurrentDictionary<string, byte>()

let isDuplicate (transactionId: string) =
    processed.ContainsKey(transactionId)

let markProcessed (transactionId: string) =
    processed.TryAdd(transactionId, 0uy) |> ignore

let clearState () =
    processed.Clear() 