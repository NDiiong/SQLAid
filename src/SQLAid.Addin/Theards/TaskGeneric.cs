using SQLAid.Helpers;
using System;

namespace SQLAid.Theards
{
    public class TaskGeneric : IThreadTask
    {
        public delegate void TaskToRun();

        private readonly TaskToRun _taskToRun;
        private readonly TaskToRun _taskToRunStarting;
        private readonly TaskToRun _taskToRunEnding;
        private readonly bool _shouldExecuteInMainThread;

        public TaskGeneric(TaskToRun taskToRun, bool shouldExecuteInMainThread)
        {
            _taskToRun = taskToRun;
            _shouldExecuteInMainThread = shouldExecuteInMainThread;
        }

        public TaskGeneric(TaskToRun taskToRun, TaskToRun taskToRunStarting, TaskToRun taskToRunEnding, bool shouldExecuteInMainThread)
        {
            _taskToRun = taskToRun;
            _taskToRunEnding = taskToRunEnding;
            _taskToRunStarting = taskToRunStarting;
            _shouldExecuteInMainThread = shouldExecuteInMainThread;
        }

        public bool ShouldExecuteInMainThread
        {
            get { return _shouldExecuteInMainThread; }
        }

        public bool LogQueueStatus
        {
            get { return true; }
        }

        public void Execute()
        {
            try
            {
                _taskToRun();
            }
            catch (Exception e)
            {
                Logger.Error(nameof(TaskGeneric), nameof(TaskGeneric.Execute), e);
            }
        }

        public void Starting()
        {
            if (null != _taskToRunStarting)
            {
                _taskToRunStarting();
            }
        }

        public void Ending()
        {
            if (_taskToRunEnding != null)
            {
                _taskToRunEnding();
            }
        }
    }
}