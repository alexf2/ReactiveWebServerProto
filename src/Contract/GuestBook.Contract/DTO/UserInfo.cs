using System;

namespace AnywayAnyday.GuestBook.Contract.DTO
{
    /// <summary>
    /// Represents a user
    /// </summary>
    public struct UserInfo
    {
        /// <summary>
        /// User's login. Is used as primary key.
        /// </summary>
        public string UserLogin { get; set; }
        /// <summary>
        /// Friendly name.
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// When the used was created.
        /// </summary>
        public DateTime Created { get; set; }
    }
}
