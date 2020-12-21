using System;
using System.Collections.Generic;

namespace Shortcodes
{
    public class Scanner
    {
        private readonly string _text;
        private Token _token;
        private Cursor _cursor;
        

        public Scanner(string text)
        {
            _text = text;
            _cursor = new Cursor(_text, 0, false);
        }

        public Action<Token> OnToken { get; set; }

        /// <summary>
        /// Reads any whitespace without generating a token.
        /// </summary>
        /// <returns>Whether some white space was read.</returns>
        public bool SkipWhiteSpace()
        {
            if (!Character.IsWhiteSpace(_cursor.Peek()))
            {
                return false;
            }

            while (Character.IsWhiteSpace(_cursor.Peek()))
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

        public bool ReadIdentifier()
        {
            var start = _cursor.Offset;

            if (!Character.IsIdentifierStart(_cursor.Peek()))
            {
                return false;
            }

            _cursor.Advance();

            while (Character.IsIdentifierPart(_cursor.Peek()))
            {
                if (_cursor.Eof)
                {
                    return false;
                }

                _cursor.Advance();
            }

            EmitToken("identifier", start, _cursor.Offset - start);

            return true;
        }

        public bool ReadValue()
        {
            var start = _cursor.Offset;

            if (_cursor.Match("]") || _cursor.Match("'") || _cursor.Match("\""))
            {
                return false;
            }

            if (_cursor.Match("/]"))
            {
                return false;
            } 

            while (!Character.IsWhiteSpace(_cursor.Peek()) && !_cursor.Match("]"))
            {
                if (_cursor.Eof)
                {
                    return false;
                }

                _cursor.Advance();
            }

            var length = _cursor.Offset - start; 
            
            if (length == 0)
            {
                return false;
            }

            EmitToken("value", start, length);

            return true;
        }

        public List<Node> Scan()
        {
            var nodes = new List<Node>();

            var start = _cursor.Offset;

            while (!_cursor.Eof)
            {
                var startShortcode = _cursor.Offset;

                if (ReadShortcode(out var shortcode))
                {
                    if (startShortcode - start > 0)
                    {
                        nodes.Add(new RawText(_text.Substring(start, startShortcode - start)));
                    }

                    nodes.Add(shortcode);

                    start = _cursor.Offset;
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
            // Number of opening braces
            var openBraces = 0;

            // Number of closing braces
            var closeBraces = 0;
            
            shortcode = null;
            var style = ShortcodeStyle.Open;

            if (!_cursor.Match("["))
            {
                return false;
            }

            _cursor.RecordLocation();

            // Start position of the shortcode
            var index = _cursor.Offset;

            // Read all '[' so we can detect escaped tags
            do 
            {
                openBraces += 1;
                _cursor.Advance();
            } while (_cursor.Match("["));

            // Is it a closing tag?
            if (_cursor.Match("/"))
            {
                style = ShortcodeStyle.Close;

                _cursor.Advance();
            }

            // Reach Eof before end of shortcode
            if (_cursor.Eof)
            {
               _cursor.RollbackLocation();

                return false;
            }

            SkipWhiteSpace();

            if (!ReadIdentifier())
            {
                _cursor.RollbackLocation();

                return false;
            }

            var identifier = _token.ToString();

            SkipWhiteSpace();

            Dictionary<string, string> arguments = null;

            int argumentIndex = 0;

            // Arguments?
            while (true)
            {
                if (ReadString())
                {
                    arguments ??= CreateArgumentsDictionary();

                    arguments[argumentIndex.ToString()] = Character.DecodeString(_token);

                    argumentIndex += 1;
                }
                else if (ReadIdentifier())
                {
                    var argument = _token;

                    SkipWhiteSpace();

                    // It might just be a value
                    if (ReadEqualSign())
                    {
                        SkipWhiteSpace();

                        if (ReadString())
                        {
                            arguments ??= CreateArgumentsDictionary();

                            arguments[argument.ToString()] = Character.DecodeString(_token);
                        }
                        else if (ReadValue())
                        {
                            arguments ??= CreateArgumentsDictionary();

                            arguments[argument.ToString()] = _token.ToString();
                        }
                        else
                        {
                            _cursor.RollbackLocation();

                            return false;
                        }
                    }
                    else
                    {
                        // Positional argument that looks like an identifier

                        _cursor.Seek(argument.StartIndex);

                        if (ReadValue())
                        {
                            arguments ??= CreateArgumentsDictionary();
                            
                            arguments[argumentIndex.ToString()] = _token.ToString();

                            argumentIndex += 1;
                        }
                        else
                        {
                            _cursor.Seek(argument.StartIndex);

                            break;
                        }
                    }
                }
                else if (ReadValue())
                {
                    arguments ??= CreateArgumentsDictionary();

                    arguments[argumentIndex.ToString()] = _token.ToString();

                    argumentIndex += 1;
                }
                else
                {
                    break;
                }

                SkipWhiteSpace();
            }

            // Is it a self-closing tag?
            if (_cursor.Match("/]"))
            {
                style = ShortcodeStyle.SelfClosing;

                _cursor.Advance();
            }

            // Expect closing bracket
            if (!_cursor.Match("]"))
            {
                _cursor.RollbackLocation();

                return false;
            }

            // Read all ']' so we can detect escaped tags
            do 
            {
                closeBraces += 1;
                _cursor.Advance();
            } while (_cursor.Match("]"));

            shortcode = new Shortcode(identifier, style, openBraces, closeBraces, index, _cursor.Offset - index - 1);
            shortcode.Arguments = new Arguments(arguments);

            _cursor.CommitLocation();

            return true;

            // Local function to use the same logic to create the arguments dictionary
            static Dictionary<string, string> CreateArgumentsDictionary()
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        public bool ReadEqualSign()
        {
            if (!_cursor.Match("="))
            {
                return false;
            }

            _cursor.Advance();

            return true;
        }

        public bool ReadString()
        {
            var start = _cursor.Offset;

            var startChar = _cursor.Peek();

            if (startChar != '\'' && startChar != '"')
            {
                return false;
            }
            
            _cursor.RecordLocation();

            _cursor.Advance();

            while (!_cursor.Match(startChar))
            {
                if (_cursor.Eof)
                {
                    return false;
                }

                if (_cursor.Match("\\"))
                {
                    _cursor.Advance();

                    switch (_cursor.Peek())
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
                            break;
                        case 'u':
                            var isValidUnicode = false;

                            _cursor.Advance();

                            if (!_cursor.Eof && Character.IsHexDigit(_cursor.Peek()))
                            {
                                _cursor.Advance();
                                if (!_cursor.Eof && Character.IsHexDigit(_cursor.Peek()))
                                {
                                    _cursor.Advance();
                                    if (!_cursor.Eof && Character.IsHexDigit(_cursor.Peek()))
                                    {
                                        _cursor.Advance();
                                        isValidUnicode = true;
                                    }
                                }
                            }

                            if (!isValidUnicode)
                            {
                                _cursor.RollbackLocation();

                                return false;
                            }

                            break;
                        case 'x':
                            bool isValidHex = false;

                            _cursor.Advance();

                            if (!_cursor.Eof && Character.IsHexDigit(_cursor.Peek()))
                            {
                                _cursor.Advance();
                                if (!_cursor.Eof && Character.IsHexDigit(_cursor.Peek()))
                                {
                                    isValidHex = true;
                                }
                            }

                            if (!isValidHex)
                            {
                                _cursor.RollbackLocation();
                                
                                return false;
                            }

                            break;
                        default:
                            _cursor.RollbackLocation();

                            return false;
                    }
                }

                _cursor.Advance();
            }

            _cursor.Advance();

            _cursor.CommitLocation();
            
            EmitToken("string", start + 1, _cursor.Offset - start - 2);

            return true;
        }
    }
}
