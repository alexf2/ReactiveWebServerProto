namespace AnywayAnyday.ReactiveWebServer.Contract
{
    /// <summary>
    /// Configurator contract.
    /// </summary>
    public interface IHttpListenerConfigurator
    {
        void AddPrefix(string pref);
    }
}
