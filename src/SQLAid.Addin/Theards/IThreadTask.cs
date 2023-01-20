namespace SQLAid.Theards
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