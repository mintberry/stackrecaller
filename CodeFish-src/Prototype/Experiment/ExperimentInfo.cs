using System;
using System.Collections.Generic;
using System.Text;

namespace Prototype
{
    public struct TaskSet
    {
        public string tasksfile;
        public string sourcefile;
        public string strategy;
    }

    class ExperimentInfo
    {
        public delegate void TaskSetChangedDelegate(TaskSet setName);
        public event TaskSetChangedDelegate OnTasksetChanged;

        private int _participantID;

        public int ParticipantID
        {
            get { return _participantID; }
            set
            {
                _participantID = value;
                _taskSet1 = GetTasksetFromPID(true);
                _taskSet2 = GetTasksetFromPID(false);
            }
        }

        private TaskSet _taskSet1;

        public TaskSet TaskSet1
        {
            get { return _taskSet1; }
        }

        private TaskSet _taskSet2;

        public TaskSet TaskSet2
        {
            get { return _taskSet2; }
        }

        private TaskSet _currentTaskSet;

        public TaskSet CurrentTaskSet
        {
            get { return _currentTaskSet; }
            set
            {
                _currentTaskSet = value;
                if (OnTasksetChanged != null)
                    OnTasksetChanged(value);
            }
        }

        private TaskSet GetTasksetFromPID(bool first)
        {
            TaskSet ts = new TaskSet();

            string pairs = 
                "Graph1|semantic,Cell2|dynamic " +
                "Sheet1|semantic,MulticastClient2|dynamic " +
                "Cell1|dynamic,Graph2|semantic " +
                "MulticastClient1|dynamic,Sheet2|semantic " +
                "Cell2|semantic,Graph1|dynamic " +
                "MulticastClient2|semantic,Sheet1|dynamic " +
                "Graph2|dynamic,Cell1|semantic " +
                "Sheet2|dynamic,MulticastClient1|semantic";


            string[] pairList = pairs.Split(new char[] { ' ' });

            ts.tasksfile = pairList[ParticipantID % pairList.Length].Split(new char[] { ',' })[first ? 0 : 1].Split(new char[] { '|' })[0] + ".txt";
            ts.strategy = pairList[ParticipantID % pairList.Length].Split(new char[] { ',' })[first ? 0 : 1].Split(new char[] { '|' })[1];

            return ts;
        }

        #region ExperimentInfo Singleton
        public static ExperimentInfo Instance
        {
            get
            {
                return NestedExperimentInfo.instance;
            }
        }

        class NestedExperimentInfo
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static NestedExperimentInfo()
            {
            }

            internal static readonly ExperimentInfo instance = new ExperimentInfo();
        }
        #endregion

    }
}
