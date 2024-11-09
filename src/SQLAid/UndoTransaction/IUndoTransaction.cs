using System;

namespace SQLAid.UndoTransaction
{
    public interface IUndoTransaction : IDisposable
    {
        void Run(Action action);
    }
}