using Microsoft.SqlServer.Management.UI.Grid;
using SQLAid.Extensions;
using System;
using System.Data;
using System.Drawing;

namespace SQLAid.Integration.DTE.Grid.Result
{
    public sealed class GridMetadata : IDisposable
    {
        private readonly IGridControl _gridControl;
        private readonly DataTable _schemaTable;
        private bool _isDisposed;

        public GridMetadata(IGridControl gridControl)
        {
            _gridControl = gridControl;
            _schemaTable = _gridControl.GridStorage.GetField<DataTable>("m_schemaTable");
        }

        public Type GetColumnType(int colIndex)
        {
            ThrowIfDisposed();
            return _schemaTable.Rows[colIndex - 1][12].As<Type>();
        }

        public string GetColumnName(int colIndex)
        {
            ThrowIfDisposed();

            if (IsValidColumnIndex(colIndex))
            {
                _gridControl.GetHeaderInfo(colIndex, out var columnName, out Bitmap _);
                return columnName;
            }

            return string.Empty;
        }

        public (Type, string) GetColumnInfo(int colIndex)
        {
            ThrowIfDisposed();

            if (IsValidColumnIndex(colIndex))
            {
                return (GetColumnType(colIndex), GetColumnName(colIndex));
            }

            return (default, string.Empty);
        }

        public bool IsValidColumnIndex(int colIndex)
        {
            return colIndex > 0 && colIndex < _gridControl.ColumnsNumber;
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _schemaTable?.Dispose();
            _isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(GridMetadata));
        }
    }
}