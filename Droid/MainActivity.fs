namespace Pushy.Droid
open System

open Android.App
open Android.Content
open Android.Content.PM
open Android.Runtime
open Android.Views
open Android.Widget
open Android.OS
open Xamarin.Forms.Platform.Android
open Firebase.Messaging
open Firebase.Iid
open Android.Util
open Android.Gms.Common
open System.Diagnostics
open Android.Support.V4
open Android.Support.V4.App
open Android.Support.V4.Content
open Android.Support.V7.App
open Pushy
open Plugin.CurrentActivity

#if DEBUG
[<Application(Debuggable = true)>]
#else
[<Application(Debuggable = false)>]
#endif
type MainApplication(handle:IntPtr, transer:JniHandleOwnership) =
    inherit Application(handle, transer)

    override this.OnCreate() =
        base.OnCreate()
        CrossCurrentActivity.Current.Init(this)

[<Service; IntentFilter([| "com.google.firebase.INSTANCE_ID_EVENT" |])>]
type MyFirebaseIIDService() =
    inherit FirebaseInstanceIdService()

    let sendRegistrationToServer token = ()

    override this.OnTokenRefresh() = 
        let refreshedToken = FirebaseInstanceId.Instance.Token;
        Debug.WriteLine <| "Refreshed token: " + refreshedToken 
        sendRegistrationToServer refreshedToken

type NotificationHandler(context:Context) = 

    let createNotificationChannel () = 
        let CHANNEL_ID = "my_channel_01";// The id of the channel. 
        let name = "FcmChannel"
        let importance = Android.App.NotificationImportance.High
        let mChannel = 
            new NotificationChannel(CHANNEL_ID, name, importance,
                LockscreenVisibility = NotificationVisibility.Public)
        mChannel.EnableVibration true
        mChannel.EnableLights true

        (CHANNEL_ID, mChannel)

    member this.ShowNotification (title:string) (body:string) = 

        Debug.WriteLine "Showing notification"
        let intent = new Intent(context, typeof<MainActivity>)
        let pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.UpdateCurrent)

        let notification = 
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O) then
                let (CHANNEL_ID, mChannel) = createNotificationChannel()
                NotificationManager.FromContext(context).CreateNotificationChannel mChannel
                (new Android.App.Notification.Builder(context, CHANNEL_ID))
                    .SetSmallIcon(Resources.Drawable.icon)
                    .SetContentTitle(title)
                    .SetContentText(body)
                    .SetChannelId(CHANNEL_ID)
                    .SetContentIntent(pendingIntent)
                    .SetAutoCancel(true)
                    .Build();

            else
                (new App.NotificationCompat.Builder(context))
                    .SetPriority(App.NotificationCompat.PriorityDefault)
                    .SetSmallIcon(Resources.Drawable.icon)
                    .SetContentTitle(title)
                    .SetContentText(body)
                    .SetContentIntent(pendingIntent)
                    .SetAutoCancel(true)
                    .Build();

        NotificationManager.FromContext(context).Notify(1, notification);
    
    interface ShowNotification with 
        member this.ShowNotification title body =
            this.ShowNotification title body

and [<Service; IntentFilter([| "com.google.firebase.MESSAGING_EVENT" |])>]
    MyFcmListenerService() as this =
    inherit FirebaseMessagingService()

    let dictToMap (dic : System.Collections.Generic.IDictionary<_,_>) = 
        dic 
        |> Seq.map (|KeyValue|)  
        |> Map.ofSeq

    let (|HasForegroundUI|InBackground|) (activity: Activity) = 
        match activity with 
        | :? MainActivity as mainActivity -> 
            match mainActivity.MainPage () with 
            | Some mainPage -> 
                match mainPage with 
                | :? Pushy.MainPage as mainPage -> HasForegroundUI mainPage
                | _ -> InBackground
            | None -> InBackground
        | _ -> InBackground

    // We must show on the UI and return response if successful. If we do a check first, the 
    // user could change the state of the application, leading to a missed notification. 
    let handleNotification title body = 
        match CrossCurrentActivity.Current.Activity with 
        | HasForegroundUI mainPage -> mainPage.HandleMessage title body
        | InBackground -> 
            Debug.WriteLine "In background, showing notification"
            let notifHandler = NotificationHandler(this)
            notifHandler.ShowNotification title body

    override this.OnMessageReceived(message: RemoteMessage) = 

        if message.GetNotification() <> null then 
            let notif = message.GetNotification()
            handleNotification notif.Title notif.Body

        let data = dictToMap message.Data
        if data.ContainsKey "title" && data.ContainsKey "body" then 
            let title, body = (data.["title"], data.["body"])
            handleNotification title body

        data |> Map.iter (fun key value -> Debug.WriteLine <| sprintf "%s:%s" key value)

and [<Activity (Label = "Pushy.Droid", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = (ConfigChanges.ScreenSize ||| ConfigChanges.Orientation))>]
    MainActivity() as this =
    inherit FormsAppCompatActivity()

    let isPlayServicesAvailable () =
        let resultCode =
            GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this)

        if resultCode = ConnectionResult.Success then
            HasGooglePlayServices
        else 
            if GoogleApiAvailability.Instance.IsUserResolvableError resultCode then
                GoogleApiAvailability.Instance.GetErrorString(resultCode) |> RequiresUser
            else NoGooglePlayServices


    let mutable formsApp: Pushy.App option = None
    member this.MainPage(): Xamarin.Forms.Page Option = 
        formsApp |> Option.map (fun x -> x.MainPage)

    override this.OnCreate (bundle: Bundle) =
        FormsAppCompatActivity.TabLayoutResource <- Resources.Layout.Tabbar
        FormsAppCompatActivity.ToolbarResource <- Resources.Layout.Toolbar
        Debug.WriteLine <| sprintf "Token: %s" FirebaseInstanceId.Instance.Token

        base.OnCreate (bundle)
        
        CrossCurrentActivity.Current.Init(this, bundle)
        Xamarin.Essentials.Platform.Init(this, bundle)
        Xamarin.Forms.Forms.Init (this, bundle)

        let app = new Pushy.App (isPlayServicesAvailable, NotificationHandler this)
        formsApp <- Some app
        this.LoadApplication app