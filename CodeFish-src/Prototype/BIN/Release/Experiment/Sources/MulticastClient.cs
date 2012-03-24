using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Collections;
using Logger;
using System.IO;


namespace Multicasting
{

    public delegate bool DeleteFileDelegate(string uri);
    public delegate bool UpdateFileDelegate(byte[] data, string uri);
    public delegate byte[] ReadFileDelegate(string uri);
    public delegate FileInfo ReadFileInfoDelegate(string uri);

    public class MulticastClient : MarshalByRefObject
    {

        #region Internal structs and enums
        private struct MethodData
        {
            public string Uri;
            public byte[] Data;
            public OperationType Operation;

            public MethodData(OperationType o, string u, byte[] d)
            {
                Operation = o;
                Uri = u;
                Data = d;
            }
        }

        private enum OperationType
        {
            Delete,
            Update
        }
        #endregion

        // Msg handling
        private List<int> _receivedMessages = new List<int>();
        private int _highetstSequenceNumber;
        private int _lastHandledSequenceNumber;
        private Dictionary<int, MethodData> _queuedOperations = new Dictionary<int, MethodData>();

        // Group members and sequencer
        private Dictionary<int, MulticastClientInfo> _groupMembers;
        private Sequencer _sequencer;
        private TcpChannel _channel;



        // For sync
        private object _electionLock = new object();
        private bool _electionRunning = false;

        // Fields
        private int _processId;
        private int _port;


        // Events
        public event DeleteFileDelegate NotifyDeleteFile;
        public event UpdateFileDelegate NotifyUpdateFile;
        public event ReadFileDelegate GetFileCallback;
        public event ReadFileInfoDelegate GetFileInfoCallback;


        public int HighetstSequenceNumber
        {
            get
            {
                return _highetstSequenceNumber;
            }
        }

        public int LastHandledSequenceNumber
        {
            get
            {
                return _lastHandledSequenceNumber;
            }
        }

        public int Port
        {
            get
            {
                return _port;
            }
            set
            {
                _port = port;
            }
        }

        #region Constructing and the like
        public MulticastClient(int pid, int portForPublishing)
        {
            _processId = pid;
            _port = portForPublishing;
            _highetstSequenceNumber = 0;
            OpenListeningChannel();
        }

        // Setting up channel for incomming connections
        private void OpenListeningChannel()
        {
            string name = "MulticastClient" + _port.ToString();
            Hashtable properties = new Hashtable();
            properties["name"] = name;
            properties["port"] = _port.ToString();
            _channel = new TcpChannel(properties, null, null);
            ChannelServices.RegisterChannel(_channel, false);
            RemotingServices.Marshal(this, name);
        }

        public void ConnectToGroupMembers()
        {
            //Could be fetched from the StaticCertificateAuthority
            _groupMembers = MulticastCommonTools.ReadGroupMembers();
        }

        // Making the object live forever
        public override object InitializeLifetimeService()
        {
            return null;
        }
        #endregion

        #region Processing operations
        private void ProcessOperation(int pid, OperationType type, int sequenceNumber, byte[] data, string uri)
        {

            // Message already processed
            if (_receivedMessages.Contains(sequenceNumber))
                return;

            //Log("Processing a " + type.ToString() + " on " + uri + " from " + pid.ToString(), LogEntryType.Information);

            _receivedMessages.Add(sequenceNumber);
            _highetstSequenceNumber = Math.Max(_highetstSequenceNumber, sequenceNumber);

            // If origin is this don't multicast again
            if (pid != _processId)
                // Implementing reliable messages - all operations are send |g| times to each group member
                MulticastOperationToGroupMembers(pid, sequenceNumber, type, data, uri);

            _queuedOperations.Add(sequenceNumber, new MethodData(type, uri, data));
            ProcessQueue();
        }

        private void ProcessQueue()
        {
            int sequenceEnd;
            // Finding the last sequence number in a contnious sequence of queued operations
            for (sequenceEnd = _lastHandledSequenceNumber + 1;
                sequenceEnd <= _highetstSequenceNumber
                && _queuedOperations.ContainsKey(sequenceEnd);
                sequenceEnd++) ;

            // Performing queued operations
            for (int j = _lastHandledSequenceNumber + 1; j < sequenceEnd; j++)
            {
                MethodData md = _queuedOperations[j];
                FireEvent(md.Operation, md.Data, md.Uri);
                _queuedOperations.Remove(j);
            }
            _lastHandledSequenceNumber = sequenceEnd - 1;
        }

