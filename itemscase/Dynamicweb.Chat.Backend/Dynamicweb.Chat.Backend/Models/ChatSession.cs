using Dynamicweb.Content.Items;

namespace Dynamicweb.Chat.Backend.Models
{
    /// <summary>
    /// Represents a chat session.
    /// </summary>
    public class ChatSession : ItemEntry
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name of the session.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the chat session was started.
        /// </summary>
        public System.DateTime Started { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of an object.
        /// </summary>
        public ChatSession() { Started = System.DateTime.Now; }
    }
}
