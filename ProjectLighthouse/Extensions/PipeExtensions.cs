using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace LBPUnion.ProjectLighthouse.Extensions;

public static class PipeExtensions
{

    public static async Task<byte[]> ReadAllAsync(this PipeReader reader)
    {
        do
        {
            ReadResult readResult = await reader.ReadAsync();
            if (readResult.IsCompleted || readResult.IsCanceled)
            {
                return readResult.Buffer.ToArray();
            }

            // consume nothing, keep reading from the pipe reader until all data is there
            reader.AdvanceTo(readResult.Buffer.Start, readResult.Buffer.End);
        }
        while (true);
    }

}