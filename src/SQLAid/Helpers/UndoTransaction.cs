﻿using EnvDTE;
using System;

namespace SQLAid.Helpers
{
    internal class UndoTransaction
    {
        private readonly _DTE _dte;
        private readonly string _transactionName;

        public UndoTransaction(_DTE dte, string transactionName)
        {
            _dte = dte;
            _transactionName = transactionName;
        }

        public void Run(Action tryAction, Action<Exception> catchAction = null)
        {
            var shouldCloseUndoContext = false;

            if (!_dte.UndoContext.IsOpen)
            {
                _dte.UndoContext.Open(_transactionName);
                shouldCloseUndoContext = true;
            }

            try
            {
                tryAction();
            }
            catch (Exception ex)
            {
                var message = $"{_transactionName} was stopped";
                _dte.StatusBar.Text = $"{message}.  See output window for more details.";

                catchAction?.Invoke(ex);

                if (shouldCloseUndoContext)
                {
                    _dte.UndoContext.SetAborted();
                    shouldCloseUndoContext = false;
                }
            }
            finally
            {
                // Always close the undo transaction to prevent ongoing interference with the IDE.
                if (shouldCloseUndoContext)
                {
                    _dte.UndoContext.Close();
                }
            }
        }
    }
}