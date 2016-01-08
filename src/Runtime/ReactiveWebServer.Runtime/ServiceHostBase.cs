using System;
using AnywayAnyday.ReactiveWebServer.Contract;
using Castle.Core.Logging;

namespace AnywayAnyday.ReactiveWebServer.Runtime
{
    /// <summary>
    /// A base to service hosts. Providing an implementation for basic lifecycle functionality.
    /// </summary>
    public abstract class ServiceHostBase
    {        
        State _state = State.Initial;
        readonly ILogger _logger;

        protected ServiceHostBase(ILogger logger)
        {
            _logger = logger;
        }

        public void Start()
        {
            _logger.Info($"{DisplayName} is being started...");
            CheckDisposed();
            if (_state == State.Initial || _state == State.Stopped)
            {
                InternalStart();
                _state = State.Started;
            }
            else
                throw new InvalidOperationException($"Can't start {GetType().Name}: it is in {_state} mode");
            _logger.Info($"{DisplayName} started");
        }

        public void Stop()
        {
            _logger.Info($"{DisplayName} is being stopped...");
            CheckDisposed();
            if (_state == State.Started)
            {
                InternalStop();
                _state = State.Stopped;
            }
            else
                throw new InvalidOperationException($"Can't stop {GetType().Name}: it is in {_state} mode");
            _logger.Info($"{DisplayName} stopped");
        }

        public State State => _state;

        protected abstract void InternalStart();
        protected abstract void InternalStop();
        protected abstract void Dispose(bool disposing);
        public abstract string DisplayName { get; }

        public void Dispose()
        {
            _logger.Info($"{DisplayName} is being disposed...");
            if (_state == State.Started)
                Stop();
            if (_state == State.Started)
            {
                _state = State.Disposed;
                Dispose(true);
            }
            _logger.Info($"{DisplayName} disposed");
        }

        protected void CheckDisposed()
        {
            if (_state == State.Disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

    }
}
