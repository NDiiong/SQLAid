using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading.Tasks;
using SQLAid.Integration.DTE;
using SQLAid.Integration;

namespace SQLAid.Commands.TextEditor
{
    internal sealed class UnsafeUpdateAlertCommand
    {
        private readonly IFrameDocumentView _documentView;

        public UnsafeUpdateAlertCommand(IFrameDocumentView documentView)
        {
            _documentView = documentView;
        }

        public async Task ExecuteAsync()
        {
            var sql = _documentView.QueryText;
            if (IsUnsafeUpdate(sql))
            {
                var result = MessageBox.Show(
                    "Warning: You are about to execute an UPDATE statement without a WHERE clause. This may affect all rows.\n\nDo you want to continue?",
                    "Unsafe UPDATE Detected",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );
                if (result == DialogResult.No)
                {
                    // Cancel execution
                    return;
                }
            }
            // Proceed with execution logic here
        }

        /// <summary>
        /// Detects unsafe UPDATE statements (no WHERE, not temp table), robust to whitespace and multi-line
        /// </summary>
        public static bool IsUnsafeUpdate(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) return false;
            // Normalize whitespace and collapse lines
            var normalized = Regex.Replace(sql, "[\r\n]+", " ");
            normalized = Regex.Replace(normalized, "\s+", " ");
            // Regex: UPDATE [table] SET ... (no WHERE), exclude temp tables
            var updatePattern = @"update\s+(?!#)([\w\d_]+)\s+set[\s\S]+?(;|$)";
            var matches = Regex.Matches(normalized, updatePattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var updateBlock = match.Value;
                // If WHERE is not present in the block, it's unsafe
                if (!Regex.IsMatch(updateBlock, "where", RegexOptions.IgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
