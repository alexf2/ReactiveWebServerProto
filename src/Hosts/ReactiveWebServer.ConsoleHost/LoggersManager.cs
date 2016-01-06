using System;
using Castle.Core.Logging;

namespace AnywayAnyday.ReactiveWebServer.ConsoleHost
{
    public sealed class LoggersManager
    {
        public const string AppLoggerInst = "AppLogger";
        public const string WebServerLoggerInst = "WebServerLogger";
        public const string DalLoggerInst = "DalLogger";

        readonly LoggerLevel _level;
        readonly ILoggerFactory _factory;
        readonly Lazy<ILogger> _appLogger, _webServerLogger, _dataLogger, _handlersLogger;

        public LoggersManager (ILoggerFactory fac, LoggerLevel level)
        {
            _level = level;
            _factory = fac;

            _appLogger = new Lazy<ILogger>(() => _factory.Create("Application", _level));
            _webServerLogger = new Lazy<ILogger>(() => _factory.Create("WebServer", _level));
            _dataLogger = new Lazy<ILogger>(() => _factory.Create("DAL", _level));
            _handlersLogger = new Lazy<ILogger>(() => _factory.Create("Handler", _level));
        }
        
        public ILogger AppLogger => _appLogger.Value;

        public ILogger WebServerLogger => _webServerLogger.Value;

        public ILogger DalLogger => _dataLogger.Value;

        public ILogger HandlerLogger => _handlersLogger.Value;
    }    
}
