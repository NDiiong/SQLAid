using System.Collections.Generic;

namespace SQLAid.Integration.DTE.Grid
{
    public class ColumnNameTracker
    {
        private readonly Dictionary<string, int> _nameCount = new Dictionary<string, int>();

        public string GetUniqueColumnName(string baseName)
        {
            if (string.IsNullOrEmpty(baseName))
            {
                baseName = "Column";
            }

            if (!_nameCount.ContainsKey(baseName))
            {
                _nameCount[baseName] = 0;
                return baseName;
            }

            _nameCount[baseName]++;
            return $"{baseName}:{_nameCount[baseName]}";
        }

        public void Reset()
        {
            _nameCount.Clear();
        }
    }
}