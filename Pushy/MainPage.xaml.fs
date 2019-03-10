namespace Pushy

open Xamarin.Forms
open Xamarin.Forms.Xaml
open Xamarin.Essentials
open System.Diagnostics

type GooglePlayServicesAvailable = 
| HasGooglePlayServices
| RequiresUser of string
| NoGooglePlayServices

type ShowNotification = 
    abstract member ShowNotification: title:string -> body:string -> unit

type MainPage(isPlayServicesAvailable, isInForegound, showNotification:ShowNotification) =
    inherit ContentPage()
    let _ = base.LoadFromXaml(typeof<MainPage>)
    let label = base.FindByName<Label>("Label")
    
    do 
        let message = 
            match isPlayServicesAvailable () with 
            | HasGooglePlayServices -> "Pushy is ready for push"
            | NoGooglePlayServices -> "Pushy is not supported on this device"
            | RequiresUser x -> sprintf "Pushy needs your help: %s" x

        label.Text <- message

    member this.HandleMessage title body = 
        // The Android Activity can still exist, but not be shown on the UI.
        // only show when the activity (this page) is on the UI
        if isInForegound () then 
            MainThread.BeginInvokeOnMainThread (fun () -> 
                label.Text <- sprintf "Received Message: %s" body )
        else 
            Debug.WriteLine "Activity alive but in the background, showing notificaiton"
            showNotification.ShowNotification title body