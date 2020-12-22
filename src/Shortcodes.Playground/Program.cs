using System;
using System.Threading.Tasks;

namespace Shortcodes.Playground
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var provider = new NamedShortcodeProvider
            {
                ["upper"] = (args, content, ctx) => new ValueTask<string>(content.ToUpperInvariant()),
            };

            var processor = new ShortcodesProcessor(provider);

            for (var i = 0; i < 100; i++)
            {
                Console.WriteLine(await processor.EvaluateAsync("Lorem [upper]ipsum[/upper] dolor est"));
            }
        }
    }
}
