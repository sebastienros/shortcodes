namespace Shortcodes
{
    public struct Token
    {
        public static readonly Token Empty = new Token();
        private string _value;

        public Token(string type, string text, int offset, int length)
        {
            Type = type;
            SourceText = text;
            Start = offset;
            Length = length;
            _value = null;
        }

        public string Type { get; }

        public int Start { get; }
        public int Length { get; }
        public string SourceText { get; }

        public override string ToString()
        {
            return _value ?? (_value = SourceText.Substring(Start, Length));
        }
    }
}
