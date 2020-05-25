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
