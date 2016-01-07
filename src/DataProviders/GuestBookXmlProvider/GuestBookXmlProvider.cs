using System;
using System.Collections.Generic;
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
using Castle.Core.Logging;

namespace AnywayAnyday.DataProviders.GuestBookXmlProvider
{
    public sealed class GuestBookXmlProvider : IGuestBookDataProvider
    {
        readonly string _filePath;
        volatile WeakReference _xdocRef;
        readonly ILogger _logger;
        bool _stgCreated;

        //http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266988.aspx
        //http://stackoverflow.com/questions/12694613/resource-locking-with-async-await
        AsyncLock _docLock = new AsyncLock();

        public GuestBookXmlProvider (string filePath, ILogger logger)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("The constructor parameter '{nameof(filePath)}' is empty");

            filePath = Path.GetFullPath(filePath);

            var path = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(path))
                throw new ArgumentException("The path '{path}' does not exist");

            _filePath = filePath;
            _logger = logger;
        }


        public async Task<UserMessage> AddMessage(string userLogin, string text)
        {
            using (await _docLock.LockAsync().ConfigureAwait(false))
            {
                var doc = await GetStorage();
                var user = GetUserNode(doc, userLogin, true);

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

                var user = GetUserNode(doc, userLogin);
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
            ValidatePagination(pageNumber, pageSize);
            using (await _docLock.LockAsync().ConfigureAwait(false))
            {
                var doc = await GetStorage();

                var usr = GetUserNode(doc, userLogin);
                if (usr == null)
                    return new DataPage<UserMessage>(pageNumber, pageSize, 0, null); //not found

                /*var elMessage = usr.Elements("message");
                int totalCount;
                IEnumerable<XElement> items;

                if (elMessage == null)
                {
                    totalCount = 0;
                    items = new XElement[0];
                }
                else
                {
                    totalCount = usr.Elements("message") == null ? 0 : usr.Elements("message").Count();

                    items = from m in usr.Elements("message")
                            orderby m.Attribute("created").Value
                            select m;
                } */

                var totalCount = usr.Elements("message") == null ? 0 : usr.Elements("message").Count();

                IEnumerable<XElement>  items = from m in usr.Elements("message")
                        orderby m.Attribute("created").Value
                        select m;

                if (pageSize != -1)
                    items = items.Skip((pageNumber - 1) * pageSize).Take(pageSize);

                return new DataPage<UserMessage>(pageNumber, pageSize, totalCount, items.Select(i => Mapper.Map<UserMessage>(i)).ToList());
            }
        }

        public async Task<DataPage<UserInfo>> GetUsers(int pageNumber, int pageSize)
        {
            ValidatePagination(pageNumber, pageSize);
            using (await _docLock.LockAsync().ConfigureAwait(false))
            {
                var doc = await GetStorage();

                var totalCount = doc.Root.Elements("user").Count();

                IEnumerable<XElement> items = from u in doc.Root.Elements("user")                            
                            orderby u.Attribute("login").Value
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

                GetUserNode(doc, userLogin)?.Remove();                

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
                _stgCreated = true;                
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
            if (_stgCreated)
            {
                _stgCreated = false;
                _logger.Info($"Created XML storage: {_filePath}");
            }
        }
        #endregion Storage

        XElement GetUserNode(XDocument doc, string login, bool autoCreate = false)
        {
            var el = (from u in doc.Root.Elements("user")
                      where u.Attribute("login").Value.Equals(login, StringComparison.OrdinalIgnoreCase)
                      select u).FirstOrDefault();

            if (autoCreate && el == null)
            {
                el = new XElement("user", new XAttribute("login", login), new XAttribute("created", DateTime.UtcNow));
                doc.Root.Add(el);
            }
            return el;
        }

        static void ValidatePagination(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new Exception("Page number should be 1 or greater");
            if (pageSize < -1)
                throw new Exception("Page size should be positive or -1, to return all records");
        }
    }
}
