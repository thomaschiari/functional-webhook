module WebhookService.HttpClients

open System.Net.Http
open System.Text
open System.Text.Json
open System.Threading.Tasks
open WebhookService.Types

let private client = new HttpClient()
let private confirmarUrl = "http://127.0.0.1:5001/confirmar"
let private cancelarUrl = "http://127.0.0.1:5001/cancelar"

let postJsonAsync (url: string) (data: obj) =
    task {
        let json = JsonSerializer.Serialize(data)
        use content = new StringContent(json, Encoding.UTF8, "application/json")
        let! _ = client.PostAsync(url, content)
        return ()
    }

let confirmTransaction (transactionId: string) =
    postJsonAsync confirmarUrl { TransactionId = transactionId }

let cancelTransaction (transactionId: string) =
    postJsonAsync cancelarUrl { TransactionId = transactionId } 