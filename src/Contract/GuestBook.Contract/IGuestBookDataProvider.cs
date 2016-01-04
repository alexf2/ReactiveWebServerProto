using System.Threading.Tasks;
using AnywayAnyday.GuestBook.Contract.DTO;

namespace AnywayAnyday.GuestBook.Contract
{
    public interface IGuestBookDataProvider
    {
        Task<DataPage<UserInfo>> GetUsers(int pageNumber, int pageSize);
        Task<DataPage<UserMessage>> GetUserMessages(string userLogin, int pageNumber, int pageSize);
        Task<UserInfo> CreateUser(string userLogin, string displayName);
        Task<UserMessage> AddMessage(string userLogin, string text);
        Task Clear();
        Task RemoveUser(string userLogin);
    }
}
