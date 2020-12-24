﻿using Parlot;

namespace Shortcodes
{
    public class ShortcodesScanner : Scanner
    {
        public ShortcodesScanner(string buffer) : base(buffer)
        {

        }

        public bool ReadRawText(TokenResult result = null)
        {
            var start = Cursor.Position;

            while (Cursor.Match('['))
            {
                Cursor.Advance();
            }

            while (!Cursor.Match('[') && !Cursor.Eof)
            {
                Cursor.Advance();
            }

            var length = Cursor.Position - start;

            if (length == 0)
            {
                result?.Fail();
                return false;
            }

            result?.Succeed(Buffer, start, Cursor.Position);

            return true;
        } 

        public bool ReadValue(TokenResult result = null)
        {
            if (Cursor.Match(']') || Cursor.Match('\'') || Cursor.Match('"') || Character.IsWhiteSpaceOrNewLine(Cursor.Current))
            {
                result?.Fail();
                return false;
            }

            if (Cursor.Match("/]"))
            {
                result?.Fail();
                return false;
            }

            var start = Cursor.Position;

            while (!Character.IsWhiteSpaceOrNewLine(Cursor.Current) && !Cursor.Match(']'))
            {
                if (Cursor.Eof)
                {
                    result?.Fail();
                    return false;
                }

                Cursor.Advance();
            }

            result?.Succeed(Buffer, start, Cursor.Position);

            return true;
        }
    }
}
