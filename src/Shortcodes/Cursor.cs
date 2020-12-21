using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Shortcodes
{
    public struct Cursor
    {
        private Stack<int> _stack;
        private readonly bool _track;
        private readonly int _textLength;
        private char _current;

        public Cursor(string text, int start, bool track)
        {
            _stack = new Stack<int>();
            _track = track;
            Line = 0;
            Column = 0;
            Offset = start;
            Text = text;
            _textLength = text.Length;
            _current = _textLength == 0 ? '\0' : Text[start];
            Eof = _textLength == 0;
        }

        /// <summary>
        /// Records the current location of the cursor.
        /// Use this method when the current location of the text needs to be kept in case the parsing doesn't reach a successful state and
        /// another token needs to be tried.
        /// </summary>
        public void RecordLocation()
        {
            _stack.Push(Offset);
        }

        /// <summary>
        /// Restores the cursor to the last recorded location.
        /// Use this method when a token wasn't found and the cursor needs to be pointing to the previously recorded location.
        /// </summary>
        public void RollbackLocation()
        {
            Seek(_stack.Pop());
        }

        /// <summary>
        /// Discard the previously recorded location.
        /// Use this method when a token was successfuly found and the recorded location can be discaded.
        /// </summary>
        public void CommitLocation()
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

            // Should we track the cursor position in the text?
            if (_track)
            {
                if (c == '\n' || (c == '\r' && _current != '\n'))
                {
                    Column = 0;
                    Line += 1;
                }
                else
                {
                    Column++;
                }
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

        /// <summary>
        /// Whether a char is at the current position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Match(char c)
        {
            if (Eof)
            {
                return false;
            }

            return _current == c;
        }

        /// <summary>
        /// Whether a string is at the current position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Match(string s)
        {
            var length = s.Length;

            if (length == 1)
            {
                return !Eof && _current == s[0];
            }

            if (Eof || Offset + length - 1 >= _textLength)
            {
                return false;
            }

            if (length == 2)
            {
                return s[0] == Text[Offset + 0] && s[1] == Text[Offset + 1];
            }

            for (var i = 0; i < length; i++)
            {
                if (s[i] != Text[Offset + i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
