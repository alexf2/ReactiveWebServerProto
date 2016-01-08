using System;

namespace AnywayAnyday.ReactiveWebServer.Contract
{
    /// <summary>
    /// Service state.
    /// </summary>
    public enum State
    {
        Initial, Started, Stopped, Disposed
    };

    /// <summary>
    /// The contract for the service basics.
    /// </summary>
    public interface IDisposableService: IDisposable
    {
        void Start();
        void Stop();
        State State { get; }
        string DisplayName { get; }
    }    
}
