using System.Threading.Tasks;

namespace Shortcodes
{
    /// <summary>
    /// Delegate evaluated when a shortcode has matched.
    /// </summary>
    /// <param name="arguments">The arguments passed with the shortcode.</param>
    /// <param name="content">The inner content of the shortcode.<code>null</code> for self-closing shortcodes.</param>
    /// <param name="context">Custom properties used to evaluate a template.</param>
    /// <returns>The string to substitue the shortcode with.</returns>
    public delegate ValueTask<string> ShortcodeDelegate(Arguments arguments, string content, Context context);

    public interface IShortcodeProvider
    {
        /// <summary>
        /// Evaluates a named shortcode.
        /// </summary>
        /// <param name="identifier">The name of the shortcode to evaluate.</param>
        /// <param name="arguments">The arguments passed with the shortcode.</param>
        /// <param name="content">The inner content of the shortcode.<code>null</code> for self-closing shortcodes.</param>
        /// <param name="context">Custom properties used to evaluate a template.</param>
        /// <returns>The string to substitue the shortcode with.</returns>
        ValueTask<string> EvaluateAsync(string identifier, Arguments arguments, string content, Context context);
    }
}
