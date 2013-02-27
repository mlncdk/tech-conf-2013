using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Dynamicweb;
using Dynamicweb.Content;
using Dynamicweb.Content.Items;

namespace Dynamicweb.Chat.Backend.Models
{
    /// <summary>
    /// Represents a chat session manager. This class cannot be inherited.
    /// </summary>
    public sealed class ChatSessionManager
    {
        #region Properties

        private string _hostId = string.Empty;
        private Microsoft.AspNet.SignalR.Hub _hub = null;
        private ConcurrentDictionary<string, string> _participants = null;
        private ConcurrentDictionary<string, ChatSession> _sessions = null;
        private ConcurrentDictionary<string, ChatSession> _newSessions = null;
        private ConcurrentDictionary<string, List<ChatMessage>> _messages = null;
        private static ChatSessionManager _current = new ChatSessionManager();

        /// <summary>
        /// Gets or sets the Id of the host for all chat sessions.
        /// </summary>
        public string HostId
        {
            get { return _hostId; }
            set 
            { 
                _hostId = value;
                this._newSessions = new ConcurrentDictionary<string, ChatSession>();
            }
        }

        /// <summary>
        /// Gets or sets the total number of current open chat sessions.
        /// </summary>
        public int Count
        {
            get { return _sessions.Count; }
        }

        /// <summary>
        /// Gets the list of all active chat sessions.
        /// </summary>
        public IEnumerable<ChatSession> All
        {
            get { return _sessions.Values; }
        }

        /// <summary>
        /// Gets the list of new chat sessions.
        /// </summary>
        public IEnumerable<ChatSession> New
        {
            get { return _newSessions.Values; }
        }

