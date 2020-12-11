using BenchmarkDotNet.Running;

namespace Shortcodes.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<RenderBenchmarks>();
        }
    }
}
