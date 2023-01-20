using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace SQLAid.Addin.Thread
{
    public class BackgroundTask : IDisposable
    {
        private bool _keepRunning = true;
        private static object _internalSyncObject;
        private readonly System.Threading.Thread _primaryThread;

        private BackgroundWorker _worker;
        private EventWaitHandle _runTaskRequestPending;
        private ManualResetEvent _runTaskThreadTerminated;
        private readonly List<IThreadTask> _threadTasks = new List<IThreadTask>();

        public BackgroundTask(System.Threading.Thread primaryThread)
        {
            _primaryThread = primaryThread;
            StartThread();
        }

        internal void StartThread()
        {
            if (null != _worker)
                return;

            _runTaskRequestPending = new EventWaitHandle(false, EventResetMode.AutoReset);
            _runTaskThreadTerminated = new ManualResetEvent(false);
            _worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _worker.DoWork += Worker_DoWork;
            _worker.ProgressChanged += Worker_ProgressChanged;
            _worker.RunWorkerAsync();
        }

        internal void StopThread()
        {
            if (_worker != null)
            {
                var ptt = _runTaskThreadTerminated;
                _keepRunning = false;
                lock (InternalSyncObject)
                {
                    _threadTasks.Clear();
                }

                _runTaskRequestPending.Set();
                if (!ptt.WaitOne(50, false))
                    _worker.CancelAsync();
            }

            CleanupThread();
        }

        internal void CleanupThread()
        {
            if (_worker != null)
            {
                _worker.DoWork -= Worker_DoWork;
                _worker.ProgressChanged -= Worker_ProgressChanged;
                _worker = null;
            }

            _runTaskRequestPending = null;
            _runTaskThreadTerminated = null;
        }

        private static object InternalSyncObject
        {
            get
            {
                if (_internalSyncObject == null)
                {
                    var o = new object();
                    Interlocked.CompareExchange(ref _internalSyncObject, o, null);
                }

                return _internalSyncObject;
            }
        }

        public void Dispose()
        {
            StopThread();
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (_primaryThread != System.Threading.Thread.CurrentThread)
                return;

            var taskToRun = e.UserState as IThreadTask;
            if (null == taskToRun)
                return;

            switch (e.ProgressPercentage)
            {
                case 0:
                    taskToRun.Starting();
                    break;

                case 50:
                    taskToRun.Execute();
                    break;

                case 100:
                    taskToRun.Ending();
                    break;
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            RunBackgroundTaskThread();
        }

        private void RunBackgroundTaskThread()
        {
            try
            {
                while (_keepRunning && !_worker.CancellationPending)
                {
                    try
                    {
                        _runTaskRequestPending.WaitOne();

                        if (_worker.CancellationPending)
                        {
                            break;
                        }

                        var taskToRun = DeQueue();
                        while (null != taskToRun)
                        {
                            try
                            {
                                _worker.ReportProgress(0, taskToRun);
                                if (taskToRun.ShouldExecuteInMainThread)
                                {
                                    _worker.ReportProgress(50, taskToRun);
                                }
                                else
                                {
                                    taskToRun.Execute();
                                }
                                _worker.ReportProgress(100, taskToRun);
                            }
                            catch (Exception)
                            {
                            }

                            taskToRun = DeQueue();
                        }
                    }
                    catch (Exception)
                    {
                        _worker.CancelAsync();
                        _keepRunning = false;
                    }
                }
                var ptt = _runTaskThreadTerminated;
                CleanupThread();
                ptt.Set();
            }
            catch (Exception)
            {
            }
        }

        public void QueueTask(IThreadTask taskToRun)
        {
            _threadTasks.Add(taskToRun);
            _runTaskRequestPending.Set();
        }

        private IThreadTask DeQueue()
        {
            lock (InternalSyncObject)
            {
                IThreadTask taskToRun = null;
                if (_threadTasks.Count > 0)
                {
                    taskToRun = _threadTasks[0];
                    _threadTasks.RemoveAt(0);
                }

                return taskToRun;
            }
        }
    }
}