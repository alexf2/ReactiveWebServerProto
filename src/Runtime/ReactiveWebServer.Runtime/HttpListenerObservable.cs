using System;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using AnywayAnyday.ReactiveWebServer.Contract;
using Castle.Core.Logging;

namespace AnywayAnyday.ReactiveWebServer.Runtime
{
    public class HttpListenerObservable: ServiceHostBase, IHttpListenerObservable
    {        
        sealed class ListenerConfigurator : IHttpListenerConfigurator, IDisposable
        {
            HttpListener _listener;
            internal ListenerConfigurator(HttpListener listener)
            {
                _listener = listener;
            }
            public void AddPrefix (string pref) => _listener.Prefixes.Add(pref);

            public void Dispose()
            {
                _listener = null;
            }
        }

        readonly ILogger _logger;
        readonly HttpListener _listener;

        //http://habrahabr.ru/post/129861/
        //http://www.introtorx.com/content/v1.0.10621.0/04_CreatingObservableSequences.html
        //http://enumeratethis.com/2010/04/17/warm-observables-with-publish-refcount/
        //http://blogs.microsoft.co.il/bnaya/2010/03/13/rx-for-beginners-part-9-hot-vs-cold-observable/
        IObservable<HttpListenerContext> _listenerObservable;
        

        public HttpListenerObservable(Action<IHttpListenerConfigurator> configuratorCallback, ILogger logger) : base(logger)
        {            
            if (!HttpListener.IsSupported)
                throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later");

            if (configuratorCallback == null)
                throw new ArgumentNullException($"{nameof(configuratorCallback)} should not be null");

            _logger = logger;

            _listener = new HttpListener();
            using (var config = new ListenerConfigurator(_listener))
                configuratorCallback(config);
        }

        public override string DisplayName => "HttpListener";

        protected override void InternalStart()
        {            
            _listener.Start();

            IObserver<HttpListenerContext> httpObserver = null;
            _listenerObservable = Observable.Create<HttpListenerContext>(
                observer =>
                {
                    httpObserver = observer;
                    var httpSubscription = Observable.FromAsyncPattern(_listener.BeginGetContext, _listener.EndGetContext)().Subscribe(observer);

                    return Disposable.Create(() =>
                    {
                        httpSubscription.Dispose();
                        //_logger.Info("Http subscription disposed!");
                    });
                })
                .DoWhile(() => _listener.IsListening)
                //Repeat().
                .Retry()
                .Publish()
                .RefCount();
            //.Finally( () => httpObserver?.OnCompleted() );
        }

        protected override void InternalStop()
        {
            _listener.Stop();            
        }

        protected override void Dispose(bool disposing)
        {
           (_listener as IDisposable).Dispose();
        }

        public IDisposable Subscribe (IObserver<HttpListenerContext> observer)
        {
            CheckDisposed();
            if (State != State.Started)
                throw new InvalidOperationException($"You have to start the listener before subscribing. Current state is {State}");
            return _listenerObservable.Subscribe(observer);
        }        
    }    
}
