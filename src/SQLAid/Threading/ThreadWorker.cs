using System.ComponentModel;
using System.Threading;

namespace SQLAid.Threading
{
    public class ThreadWorker : BackgroundWorker
    {
        private Thread _workerThread;

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            _workerThread = Thread.CurrentThread;
            try
            {
                base.OnDoWork(e);
            }
            catch (ThreadAbortException)
            {
                e.Cancel = true;
                Thread.ResetAbort();
            }
        }

        public void Abort()
        {
            if (_workerThread != null && _workerThread.IsAlive)
            {
                _workerThread.Abort();
                _workerThread.Join(500);
                _workerThread = null;
            }
        }

        //private ThreadWorker _worker;

        //private void TryOpenConnection()
        //{
        //    _worker = new ThreadWorker();
        //    _worker.DoWork += OpenConnection;
        //    _worker.RunWorkerCompleted += CheckConnection;
        //    _worker.RunWorkerAsync();
        //}

        //private void CancelConnection()
        //{
        //    if (_worker != null && _worker.IsBusy)
        //    {
        //        _worker.RunWorkerCompleted -= CheckConnection;
        //        _worker.Abort();
        //        _worker.Dispose();
        //        UpdateControlUsage(true);
        //        boxServer.Focus();
        //    }
        //    else
        //    {
        //        DialogResult = DialogResult.Cancel;
        //    }
        //}
    }
}