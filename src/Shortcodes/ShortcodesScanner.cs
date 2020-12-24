using Parlot;

namespace Shortcodes
{
    public class ShortcodesScanner : Scanner
    {
        public ShortcodesScanner(string buffer) : base(buffer)
        {

        }

        public bool ReadRawText(TokenResult result = null)
        {
            result?.Reset();

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
                return false;
            }

            result?.Set(Buffer, start, Cursor.Position);

            return true;
        } 

        public bool ReadValue(TokenResult result = null)
        {
            result?.Reset();

            if (Cursor.Match(']') || Cursor.Match('\'') || Cursor.Match('"'))
            {
                return false;
            }

            if (Cursor.Match("/]"))
            {
                return false;
            }

            var start = Cursor.Position;

            while (!Character.IsWhiteSpaceOrNewLine(Cursor.Current) && !Cursor.Match(']'))
            {
                if (Cursor.Eof)
                {
                    return false;
                }

                Cursor.Advance();
            }

            var length = Cursor.Position - start;

            if (length == 0)
            {
                return false;
            }

            result?.Set(Buffer, start, Cursor.Position);

            return true;
        }
    }
}
