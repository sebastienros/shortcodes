using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shortcodes
{
    public delegate ValueTask<string> ShortcodeDelegate(Dictionary<string, string> arguments, string content);

    public interface IShortcodeProvider
    {
        ValueTask<string> EvaluateAsync(string identifier, Dictionary<string, string> arguments, string content);
    }
}
