module WebhookService.Program

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe
open System.Text.Json
open System.IO

let deserializePayload (ctx: Microsoft.AspNetCore.Http.HttpContext) = task {
    try
        use reader = new StreamReader(ctx.Request.Body)
        let! body = reader.ReadToEndAsync()
        if System.String.IsNullOrWhiteSpace(body) then
            return None
        else
            return JsonSerializer.Deserialize<WebhookService.Types.WebhookPayload>(body) |> Option.ofObj
    with _ ->
        return None
}

let webhookHandler : HttpHandler = fun next (ctx: Microsoft.AspNetCore.Http.HttpContext) -> task {
    // Step 1: Token validation
    let tokenOpt =
        match ctx.Request.Headers.TryGetValue("X-Webhook-Token") with
        | true, v when v.Count > 0 -> Some (v.[0])
        | _ -> None
    match WebhookService.Validation.validateToken tokenOpt with
    | Error WebhookService.Validation.InvalidToken ->
        return! (setStatusCode 401 >=> text "Unauthorized") next ctx
    | Ok () ->
        // Step 2: Payload deserialization and integrity
        let! payloadOpt = deserializePayload ctx
        match WebhookService.Validation.validatePayload payloadOpt with
        | Error WebhookService.Validation.MalformedPayload ->
            return! (setStatusCode 400 >=> text "Malformed payload") next ctx
        | Error WebhookService.Validation.MissingFields ->
            match payloadOpt with
            | Some p ->
                do! WebhookService.HttpClients.cancelTransaction p.TransactionId
                return! (setStatusCode 422 >=> text "Missing fields") next ctx
            | None ->
                return! (setStatusCode 400 >=> text "Malformed payload") next ctx
        | Ok payload ->
            // Step 3: Idempotency check
            if WebhookService.State.isDuplicate payload.TransactionId then
                return! (setStatusCode 409 >=> text "Duplicate transaction") next ctx
            else
                // Step 4: Business rules
                match WebhookService.Validation.validateBusinessRules payload with
                | Ok () ->
                    WebhookService.State.markProcessed payload.TransactionId
                    do! WebhookService.HttpClients.confirmTransaction payload.TransactionId
                    return! (setStatusCode 200 >=> text "OK") next ctx
                | Error _ ->
                    do! WebhookService.HttpClients.cancelTransaction payload.TransactionId
                    return! (setStatusCode 422 >=> text "Business rule failed") next ctx
}

let webApp =
    choose [
        POST >=> route "/webhook" >=> webhookHandler
        setStatusCode 404 >=> text "Not Found"
    ]

[<EntryPoint>]
let main _ =
    // Clear state when server starts
    WebhookService.State.clearState()
    
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .ConfigureServices(fun services ->
                    services.AddGiraffe() |> ignore)
                .Configure(fun app ->
                    app.UseGiraffe webApp)
                .UseUrls("http://localhost:5000")
            |> ignore)
        .Build()
        .Run()
    0 