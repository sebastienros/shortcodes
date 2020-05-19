using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shortcodes
{
    public class NamedShortcodeProvider : IShortcodeProvider
    {
        private static readonly ValueTask<string> Null = new ValueTask<string>((string)null);

        public Dictionary<string, ShortcodeDelegate> Shortcodes { get; set; } = new Dictionary<string, ShortcodeDelegate>(StringComparer.OrdinalIgnoreCase);

        public NamedShortcodeProvider()
        {
        }

        public ValueTask<string> EvaluateAsync(string identifier, Dictionary<string, string> arguments, string content)
        {
            if (Shortcodes.TryGetValue(identifier, out var shortcode))
            {
                if (shortcode == null)
                {
                    return Null;
                }

                return shortcode.Invoke(arguments, content);
            }

            return Null;
        }
    }
}
