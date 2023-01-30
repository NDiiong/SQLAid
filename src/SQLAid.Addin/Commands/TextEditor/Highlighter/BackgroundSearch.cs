using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;

namespace SQLAid.Commands.TextEditor.Highlighter
{
    public class BackgroundSearch
    {
        private bool _abort;
        private static List<SnapshotSpan> s_emptyList = new List<SnapshotSpan>(0);
        private IList<SnapshotSpan> _matches = s_emptyList;
        public readonly ITextSnapshot Snapshot;

        public IList<SnapshotSpan> Matches => _matches;

        public BackgroundSearch(ITextSnapshot snapshot, string searchText, bool matchWholeWord, Action completionCallback)
        {
            this.Snapshot = snapshot;

            var newMatches = new List<SnapshotSpan>();

            var start = 0;
            while (true)
            {
                var end = Math.Min(snapshot.Length, start + 4096);
                var text = snapshot.GetText(start, end - start);

                var offset = (start == 0) ? 0 : 1;
                while (true)
                {
                    var match = text.IndexOf(searchText, offset, StringComparison.Ordinal);
                    if (match == -1)
                        break;

                    if (matchWholeWord)
                    {
                        if ((match == 0) || !BackgroundSearch.IsWordCharacter(text[match - 1]))
                        {
                            if ((match + searchText.Length == text.Length)
                                ? (end == snapshot.Length)
                                : !BackgroundSearch.IsWordCharacter(text[match + searchText.Length]))
                            {
                                var matchSpan = new SnapshotSpan(snapshot, match + start, searchText.Length);
                                newMatches.Add(matchSpan);
                            }
                        }
                    }
                    else
                    {
                        var matchSpan = new SnapshotSpan(snapshot, match + start, searchText.Length);
                        newMatches.Add(matchSpan);
                    }

                    offset = match + searchText.Length;
                }

                if (_abort)
                    return;

                if (end == snapshot.Length)
                    break;

                start = end - (searchText.Length + 1);
            }

            _matches = newMatches;
        }

        public static bool IsWordCharacter(char c)
        {
            return (c == '_') | char.IsLetterOrDigit(c);
        }
    }
}