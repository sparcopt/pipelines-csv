using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Parser;
using Directory = Parser.Directory;

#if DEBUG

var filePath = Directory.GetTestDataFilePath();
using var streamReader = new StreamReader(filePath);
var data = await Utf8StreamProcessor.AggregateUserData(streamReader.BaseStream);

#else

var config = DefaultConfig.Instance
    .WithSummaryStyle(SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend))
    .HideColumns(Column.RatioSD);

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);

#endif
