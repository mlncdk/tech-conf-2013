using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace Dynamicweb.Chat.Backend
{
    /// <summary>
    /// Represents a chat hub.
    /// </summary>
    public class ChatHub  : Hub
    {
        #region Properties

        /// <summary>
        /// Gets the current chat sessions.
        /// </summary>
        public Models.ChatSessionManager Sessions
        {
            get { return Models.ChatSessionManager.Current; }
        }

        /// <summary>
        /// Gets the name of the participant.
        /// </summary>
        public string Name
        {
            get { return Clients.Caller.name != null ? Clients.Caller.name.ToString() : Context.QueryString["name"] ?? string.Empty; }
        }

        /// <summary>
        /// Gets value indicating whether the current caller is a manager.
        /// </summary>
        public bool IsManager
        {
            get { return Clients.Caller.manager != null ? (bool)Clients.Caller.manager : string.Compare(Context.QueryString["manager"] ?? string.Empty, "true") == 0; }
        }

        #endregion

        /// <summary>
        /// Occurs when new client is connected.
        /// </summary>
        /// <returns>Task.</returns>
        public override Task OnConnected()
        {
            string hostId = string.Empty;
            IEnumerable<Models.ChatSession> existingSessions = null;

            if (IsManager)
            {
                // Telling all other customers that the manager is online and ready to chat.
                Clients.Others.onManagerConnected();

                hostId = Context.ConnectionId;

                // Determining whether manager is reconnecting
                if (string.IsNullOrEmpty(Sessions.HostId))
                    existingSessions = Sessions.All;
                else
                    existingSessions = Sessions.New;

                foreach (Models.ChatSession awaitingSession in existingSessions)
                    Clients.Client(hostId).onClientConnected(awaitingSession.Name);

                Sessions.HostId = hostId;
            }
            else if (!string.IsNullOrEmpty(Name) && !Sessions.IsStarted(Name))
            {
                hostId = Sessions.HostId;

                Sessions.Start(Name, Context.ConnectionId);

                if (!string.IsNullOrEmpty(hostId))
                {
                    // Notifying the manager that new client is connected.
                    Clients.Client(hostId).onClientConnected(Name);

                    // Notifying the client that the manager is online.
                    Clients.Caller.onManagerConnected();
                }
            }

            return base.OnConnected();
        }

        /// <summary>
        /// Occurs when new client is disconnected.
        /// </summary>
        /// <returns>Task.</returns>
        public override Task OnDisconnected()
        {
            bool managerDisconnected = string.Compare(Context.ConnectionId,
                Sessions.HostId ?? string.Empty) == 0;

            if (managerDisconnected)
            {
                // Telling all other customers that the manager is no longer online.
                Clients.Others.onManagerDisconnected();
                
                // Do not end sessions from here, need HTTP context
                // Sessions.EndAll();

                Sessions.HostId = string.Empty;
            }
            else if (!string.IsNullOrEmpty(Name) && Sessions.IsStarted(Name))
            {
                // Do not end session from here, need HTTP context
                // Sessions.End(Name);

                // Notifying the manager that new client is connected.
                if (!string.IsNullOrEmpty(Sessions.HostId))
                    Clients.Client(Sessions.HostId).onClientDisconnected(Name);
            }

            return base.OnDisconnected();
        }

        /// <summary>
        /// Sends a message. 
        /// </summary>
        /// <param name="name">Client name.</param>
        /// <param name="message">Message text.</param>
        public void SendMessage(string name, string message)
        {
			DEBUG("SendMessage({0}, {1})", name, message);
			DEBUG("Sessions.HostId: {0}", Sessions.HostId);
        	
            string participant = null;
            Models.ChatMessageSender sender = Models.ChatMessageSender.Customer;

            if (!string.IsNullOrEmpty(Sessions.HostId) && Sessions.IsStarted(name) && !string.IsNullOrEmpty(message))
            {
                sender = string.Compare(Context.ConnectionId, Sessions.HostId) == 0 ?
                    Models.ChatMessageSender.Manager : Models.ChatMessageSender.Customer;

                Sessions.AddMessage(name, sender, message);

                if (sender == Models.ChatMessageSender.Customer && !string.IsNullOrEmpty(Sessions.HostId))
                    Clients.Client(Sessions.HostId).onMessageReceived(name, message);
                else if (sender == Models.ChatMessageSender.Manager)
                {
                    participant = Sessions.Participant(name);

                    if (!string.IsNullOrEmpty(participant))
                        Clients.Client(participant).onMessageReceived(string.Empty, message);
                }
            }
        }

		private static void DEBUG(string format, params object[] args)
		{
			LogToFile.Log(string.Format(format, args), "Chat", LogToFile.LogType.ManyEntriesPerFile);
		}
    }
}
