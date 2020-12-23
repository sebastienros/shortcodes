﻿using System;
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
    public class ShortcodesParser : Parser<List<Node>>
    {
        private ShortcodesScanner _scanner;

        public override List<Node> Parse(string text)
        {
            _scanner = new ShortcodesScanner(text);

            return ParseNodes();
        }

        private List<Node> ParseNodes()
        {
            var nodes = new List<Node>(8);

            while (!_scanner.Cursor.Eof)
            {
                var shortcode = ParseShortcode();

                if (shortcode != null)
                {
                    nodes.Add(shortcode);
                }
                else
                {
                    var rawText = ParseRawText();

                    if (rawText == null)
                    {
                        throw new ParseException("Text didn't match any expected sequence.", _scanner.Cursor.Position);
                    }

                    nodes.Add(rawText);
                }
            }

            return nodes;
        }

        private RawText ParseRawText()
        {
            if (_scanner.ReadRawText(out var rawText))
            {
                return new RawText(_scanner.Buffer, rawText.Start.Offset, rawText.Length);
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

            if (!_scanner.Cursor.Match('['))
            {
                return null;
            }

            // Start position of the shortcode
            var start = _scanner.Cursor.Position;

            // Read all '[' so we can detect escaped tags
            do
            {
                openBraces += 1;
                _scanner.Cursor.Advance();
            } while (_scanner.Cursor.Match('['));

            // Is it a closing tag?
            if (_scanner.Cursor.Match('/'))
            {
                style = ShortcodeStyle.Close;

                _scanner.Cursor.Advance();
            }

            // Reach Eof before end of shortcode
            if (_scanner.Cursor.Eof)
            {
                _scanner.Cursor.ResetPosition(start);

                return null;
            }

            _scanner.SkipWhiteSpace();

            if (!_scanner.ReadIdentifier(out var identifier))
            {
                _scanner.Cursor.ResetPosition(start);

                return null;
            }

            _scanner.SkipWhiteSpace();

            Dictionary<string, string> arguments = null;

            int argumentIndex = 0;

            // Arguments?
            while (!_scanner.Cursor.Eof && !_scanner.Cursor.Match(']'))
            {
                // Record location in case it doesn't have a value
                var argumentStart = _scanner.Cursor.Position;

                if (_scanner.ReadQuotedString(out var resultString))
                {
                    arguments ??= CreateArgumentsDictionary();

                    arguments[argumentIndex.ToString()] = Character.DecodeString(resultString.Buffer.Substring(resultString.Start.Offset + 1, resultString.Length - 2));

                    argumentIndex += 1;
                }
                else if (_scanner.ReadIdentifier(out var argIdentifier))
                {
                    _scanner.SkipWhiteSpace();

                    var argumentName = argIdentifier.Text;

                    // It might just be a value
                    if (_scanner.ReadText("=", out _))
                    {
                        _scanner.SkipWhiteSpace();

                        if (_scanner.ReadQuotedString(out var stringValue))
                        {
                            arguments ??= CreateArgumentsDictionary();

                            arguments[argumentName] = Character.DecodeString(stringValue.Buffer.Substring(stringValue.Start.Offset + 1, stringValue.Length - 2));
                        }
                        else if (_scanner.ReadValue(out var otherValue))
                        {
                            arguments ??= CreateArgumentsDictionary();

                            arguments[argumentName] = otherValue.Text.ToString();
                        }
                        else
                        {
                            _scanner.Cursor.ResetPosition(start);

                            return null;
                        }
                    }
                    else
                    {
                        // Positional argument that looks like an identifier

                        _scanner.Cursor.ResetPosition(argumentStart);

                        if (_scanner.ReadValue(out var value))
                        {
                            arguments ??= CreateArgumentsDictionary();

                            arguments[argumentIndex.ToString()] = value.Text;

                            argumentIndex += 1;
                        }
                        else
                        {
                            _scanner.Cursor.ResetPosition(start);

                            break;
                        }
                    }
                }
                else if (_scanner.ReadValue(out var value))
                {
                    arguments ??= CreateArgumentsDictionary();

                    arguments[argumentIndex.ToString()] = value.Text;

                    argumentIndex += 1;
                }
                else
                {
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
            if (!_scanner.Cursor.Match(']'))
            {
                _scanner.Cursor.ResetPosition(start);

                return null;
            }

            // Read all ']' so we can detect escaped tags
            do
            {
                closeBraces += 1;
                _scanner.Cursor.Advance();
            } while (_scanner.Cursor.Match(']'));

            shortcode = new Shortcode(identifier.Text, style, openBraces, closeBraces, start.Offset, _scanner.Cursor.Position - start - 1);
            shortcode.Arguments = new Arguments(arguments);

            return shortcode;

            // Local function to use the same logic to create the arguments dictionary
            static Dictionary<string, string> CreateArgumentsDictionary()
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