        private void FireEvent(OperationType type, byte[] data, string uri)
        {
            //Log("Fireing event: " + type.ToString(), LogEntryType.Information);

            if (type == OperationType.Update)
            {
                if (NotifyUpdateFile != null)
                    NotifyUpdateFile(data, uri);
            }
            else if (type == OperationType.Delete)
            {
                if (NotifyDeleteFile != null)
                    NotifyDeleteFile(uri);
            }
        }
        #endregion



        #region Sending operations to all group members
        private void MulticastOperationToGroupMembers(int pid, OperationType type, byte[] data, string uri)
        {
            int sequenceNumber = -1;

            // Catch dead sequencer
            while (sequenceNumber == -1)
            {
                //Log("Contacting sequencer", LogEntryType.Information);
                try
                {
                    sequenceNumber = _sequencer.GetSequenceNumber();
                    //Log("Got seqno: " + sequenceNumber.ToString(), LogEntryType.Information);
                }
                catch (Exception e)
                {
                    Log("(No sequencer) " + e.Message, LogEntryType.Error);
                    sequenceNumber = -1;

                    if (!_electionRunning)
                    {
                        AnnounceElection();
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
            }
            MulticastOperationToGroupMembers(pid, sequenceNumber, type, data, uri);
        }

        private void MulticastOperationToGroupMembers(int pid, int sequenceNumber, OperationType type, byte[] data, string uri)
        {
            List<int> deadGroupMembers = new List<int>();
            foreach (KeyValuePair<int, MulticastClientInfo> kv in _groupMembers)
            {
                MulticastClientInfo gm = kv.Value;

                try // Catch dead group members
                {
                    if (type == OperationType.Delete)
                        gm.Client.DeleteFile(pid, sequenceNumber, uri);
                    else if (type == OperationType.Update)
                        gm.Client.UpdateFile(pid, sequenceNumber, data, uri);
                }
                catch (Exception e)
                {
                    Log("(Dead group member) " + e.Message, LogEntryType.Error);
                    //Caught dead member
                    deadGroupMembers.Add(kv.Key);
                }
            }

            RemoveDeadGroupMembers(deadGroupMembers);
        }


        #region Handling elections
        private void AnnounceElection()
        {
            if (_electionRunning)
            {
                Log("Election already running", LogEntryType.Error);
                return;
            }

            _electionRunning = true;

            Log("(Snd) Announce election", LogEntryType.Information);
            _sequencer = null;
            if (!PossibleSetSelfCoordinator())
            {
                Monitor.Enter(_electionLock);
                // Start elections until a new coordinator is elected
                do
                {
                    List<int> deadGroupMembers = new List<int>();
                    bool answer = false;
                    foreach (KeyValuePair<int, MulticastClientInfo> kv in _groupMembers)
                    {
                        if (kv.Key > _processId)
                        {
                            bool tmpAnswer = false;
                            try
                            {
                                tmpAnswer = kv.Value.Client.Elect();
                            }
                            catch (Exception e)
                            {
                                Log("(Dead group member) " + e.ToString(), LogEntryType.Error);
                                // Caught a dead member - enqueing for removal
                                deadGroupMembers.Add(kv.Key);
                            }
                            finally
                            {
                                answer |= tmpAnswer;
                            }
                        }
                    }

                    RemoveDeadGroupMembers(deadGroupMembers);

                    if (!answer)
                    {
                        Monitor.Exit(_electionLock);
                        StateCoordinator();
                        return;
                    }



                } while (!Monitor.Wait(_electionLock, 5000));

                _electionRunning = false;
                Monitor.Exit(_electionLock);
            }
        }

        public bool Elect()
        {
            Log("(Rcv) Elect", LogEntryType.Information);
            Thread t = new Thread(new ThreadStart(this.AnnounceElection));
            t.Start();
            return true;
        }

        private bool PossibleSetSelfCoordinator()
        {
            if (_processId == GetHighestProcessId())
            {
                Thread t = new Thread(new ThreadStart(this.StateCoordinator));
                t.Start();
                return true;
            }
            else
            {
                return false;
            }

        }

        private void StateCoordinator()
        {

            Log("(Snd) Coordinator", LogEntryType.Information);
            _sequencer = new Sequencer(_highetstSequenceNumber);

            int sequencerPort = _sequencer.Port;
            SetCoordinator(_processId, sequencerPort);

            List<int> deadGroupMembers = new List<int>();
            foreach (KeyValuePair<int, MulticastClientInfo> kv in _groupMembers)
            {
                if (kv.Key < _processId)
                {
                    try
                    {
                        kv.Value.Client.SetCoordinator(_processId, sequencerPort);
                    }
                    catch (Exception e)
                    {
                        Log("(Dead group member) " + e.Message, LogEntryType.Error);
                        deadGroupMembers.Add(kv.Key);
                    }
                }
            }
            RemoveDeadGroupMembers(deadGroupMembers);
            _electionRunning = false;
        }

        // Used for receiving _coordinator_ messages from newly elected coordinators
        public void SetCoordinator(int processId, int port)
        {
            Monitor.Enter(_electionLock);
            Log("Setting process " + processId.ToString() + " as sequencer on port " + port.ToString(), LogEntryType.Information);


            Sequencer s = (Sequencer)Activator.GetObject(
                typeof(Sequencer),
                "tcp://" + _groupMembers[processId].Address
                + ":" + port.ToString() + "/Sequencer" + port.ToString());

            if (s == null)
                Log("Could not locate sequencer", LogEntryType.Error);


            _sequencer = s;
            Monitor.PulseAll(_electionLock);
            Monitor.Exit(_electionLock);




        }
        #endregion
        #endregion


        #region Remote activated methods
        // Used when a group member should delete  / update files from their
        // file system, posted by another group member
        public void UpdateFile(int pid, int sequenceNumber, byte[] data, string uri)
        {
            ProcessOperation(pid, OperationType.Update, sequenceNumber, data, uri);
        }
        public void DeleteFile(int pid, int sequenceNumber, string uri)
        {
            ProcessOperation(pid, OperationType.Delete, sequenceNumber, null, uri);
        }
        #endregion

        #region Local activated methods
        // Used when a MulticastClient - ie. a fileserver - wants to post file updates
        // to the group
        public void PostUpdateFile(byte[] data, string uri)
        {
            MulticastOperationToGroupMembers(_processId, OperationType.Update, data, uri);
        }

        // Used when a group member - ie. a file server - wants to post file deletions
        // to the group
        public void PostDeleteFile(string uri)
        {
            MulticastOperationToGroupMembers(_processId, OperationType.Delete, null, uri);
        }
        #endregion

        #region Byzantine fault tolerant read operations

        private delegate T ReadDelegate<T>(MulticastClient gm, string uri);
        private delegate bool EqualDelegate<T>(T t1, T t2);

        public byte[] PostReadFile(string uri)
        {
            //Getting files from all group members
            List<byte[]> files = GetData<byte[]>(new ReadDelegate<byte[]>(ReadFileHelper), uri);
            //Building votes vector
            Dictionary<byte[], int> votes = GetVotesVector<byte[]>(new EqualDelegate<byte[]>(ByteArraysEqueal), files);
            //Returning the object with the most votes - or an error
            return GetMajorityData<byte[]>(votes);
        }
       

        public FileInfo PostReadFileInfo(string uri)
        {
            //Getting files from all group members
            List<FileInfo> files = GetData<FileInfo>(new ReadDelegate<FileInfo>(ReadFileInfoHelper), uri);
            //Building votes vector
            Dictionary<FileInfo, int> votes = GetVotesVector<FileInfo>(new EqualDelegate<FileInfo>(FileInfosEqual), files);
            //Returning the object with the most votes - or an error
            return GetMajorityData<FileInfo>(votes);
        }


        public byte[] ReadFile(string uri)
        {
            if (GetFileCallback != null)
                return GetFileCallback(uri);
            else
                return null;
        }

        public FileInfo ReadFileInfo(string uri)
        {

            if (GetFileInfoCallback != null)
                return GetFileInfoCallback(uri);
            else
                return null;

        }

        // Building a votes vector from a list of files using a EqualDelegate to check equality
        private Dictionary<T, int> GetVotesVector<T>(EqualDelegate<T> Equal, List<T> files)
        {
            Dictionary<T, int> votes = new Dictionary<T, int>();
            foreach (T f in files)
            {
                bool isHere = false;

                foreach (KeyValuePair<T, int> kv in votes)
                {
                    if (Equal(kv.Key, f))
                    {
                        votes[kv.Key]++; //Cast vote for this
                        isHere = true;
                        break;
                    }
                }

                if (!isHere)
                    votes.Add(f, 1);
            }
            return votes;
        }

        // Fetching data of type T from all group members
        private List<T> GetData<T>(ReadDelegate<T> Read, string uri)
        {
            List<T> files = new List<T>();
            List<int> deadGroupMembers = new List<int>();
            foreach (KeyValuePair<int, MulticastClientInfo> kv in _groupMembers)
            {
                MulticastClientInfo gm = kv.Value;

                try // Catch dead group members
                {
                    // If it is this process, don't use the remoting framework
                    T currentFile = Read((kv.Key == _processId ? null : gm.Client), uri);
                    if (currentFile != null)
                        files.Add(currentFile);
                }
                catch (Exception e)
                {
                    Log("(Dead group member) " + e.Message, LogEntryType.Error);
                    //Caught dead member
                    deadGroupMembers.Add(kv.Key);
                }
            }
            RemoveDeadGroupMembers(deadGroupMembers);
            return files;

            
        }

        
        private byte[] ReadFileHelper(MulticastClient gm, string uri)
        {
            if (gm == null) //Not a remote object, just call locally
                return ReadFile(uri);
            else
                return gm.ReadFile(uri);
        }

        private FileInfo ReadFileInfoHelper(MulticastClient gm, string uri)
        {
            if (gm == null) //Not a remote object, just call locally
                return ReadFileInfo(uri);
            else
                return gm.ReadFileInfo(uri);
        }

        private bool ByteArraysEqueal(byte[] b1, byte[] b2)
        {
            if (b1.Length != b2.Length)
                return false;
            else
            {
                for (int i = 0; i < b1.Length; i++)
                {
                    if (b1[i] != b2[i])
                        return false;
                }
                return true;
            }
        }

        private bool FileInfosEqual(FileInfo f1, FileInfo f2)
        {
            // Using a fair estimate of equality
            return f1.Name == f2.Name && f1.Length == f2.Length && f1.Exists && f2.Exists;
        }

        // Return the object with the majority of votes - is one such excists
        private T GetMajorityData<T>(Dictionary<T, int> votes)
        {
            bool majority = false;
            T currentMajorityHolderData = default(T);
            int currentMajorityHolderVotes = 0;

            foreach (KeyValuePair<T, int> kv in votes)
            {
                if (currentMajorityHolderData == null)
                {
                    currentMajorityHolderData = kv.Key;
                    currentMajorityHolderVotes = kv.Value;
                    majority = true;
                }

                if (currentMajorityHolderData.Equals(kv.Key))
                    continue;

                if (kv.Value > currentMajorityHolderVotes)
                {
                    currentMajorityHolderData = kv.Key;
                    currentMajorityHolderVotes = kv.Value;
                    majority = true;
                }
                else if (kv.Value == currentMajorityHolderVotes)
                {
                    majority = false;
                }
            }

            //If there is a unique local majority and it is a global majority return the data
            if (majority && currentMajorityHolderVotes > (_groupMembers.Count / 2.0))
                return currentMajorityHolderData;
            else
                throw new Exception("No majority for file");
                //return default(T);
        }



        #endregion

        #region Helper methods
        private int GetHighestProcessId()
        {
            int highest = -1;
            foreach (int i in _groupMembers.Keys)
                highest = Math.Max(highest, i);
            return highest;
        }
        private void RemoveDeadGroupMembers(List<int> deadGroupMembers)
        {
            // Removing caught dead members
            foreach (int i in deadGroupMembers)
                _groupMembers.Remove(i);
        }

        private void Log(string text, LogEntryType type)
        {
            LoggingProvider.Instance.Log("[Process" + _processId.ToString() + "] " + text, type);
        }
        #endregion

        #region Testing
        public void Crash()
        {
            RemotingServices.Disconnect(this);
            ChannelServices.UnregisterChannel(_channel);
            _sequencer = null;
            _groupMembers = null;
        }

        public void CrashSequencer()
        {
            try
            {
                _sequencer.Crash();
            }
            catch (Exception)
            {


            }

        }
        #endregion
    }
}