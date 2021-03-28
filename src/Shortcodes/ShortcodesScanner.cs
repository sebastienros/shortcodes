using Parlot;

namespace Shortcodes
{
    public class ShortcodesScanner : Scanner
    {
        public ShortcodesScanner(string buffer) : base(buffer)
        {

        }

        public bool ReadRawText(out TokenResult result)
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
                result = TokenResult.Fail();
                return false;
            }

            result = TokenResult.Succeed(Buffer, start, Cursor.Offset);

            return true;
        } 

        public bool ReadValue(out TokenResult result)
        {
            if (Cursor.Match(']') || Cursor.Match('\'') || Cursor.Match('"') || Character.IsWhiteSpaceOrNewLine(Cursor.Current))
            {
                result = TokenResult.Fail();
                return false;
            }

            if (Cursor.Match("/]"))
            {
                result = TokenResult.Fail();
                return false;
            }

            var start = Cursor.Offset;

            while (!Character.IsWhiteSpaceOrNewLine(Cursor.Current) && !Cursor.Match(']'))
            {
                if (Cursor.Eof)
                {
                    result = TokenResult.Fail();
                    return false;
                }

                Cursor.Advance();
            }

            result = TokenResult.Succeed(Buffer, start, Cursor.Offset);

            return true;
        }
    }
}
