using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace dotNetMT
{
    public static class Logger
    {
        private static string _logPath = "./debug.log"; // log file path
        private static int _maxLogAge = 5; // Max Age in seconds
        private static int _queueSize = 1; // Max Queue Size

        private static bool _logOnlyTime = true;
        private static Queue<LogEntry> _logQueue = new Queue<LogEntry>();
        private static DateTime _lastFlushed = DateTime.Now;

        //private static readonly Destructor Finalise = new Destructor();
        //private sealed class Destructor { ~Destructor() { FlushLog(); }}

        public class LogEntry
        {
            public string time { get; set; }
            public string date { get; set; }
            public string message { get; set; }

            public LogEntry(string message)
            {
                date = DateTime.Now.ToString("yyyy-MM-dd");
                time = DateTime.Now.ToString("HH:mm:ss.fff");
                this.message = message;
            }
        }

        static Logger() 
        {
            AppDomain.CurrentDomain.ProcessExit += ClassDestructor; // DomainUnload
        }
        
        static void ClassDestructor(object sender, EventArgs e) 
        {
            if(_logQueue.Count > 0) FlushLog(); 
        }

        public static void SetLogPath(string path) 
        { 
            _logPath = path; 
        }

        public static string GetLogPath() 
        { 
            return (_logPath); 
        }

        public static void SetLogMaxAge(int age)
        {
            _maxLogAge = age;
        }

        public static int GetLogMaxAge()
        {
            return (_maxLogAge);
        }

        public static void SetLogMaxQueueSize(int size)
        {
            _queueSize = size;
        }

        public static int GetLogMaxQueueSize()
        {
            return (_queueSize);
        }

        public static void Write(string message, bool flush = false)
        {
            lock (_logQueue) // Lock the queue while writing to prevent contention for the log file
            {
                LogEntry logEntry = new LogEntry(message.Trim('\r', '\n')); // Create the entry and push to the Queue
                _logQueue.Enqueue(logEntry);
                if (flush || _logQueue.Count >= _queueSize || DoPeriodicFlush()) FlushLog(); // If we have reached the Queue Size then flush the Queue
            }
        }

        public static void Write(string message) 
        { 
            Write(message, false); 
        }

        private static bool DoPeriodicFlush()
        {
            TimeSpan logAge = DateTime.Now - _lastFlushed;
            if (logAge.TotalSeconds >= _maxLogAge)
            {
                _lastFlushed = DateTime.Now;
                return(true);
            }
            return(false);
        }

        public static void FlushLog()
        {
            using (FileStream fs = File.Open(_logPath, FileMode.Append, FileAccess.Write))
                using (StreamWriter log = new StreamWriter(fs))
                    while (_logQueue.Count > 0)
                    {
                        LogEntry entry = _logQueue.Dequeue();
                        string timestamp;
                        if(_logOnlyTime) timestamp = entry.time.Trim();
                        else timestamp = entry.date.Trim() + " " + entry.time.Trim();
                        log.WriteLine(string.Format("[{0}]: {1}", timestamp, entry.message));
                    }
        }
    }
}