using System;

namespace AnywayAnyday.GuestBook.Contract.DTO
{
    public struct UserMessage
    {
        public string UserLogin { get; set; }
        public string Text { get; set; }
        public DateTime Created { get; set; }
    }
}
