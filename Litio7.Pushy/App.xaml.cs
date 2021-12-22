using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Litio7.Pushy.MainPage;

namespace Litio7.Pushy
{
  public partial class App : Application
  {
    public static bool IsInForeground;

    public App(GooglePlayServiceAvailable googleAvailability, IShowNotification notificationHandler)
    {
      InitializeComponent();

      MainPage = new MainPage(googleAvailability, App.IsInForeground, notificationHandler);
    }

    protected override void OnStart()
    {
      App.IsInForeground = true;
    }

    protected override void OnSleep()
    {
      App.IsInForeground = false;
    }

    protected override void OnResume()
    {
      App.IsInForeground = true;
    }
  }
}
