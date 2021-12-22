using System;

namespace Litio7.Pushy
{
  public interface IShowNotification
  {
    event EventHandler<FirebaseNotificationArgs> RaiseNotificationReceivedEvent;
    void ShowNotification(string title, string body);
    void OnRaiseNotificationReceivedEvent(string title, string body);
  }

  public class FirebaseNotificationArgs : EventArgs
  {
    public string Title { get; set; }
    public string Body { get; set; }
  }
}