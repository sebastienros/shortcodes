﻿using System.Text;

namespace Shortcodes
{
    public class Character
    {
        public static bool IsDecimalDigit(char cp)
        {
            return (cp >= '0' && cp <= '9');
        }

        public static bool IsHexDigit(char cp)
        {
            return (cp >= '0' && cp <= '9') ||
                (cp >= 'A' && cp <= 'F') ||
                (cp >= 'a' && cp <= 'f');
        }

        public static bool IsIdentifierStart(char ch)
        {
            return (ch == '$') || (ch == '_') ||
                   (ch >= 'A' && ch <= 'Z') ||
                   (ch >= 'a' && ch <= 'z');
        }

        public static bool IsIdentifierPart(char ch)
        {
            return (ch == '$') || (ch == '_') ||
                   (ch >= 'A' && ch <= 'Z') ||
                   (ch >= 'a' && ch <= 'z') ||
                   (ch >= '0' && ch <= '9');
        }

        public static bool IsWhiteSpace(char ch)
        {
            return (ch == 32) || // space
                   (ch == 9) || // tab
                   (ch == 0xB) ||
                   (ch == 0xC) ||
                   (ch == 0xA0) ||
                   (ch >= 0x1680 && (
                                        ch == 0x1680 ||
                                        ch == 0x180E ||
                                        (ch >= 0x2000 && ch <= 0x200A) ||
                                        ch == 0x202F ||
                                        ch == 0x205F ||
                                        ch == 0x3000 ||
                                        ch == 0xFEFF));
        }

        public static char ScanHexEscape(string text, int index)
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

        public static string DecodeString(Token token)
        {
            return DecodeString(token.Text, token.StartIndex, token.Length);
        }

        public static string DecodeString(string text, int startIndex, int count)
        {
            // Nothing to do if the string doesn't have any escape char
            if (text.IndexOf('\\', startIndex, count) == -1)
            {
                return text.Substring(startIndex, count);
            }

            // The asumption is that the new string will be shorted since most escapes are smaller
            var sb = new StringBuilder(count);

            var endIndex = startIndex + count;

            for (var i = startIndex; i < endIndex; i++)
            {
                var c = text[i];

                if (c == '\\')
                {
                    i++;
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
                            sb.Append(Character.ScanHexEscape(text, i));
                            i += 4;
                            break;
                        case 'x':
                            sb.Append(Character.ScanHexEscape(text, i));
                            i += 2;
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
    }
}
