using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;

namespace BenchmarkSuite1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var config = ManualConfig.CreateEmpty();
            var _ = BenchmarkRunner.Run(typeof(Program).Assembly, config);
        }
    }
}
