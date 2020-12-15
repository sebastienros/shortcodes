using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Shortcodes
{
    public class Cursor
    {
        private Stack<int> _stack = new Stack<int>();
        private readonly int _textLength;
        private char _current;

        public Cursor(string text, int start)
        {
            Line = 0;
            Column = 0;
            Offset = start;
            Text = text;
            _textLength = text.Length;
            _current = _textLength == 0 ? '\0' : Text[start];
            Eof = _textLength == 0;
        }

        /// <summary>
        /// Creates and new cursor and keeps a backup of the previous one.
        /// Use this method when the current location of the text needs to be kept in case the parsing doesn't reach a successful state and
        /// another token needs to be tried.
        /// </summary>
        public void RecordLocation()
        {
            _stack.Push(Offset);
        }

        /// <summary>
        /// Discard the current cursor and go back to the previous one.
        /// Use this method when a cursor needs to be reverted to the previously saved one.
        /// </summary>
        public void RestoreLocation()
        {
            Seek(_stack.Pop());
        }

        /// <summary>
        /// Get rid of any previously saved state.
        /// Use this method when the changes made on the cursor need to be kept.
        /// </summary>
        public void SaveLocation()
        {
            _stack.Pop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance()
        {
            if (Eof)
            {
                return;
            }

            Offset++;

            Eof = Offset >= _textLength;
            
            if (Eof)
            {
                _current = '\0';
                return;
            }

            var c = Text[Offset];

            if (c == '\n' || (c == '\r' && _current != '\n'))
            {
                Column = 0;
                Line += 1;
            }
            else
            {
                Column++;
            }

            _current = c;            
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Seek(int offset)
        {
            if (offset < 0 || offset >= _textLength)
            {
                throw new ArgumentException("Offset out of bounds");
            }
            
            Offset = offset;
            _current = Text[Offset];
            Eof = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char Peek()
        {
            if (Eof)
            {
                return '\0';
            }

            return _current;
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

            return Text[nextIndex];
        }

        public bool Eof { get; private set; }
        public int Offset { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }
        public string Text { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Match(char c)
        {
            if (Eof)
            {
                return false;
            }

            return _current == c;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Match(char c1, char c2)
        {
            if (Eof || Offset + 1 >= _textLength)
            {
                return false;
            }

            return _current == c1 && Text[Offset + 1] == c2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Match(string s)
        {
            if (s.Length == 1)
            {
                return !Eof && _current == s[0];
            }

            if (Eof || Offset + s.Length - 1 >= _textLength)
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
