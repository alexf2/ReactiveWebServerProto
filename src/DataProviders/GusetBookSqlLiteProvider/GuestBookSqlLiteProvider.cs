using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AnywayAnyday.GuestBook.Contract;
using AnywayAnyday.GuestBook.Contract.DTO;
using Castle.Core.Logging;
using Dapper;

namespace AnywayAnyday.GusetBookSqlLiteProvider
{
    public sealed class GuestBookSqlLiteProvider : IGuestBookDataProvider
    {
        readonly string _connectionString;
        readonly ILogger _logger;
        bool _stgCreated;

        public GuestBookSqlLiteProvider (string connectionString, ILogger logger)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("The constructor parameter '{nameof(connectionString)}' is empty");

            _connectionString = connectionString;
            _logger = logger;

            EnsureDbCreated();
        }

        public async Task<UserMessage> AddMessage(string userLogin, string text)
        {
            var res = new UserMessage() {Text = text, UserLogin = userLogin, Created = DateTime.UtcNow};
            using (var conn = new SQLiteConnection(_connectionString))
            {
                await conn.OpenAsync().ConfigureAwait(false);

                int count = await conn.ExecuteAsync(
                    "insert or ignore into [Message] (Text, Created, UserId) select @text as Text, @created as Created, UserId from [User] where [Login] = @userLogin",
                        new {text, created = res.Created, userLogin}).ConfigureAwait(false);
                if (count == 0)
                {
                    int userId =
                        await
                            conn.ExecuteScalarAsync<int>(
                                "insert into [User] (Login, Created) values(@userLogin, @created); SELECT last_insert_rowid() FROM [User]",
                                new {userLogin, created = res.Created}).ConfigureAwait(false);

                    await conn.ExecuteAsync("insert into [Message] (Text, Created, UserId) values(@text, @created, @userId)", new { text, created = res.Created, userId}).ConfigureAwait(false);
                }
            }
            return res;
        }

        public async Task<UserInfo> AddUser(string userLogin, string displayName)
        {
            var res = new UserInfo() {UserLogin = userLogin, DisplayName = displayName, Created = DateTime.UtcNow};
            using (var conn = new SQLiteConnection(_connectionString))
            {
                await conn.OpenAsync().ConfigureAwait(false);

                int count = await conn.ExecuteAsync("insert or ignore into [User] (Login, Display, Created) values(@userLogin, @displayName, @created)", 
                    new {userLogin, displayName, created = res.Created }).ConfigureAwait(false);

                if (count == 0)
                    throw new Exception($"The user '{userLogin}' already exists");
            }
            return res;
        }

        public async Task Clear()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                await conn.OpenAsync().ConfigureAwait(false);
                await conn.ExecuteAsync("delete from [User]").ConfigureAwait(false);
            }
        }

        public async Task<DataPage<UserMessage>> GetUserMessages(string userLogin, int pageNumber, int pageSize)
        {
            ValidatePagination(pageNumber, pageSize);
            using (var conn = new SQLiteConnection(_connectionString))
            {
                await conn.OpenAsync().ConfigureAwait(false);

                if (await conn.ExecuteScalarAsync<int>("select count(*) from [User] where Login = @userLogin", new { userLogin }).ConfigureAwait(false) == 0)
                    return new DataPage<UserMessage>(pageNumber, pageSize, 0, null); //not found

                int count = await conn.ExecuteScalarAsync<int>("select count(*) from [Message] m join [User] u on m.UserId=u.UserId where u.Login = @userLogin", new {userLogin}).ConfigureAwait(false);

                string q = "select u.Login as UserLogin, m.Text, m.Created from [Message] m join [User] u on m.UserId=u.UserId where u.Login = @userLogin order by m.Created";
                object prms;
                if (pageSize == -1)
                    prms = new { userLogin };
                else
                {
                    q += " limit @size offset @skip";
                    prms = new { userLogin, size = pageSize, skip = (pageNumber - 1) * pageSize };
                }

                return
                    new DataPage<UserMessage>(pageNumber, pageSize, count,
                        (await conn.QueryAsync<UserMessage>(q, prms)).ToList());
            }
        }

        public async Task<DataPage<UserInfo>> GetUsers(int pageNumber, int pageSize)
        {
            ValidatePagination(pageNumber, pageSize);
            using (var conn = new SQLiteConnection(_connectionString))
            {
                await conn.OpenAsync().ConfigureAwait(false);

                int count = await conn.ExecuteScalarAsync<int>("select count(*) from [User]").ConfigureAwait(false);

                string q = "select [Login] as UserLogin, Display as DisplayName, Created from [User] order by [Login]";
                object prms;
                if (pageSize == -1)                
                    prms = null;                
                else
                {
                    q += " limit @size offset @skip";
                    prms = new { size = pageSize, skip = (pageNumber - 1) * pageSize };
                }

                return
                    new DataPage<UserInfo>(pageNumber, pageSize, count,
                        (await conn.QueryAsync<UserInfo>(q, prms).ConfigureAwait(false)).ToList());
            }
        }

        public async Task<int> RemoveUser(string userLogin)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                await conn.OpenAsync().ConfigureAwait(false);
                return await conn.ExecuteAsync("delete from [User] where [Login] = @userLogin", new {userLogin}).ConfigureAwait(false);
            }
        }

        static void ValidatePagination(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new Exception("Page number should be 1 or greater");
            if (pageSize < -1)
                throw new Exception("Page size should be positive or -1, to return all records");
        }

        void EnsureDbCreated()
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                if (conn.ExecuteScalar<int>("PRAGMA user_version") == 0)
                {                    
                    conn.Execute(GetDdl());
                    conn.Execute("PRAGMA user_version = 1");
                    _logger.Info("New SqlLite DB created");
                }                
            }
        }

        string GetDdl()
        {
            var asm = GetType().Assembly;
            using (var stm = asm.GetManifestResourceStream(GetType(), "Schema.sql"))
                return new StreamReader(stm).ReadToEnd();
        }
    }
}
