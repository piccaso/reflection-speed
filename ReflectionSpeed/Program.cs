using BenchmarkDotNet.Running;

namespace ReflectionSpeed {
    internal class Program {
        private static void Main(string[] args) {
            BenchmarkRunner.Run<ClassPropertyBenchmark>();
        }
    }
}