        /// <summary>
        /// Gets the current instance of the chat session manager.
        /// </summary>
        public static ChatSessionManager Current
        {
            get { return _current; }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of an object.
        /// </summary>
        private ChatSessionManager()
        {
            this.HostId = string.Empty;

            this._newSessions = new ConcurrentDictionary<string, ChatSession>();
            this._participants = new ConcurrentDictionary<string, string>();
            this._sessions = new ConcurrentDictionary<string, ChatSession>();
            this._messages = new ConcurrentDictionary<string, List<ChatMessage>>();
        }

        /// <summary>
        /// Returns all messages from the given chat session.
        /// </summary>
        /// <param name="name">Chat session name.</param>
        /// <returns>All messages from the given chat session.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="name">name</paramref> is null.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="name">name</paramref> is an empty string or not session with the given name was started.</exception>
        public IList<ChatMessage> Messages(string name)
        {
            IList<ChatMessage> ret = null;
            List<ChatMessage> matchingMessages = null;

            if (ret == null)
                ret = new List<ChatMessage>();

            if (name == null)
                throw new ArgumentNullException("name");
            else if (name.Length == 0)
                throw new ArgumentException("Session name cannot be an empty string.", "name");
            else if (!IsStarted(name))
                throw new ArgumentException("Session with the given name has not been started.", "name");
            else
            {
                // Copying elements in order not to have side effects with adding/removing them 
                if (this._messages.TryGetValue(name, out matchingMessages))
                    ret = new List<ChatMessage>(matchingMessages);
            }

            return ret;
        }

        /// <summary>
        /// Returns all messages from the given chat session.
        /// </summary>
        /// <param name="session">Chat session.</param>
        /// <returns>All messages from the given chat session.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="session">session</paramref> is null.</exception>
        public IList<ChatMessage> Messages(ChatSession session)
        {
            IList<ChatMessage> ret = null;
            ChatSession matchingSession = null;

            if (session == null)
                throw new ArgumentNullException("session");
            else
            {
                // Ensuring that session wasn't created outside of the manager 
                matchingSession = All.FirstOrDefault(s => s == session);

                if (matchingSession != null)
                    ret = Messages(matchingSession.Name);
            }

            if (ret == null)
                ret = new List<ChatMessage>();

            return ret;
        }

        /// <summary>
        /// Gets the participant in the current session.
        /// </summary>
        /// <param name="name">Chat session name.</param>
        /// <returns>Participant Id.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="name">name</paramref> is null.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="name">name</paramref> is an empty string or not session with the given name was started.</exception>
        public string Participant(string name)
        {
            string ret = string.Empty;

            if (name == null)
                throw new ArgumentNullException("name");
            else if (name.Length == 0)
                throw new ArgumentException("Session name cannot be an empty string.", "name");
            else if (!IsStarted(name))
                throw new ArgumentException("Session with the given name has not been started.", "name");
            else
                this._participants.TryGetValue(name, out ret);

            return ret;
        }

        /// <summary>
        /// Starts the new chat session.
        /// </summary>
        /// <param name="name">Session name.</param>
        /// <param name="participant">An Id of the participant.</param>
        /// <returns>Started chat session.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="name">name</paramref> or <paramref name="participant">participant</paramref> is null.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="name">name</paramref> or <paramref name="participant">participant</paramref> is an empty string or the session with the given name already started.</exception>
        public ChatSession Start(string name, string participant)
        {
            ChatSession ret = null;

            if (name == null)
                throw new ArgumentNullException("name");
            else if (name.Length == 0)
                throw new ArgumentException("Session name cannot be an empty string.", "name");
            if (participant == null)
                throw new ArgumentNullException("participant");
            else if (participant.Length == 0)
                throw new ArgumentException("Session participant cannot be an empty string.", "participant");
            else if (IsStarted(name))
                throw new ArgumentException("Session with the given name has already been started.", "name");
            else
            {
                ret = new ChatSession() { Name = name };

                this._participants.AddOrUpdate(name, participant, (k, e) => participant);
                this._sessions.AddOrUpdate(name, ret, (k, e) => ret);
                this._messages.AddOrUpdate(name, new List<ChatMessage>(), (k, e) => new List<ChatMessage>());

                if (string.IsNullOrEmpty(HostId))
                    this._newSessions.AddOrUpdate(name, ret, (k, e) => ret);
            }

            return ret;
        }

        /// <summary>
        /// Returns value indicating whether the given session is already started.
        /// </summary>
        /// <param name="name">Session name.</param>
        /// <returns>Value indicating whether the given session is already started.</returns>
        public bool IsStarted(string name)
        {
            return this._sessions.ContainsKey(name ?? string.Empty);
        }

        /// <summary>
        /// Ends the given chat session.
        /// </summary>
        /// <param name="name">Chat session name.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="name">name</paramref> is null.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="name">name</paramref> is an empty string or not session with the given name was started.</exception>
        public void End(string name)
        {
            ChatSession session = null;
            List<ChatMessage> messages = null;
            string participant = string.Empty;

            if (name == null)
                throw new ArgumentNullException("name");
            else if (name.Length == 0)
                throw new ArgumentException("Session name cannot be an empty string.", "name");
            else if (!IsStarted(name))
                throw new ArgumentException("Session with the given name has not been started.", "name");
            else
            {
                this._sessions.TryRemove(name, out session);
                this._messages.TryRemove(name, out messages);
                this._participants.TryRemove(name, out participant);
                
                SaveHistory(session, messages);

                this._newSessions.TryRemove(name, out session);
            }
        }

        /// <summary>
        /// Ends all sessions.
        /// </summary>
        public void EndAll()
        {
            string[] allSessions = this._sessions.Keys.ToArray();

            foreach (string name in allSessions)
                End(name);
        }

        /// <summary>
        /// Adds new messages to the given chat session.
        /// </summary>
        /// <param name="name">Chat session name.</param>
        /// <param name="sender">Messages sender.</param>
        /// <param name="message">Message text.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="name">name</paramref> or <paramref name="message">message</paramref> is null.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="name">name</paramref> or <paramref name="message">message</paramref> is an empty string or the session with the given name has not been started.</exception>
        public void AddMessage(string name, ChatMessageSender sender, string message)
        {
            ChatMessage msg = null;

            if (name == null)
                throw new ArgumentNullException("name");
            else if (message == null)
                throw new ArgumentNullException("message");
            else if (name.Length == 0)
                throw new ArgumentException("Session name cannot be an empty string.", "name");
            else if (message.Length == 0)
                throw new ArgumentException("Message text cannot be an empty string.", "message");
            else if (!IsStarted(name))
                throw new ArgumentException("Session with the given name has not been started.", "name");
            else
            {
                msg = new ChatMessage() 
                { 
                    Sender = sender, 
                    Text = message,
                    Created = DateTime.Now
                };

                this._messages.AddOrUpdate(name, new List<ChatMessage>(new ChatMessage[] { msg }), 
                    (k, e) => new List<ChatMessage>(e.Concat(new ChatMessage[] { msg })));
            }
        }

        /// <summary>
        /// Saves the chat session to the history.
        /// </summary>
        /// <param name="session">Chat session.</param>
        /// <param name="message">Messages.</param>
        private void SaveHistory(ChatSession session, IList<ChatMessage> messages)
        {
            int historyPageId = -1;

            if (session != null && messages != null && messages.Any(m => m != null && !string.IsNullOrWhiteSpace(m.Text)))
            {
                historyPageId = Input.FormatInteger(Database.ExecuteScalar("SELECT TOP 1 [PageID] FROM [Page] WHERE [PageMenuText] = 'Chat history'"));
                
                if (historyPageId > 0)
                {
                    var page = new Page(1, historyPageId);

                    page.MenuText = string.Format("{0}, {1}", session.Name, session.Started.ToLongDateString());

                    session.Save();

                    page.ItemType = "ChatSession";
                    page.ItemId = session.Id;

                    page.Save();

                    foreach (ChatMessage message in messages.Where(m => m != null && !string.IsNullOrWhiteSpace(m.Text)))
                    {
                        var messagePage = new Page(page.AreaID, page.ID);

                        messagePage.MenuText = message.Text.Length > 15 ? 
                            message.Text.Substring(0, 15) + "..." : message.Text;

                        message.Save();

                        messagePage.ItemType = "ChatMessage";
                        messagePage.ItemId = message.Id;

                        messagePage.Save();
                    }
                }
            }
        }
    }
}
