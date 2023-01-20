namespace SQLAid.Addin.Thread
{
    public class TaskGeneric : IThreadTask
    {
        public delegate void TaskToRun();

        private readonly TaskToRun _taskToRun;
        private readonly TaskToRun _taskToRunStarting;
        private readonly TaskToRun _taskToRunEnding;

        public TaskGeneric(TaskToRun taskToRun, bool shouldExecuteInMainThread)
        {
            _taskToRun = taskToRun;
            ShouldExecuteInMainThread = shouldExecuteInMainThread;
        }

        public TaskGeneric(TaskToRun taskToRun, TaskToRun taskToRunStarting, TaskToRun taskToRunEnding, bool shouldExecuteInMainThread)
        {
            _taskToRun = taskToRun;
            _taskToRunEnding = taskToRunEnding;
            _taskToRunStarting = taskToRunStarting;
            ShouldExecuteInMainThread = shouldExecuteInMainThread;
        }

        public bool ShouldExecuteInMainThread { get; }

        public bool LogQueueStatus => true;

        public void Execute()
        {
            _taskToRun();
        }

        public void Starting()
        {
            if (_taskToRunStarting != null)
                _taskToRunStarting();
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