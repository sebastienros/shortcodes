using Parlot;
using System;

namespace Shortcodes
{
    public class RawText : Node
    {
        private readonly TextSpan _textSpan;

        public RawText(TextSpan textSpan)
        {
            _textSpan = textSpan;
        }

        public ReadOnlySpan<char> Span => _textSpan.Span;
    }
}
