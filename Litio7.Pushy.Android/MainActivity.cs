using System;

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Gms.Common;
using static Litio7.Pushy.MainPage;
using Firebase.Iid;
using Firebase.Messaging;
using Android.Content;
using Xamarin.Forms;
using Plugin.CurrentActivity;

namespace Litio7.Pushy.Droid
{
  [Activity(Label = "Litio7.Pushy", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
  public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
  {
    private const string TOPIC = "android_en-US_un_americas_unitedstates_uny";
    public Pushy.App App { get; private set; }

    protected override void OnCreate(Bundle savedInstanceState)
    {
      base.OnCreate(savedInstanceState);

      CrossCurrentActivity.Current.Init(this, savedInstanceState);
      Xamarin.Essentials.Platform.Init(this, savedInstanceState);
      Xamarin.Forms.Forms.Init(this, savedInstanceState);

      this.App = new App(IsGooglePlayServicesAvailable());
      this.LoadApplication(App);

      Console.WriteLine("InstanceId Token: " + FirebaseInstanceId.Instance.Token);
      FirebaseMessaging.Instance.GetToken();
      FirebaseMessaging.Instance.SubscribeToTopic(TOPIC);
    }

    private GooglePlayServiceAvailable IsGooglePlayServicesAvailable()
    {
      var resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);

      if (resultCode == ConnectionResult.Success)
      {
        return GooglePlayServiceAvailable.HasGooglePlayService;
      }
      else if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
      {
        Console.WriteLine(GoogleApiAvailability.Instance.GetErrorString(resultCode));
        return GooglePlayServiceAvailable.RequiresUser;
      }

      return GooglePlayServiceAvailable.NoGooglePlayServices;
    }
  }

  [Service]
  [IntentFilter(new string[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
  public class MyFirebaseIdService : FirebaseInstanceIdService
  {
    public void SendRegistrationTokenToServer(string token) { }

    public override void OnTokenRefresh()
    {
      var refreshToken = FirebaseInstanceId.Instance.Token;
      Console.WriteLine("Refresh Token: " + refreshToken);
      SendRegistrationTokenToServer(refreshToken);
    }
  }

  [Service]
  [IntentFilter(new string[] { "com.google.firebase.MESSAGING_EVENT" })]
  public class MyFcmListenerService : FirebaseMessagingService
  {
    public IShowNotification notificationService = DependencyService.Get<IShowNotification>();

    public override void OnMessageReceived(RemoteMessage message)
    {
      base.OnMessageReceived(message);

      if (message.Data.ContainsKey("title") && message.Data.ContainsKey("body"))
      {
        this.notificationService.OnRaiseNotificationReceivedEvent(message.Data["title"], message.Data["body"]);
      }

      Console.WriteLine("FCM received message: ");
      foreach (var keyValue in message.Data)
      {
        Console.WriteLine(keyValue);
      }
    }
  }
}