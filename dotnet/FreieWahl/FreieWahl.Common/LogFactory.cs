using System;
using Microsoft.Extensions.Logging;

namespace FreieWahl.Common
{
    public static class LogFactory
    {
        private static ILoggerFactory _factory;
        private static bool _initialized = false;

        public static void Setup(ILoggerFactory factory)
        {
            if (_initialized)
                throw new InvalidOperationException("Logger factory has already been initialized!");

            _initialized = true;
            _factory = factory;
        }

        public static ILogger CreateLogger(string categoryName)
        {
            if(!_initialized)
                throw new InvalidOperationException("Logger factory needs to be initialized!");

            return _factory.CreateLogger(categoryName);
        }
    }
}
