namespace SQLAid.Threading
{
    public interface IThreadTask
    {
        bool ShouldExecuteInMainThread { get; }

        bool LogQueueStatus { get; }

        void Execute();

        void Starting();

        void Ending();
    }
}