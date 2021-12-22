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

[assembly: Dependency(typeof(Litio7.Pushy.Droid.MyFcmListenerService))]
namespace Litio7.Pushy.Droid
{
  [Activity(Label = "Litio7.Pushy", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
  public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
  {
    public Pushy.App App { get; private set; }
    public IShowNotification showNotification;


    protected override void OnCreate(Bundle savedInstanceState)
    {
      base.OnCreate(savedInstanceState);

      CrossCurrentActivity.Current.Init(this, savedInstanceState);
      Xamarin.Essentials.Platform.Init(this, savedInstanceState);
      Xamarin.Forms.Forms.Init(this, savedInstanceState);

      this.showNotification = DependencyService.Get<IShowNotification>();
      this.App = new App(IsGooglePlayServicesAvailable(), showNotification);
      this.LoadApplication(App);

      Console.WriteLine("InstanceId Token: " + FirebaseInstanceId.Instance.Token);
      FirebaseMessaging.Instance.GetToken();
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

    public (bool, Xamarin.Forms.Page) IsInBackground(Activity activity, Pushy.App app)
    {
      if (activity is MainActivity ma)
      {
        if (app.MainPage is Pushy.MainPage mp)
        {
          return (true, mp);
        }
      }

      return (false, null);
    }

    public void HandleNotification(string title, string body)
    {
      bool inBackground;
      Xamarin.Forms.Page page;
      (inBackground, page) = IsInBackground(CrossCurrentActivity.Current.Activity, this.App);
      var mp = page as Pushy.MainPage;

      if (!inBackground)
      {
        this.showNotification.ShowNotification(title, body);
      }
      else
      {
        // TODO: show notification on screen
        mp.HandleMessage(title, body);
      }
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
  public class MyFcmListenerService : FirebaseMessagingService, IShowNotification
  {
    public override void OnMessageReceived(RemoteMessage message)
    {
      base.OnMessageReceived(message);

      if(message.Data.ContainsKey("title") && message.Data.ContainsKey("body"))
      {
        var currentActivity = CrossCurrentActivity.Current.Activity as Pushy.Droid.MainActivity;
        currentActivity.HandleNotification(message.Data["title"], message.Data["body"]);
      }

      Console.WriteLine("FCM received message: ");
      foreach (var keyValue in message.Data)
      {
        Console.WriteLine(keyValue);
      }
    }

    public (string, NotificationChannel) CreateNotificationChannel()
    {
      var CHANNEL_ID = "my_channel_01";
      var name = "PushyChannel";
      var importance = Android.App.NotificationImportance.High;
      var mChannel = new NotificationChannel(CHANNEL_ID, name, importance)
      {
        LockscreenVisibility = NotificationVisibility.Public
      };
      mChannel.EnableVibration(true);
      mChannel.EnableLights(true);

      return (CHANNEL_ID, mChannel);
    }

    public void ShowNotification(string title, string body)
    {
      Console.WriteLine("Showing Notification");

      var intent = new Intent(CrossCurrentActivity.Current.AppContext, typeof(MainActivity));
      var pendingIntent = PendingIntent.GetActivity(CrossCurrentActivity.Current.AppContext, 0, intent, PendingIntentFlags.UpdateCurrent);

      string channel_id;
      NotificationChannel mChannel;
      Notification notification;
      if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
      {
        (channel_id, mChannel) = CreateNotificationChannel();
        NotificationManager.FromContext(CrossCurrentActivity.Current.AppContext)
          .CreateNotificationChannel(mChannel);

        notification = new Android.App.Notification.Builder(CrossCurrentActivity.Current.AppContext, channel_id)
          .SetSmallIcon(Resource.Drawable.common_google_signin_btn_icon_dark)
          .SetContentTitle(title)
          .SetContentText(body)
          .SetChannelId(channel_id)
          .SetContentIntent(pendingIntent)
          .SetAutoCancel(true)
          .Build();
      }
      else
      {
        notification = new AndroidX.Core.App.NotificationCompat.Builder(CrossCurrentActivity.Current.AppContext)
          .SetPriority(AndroidX.Core.App.NotificationCompat.PriorityDefault)
          .SetSmallIcon(Resource.Drawable.common_google_signin_btn_icon_dark_focused)
          .SetContentText(body)
          .SetContentTitle(title)
          .SetContentIntent(pendingIntent)
          .SetAutoCancel(true)
          .Build();
      }

      NotificationManager.FromContext(CrossCurrentActivity.Current.AppContext).Notify(1, notification);
    }
  }
}