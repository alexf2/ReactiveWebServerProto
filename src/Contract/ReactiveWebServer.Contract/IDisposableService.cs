using System;

namespace AnywayAnyday.ReactiveWebServer.Contract
{
    public enum State
    {
        Initial, Started, Stopped, Disposed
    };

    public interface IDisposableService: IDisposable
    {
        void Start();
        void Stop();
        State State { get; }
        string DisplayName { get; }
    }    
}
