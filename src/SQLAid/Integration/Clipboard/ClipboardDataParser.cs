using System;
using System.Collections.Generic;
using System.Linq;

namespace SQLAid.Integration.Clipboard
{
    public class ClipboardDataParser
    {
        public (string[] Headers, IEnumerable<string[]> Rows) Parse(string content)
        {
            var lines = content.Split(new[] { Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries);

            if (!lines.Any())
                throw new ArgumentException("Clipboard content is empty");

            var headers = lines.First().Split('\t');
            var rows = lines.Skip(1)
                .Select(line => line.Split('\t'))
                .ToList();

            return (headers, rows);
        }
    }
}