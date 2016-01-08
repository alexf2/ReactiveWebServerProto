namespace AnywayAnyday.ReactiveWebServer.Contract
{
    /// <summary>
    /// Collects data needed to create an URL on which the server will listen.
    /// Masks + and * require administrator permission. I.e. you should run it under a privileged account.
    /// Another option - is to grant permissions to your account or to use 'localhost'.
    /// Example how to grant the permission: netsh http add urlacl url=http://+:80/ user=DOMAIN\user
    /// </summary>
    public struct Endpoint
    {
        public int Port;
        public string HostName;
        public string Protocol;

        public static Endpoint AllHttpStrong(int port)
        {
            return new Endpoint {Port = port, HostName = "+", Protocol = "http"};
        }
        public static Endpoint AllHttpWeak(int port)
        {
            return new Endpoint { Port = port, HostName = "*", Protocol = "http" };
        }
        public static Endpoint HttpLocal(int port)
        {
            return new Endpoint { Port = port, HostName = "localhost", Protocol = "http" };
        }

        public override string ToString() => $"{Protocol}://{HostName}:{Port}/";
    }
}
