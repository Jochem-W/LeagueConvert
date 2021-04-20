using System;
using System.IO;
using System.Threading.Tasks;

namespace LeagueConvert.Helpers
{
    public static class VersionConverter
    {
        public static async Task<Stream> Version3ToVersion2(this Stream stream)
        {
            // check for PropertyBin
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            await stream.DisposeAsync();
            memoryStream.Position = 4;
            var versionBuffer = new byte[1];
            await memoryStream.ReadAsync(versionBuffer.AsMemory(0, 1));
            if (versionBuffer[0] == 3)
            {
                memoryStream.Position = 4;
                await memoryStream.WriteAsync(new byte[] {2}.AsMemory(0, 1));
            }

            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}