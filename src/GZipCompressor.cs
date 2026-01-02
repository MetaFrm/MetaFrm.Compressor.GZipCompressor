using System.IO.Compression;
using System.Text;

namespace MetaFrm.Compressor
{
    /// <summary>
    /// GZipCompressor
    /// </summary>
    public class GZipCompressor : ICompressor
    {
        static readonly System.Text.Json.JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = false };
        byte[] ICompressor.Compress(byte[] source)
        {
            using MemoryStream memoryStream = new();
            using (GZipStream gzip = new(memoryStream, CompressionMode.Compress, true))
            {
                gzip.Write(source);
                gzip.Flush();// 의도 표현
            }

            return memoryStream.ToArray();
        }
        byte[] ICompressor.Compress<TValue>(TValue source)
        {
            return ((ICompressor)this).Compress(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(source, JsonSerializerOptions));
        }
        string ICompressor.CompressToString<TValue>(TValue source)
        {
            return Convert.ToBase64String(((ICompressor)this).Compress(source));
        }
        string ICompressor.CompressToString(string source)
        {
            return Convert.ToBase64String(((ICompressor)this).Compress(Encoding.UTF8.GetBytes(source)));
        }

        async Task<byte[]> ICompressor.CompressAsync(byte[] source)
        {
            using MemoryStream memoryStream = new();
            using (GZipStream gzip = new(memoryStream, CompressionMode.Compress, true))
            {
                await gzip.WriteAsync(source.AsMemory(0));
                gzip.Flush();// 의도 표현
            }

            return memoryStream.ToArray();
        }
        async Task<byte[]> ICompressor.CompressAsync<TValue>(TValue source)
        {
            return await ((ICompressor)this).CompressAsync(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(source, JsonSerializerOptions));
        }
        async Task<string> ICompressor.CompressToStringAsync<TValue>(TValue source)
        {
            return Convert.ToBase64String(await ((ICompressor)this).CompressAsync(source));
        }
        async Task<string> ICompressor.CompressToStringAsync(string source)
        {
            return Convert.ToBase64String(await ((ICompressor)this).CompressAsync(Encoding.UTF8.GetBytes(source)));
        }


        byte[] ICompressor.Decompress(byte[] source)
        {
            byte[] buffer = new byte[8192];
            int read;

            using MemoryStream sourceMemoryStream = new(source);
            using MemoryStream resultMemoryStream = new();
            using (GZipStream gzip = new(sourceMemoryStream, CompressionMode.Decompress))
            {
                while ((read = gzip.Read(buffer, 0, buffer.Length)) > 0)
                    resultMemoryStream.Write(buffer, 0, read);
            }

            return resultMemoryStream.ToArray();
        }
        TValue ICompressor.Decompress<TValue>(byte[] source)
        {
            TValue? value = System.Text.Json.JsonSerializer.Deserialize<TValue>(
                ((ICompressor)this).Decompress(source), JsonSerializerOptions);

            if (value != null)
                return value;
            else
                throw new MetaFrmException($"Failed to deserialize {typeof(TValue).FullName}");
        }
        TValue ICompressor.DecompressFromString<TValue>(string source)
        {
            return ((ICompressor)this).Decompress<TValue>(Convert.FromBase64String(source));
        }
        string ICompressor.DecompressFromString(string source)
        {
            return Encoding.UTF8.GetString(((ICompressor)this).Decompress(Convert.FromBase64String(source)));
        }

        async Task<byte[]> ICompressor.DecompressAsync(byte[] source)
        {
            byte[] buffer = new byte[8192];
            int read;

            using MemoryStream sourceMemoryStream = new(source);
            using MemoryStream resultMemoryStream = new();
            using (GZipStream gzip = new(sourceMemoryStream, CompressionMode.Decompress))
            {
                while ((read = await gzip.ReadAsync(buffer.AsMemory())) > 0)
                    await resultMemoryStream.WriteAsync(buffer.AsMemory(0, read));
            }

            return resultMemoryStream.ToArray();
        }
        async Task<TValue> ICompressor.DecompressAsync<TValue>(byte[] source)
        {
            TValue? value = System.Text.Json.JsonSerializer.Deserialize<TValue>(
                await ((ICompressor)this).DecompressAsync(source), JsonSerializerOptions);

            if (value != null)
                return value;
            else
                throw new MetaFrmException($"Failed to deserialize {typeof(TValue).FullName}");
        }
        async Task<TValue> ICompressor.DecompressFromStringAsync<TValue>(string source)
        {
            return await ((ICompressor)this).DecompressAsync<TValue>(Convert.FromBase64String(source));
        }
        async Task<string> ICompressor.DecompressFromStringAsync(string source)
        {
            return Encoding.UTF8.GetString(await ((ICompressor)this).DecompressAsync(Convert.FromBase64String(source)));
        }
    }
}