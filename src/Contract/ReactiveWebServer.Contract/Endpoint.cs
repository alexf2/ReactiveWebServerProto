namespace AnywayAnyday.ReactiveWebServer.Contract
{
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
