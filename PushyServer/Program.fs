// Learn more about F# at http://fsharp.org

open System
open Google.Apis.Auth.OAuth2
open FirebaseAdmin
open FirebaseAdmin.Messaging
open System.Collections.ObjectModel

[<EntryPoint>]
let main argv =

    let firebaseApp = 
        FirebaseApp.Create(
            AppOptions(
                Credential = GoogleCredential.FromFile(Constans.secretFilename)))

    let message =
        Message(
            Data = (dict [
                "title", "Pushy"
                "body", "Pushy is alive and well"
            ] |> ReadOnlyDictionary),
            Token = Constans.registrationToken)

    try
        let response = FirebaseMessaging.DefaultInstance.SendAsync(message) |> Async.AwaitTask |> Async.RunSynchronously
        Console.WriteLine("Successfully sent message: " + response)
    with e -> printfn "Failed to send message:\n%s\n%s" e.Message e.StackTrace   

    0
