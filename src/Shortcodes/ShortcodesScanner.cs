using Parlot;

namespace Shortcodes
{
    public class ShortcodesScanner : Scanner
    {
        public ShortcodesScanner(string buffer) : base(buffer)
        {

        }

        public bool ReadRawText(out Token<string> token)
        {
            token = Token<string>.Empty;

            var start = Cursor.Position;

            Cursor.RecordPosition();

            while (!Cursor.Eof && Cursor.Match('['))
            {
                Cursor.Advance();
            }

            while (!Cursor.Eof && !Cursor.Match('['))
            {
                Cursor.Advance();
            }

            var length = Cursor.Position - start;

            if (length == 0)
            {
                Cursor.RollbackPosition();

                return false;
            }

            Cursor.CommitPosition();

            token = EmitToken(null, start, Cursor.Position);

            return true;
        } 

        public bool ReadValue(out Token<string> token)
        {
            token = Token<string>.Empty;

            var start = Cursor.Position;

            if (Cursor.Match("]") || Cursor.Match("'") || Cursor.Match("\""))
            {
                return false;
            }

            if (Cursor.Match("/]"))
            {
                return false;
            }

            while (!Character.IsWhiteSpace(Cursor.Peek()) && !Cursor.Match("]"))
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

            token = EmitToken("value", start, Cursor.Position);
            return true;
        }
    }
}
