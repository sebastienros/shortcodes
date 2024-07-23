﻿using Parlot;
using System;

namespace Shortcodes
{
    public class ShortcodesScanner : Scanner
    {
        public ShortcodesScanner(string buffer) : base(buffer)
        {

        }

        public bool ReadRawText(out TextSpan result)
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
                result = null;
                return false;
            }

            result = new TextSpan(Buffer, start, length);

            return true;
        } 

        public bool ReadValue(out TextSpan result)
        {
            if (Cursor.Match(']') || Cursor.Match('\'') || Cursor.Match('"') || Character.IsWhiteSpaceOrNewLine(Cursor.Current))
            {
                result = null;
                return false;
            }

            if (Cursor.Match("/]"))
            {
                result = null;
                return false;
            }

            var start = Cursor.Offset;

            while (!Character.IsWhiteSpaceOrNewLine(Cursor.Current) && !Cursor.Match(']'))
            {
                if (Cursor.Eof)
                {
                    result = null;
                    return false;
                }

                Cursor.Advance();
            }

            result = new TextSpan(Buffer, start, Cursor.Offset - start);

            return true;
        }
    }
}
