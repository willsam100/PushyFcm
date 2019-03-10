namespace Pushy

open Xamarin.Forms

type App(isPlayServicesAvailable, notificationHandler:ShowNotification) =
    inherit Application()

    do base.MainPage <- MainPage(isPlayServicesAvailable, App.CheckInForeground, notificationHandler)

    static member val IsInForeground: bool = false with get,set

    static member CheckInForeground () = 
        App.IsInForeground

    override this.OnStart() = App.IsInForeground <- true
    override this.OnResume() = App.IsInForeground <- true
    override this.OnSleep() = App.IsInForeground <- false

