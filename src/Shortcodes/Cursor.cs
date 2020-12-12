namespace Shortcodes
{
    public class Cursor
    {
        private readonly int _textLength;
        private char _prev;

        public Cursor(string text, int start)
        {
            Line = 0;
            Column = 0;
            Offset = start;
            Text = text;
            _textLength = text.Length;
            Char = Text.Length == 0 ? '\0' : Text[0];
            _prev = '\0';
        }

        public Cursor Clone()
        {
            return new Cursor(Text, Offset) { Char = Char, Line = Line, Column = Column };
        }

        public void Advance()
        {
            if (Eof)
            {
                return;
            }

            Offset++;

            if (Offset < _textLength)
            {
                Char = Text[Offset];

                if (Char == '\n' || (Char == '\r' && _prev != '\n'))
                {
                    Column = 0;
                    Line += 1;
                }
                else
                {
                    Column++;
                }
            }
            else
            {
                Char = '\0';
            }
        }

        public void Seek(int offset)
        {
            if (Eof)
            {
                return;
            }

            Offset = offset;

            if (Offset < _textLength)
            {
                Char = Text[Offset];
            }
            else
            {
                Char = '\0';
            }
        }

        public char PeekNext(int index = 1)
        {
            if (_textLength == 0)
            {
                return '\0';
            }

            var nextIndex = Offset + index;

            if (nextIndex >= _textLength || nextIndex < 0)
            {
                return '\0';
            }

            return Text[Offset + index];
        }

        public bool Eof => Offset >= _textLength;
        public char Char { get; private set; }
        public int Offset { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }
        public string Text { get; }
    }
}
