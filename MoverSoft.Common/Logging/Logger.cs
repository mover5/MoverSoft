using System;
using System.Collections.Generic;
using System.IO;

namespace MoverSoft.Common.Logging
{
    public delegate void LogMessage(string message);

    public class Logger : IDisposable
    {
        public static Logger Instance
        {
            get
            {
                return Logger.InitInstance.Value;
            }
        }

        private static Lazy<Logger> InitInstance = new Lazy<Logger>(() => new Logger());

        public LogMessage LogMessageHandler { get; set; }

        public LoggingTimestampHandling TimestampHandling { get; set; }

        private List<IDisposable> Disposables { get; set; }

        public Logger()
        {
            this.Disposables = new List<IDisposable>();
        }

        public Logger(LoggingTimestampHandling timestampHandling) : base()
        {
            this.TimestampHandling = timestampHandling;
        }

        public void Dispose()
        {
            if (this.Disposables != null)
            {
                foreach (var disposable in this.Disposables)
                {
                    if (disposable != null)
                    {
                        disposable.Dispose();

                        if (disposable.GetType() == typeof(StreamWriter))
                        {
                            ((StreamWriter)disposable).Close();
                        }
                    }
                }
            }
        }

        public void IncludeConsoleLogger()
        {
            this.LogMessageHandler += Console.WriteLine;
        }

        public void IncludeLogFile(string logFilePath, FileMode logFileMode = FileMode.Create)
        {
            StreamWriter streamWriter = null;

            switch (logFileMode)
            {
                case FileMode.Create:
                    streamWriter = File.CreateText(logFilePath);
                    break;
                case FileMode.Append:
                    streamWriter = File.AppendText(logFilePath);
                    break;
            }

            if (streamWriter == null)
            {
                throw new ArgumentException("logFileMode must either be 'Create' or 'Append'", "logFileMode");
            }

            this.LogMessageHandler += streamWriter.WriteLine;
            this.Disposables.Add(streamWriter);
        }

        public void IncludeLogStreamWriter(StreamWriter streamWriter)
        {
            this.LogMessageHandler += streamWriter.WriteLine;
            this.Disposables.Add(streamWriter);
        }

        #region Logging Methods

        public void LogMessage(string message)
        {
            var formattedMessage = message;

            if (this.TimestampHandling != LoggingTimestampHandling.None)
            {
                string timestamp = string.Empty;

                switch (this.TimestampHandling)
                {
                    case LoggingTimestampHandling.DateAndTime:
                        timestamp = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();
                        break;
                    case LoggingTimestampHandling.TimeOnly:
                        timestamp = DateTime.Now.ToLongTimeString();
                        break;
                    case LoggingTimestampHandling.DateOnly:
                        timestamp = DateTime.Now.ToShortDateString();
                        break;
                }

                formattedMessage = string.Format("{0}: {1}", timestamp, formattedMessage);
            }

            this.LogMessageHandler(formattedMessage);
        }

        public void LogMessage(string format, object arg0)
        {
            this.LogMessage(string.Format(format, arg0));
        }

        public void LogMessage(string format, object arg0, object arg1)
        {
            this.LogMessage(string.Format(format, arg0, arg1));
        }

        public void LogMessage(string format, object arg0, object arg1, object arg2)
        {
            this.LogMessage(string.Format(format, arg0, arg1, arg2));
        }

        public void LogMessage(string format, object arg0, object arg1, object arg2, object arg3)
        {
            this.LogMessage(string.Format(format, arg0, arg1, arg2, arg3));
        }

        public void LogMessage(string format, object[] args)
        {
            this.LogMessage(string.Format(format, args));
        }

        #endregion
    }
}
