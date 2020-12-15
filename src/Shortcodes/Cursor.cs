using System;

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
            _prev = '\0';
        }

        public Cursor Clone()
        {
            return new Cursor(Text, Offset) { Line = Line, Column = Column };
        }

        public void Advance()
        {
            if (Eof)
            {
                return;
            }

            Offset++;

            var c = Peek();

            if (c == '\n' || (c == '\r' && _prev != '\n'))
            {
                Column = 0;
                Line += 1;
            }
            else
            {
                Column++;
            }
            
            _prev = c; 
        }

        public void Seek(int offset)
        {
            if (offset < 0 || offset >= _textLength)
            {
                throw new ArgumentException("Offset out of bounds");
            }
            
            Offset = offset;
        }

        public char Peek()
        {
            if (Eof)
            {
                return '\0';
            }

            return Text[Offset];
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
        public int Offset { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }
        public string Text { get; }
        public bool Match(char c)
        {
            if (Eof)
            {
                return false;
            }

            return c.Equals(Text[Offset]);
        }

        public bool Match(string s)
        {
            if (Eof || Offset + s.Length >= _textLength + 1)
            {
                return false;
            }

            for (var i = 0; i < s.Length; i++)
            {
                if (!s[i].Equals(Text[Offset + i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
