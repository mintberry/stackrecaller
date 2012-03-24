using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Prototype.Experiment;

namespace Prototype
{
    class Logger
    {
        public enum EntryType
        {
            Init,
            Search,
            Click,
            Scroll,
            KeyPress,
            NextQuestion
        }

        public struct LogEntry
        {
            public EntryType Type;
            public DateTime Time;
            public object Data;
            public FocusArea Focus;
        }

        private List<LogEntry> _logEntries = new List<LogEntry>();
        private DateTime _startTime = DateTime.Now;
        StreamWriter _sw;

        void Instance_OnTasksetChanged(TaskSet setName)
        {
            if (_sw != null)
                _sw.Close();

            _sw = File.CreateText(_startTime.ToString("yyyyMMddHHmm") + "." + ExperimentInfo.Instance.CurrentTaskSet.tasksfile + ".log");
            string header =
                ExperimentInfo.Instance.ParticipantID.ToString() + ";" +
                ExperimentInfo.Instance.CurrentTaskSet + ";" +
                setName.strategy.ToString();

            _startTime = DateTime.Now;

            _sw.WriteLine(header);
        }

        public void Log(EntryType type, object data)
        {
            if (_sw == null)
                return;

            LogEntry le = new LogEntry();
            le.Data = data;
            le.Time = DateTime.Now;
            le.Type = type;
            le.Focus = Model.Default.Focus;

            _logEntries.Add(le);
            WriteToLogFile(le);
        }

        private void WriteToLogFile(LogEntry le)
        {
            string line = (int)(le.Time - _startTime).TotalMilliseconds +
                ";" + le.Type.ToString() +
                ";" + le.Focus.Start.ToString() +
                ";" + le.Focus.End.ToString();

            if (le.Type == EntryType.Search)
                line += ";" + (string)le.Data;

            _sw.WriteLine(line);
            _sw.Flush();
        }

        #region Singleton
        static private Logger logger = null;
        
        protected Logger()
        {
            ExperimentInfo.Instance.OnTasksetChanged += new ExperimentInfo.TaskSetChangedDelegate(Instance_OnTasksetChanged);
        }

        public static Logger Default
        {
            get
            {
                if (logger == null)
                    logger = new Logger();

                return logger;
            }
        }

        #endregion
       
        
    }
}
