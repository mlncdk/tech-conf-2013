using System.Web.Routing;
using Dynamicweb.Extensibility;

namespace Dynamicweb.Chat.Backend
{
    /// <summary>
    /// Registers SignalR-specific routes when Dynamicweb application starts.
    /// </summary>
    [Subscribe(Notifications.Standard.Application.Start)]
    public class RouteRegistrator : NotificationSubscriber
    {
        /// <summary>
        /// Initializes a new instance of an object.
        /// </summary>
        public RouteRegistrator() { }

        /// <summary>
        /// Handles notification.
        /// </summary>
        /// <param name="notification">Notification name.</param>
        /// <param name="args">Notification arguments.</param>
        public override void OnNotify(string notification, NotificationArgs args)
        {
            // Registering SignalR-specific routes
            RouteTable.Routes.MapHubs();
        }
    }
}
