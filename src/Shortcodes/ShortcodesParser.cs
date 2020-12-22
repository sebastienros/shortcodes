using System;
using System.Collections.Generic;
using Parlot;

namespace Shortcodes
{
    /*
     * nodes        => ( shortcode | TEXT )*
     * shortcode    => '['+ identifier (arguments)* ']'+
     *               | '['+ '/' identifier ']'+
     * arguments    => identifer '=' literal
     * literal      => STRING | NUMBER 
     */
    public class ShortcodesParser : Parser<IEnumerable<Node>>
    {
        private ShortcodesScanner _scanner;

        public override IEnumerable<Node> Parse(string text)
        {
            _scanner = new ShortcodesScanner(text);

            return ParseNodes();
        }

        private IEnumerable<Node> ParseNodes()
        {
            while (!_scanner.Cursor.Eof)
            {
                var shortcode = ParseShortcode();

                if (shortcode != null)
                {
                    yield return shortcode;
                }
                else
                {
                    var rawText = ParseRawText();

                    if (rawText == null)
                    {
                        throw new ParseException("Text didn't match any expected sequence.", _scanner.Cursor.Position);
                    }

                    yield return rawText;
                }
            }
        }

        private RawText ParseRawText()
        {
            if (_scanner.ReadRawText(out var rawText))
            {
                return new RawText(_scanner.Buffer, rawText.Start.Offset, rawText.Span.Length);
            }

            return null;
        }

        private Shortcode ParseShortcode()
        {
            // Number of opening braces
            var openBraces = 0;

            // Number of closing braces
            var closeBraces = 0;

            Shortcode shortcode;

            var style = ShortcodeStyle.Open;

            if (!_scanner.Cursor.Match("["))
            {
                return null;
            }

            _scanner.Cursor.RecordPosition();

            // Start position of the shortcode
            var start = _scanner.Cursor.Position;

            // Read all '[' so we can detect escaped tags
            do
            {
                openBraces += 1;
                _scanner.Cursor.Advance();
            } while (_scanner.Cursor.Match("["));

            // Is it a closing tag?
            if (_scanner.Cursor.Match("/"))
            {
                style = ShortcodeStyle.Close;

                _scanner.Cursor.Advance();
            }

            // Reach Eof before end of shortcode
            if (_scanner.Cursor.Eof)
            {
                _scanner.Cursor.RollbackPosition();

                return null;
            }

            _scanner.SkipWhiteSpace();

            if (!_scanner.ReadIdentifier(out var identifier))
            {
                _scanner.Cursor.RollbackPosition();

                return null;
            }

            _scanner.SkipWhiteSpace();

            Dictionary<string, string> arguments = null;

            int argumentIndex = 0;

            // Arguments?
            while (!_scanner.Cursor.Eof && !_scanner.Cursor.Match(']'))
            {
                // Record location in case it doesn't have a value
                _scanner.Cursor.RecordPosition();

                if (_scanner.ReadQuotedString(out var resultString))
                {
                    arguments ??= CreateArgumentsDictionary();

                    arguments[argumentIndex.ToString()] = Character.DecodeString(resultString.Span[1..^1]).ToString();

                    argumentIndex += 1;
                }
                else if (_scanner.ReadIdentifier(out var argIdentifier))
                {
                    _scanner.SkipWhiteSpace();

                    var argumentName = argIdentifier.Span.ToString();

                    // It might just be a value
                    if (_scanner.ReadText("=", out _))
                    {
                        _scanner.SkipWhiteSpace();

                        if (_scanner.ReadQuotedString(out var stringValue))
                        {
                            arguments ??= CreateArgumentsDictionary();

                            arguments[argumentName] = Character.DecodeString(stringValue.Span[1..^1]).ToString();
                        }
                        else if (_scanner.ReadValue(out var otherValue))
                        {
                            arguments ??= CreateArgumentsDictionary();

                            arguments[argumentName] = otherValue.Span.ToString();
                        }
                        else
                        {
                            // Pop position twice
                            _scanner.Cursor.RollbackPosition();
                            _scanner.Cursor.RollbackPosition();

                            return null;
                        }
                    }
                    else
                    {
                        // Positional argument that looks like an identifier

                        _scanner.Cursor.RollbackPosition();

                        if (_scanner.ReadValue(out var value))
                        {
                            arguments ??= CreateArgumentsDictionary();

                            arguments[argumentIndex.ToString()] = value.Span.ToString();

                            argumentIndex += 1;
                        }
                        else
                        {
                            _scanner.Cursor.RollbackPosition();

                            break;
                        }
                    }
                }
                else if (_scanner.ReadValue(out var value))
                {
                    _scanner.Cursor.CommitPosition();

                    arguments ??= CreateArgumentsDictionary();

                    arguments[argumentIndex.ToString()] = value.Span.ToString();

                    argumentIndex += 1;
                }
                else
                {
                    _scanner.Cursor.RollbackPosition();
                    
                    break;
                }

                _scanner.SkipWhiteSpace();
            }

            // Is it a self-closing tag?
            if (_scanner.Cursor.Match("/]"))
            {
                style = ShortcodeStyle.SelfClosing;

                _scanner.Cursor.Advance();
            }

            // Expect closing bracket
            if (!_scanner.Cursor.Match("]"))
            {
                _scanner.Cursor.RollbackPosition();

                return null;
            }

            // Read all ']' so we can detect escaped tags
            do
            {
                closeBraces += 1;
                _scanner.Cursor.Advance();
            } while (_scanner.Cursor.Match("]"));

            shortcode = new Shortcode(identifier.Span.ToString(), style, openBraces, closeBraces, start.Offset, _scanner.Cursor.Position - start - 1);
            shortcode.Arguments = new Arguments(arguments);

            _scanner.Cursor.CommitPosition();

            return shortcode;

            // Local function to use the same logic to create the arguments dictionary
            static Dictionary<string, string> CreateArgumentsDictionary()
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
