namespace Shortcodes
{
    public class RawText : Node
    {
        public RawText(string buffer, int offset, int count)
        {
            Buffer = buffer;
            Offset = offset;
            Count = count;
        }

        public string Buffer { get; }
        public int Offset { get; }
        public int Count { get; }
    }
}
