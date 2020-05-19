namespace Shortcodes
{
    public class Cursor
    {
        private readonly int _textLength;

        public Cursor(string text, int start)
        {
            Offset = start;
            Text = text;
            _textLength = text.Length;
            Char = Text.Length == 0 ? '\0' : Text[0];
        }

        public Cursor Clone()
        {
            return new Cursor(Text, Offset);
        }

        public void Advance()
        {
            if (Eof)
            {
                return;
            }

            Offset = Offset + 1;

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
        public int Offset { get; set; }
        public string Text { get; }
    }
}
