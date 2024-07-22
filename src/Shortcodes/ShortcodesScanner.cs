using Parlot;
using System;

namespace Shortcodes
{
    public class ShortcodesScanner : Scanner
    {
        public ShortcodesScanner(string buffer) : base(buffer)
        {

        }

        public bool ReadRawText(out ReadOnlySpan<char> result)
        {
            var start = Cursor.Offset;

            while (Cursor.Match('['))
            {
                Cursor.Advance();
            }

            while (!Cursor.Match('[') && !Cursor.Eof)
            {
                Cursor.Advance();
            }

            var length = Cursor.Offset - start;

            if (length == 0)
            {
                result = ReadOnlySpan<char>.Empty;
                return false;
            }

            result = Buffer.AsSpan(start, Cursor.Offset);

            return true;
        }

        public bool ReadValue(out ReadOnlySpan<char> result)
        {
            if (Cursor.Match(']') || Cursor.Match('\'') || Cursor.Match('"') || Character.IsWhiteSpaceOrNewLine(Cursor.Current))
            {
                result = ReadOnlySpan<char>.Empty;
                return false;
            }

            if (Cursor.Match("/]"))
            {
                result = ReadOnlySpan<char>.Empty;
                return false;
            }

            var start = Cursor.Offset;

            while (!Character.IsWhiteSpaceOrNewLine(Cursor.Current) && !Cursor.Match(']'))
            {
                if (Cursor.Eof)
                {
                    result = ReadOnlySpan<char>.Empty;
                    return false;
                }

                Cursor.Advance();
            }

            result = Buffer.AsSpan(start, Cursor.Offset);

            return true;
        }
    }
}
