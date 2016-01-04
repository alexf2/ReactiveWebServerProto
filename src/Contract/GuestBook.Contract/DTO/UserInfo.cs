using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnywayAnyday.GuestBook.Contract.DTO
{
    public struct UserInfo
    {
        public string UserLogin { get; set; }
        public string DisplayName { get; set; }
        public DateTime Created { get; set; }
    }
}
