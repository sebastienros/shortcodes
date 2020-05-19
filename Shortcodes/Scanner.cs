using System;
using System.Collections.Generic;
using System.Text;

namespace Shortcodes
{
    public class Scanner
    {
        private readonly string _text;
        private Token _token;
        private Stack<Cursor> _cursors = new Stack<Cursor>();
        private Cursor _cursor;
        private StringBuilder _sb;

        public Scanner(string text)
        {
            _text = text;
            _cursor = new Cursor(_text, 0);
            _cursors.Push(_cursor);
        }

        public Action<Token> OnToken { get; set; }

        public void CreateCursor()
        {
            _cursor = _cursor.Clone();
            _cursors.Push(_cursor);
        }

        public void DiscardCursor()
        {
            _cursor = _cursors.Pop();
        }

        public void PromoteCursor()
        {
            _cursors.Pop();
        }

        /// <summary>
        /// Reads any whitespace without generating a token.
        /// </summary>
        /// <returns>Whether some white space was read.</returns>
        public bool SkipWhiteSpace()
        {
            if (!IsWhiteSpace())
            {
                return false;
            }

            while (IsWhiteSpace())
            {
                _cursor.Advance();
            }

            return true;
        }

        public void EmitToken(string type, int start, int length)
        {
            _token = new Token(type, _text, start, length);

            OnToken?.Invoke(_token);
        }

        public bool IsHex()
        {
            return Uri.IsHexDigit(_cursor.Char);
        }

        public bool IsWhiteSpace()
        {
            const char NonBreakingSpace = (char)160;

            switch (_cursor.Char)
            {
                case ' ':
                case '\t':
                case NonBreakingSpace:
                case '\r':
                case '\n':
                case '\v':
                    return true;
                default:
                    return false;
            }
        }

        public bool IsIdentifier()
        {
            var ch = _cursor.Char;

            return
                (ch >= 'A' && ch <= 'Z') ||
                (ch >= 'a' && ch <= 'z');
        }

        public bool ReadIdentifier()
        {
            var start = _cursor.Offset;

            if (!IsIdentifier())
            {
                return false;
            }

            while (IsIdentifier())
            {
                _cursor.Advance();
            }

            EmitToken("identifier", start, _cursor.Offset - start);

            return true;
        }

        public List<Node> Scan()
        {
            var nodes = new List<Node>();

            var start = _cursor.Offset;

            CreateCursor();

            while (!_cursor.Eof)
            {
                var startShortcode = _cursor.Offset;

                if (_cursor.Char == '[')
                {
                    if (_cursor.PeekNext() == '[')
                    {
                        _cursor.Advance();
                        _cursor.Advance();
                    }
                    else if (ReadShortcode(out var shortcode))
                    {
                        if (startShortcode - start > 0)
                        {
                            nodes.Add(new RawText(_text.Substring(start, startShortcode - start)));
                        }

                        nodes.Add(shortcode);

                        start = _cursor.Offset + 1;

                        PromoteCursor();
                    }
                }
                else
                {
                    _cursor.Advance();
                }
            }

            if (start < _text.Length)
            {
                nodes.Add(new RawText(_text.Substring(start)));
            }

            return nodes;
        }

        public bool ReadShortcode(out Shortcode shortcode)
        {
            shortcode = null;
            var style = ShortcodeStyle.Open;

            if (_cursor.Char != '[')
            {
                return false;
            }

            CreateCursor();

            _cursor.Advance();

            // Is it a closing tag?
            if (_cursor.Char == '/')
            {
                style = ShortcodeStyle.Close;

                _cursor.Advance();
            }

            // Reach Eof before end of shortcode
            if (_cursor.Eof)
            {
                DiscardCursor();

                return false;
            }

            SkipWhiteSpace();

            if (!ReadIdentifier())
            {
                DiscardCursor();

                return false;
            }

            Token identifier = _token;

            SkipWhiteSpace();

            Dictionary<string, string> arguments = null;

            // Arguments?
            while (ReadIdentifier())
            {
                var argument = _token;

                SkipWhiteSpace();

                if (!ReadEqualSign())
                {
                    DiscardCursor();

                    return false;
                }

                SkipWhiteSpace();

                if (ReadString())
                {
                    var value = _token;

                    arguments ??= new Dictionary<string, string>();

                    arguments[argument.ToString()] = DecodeString(value.ToString());
                }
                else
                {
                    DiscardCursor();

                    return false;
                }

                SkipWhiteSpace();
            }

            // Is it a self-closing tag?
            if (_cursor.Char == '/' && _cursor.PeekNext() == ']')
            {
                style = ShortcodeStyle.SelfClosing;

                _cursor.Advance();
            }

            // Expect closing bracket
            if (_cursor.Char != ']')
            {
                DiscardCursor();

                return false;
            }

            // Ignore shortcode if the next char is also ']', making it a comment
            if (_cursor.PeekNext() == ']')
            {
                DiscardCursor();

                return false;
            }

            shortcode = new Shortcode(identifier.ToString(), style);
            shortcode.Arguments = arguments;

            PromoteCursor();

            return true;
        }

