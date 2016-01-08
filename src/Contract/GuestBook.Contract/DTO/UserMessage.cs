using System;

namespace AnywayAnyday.GuestBook.Contract.DTO
{
    /// <summary>
    /// Represents a guestbook message.
    /// </summary>
    public struct UserMessage
    {
        /// <summary>
        /// Id of the user, who created the message.
        /// </summary>
        public string UserLogin { get; set; }
        /// <summary>
        /// Message content.
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// When the message was created.
        /// </summary>
        public DateTime Created { get; set; }
    }
}
