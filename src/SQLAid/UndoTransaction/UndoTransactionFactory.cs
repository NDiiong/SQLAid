using EnvDTE80;
using SQLAid.Commands.TextEditor;
using System;

namespace SQLAid.UndoTransaction
{
    public class UndoTransactionFactory : IUndoTransactionFactory
    {
        private readonly DTE2 _application;

        public UndoTransactionFactory(DTE2 application)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
        }

        public IUndoTransaction Create(string name)
        {
            return new UndoTransactionWrapper(_application, name);
        }
    }
}