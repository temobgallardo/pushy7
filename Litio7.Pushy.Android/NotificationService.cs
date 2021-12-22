using System;
using Android.App;
using Android.Content;
using Android.OS;
using Plugin.CurrentActivity;
using Xamarin.Forms;

[assembly: Dependency(typeof(Litio7.Pushy.Droid.NotificationService))]
namespace Litio7.Pushy.Droid
{
  public class NotificationService : IShowNotification
  {
    public event EventHandler<FirebaseNotificationArgs> RaiseNotificationReceivedEvent;

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

    public void OnRaiseNotificationReceivedEvent(string title, string body)
    {
      this.RaiseNotificationReceivedEvent?.Invoke(this, new FirebaseNotificationArgs
      {
        Title = title,
        Body = body
      });
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