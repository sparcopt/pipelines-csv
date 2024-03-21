namespace Parser;

using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public static class StreamProcessor
{
    public static async Task<Dictionary<long, UserSales>> AggregateUserData(Stream input)
    {
        var sales = new Dictionary<long, UserSales>();
        await foreach (var line in ReadAllLinesAsync(input).Skip(1))
        {
            var fields = line.Split(',');
            var uid = long.Parse(fields[0]);
            var quantity = int.Parse(fields[2]);
            var price = decimal.Parse(fields[3]);

            if (!sales.TryGetValue(uid, out var stats))
            {
                sales[uid] = stats = new UserSales();
            }
        
            stats.Total += price;
            stats.Quantity += quantity;
        }

        return sales;
    }
    
    public static async Task<Dictionary<long, UserSales>> AggregateUserDataWithMemory(Stream input)
    {
        var sales = new Dictionary<long, UserSales>();
        await foreach (var line in ReadAllLinesAsync(input).Skip(1))
        {
            var readOnlyMemory = line.AsMemory();
            var startIndex = 0;
            var index = line.IndexOf(',');
            
            var slice = readOnlyMemory.Slice(startIndex, index - startIndex);
            var uid = long.Parse(slice.Span);
            
            startIndex = line.IndexOf(',', line.IndexOf(',') + 1) + 1;
            index = readOnlyMemory.Slice(startIndex).Span.IndexOf(',');
            slice = readOnlyMemory.Slice(startIndex, index);
            var quantity = int.Parse(slice.Span);
            
            startIndex += index + 1;
            index = readOnlyMemory.Slice(startIndex).Span.IndexOf(',');
            slice = readOnlyMemory.Slice(startIndex, index);
            var price = decimal.Parse(slice.Span);

            if (!sales.TryGetValue(uid, out var stats))
            {
                sales[uid] = stats = new UserSales();
            }
        
            stats.Total += price;
            stats.Quantity += quantity;
        }

        return sales;
    }
    
    public static async Task<Dictionary<long, UserSalesStruct>> AggregateUserDataWithSpan(Stream input)
    {
        using var gzipStream = new GZipStream(input, CompressionMode.Decompress);
        using var reader = new StreamReader(gzipStream);
        
        var isHeaderLine = false;
        var salesData = new Dictionary<long, UserSalesStruct>();
        
        while (true)
        {
            var line = await reader.ReadLineAsync();
            if (line is null)
            {
                break;
            }

            if (!isHeaderLine)
            {
                isHeaderLine = true;
                continue;
            }
            
            ProcessSingleLine(line, salesData);
        }

        return salesData;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ProcessSingleLine(string line, Dictionary<long, UserSalesStruct> salesData)
    {
        var span = line.AsSpan();
        var startIndex = 0;
        var index = line.IndexOf(',');
            
        var slice = span.Slice(startIndex, index - startIndex);
        var userId = long.Parse(slice);
            
        startIndex = line.IndexOf(',', line.IndexOf(',') + 1) + 1;
        index = span.Slice(startIndex).IndexOf(',');
        slice = span.Slice(startIndex, index);
        var quantity = int.Parse(slice);
            
        startIndex += index + 1;
        index = span.Slice(startIndex).IndexOf(',');
        slice = span.Slice(startIndex, index);
        var price = decimal.Parse(slice);
        
        ref var current = ref CollectionsMarshal.GetValueRefOrAddDefault(salesData, userId, out _);
        current.Total += price;
        current.Quantity += quantity;
    } 
    
    private static async IAsyncEnumerable<string> ReadAllLinesAsync(Stream input)
    {
        using var gzipStream = new GZipStream(input, CompressionMode.Decompress);
        using var reader = new StreamReader(gzipStream);

        while (true)
        {
            var line = await reader.ReadLineAsync();
            if (line is null)
            {
                break;
            }

            yield return line;
        }
    }
}