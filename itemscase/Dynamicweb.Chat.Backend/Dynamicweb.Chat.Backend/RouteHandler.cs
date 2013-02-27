using System;
using Dynamicweb.Extensibility;

namespace Dynamicweb.Chat.Backend
{
    /// <summary>
    /// Handles SignalR-specific URLs.
    /// </summary>
    [Subscribe(Notifications.Standard.Application.AuthenticateRequest)]
    public class RouteHandler : NotificationSubscriber
    {
        /// <summary>
        /// Initializes a new instance of an object.
        /// </summary>
        public RouteHandler() { }

        /// <summary>
        /// Handles notification.
        /// </summary>
        /// <param name="notification">Notification name.</param>
        /// <param name="args">Notification arguments.</param>
        public override void OnNotify(string notification, NotificationArgs args)
        {
            var e = args as Notifications.Standard.Application.AuthenticateRequestArgs;
            bool isEndingSession = e.Application.Request.QueryString["Chat.EndSessions"] != null;

            // Don't trigger Dynamicweb URL handling if accessing SignalR-specific resources
            // or if ending one or more chat sessions
            e.Handled = e.Application.Request.Url.AbsolutePath.StartsWith("/signalr", 
                StringComparison.InvariantCultureIgnoreCase) || isEndingSession;

            if (isEndingSession)
            {
                // Getting the list of sessions to end
                string[] sessionIds = e.Application.Request.QueryString["Chat.EndSessions"]
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                // Ending all sessions
                if (sessionIds.Length == 0)
                    Models.ChatSessionManager.Current.EndAll();
                else
                {
                    // Ending specific sessions
                    foreach (string sessionId in sessionIds)
                    {
                        if (Models.ChatSessionManager.Current.IsStarted(sessionId))
                            Models.ChatSessionManager.Current.End(sessionId);
                    }
                }
            }
        }
    }
}
