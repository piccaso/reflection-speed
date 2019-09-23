using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace ReflectionSpeed {
    internal class Program {
        private static void Main(string[] args) {
            Benchmark<CreateGetterOverhead>();
        }
        private static Summary Benchmark<T>() {
#if DEBUG
            var args = new[] {"--filter", $"*{typeof(T).Name}*"};
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new DebugInProcessConfig());
            return null;
#else
            return BenchmarkRunner.Run<T>();
#endif
        }
    }
}