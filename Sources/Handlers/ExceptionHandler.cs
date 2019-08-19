﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Measurements.Core.Handlers
{
    public static class ExceptionHandler
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public static event EventHandler<ExceptionEventsArgs> ExceptionEvent;

        public static void ExceptionNotify(object obj, ExceptionEventsArgs exceptionEventsArgs)
        {
            logger.Log(exceptionEventsArgs.Level, exceptionEventsArgs.Message);
            ExceptionEvent?.Invoke(obj, exceptionEventsArgs);
        }
    }

    public class ExceptionEventsArgs : EventArgs
    {
        public NLog.LogLevel Level;
        public string Message;
    }
}
