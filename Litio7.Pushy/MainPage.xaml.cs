using System;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(Litio7.Pushy.MainPage))]
namespace Litio7.Pushy
{
  public partial class MainPage : ContentPage, IHandleMessage
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
    private IShowNotification showNotificationService = DependencyService.Get<IShowNotification>();
    private int numberOfNotifications;

    public MainPage(GooglePlayServiceAvailable googleAvailability, bool isInForeground)
    {
      this.myLabel.Text = this.GetMessage(googleAvailability);
      this.myStackLayout.Children.Add(myLabel);

      this.Content = myStackLayout;

      this.isInForeground = isInForeground;

      InitializeComponent();
    }

    protected override void OnAppearing()
    {
      base.OnAppearing();

      this.showNotificationService.RaiseNotificationReceivedEvent += ShowNotification;
    }

    protected override void OnDisappearing()
    {
      base.OnDisappearing();

      this.showNotificationService.RaiseNotificationReceivedEvent -= ShowNotification;
    }

    private void ShowNotification(object sender, FirebaseNotificationArgs e)
    {
      var bodyWithNumber = e.Body + " Number: " + ++this.numberOfNotifications
      if (App.IsInForeground)
      {
        HandleMessage(e.Title, bodyWithNumber);
      }
      else
      {
        Console.WriteLine("Activity alive but in the background, showing notification");
        this.showNotificationService.ShowNotification(e.Title, bodyWithNumber);
      }
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
      MainThread.BeginInvokeOnMainThread(() =>
      {
        this.myLabel.Text = $"Received Message: {body}";
      });
    }
  }
}
