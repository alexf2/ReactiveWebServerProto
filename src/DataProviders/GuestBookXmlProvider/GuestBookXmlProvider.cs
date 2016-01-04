using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using AnywayAnyday.GuestBook.Contract;
using AnywayAnyday.GuestBook.Contract.DTO;
using Nito.AsyncEx;
using AutoMapper;

namespace AnywayAnyday.DataProviders.GuestBookXmlProvider
{
    public sealed class GuestBookXmlProvider : IGuestBookDataProvider
    {
        readonly string _filePath;
        volatile WeakReference _xdocRef;

        //http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266988.aspx
        //http://stackoverflow.com/questions/12694613/resource-locking-with-async-await
        AsyncLock _docLock = new AsyncLock();

        public GuestBookXmlProvider (string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("The constructor parameter '{nameof(filePath)}' is empty");

            filePath = Path.GetFullPath(filePath);

            var path = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(path))
                throw new ArgumentException("The path '{path}' does not exist");

            _filePath = filePath;
        }

        public async Task<UserMessage> AddMessage(string userLogin, string text)
        {
            using (await _docLock.LockAsync().ConfigureAwait(false))
            {
                var doc = await GetStorage();
                var user = GetUserNode(doc, userLogin);

                var msg = new UserMessage() {UserLogin = userLogin, Text = text, Created = DateTime.UtcNow};
                
                user.Add(Mapper.Map<XElement>(msg));

                await SaveStorage(doc).ConfigureAwait(false);
                return msg;
            }            
        }        

        public async Task Clear()
        {
            using (await _docLock.LockAsync().ConfigureAwait(false))
            {
                var doc = await GetStorage();
                doc.Root.RemoveAll();
                await SaveStorage(doc).ConfigureAwait(false);
            }
        }

        public async Task<UserInfo> AddUser(string userLogin, string displayName)
        {
            using (await _docLock.LockAsync().ConfigureAwait(false))
            {
                var doc = await GetStorage();

                var user = (from u in doc.Root.Elements("user")
                    where u.Attribute("login").Value.Equals(userLogin, StringComparison.OrdinalIgnoreCase)
                    select u).FirstOrDefault();

                if (user != null)
                    throw new Exception($"The user '{userLogin}' already exists");

                var res = new UserInfo()
                {
                    UserLogin = userLogin,
                    DisplayName = displayName,
                    Created = DateTime.UtcNow
                };                
                doc.Root.Add(Mapper.Map<XElement>(res));

                await SaveStorage(doc).ConfigureAwait(false);

                return res;
            }
        }

        public async Task<DataPage<UserMessage>> GetUserMessages(string userLogin, int pageNumber, int pageSize)
        {
            using (await _docLock.LockAsync().ConfigureAwait(false))
            {
                var doc = await GetStorage();

                var totalCount = (from u in doc.Root.Elements("user")
                    where u.Attribute("login").Value.Equals(userLogin, StringComparison.OrdinalIgnoreCase)
                                  select u.Elements("message")).Count();

                var items = from u in doc.Root.Elements("user")
                    where u.Attribute("login").Value.Equals(userLogin, StringComparison.OrdinalIgnoreCase)
                            select u.Elements("message");

                if (pageSize != -1)
                    items = items.Skip((pageNumber - 1) * pageSize).Take(pageSize);

                return new DataPage<UserMessage>(pageNumber, pageSize, totalCount, items.Select(i => Mapper.Map<UserMessage>(i)).ToList());
            }
        }

        public async Task<DataPage<UserInfo>> GetUsers(int pageNumber, int pageSize)
        {
            using (await _docLock.LockAsync().ConfigureAwait(false))
            {
                var doc = await GetStorage();

                var totalCount = (from u in doc.Root.Elements("user") select u).Count();

                var items = from u in doc.Root.Elements("user")                            
                            select u;

                if (pageSize != -1)
                    items = items.Skip((pageNumber - 1) * pageSize).Take(pageSize);

                return new DataPage<UserInfo>(pageNumber, pageSize, totalCount, items.Select(i => Mapper.Map<UserInfo>(i)).ToList());
            }
        }

        public async Task RemoveUser(string userLogin)
        {
            using (await _docLock.LockAsync().ConfigureAwait(false))
            {
                var doc = await GetStorage();

                (from u in doc.Root.Elements("user")
                 where u.Attribute("login").Value.Equals(userLogin, StringComparison.OrdinalIgnoreCase)
                 select u).Remove();

                await SaveStorage(doc).ConfigureAwait(false);
            }            
        }

        #region Storage
        async Task<XDocument> GetStorage()
        {
            if (_xdocRef != null)
            {
                var doc = (XDocument)_xdocRef.Target;
                if (doc != null)
                    return doc;
            }

            if (!File.Exists(_filePath))
            {
                XDocument res;
                _xdocRef = new WeakReference(res = CreateEmptyDoc());
                return res;
            }
            else
            {
                string content;
                using (var reader = File.OpenText(_filePath))
                    content = await reader.ReadToEndAsync().ConfigureAwait(false);

                return await Task<XDocument>.Factory.StartNew(() =>
                {
                    var doc = XDocument.Parse(content);
                    _xdocRef = new WeakReference(doc);
                    return doc;
                }).ConfigureAwait(false);
            }
        }

        XDocument CreateEmptyDoc()
        {
            return new XDocument(new XElement("users", new XAttribute("created", DateTime.UtcNow)));
        }

        async Task SaveStorage (XDocument doc)
        {
            using (var writer = XmlWriter.Create(_filePath, new XmlWriterSettings() {Indent = true, Encoding = Encoding.UTF8, CloseOutput = true}))
            {
                await Task.Factory.StartNew(() => doc.WriteTo(writer) ).ConfigureAwait(false);                
            }
        }
        #endregion Storage

        XElement GetUserNode(XDocument doc, string login)
        {
            var el = (from u in doc.Root.Elements("user")
                      where u.Attribute("login").Value.Equals(login, StringComparison.OrdinalIgnoreCase)
                      select u).FirstOrDefault();

            if (el == null)
            {
                el = new XElement("user", new XAttribute("login", login), new XAttribute("created", DateTime.UtcNow));
                doc.Root.Add(el);
            }
            return el;
        }        
    }
}
