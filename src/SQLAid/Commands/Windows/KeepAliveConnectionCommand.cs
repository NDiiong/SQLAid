using EnvDTE;

using Microsoft.VisualStudio.Shell;
using SQLAid.Integration;
using SQLAid.Integration.DTE;
using System;

#if SSMS18DLL
using System.Data.SqlClient;
#endif

#if SSMS19DLL || SSMS20DLL

#endif

using System.Collections.Generic;
using SQLAid.Logging;
using Task = System.Threading.Tasks.Task;
using SQLAid.Extensions;
using System.Linq;

namespace SQLAid.Commands.Windows
{
    internal sealed class KeepAliveConnectionCommand
    {
        private static WindowEvents _windowEvents;
        private static readonly ISqlConnection _sqlConnection;
        private static readonly KeepAliveConnectionService _keepAliveConnectionService = new KeepAliveConnectionService();

        static KeepAliveConnectionCommand()
        {
            _sqlConnection = new SQLAid.Integration.DTE.Connection.SqlConnection();
        }

        public static async Task InitializeAsync(SqlAsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _windowEvents = package.Application.Events.get_WindowEvents();
            _windowEvents.WindowCreated += WindowEvents_WindowCreated;
            _keepAliveConnectionService.StartKeepAlive(TimeSpan.FromMinutes(2));
        }

        private static void WindowEvents_WindowCreated(Window Window)
        {
            Func.Run(() =>
            {
                var connection = _sqlConnection.GetCurrentSqlConnection();
                _keepAliveConnectionService.AddItem(new SqlClientItem(connection.ConnectionString));
            });
        }
    }

    internal class KeepAliveConnectionService
    {
        private static readonly List<SqlClientItem> _itemsQueue = new List<SqlClientItem>();

        public void AddItem(SqlClientItem sqlClientItem)
        {
            if (!_itemsQueue.Any(item => item.ConnectionString == sqlClientItem.ConnectionString))
                _itemsQueue.Add(sqlClientItem);
        }

        public void StartKeepAlive(TimeSpan interval)
        {
#if SSMS19DLL || SSMS20DLL

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(interval);
                    await ExecuteKeepAliveQueryAsync();
                }
            }).RunAndForget();

#endif
        }

        private async Task ExecuteKeepAliveQueryAsync()
        {
            foreach (var item in _itemsQueue)
            {
                try
                {
                    using (var connection = new Microsoft.Data.SqlClient.SqlConnection(item.ConnectionString))
                    {
                        await connection.OpenAsync();
                        using (var command = new Microsoft.Data.SqlClient.SqlCommand("SELECT 1", connection))
                        {
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }
    }

    internal class SqlClientItem
    {
        public string ConnectionString { get; set; }

        public SqlClientItem(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}