using System;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Litio7.Pushy
{
  public partial class MainPage : ContentPage
  {

    public enum GooglePlayServiceAvailable
    {
      HasGooglePlayService = 0,
      RequiresUser,
      NoGooglePlayServices
    }

    private Label myLabel = new Label();
    private StackLayout myStackLayout = new StackLayout();
    private bool isInForeground;
    private IShowNotification showNotification;

    public MainPage(GooglePlayServiceAvailable googleAvailability, bool isInForeground, IShowNotification showNotification)
    {
      this.myLabel.Text = this.GetMessage(googleAvailability);
      this.myStackLayout.Children.Add(myLabel);

      this.Content = myStackLayout;

      this.isInForeground = isInForeground;
      this.showNotification = showNotification;

      InitializeComponent();
    }

    private string GetMessage(GooglePlayServiceAvailable googleAvailability)
    {
      switch (googleAvailability)
      {
        case GooglePlayServiceAvailable.HasGooglePlayService: return "Pushy is ready for push";
        case GooglePlayServiceAvailable.NoGooglePlayServices: return "Pushy is not supported on this device";
        case GooglePlayServiceAvailable.RequiresUser: return "Pushy needs your help";
        default: return string.Empty;
      };
    }

    public void HandleMessage(string title, string body)
    {
      /// The AndroidActivity can still exist, but not be shown on the UI.
      /// only show when the activity (this page) is on the UI
      if (this.isInForeground)
      {
        MainThread.BeginInvokeOnMainThread(() => 
        {
          this.myLabel.Text = $"Received Message: {body}";
        });
      }
      else
      {
        Console.WriteLine("Activity alive but in the background, showing notification");
        this.showNotification.ShowNotification(title, body);
      }
    }
  }
}
