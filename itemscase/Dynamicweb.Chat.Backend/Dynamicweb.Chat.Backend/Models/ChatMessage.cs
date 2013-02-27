using Dynamicweb.Content.Items;

namespace Dynamicweb.Chat.Backend.Models
{
    /// <summary>
    /// Represents the sender of this chat message.
    /// </summary>
    public enum ChatMessageSender
    {
        /// <summary>
        /// Customer.
        /// </summary>
        Customer = 0,

        /// <summary>
        /// Manager.
        /// </summary>
        Manager = 1
    }

    /// <summary>
    /// Represents a chat message.
    /// </summary>
    public class ChatMessage : ItemEntry
    {
        #region Properties

        /// <summary>
        /// Gets or sets the sender.
        /// </summary>
        public ChatMessageSender Sender { get; set; }

        /// <summary>
        /// Gets or sets the message text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the messag was created.
        /// </summary>
        public System.DateTime Created { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of an object.
        /// </summary>
        public ChatMessage() { Created = System.DateTime.Now; }

        public override void Save()
        {
            base.Save();
        }
    }
}
