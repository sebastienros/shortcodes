namespace Shortcodes
{
    public class RawText : Node
    {
        public RawText(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }
}
