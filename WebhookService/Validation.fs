module WebhookService.Validation

open WebhookService.Types

let secretToken = "meu-token-secreto"

type ValidationError =
    | InvalidToken
    | MalformedPayload
    | MissingFields
    | DuplicateTransaction
    | InvalidAmount
    | InvalidCurrency
    | InvalidEvent

let validateToken (tokenOpt: string option) =
    match tokenOpt with
    | Some token when token = secretToken -> Ok ()
    | _ -> Error InvalidToken

let validatePayload (payloadOpt: WebhookPayload option) =
    match payloadOpt with
    | None -> Error MalformedPayload
    | Some p when
        System.String.IsNullOrWhiteSpace(p.Amount) ||
        System.String.IsNullOrWhiteSpace(p.Currency) ||
        System.String.IsNullOrWhiteSpace(p.Timestamp) ||
        System.String.IsNullOrWhiteSpace(p.TransactionId) ||
        System.String.IsNullOrWhiteSpace(p.Event) -> Error MissingFields
    | Some p -> Ok p

let validateBusinessRules (payload: WebhookPayload) =
    let amountOk =
        match System.Decimal.TryParse(payload.Amount, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture) with
        | true, v when v > 0.0M -> true
        | _ -> false
    let currencyOk = payload.Currency = "BRL"
    let eventOk = payload.Event = "payment_success"
    match amountOk, currencyOk, eventOk with
    | false, _, _ -> Error InvalidAmount
    | _, false, _ -> Error InvalidCurrency
    | _, _, false -> Error InvalidEvent
    | true, true, true -> Ok () 