﻿using System.IO.Compression;

namespace LeagueToolkit.Helpers.Compression;

/// <summary>
///     A static class which contains static methods to compress/decompress GZip and ZLib
/// </summary>
public static class Compression
{
    /// <summary>
    ///     Decompresses the specified GZip Data
    /// </summary>
    /// <param name="buffer">Data to decompress</param>
    /// <returns>Decompressed Data</returns>
    public static byte[] DecompressGZip(byte[] buffer)
    {
        using (var decompressedBuffer = new MemoryStream())
        {
            using (var compressedBuffer = new MemoryStream(buffer))
            {
                using (var gzipBuffer = new GZipStream(compressedBuffer, CompressionMode.Decompress))
                {
                    gzipBuffer.CopyTo(decompressedBuffer);
                }
            }

            return decompressedBuffer.ToArray();
        }
    }

    /// <summary>
    ///     Compresses the specified Data
    /// </summary>
    /// <param name="buffer">Data to compress</param>
    /// <returns>Compressed Data</returns>
    public static byte[] CompressGZip(byte[] buffer)
    {
        using (var compressedBuffer = new MemoryStream())
        {
            using (var uncompressedBuffer = new MemoryStream(buffer))
            {
                using (var gzipBuffer = new GZipStream(compressedBuffer, CompressionMode.Compress))
                {
                    uncompressedBuffer.CopyTo(gzipBuffer);
                }
            }

            return compressedBuffer.ToArray();
        }
    }
}