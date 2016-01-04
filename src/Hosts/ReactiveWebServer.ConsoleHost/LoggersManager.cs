using System;
using Castle.Core.Logging;

namespace AnywayAnyday.ReactiveWebServer.ConsoleHost
{
    public sealed class LoggersManager
    {
        readonly LoggerLevel _level;
        readonly ILoggerFactory _factory;
        readonly Lazy<ILogger> _appLogger, _webServerLogger;

        public LoggersManager (ILoggerFactory fac, LoggerLevel level)
        {
            _level = level;
            _factory = fac;

            _appLogger = new Lazy<ILogger>(() => _factory.Create("Application", _level));
            _webServerLogger = new Lazy<ILogger>(() => _factory.Create("WebServer", _level));
        }
        
        public ILogger AppLogger => _appLogger.Value;

        public ILogger WebServerLogger => _webServerLogger.Value;
    }
}
