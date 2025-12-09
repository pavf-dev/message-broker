using BenchmarkDotNet.Running;
using MessageBroker.PerformanceTests;

BenchmarkRunner.Run<MessageDispatcherBenchmark>();
