namespace Shortcodes
{
    public class Token
    {
        public static readonly Token Empty = new Token(null, null, 0, 0);
        private string _value;

        public Token(string type, string text, int startIndex, int length)
        {
            Type = type;
            Text = text;
            StartIndex = startIndex;
            Length = length;
            _value = null;
        }

        public string Type { get; }

        public int StartIndex { get; }
        public int Length { get; }
        public string Text { get; }

        public override string ToString()
        {
            return _value ??= Text.Substring(StartIndex, Length);
        }
    }
}
