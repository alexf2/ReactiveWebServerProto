## Reactive Web Server
The server is based on `HttpListener` wrapped into an observable sequence. Server core implementation is in `ReactiveWebServer.Runtime`, 
represented  by `ReactiveHttpWebServer`. Requests handlers are in `HttpRequestHandlers.Runtime`. A simple console host
is provided for testing in `ReactiveWebServer.ConsoleHost`. 

Basic test data is created by aid of `CreateTestData` in `Program` class at `ReactiveWebServer.ConsoleHost`. Test DB is generated
in the output directory **`bin`** in the root, next to the executing assembly. 
A couple of data providers are available: **Xml** and **Sql LIte**. They are switched in `App.config`.
If there is no any DB, then a empty one is created on the first request.

### Available Http handlers ###
1. **UserListHandler**
Handles Http GET on `http://server_domain:port/Guestbook/Users{?page=xxx&size=yyy}`.
Has optional paging query string parameters.
Returns users.

2. **UserMessagesHandler**
Handles Http GET on `http://server_domain:port/Guestbook/Users/user_login{?page=xxx&size=yyy}`
Requires UserLogin in the Url. Has optional paging query string parameters.
Shows the messages for UserLogin.

3. **ProxyHandler**
Handles Http GET on `http://server_domain:port/Proxy&url=xxx`
Requires 'url' in the query string.
Loads the Url content as a proxy.

4. **DeleteUserHandler**
Handles Http DELETE on `http://server_domain:port/Guestbook/Users/user_login`
Requires UserLogin in the Url.
Removes a user with messages off the Guest Book.

5. **ClearHandler**
Handles Http DELETE on `http://server_domain:port/Guestbook`
Clears out all the data in the Guest Book.

6. **AllMessagesHandler**
Handles Http GET  on `http://server_domain:port/Guestbook`
Returns all the content.

7. **AddUserHandler**
Handles Http POST on `http://server_domain:port/Guestbook/Users`
Needs **login** and **displayname** parameters in the request body.
Creates new user.

8. **AddMessageHandler**
Handles Http POST on `http://server_domain:port/Guestbook`
Needs **login** and **msgtext** parameters in the request body.
Creates new message, associated with specified UserLogin.

### Main Configuration Options ##
Should be adjusted in **App.config** of `ReactiveWebServer.ConsoleHost`.

1. **data-provider** - specifies data storage implementation. There is a couple available: Xml and SqlLite.

2. **host** - host name to listen Http requests at. You may specify a mask **+** or <b>*</b> , but masks require Admin permission
to run WebServer under. Or, you will have to grant your account an extra permission `netsh http add urlacl url=http://+:80/ user=DOMAIN\user`.
3. **port** - a port to listen requests on.

4. **storage-file** - specifies DB storage name and path for Xml storage. Sql Lite storage is specified via the connection string.