        public bool ReadEqualSign()
        {
            if (_cursor.Char != '=')
            {
                return false;
            }

            _cursor.Advance();

            return true;
        }

        public bool ReadString()
        {
            if (_cursor.Char != '\'' && _cursor.Char != '"')
            {
                return false;
            }

            var start = _cursor.Offset;

            var startChar = _cursor.Char;

            _cursor.Advance();

            while (_cursor.Char != startChar)
            {
                if (_cursor.Eof)
                {
                    return false;
                }

                if (_cursor.Char == '\\')
                {
                    _cursor.Advance();

                    var success = false;

                    switch (_cursor.Char)
                    {
                        case '0':
                        case '\'':
                        case '"':
                        case '\\':
                        case 'b':
                        case 'f':
                        case 'n':
                        case 'r':
                        case 't':
                        case 'v':
                            _cursor.Advance();
                            break;
                        case 'u':
                            _cursor.Advance();

                            if (IsHex())
                            {
                                _cursor.Advance();
                                if (IsHex())
                                {
                                    _cursor.Advance();
                                    if (IsHex())
                                    {
                                        _cursor.Advance();
                                        success = true;
                                    }
                                }
                            }

                            if (!success)
                            {
                                return false;
                            }

                            break;
                        case 'x':
                            _cursor.Advance();

                            if (IsHex())
                            {
                                _cursor.Advance();
                                if (IsHex())
                                {
                                    success = true;
                                }
                            }
                            if (!success)
                            {
                                return false;
                            }
                            break;
                    }
                }

                _cursor.Advance();
            }

            _cursor.Advance();

            EmitToken("string", start, _cursor.Offset - start);

            return true;
        }

        public string DecodeString(string text)
        {
            var sb = GetStringBuilder();

            // Skip quotes
            for (var i = 1; i < text.Length - 1; i++)
            {
                var c = text[i];

                if (c == '\\')
                {
                    i = i + 1;
                    c = text[i];

                    switch (c)
                    {
                        case '0': sb.Append("\0"); break;
                        case '\'': sb.Append("\'"); break;
                        case '"': sb.Append("\""); break;
                        case '\\': sb.Append("\\"); break;
                        case 'b': sb.Append("\b"); break;
                        case 'f': sb.Append("\f"); break;
                        case 'n': sb.Append("\n"); break;
                        case 'r': sb.Append("\r"); break;
                        case 't': sb.Append("\t"); break;
                        case 'v': sb.Append("\v"); break;
                        case 'u':
                        case 'x':
                            sb.Append(ScanHexEscape(text, i));
                            break;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        public char ScanHexEscape(string text, int index)
        {
            var prefix = text[index];
            var len = (prefix == 'u') ? 4 : 2;
            var code = 0;

            for (var i = index + 1; i < len + index + 1; ++i)
            {
                var d = text[i];
                code = code * 16 + HexValue(d);
            }

            return (char)code;
        }

        private static int HexValue(char ch)
        {
            if (ch >= 'A')
            {
                if (ch >= 'a')
                {
                    if (ch <= 'h')
                    {
                        return ch - 'a' + 10;
                    }
                }
                else if (ch <= 'H')
                {
                    return ch - 'A' + 10;
                }
            }
            else if (ch <= '9')
            {
                return ch - '0';
            }

            return 0;
        }

        private StringBuilder GetStringBuilder()
        {
            _sb ??= new StringBuilder();
            _sb.Clear();
            return _sb;
        }
    }
}
