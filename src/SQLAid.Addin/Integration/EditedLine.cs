namespace SQLAid.Integration
{
    public class EditedLine
    {
        public string Line { get; protected set; }
        public int CaretPos { get; protected set; }

        public EditedLine(string line, int caretPos)
        {
            Line = line;
            CaretPos = caretPos;
        }

        public int Length
        { get { return Line.Length; } }
    }
}