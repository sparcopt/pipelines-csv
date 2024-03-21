namespace Parser;

using System.Buffers;
using System.Buffers.Text;
using System.IO.Compression;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

public static class PipelineProcessor
{
    private const int BufferSize = 64 * 1024;
    
    public static async ValueTask<Dictionary<long, UserSalesStruct>> AggregateUserData(Stream input)
    {
        await using var gzipStream = new GZipStream(input, CompressionMode.Decompress);
        var pipeReader = PipeReader.Create(gzipStream, new StreamPipeReaderOptions(bufferSize: BufferSize));

        var isHeaderLine = false;
        var salesData = new Dictionary<long, UserSalesStruct>();

        while (true)
        {
            var readResult = await pipeReader.ReadAsync().AsTask();
            ProcessLines(pipeReader, readResult, salesData, ref isHeaderLine);
            if (readResult.IsCompleted)
            {
                break;
            }
        }

        return salesData;
    }

    private static void ProcessLines(PipeReader pipeReader, ReadResult readResult, Dictionary<long, UserSalesStruct> salesData, ref bool isHeaderLine)
    {
        var sequenceReader = new SequenceReader<byte>(readResult.Buffer);
        while (true)
        {
            if (sequenceReader.TryReadTo(out ReadOnlySequence<byte> line, (byte)'\n') == false)
            {
                pipeReader.AdvanceTo(consumed: sequenceReader.Position, examined: readResult.Buffer.End);
                break;
            }

            if (!isHeaderLine)
            {
                isHeaderLine = true;
                continue;
            }

            ProcessSingleLine(salesData, line);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ProcessSingleLine(Dictionary<long, UserSalesStruct> salesData, ReadOnlySequence<byte> line)
    {
        var lineReader = new SequenceReader<byte>(line);
        var readAll =
            lineReader.TryReadTo(out ReadOnlySpan<byte> span, (byte)',') &
            Utf8Parser.TryParse(span, out long userId, out _) &
            lineReader.TryAdvanceTo((byte)',') &
            lineReader.TryReadTo(out span, (byte)',') &
            Utf8Parser.TryParse(span, out int quantity, out _) &
            lineReader.TryReadTo(out span, (byte)',') &
            Utf8Parser.TryParse(span, out decimal price, out _);

        if (!readAll)
        {
            throw new InvalidDataException("Couldn't parse expected fields on: " + Encoding.UTF8.GetString(line));
        }

        ref var current = ref CollectionsMarshal.GetValueRefOrAddDefault(salesData, userId, out _);
        current.Total += price;
        current.Quantity += quantity;
    }
}