using BenchmarkDotNet.Attributes;
using System.Threading.Tasks;

namespace Shortcodes.Benchmarks
{
    [MemoryDiagnoser]
    [ShortRunJob]
    public class RenderBenchmarks
    {
        private NamedShortcodeProvider _provider;

        private ShortcodesProcessor _processor;

        public RenderBenchmarks()
        {
            _provider = new NamedShortcodeProvider
            {
                ["upper"] = (args, content, ctx) => new ValueTask<string>(content.ToUpperInvariant()),
            };

            _processor = new ShortcodesProcessor(_provider);
        }

        [Benchmark]
        public async Task<string> Nop() => await _processor.EvaluateAsync("Lorem ipsum dolor est");

        [Benchmark]
        public async Task<string> Upper() => await _processor.EvaluateAsync("Lorem [upper]ipsum[/upper] dolor est");

        [Benchmark]
        public async Task<string> Unkown() => await _processor.EvaluateAsync("Lorem [lower]ipsum[/lower] dolor est");

        [Benchmark]
        public async Task<string> Big() => await _processor.EvaluateAsync("Lorem [upper]ipsum[/upper] dolor est Lorem [upper] Lorem [upper]ipsum[/upper] dolor est [/upper] dolor est Lorem [upper]ipsum[/upper] dolor est Lorem [upper] Lorem [upper]ipsum[/upper] dolor est [/upper] dolor est Lorem ipsum dolor est Lorem ipsum dolor est Lorem ipsum dolor est Lorem ipsum dolor est Lorem ipsum dolor est Lorem ipsum dolor est Lorem ipsum dolor est Lorem ipsum dolor est ");

    }
}
