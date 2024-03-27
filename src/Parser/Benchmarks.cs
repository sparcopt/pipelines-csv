namespace Parser;

using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class Benchmarks
{
    private string filePath = Directory.GetTestDataFilePath();
    
    [Benchmark(Baseline = true)]
    public async Task<Dictionary<long, UserSales>> Stream_Parse()
    {
        using var streamReader = new StreamReader(filePath);
        return await StreamProcessor.AggregateUserData(streamReader.BaseStream);
    }

    [Benchmark]
    public async Task<Dictionary<long, UserSales>> Stream_ParseAsMemory()
    {
        using var streamReader = new StreamReader(filePath);
        return await StreamProcessor.AggregateUserDataWithMemory(streamReader.BaseStream);
    }
    
    [Benchmark]
    public async Task<Dictionary<long, UserSalesStruct>> Stream_ParseAsSpan()
    {
        using var streamReader = new StreamReader(filePath);
        return await StreamProcessor.AggregateUserDataWithSpan(streamReader.BaseStream);
    }
    
    [Benchmark]
    public async Task<Dictionary<long, UserSalesStruct>> Utf8Stream_ParseAsSpan()
    {
        using var streamReader = new StreamReader(filePath);
        return await Utf8StreamProcessor.AggregateUserData(streamReader.BaseStream);
    }
    
    [Benchmark]
    public async Task<Dictionary<long, UserSalesStruct>> Pipeline_Utf8Parse()
    {
        using var streamReader = new StreamReader(filePath);
        return await PipelineProcessor.AggregateUserData(streamReader.BaseStream);
    }
}