using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Parser;

// var filePath = @"C:\Repos\pipelines-csv\data.csv.gz";
// using var streamReader = new StreamReader(filePath);
// var a = await StreamProcessor.AggregateUserDataWithSpan(streamReader.BaseStream);
//
// Console.WriteLine("end");

var config = DefaultConfig.Instance
    .WithSummaryStyle(SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend))
    .HideColumns(Column.RatioSD);

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);


