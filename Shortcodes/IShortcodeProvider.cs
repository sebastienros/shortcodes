using System.Threading.Tasks;

namespace Shortcodes
{
    public delegate ValueTask<string> ShortcodeDelegate(Arguments arguments, string content);

    public interface IShortcodeProvider
    {
        ValueTask<string> EvaluateAsync(string identifier, Arguments arguments, string content);
    }
}
