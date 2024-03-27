namespace Parser;

using Cysharp.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class Utf8StreamProcessor
{
    public static async Task<Dictionary<long, UserSalesStruct>> AggregateUserData(Stream input)
    {
        using var gzipStream = new GZipStream(input, CompressionMode.Decompress);
        using var reader = new Utf8StreamReader(gzipStream);
        
        var isHeaderLine = false;
        var salesData = new Dictionary<long, UserSalesStruct>();
        
        while (await reader.LoadIntoBufferAsync())
        {
            while (reader.TryReadLine(out var line))
            {
                if (!isHeaderLine)
                {
                    isHeaderLine = true;
                    continue;
                }

                ProcessSingleLine(line.Span, salesData);
            }
        }

        return salesData;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ProcessSingleLine(ReadOnlySpan<byte> line, Dictionary<long, UserSalesStruct> salesData)
    {
        var startIndex = 0;
        var index = line.IndexOf((byte)',');
            
        var slice = line.Slice(startIndex, index - startIndex);
        var userId = long.Parse(slice);

        // skip current field and move forward to the next quantity field
        var skipCommaIndex = line.Slice(line.IndexOf((byte)',') + 1).IndexOf((byte)',') + 1;
        startIndex = index + 1 + skipCommaIndex;
        
        index = line.Slice(startIndex).IndexOf((byte)',');
        slice = line.Slice(startIndex, index);
        var quantity = int.Parse(slice);
            
        startIndex += index + 1;
        index = line.Slice(startIndex).IndexOf((byte)',');
        slice = line.Slice(startIndex, index);
        var price = decimal.Parse(slice);
        
        ref var current = ref CollectionsMarshal.GetValueRefOrAddDefault(salesData, userId, out _);
        current.Total += price;
        current.Quantity += quantity;
    } 
}