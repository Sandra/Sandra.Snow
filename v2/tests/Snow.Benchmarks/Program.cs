using BenchmarkDotNet.Running;

namespace Snow.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        // Run all benchmarks
        BenchmarkRunner.Run(typeof(Program).Assembly);
    }
}
