using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shortcodes
{
    public class NamedShortcodeProvider : IShortcodeProvider, IEnumerable
    {
        private static readonly ValueTask<string> Null = new ValueTask<string>((string)null);

        private Dictionary<string, ShortcodeDelegate> Shortcodes { get; }

        public NamedShortcodeProvider()
        {
            Shortcodes = new Dictionary<string, ShortcodeDelegate>(StringComparer.OrdinalIgnoreCase);
        }

        public NamedShortcodeProvider(Dictionary<string, ShortcodeDelegate> shortcodes)
        {
            Shortcodes = new Dictionary<string, ShortcodeDelegate>(shortcodes, StringComparer.OrdinalIgnoreCase);
        }

        public bool Contains(string shortcode)
        {
            return Shortcodes.ContainsKey(shortcode);
        }

        public ShortcodeDelegate this[string shortcode]
        {
            get { return Shortcodes[shortcode]; }
            set { Shortcodes[shortcode] = value; }
        }

        public ValueTask<string> EvaluateAsync(string identifier, Arguments arguments, string content)
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

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)Shortcodes).GetEnumerator();
        }
    }
}
