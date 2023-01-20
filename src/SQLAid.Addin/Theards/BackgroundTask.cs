using SQLAid.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace SQLAid.Theards
{
    public class BackgroundTask : IDisposable
    {
        private bool _keepRunning = true;
        private static object _internalSyncObject;
        private readonly Thread _primaryThread;

        private BackgroundWorker _worker;
        private EventWaitHandle _runTaskRequestPending;
        private ManualResetEvent _runTaskThreadTerminated;
        private readonly List<IThreadTask> _threadTasks = new List<IThreadTask>();

        public BackgroundTask(Thread primaryThread)
        {
            _primaryThread = primaryThread;
            StartThread();
        }

        internal void StartThread()
        {
            if (null != _worker)
            {
                return;
            }

            _runTaskRequestPending = new EventWaitHandle(false, EventResetMode.AutoReset);
            _runTaskThreadTerminated = new ManualResetEvent(false);
            _worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _worker.DoWork += worker_DoWork;
            _worker.ProgressChanged += worker_ProgressChanged;
            _worker.RunWorkerAsync();
        }

        internal void StopThread()
        {
            try
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
                    {
                        try
                        {
                            _worker.CancelAsync();
                        }
                        catch
                        {
                        }
                    }
                }
                CleanupThread();
            }
            catch (Exception e)
            {
                Logger.Error(nameof(BackgroundTask), nameof(BackgroundTask.StopThread), e);
            }
        }

        internal void CleanupThread()
        {
            if (null != _worker)
            {
                _worker.DoWork -= worker_DoWork;
                _worker.ProgressChanged -= worker_ProgressChanged;
                _worker = null;
            }
            _runTaskRequestPending = null;
            _runTaskThreadTerminated = null;
        }

        private static Object InternalSyncObject
        {
            get
            {
                if (null == _internalSyncObject)
                {
                    var o = new Object();
                    Interlocked.CompareExchange(ref _internalSyncObject, o, null);
                }
                return _internalSyncObject;
            }
        }

        public void Dispose()
        {
            StopThread();
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (_primaryThread != Thread.CurrentThread)
            {
                Logger.Error(nameof(BackgroundTask), nameof(BackgroundTask.worker_ProgressChanged), $"NOT EXECUTED IN GUI THREAD. {_primaryThread} != {Thread.CurrentThread}.");
                return;
            }

            var taskToRun = e.UserState as IThreadTask;
            if (null == taskToRun)
            {
                return;
            }

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

        private void worker_DoWork(object sender, DoWorkEventArgs e)
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
                            if (taskToRun.LogQueueStatus)
                            {
                                Logger.Error(nameof(BackgroundTask), nameof(BackgroundTask.RunBackgroundTaskThread), "Running " + taskToRun);
                            }

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
                            catch (Exception e)
                            {
                                Logger.Error(nameof(BackgroundTask), nameof(BackgroundTask.RunBackgroundTaskThread), e, "Running " + taskToRun + "error");
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
            try
            {
                if (taskToRun.LogQueueStatus)
                {
                    Logger.Error(nameof(BackgroundTask), nameof(BackgroundTask.QueueTask), "Queueing task " + taskToRun);
                }
                _threadTasks.Add(taskToRun);
                _runTaskRequestPending.Set();
            }
            catch (Exception e)
            {
                Logger.Error(nameof(BackgroundTask), nameof(BackgroundTask.QueueTask), e);
            }
        }

        private IThreadTask DeQueue()
        {
            try
            {
                lock (InternalSyncObject)
                {
                    IThreadTask taskToRun = null;
                    if (_threadTasks.Count > 0)
                    {
                        taskToRun = _threadTasks[0];
                        if (taskToRun.LogQueueStatus)
                        {
                            Logger.Error(nameof(BackgroundTask), nameof(BackgroundTask.DeQueue), "Dequeuing task " + taskToRun);
                        }
                        _threadTasks.RemoveAt(0);
                    }
                    return taskToRun;
                }
            }
            catch (Exception e)
            {
                Logger.Error(nameof(BackgroundTask), nameof(BackgroundTask.DeQueue), e);
                return null;
            }
        }
    }
}