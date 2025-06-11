module WebhookService.Types

open System.Text.Json.Serialization

// Represents the incoming webhook payload
[<CLIMutable>]
type WebhookPayload = {
    [<JsonPropertyName("event")>]
    Event: string
    [<JsonPropertyName("transaction_id")>]
    TransactionId: string
    [<JsonPropertyName("amount")>]
    Amount: string
    [<JsonPropertyName("currency")>]
    Currency: string
    [<JsonPropertyName("timestamp")>]
    Timestamp: string
}

// Represents the transaction result sent to /confirmar or /cancelar
[<CLIMutable>]
type TransactionResult = {
    [<JsonPropertyName("transaction_id")>]
    TransactionId: string
} 