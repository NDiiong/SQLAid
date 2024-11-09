namespace SQLAid.UndoTransaction
{
    public interface IUndoTransactionFactory
    {
        IUndoTransaction Create(string name);
    }
}