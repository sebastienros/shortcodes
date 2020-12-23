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
        private readonly TokenResult result = new TokenResult();

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
            if (_scanner.ReadRawText(result))
            {
                return new RawText(_scanner.Buffer, result.Token.Start.Offset, result.Token.Length);
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

            // Start position of the shortcode
            var start = _scanner.Cursor.Position;

            if (!_scanner.ReadChar('['))
            {
                return null;
            }

            openBraces += 1;

            // Read all '[' so we can detect escaped tags
            while (_scanner.ReadChar('['))
            {
                openBraces += 1;
            }

            // Is it a closing tag?
            if (_scanner.ReadChar('/'))
            {
                style = ShortcodeStyle.Close;
            }

            // Reach Eof before end of shortcode
            if (_scanner.Cursor.Eof)
            {
                _scanner.Cursor.ResetPosition(start);

                return null;
            }

            _scanner.SkipWhiteSpace();

            if (!_scanner.ReadIdentifier(result))
            {
                _scanner.Cursor.ResetPosition(start);

                return null;
            }

            var identifier = result.Token;

            _scanner.SkipWhiteSpace();

            Dictionary<string, string> arguments = null;

            int argumentIndex = 0;

            // Arguments?
            while (!_scanner.Cursor.Eof)
            {
                // Record location in case it doesn't have a value
                var argumentStart = _scanner.Cursor.Position;

                if (_scanner.ReadQuotedString(result))
                {
                    arguments ??= CreateArgumentsDictionary();

                    arguments[argumentIndex.ToString()] = Character.DecodeString(result.Token.Buffer.Substring(result.Token.Start.Offset + 1, result.Token.Length - 2));

                    argumentIndex += 1;
                }
                else if (_scanner.ReadIdentifier(result))
                {
                    _scanner.SkipWhiteSpace();

                    var argumentName = result.Token.Text;
                    
                    // It might just be a value
                    if (_scanner.ReadChar('='))
                    {
                        _scanner.SkipWhiteSpace();

                        if (_scanner.ReadQuotedString(result))
                        {
                            arguments ??= CreateArgumentsDictionary();

                            arguments[argumentName] = Character.DecodeString(result.Token.Buffer.Substring(result.Token.Start.Offset + 1, result.Token.Length - 2));
                        }
                        else if (_scanner.ReadValue(result))
                        {
                            arguments ??= CreateArgumentsDictionary();

                            arguments[argumentName] = result.Token.Text.ToString();
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

                        if (_scanner.ReadValue(result))
                        {
                            arguments ??= CreateArgumentsDictionary();

                            arguments[argumentIndex.ToString()] = result.Token.Text;

                            argumentIndex += 1;
                        }
                        else
                        {
                            _scanner.Cursor.ResetPosition(start);

                            break;
                        }
                    }
                }
                else if (_scanner.ReadValue(result))
                {
                    arguments ??= CreateArgumentsDictionary();

                    arguments[argumentIndex.ToString()] = result.Token.Text;

                    argumentIndex += 1;
                }
                else if (_scanner.Cursor.Match("/]"))
                {
                    style = ShortcodeStyle.SelfClosing;
                    _scanner.Cursor.Advance();
                    break;
                }
                else if (_scanner.Cursor.Match(']'))
                {
                    break;
                }
                else
                {
                    _scanner.Cursor.ResetPosition(start);
                    return null;
                }

                _scanner.SkipWhiteSpace();
            }

            // If we exited the loop due to EOF, exit
            if (_scanner.Cursor.Eof || !_scanner.ReadChar(']'))
            {
                _scanner.Cursor.ResetPosition(start);
                return null;
            }

            closeBraces += 1;

            // Read all ']' so we can detect escaped tags
            while (_scanner.ReadChar(']'))
            {
                closeBraces += 1;
            } 

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
