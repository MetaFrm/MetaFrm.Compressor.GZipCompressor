using System.Data;
using System.IO.Compression;
using System.Text;

namespace MetaFrm.Compressor
{
    /// <summary>
    /// GZipCompressor
    /// </summary>
    public class GZipCompressor : ICompress, ICompressAsync, IDecompress, IDecompressAsync
    {
        readonly System.Text.Json.JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true, IncludeFields = true };
        byte[] ICompress.Compress(byte[] source)
        {
            byte[] compressedByte;

            using (MemoryStream memoryStream = new())
            {
                using (Stream stream = new GZipStream(memoryStream, CompressionMode.Compress, true))
                {
                    stream.Write(source, 0, source.Length);
                    stream.Close();
                }

                compressedByte = new byte[memoryStream.Length];

                memoryStream.Position = 0;
                memoryStream.Read(compressedByte, 0, (int)memoryStream.Length);
                memoryStream.Close();
            }

            return compressedByte;
        }
        byte[] ICompress.Compress<TValue>(TValue source)
        {
            //if (source is DataSet set)
            //    set.RemotingFormat = SerializationFormat.Binary;

            return ((ICompress)this).Compress(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(source, this.JsonSerializerOptions));
        }
        string ICompress.CompressToString<TValue>(TValue source)
        {
            return Convert.ToBase64String(((ICompress)this).Compress(source));
        }
        string ICompress.CompressToString(string source)
        {
            byte[] compressedByte;

            compressedByte = ((ICompress)this).Compress(Encoding.Default.GetBytes(source));

            return Convert.ToBase64String(compressedByte);
        }

        async Task<byte[]> ICompressAsync.CompressAsync(byte[] source)
        {
            byte[] compressedByte;

            using (MemoryStream memoryStream = new())
            {
                using (Stream stream = new GZipStream(memoryStream, CompressionMode.Compress, true))
                {
                    await stream.WriteAsync(source.AsMemory(0, source.Length));
                    stream.Close();
                }

                compressedByte = new byte[memoryStream.Length];

                memoryStream.Position = 0;
                await memoryStream.ReadAsync(compressedByte.AsMemory(0, (int)memoryStream.Length));
                memoryStream.Close();
            }

            return compressedByte;
        }
        async Task<byte[]> ICompressAsync.CompressAsync<TValue>(TValue source)
        {
            //if (source is DataSet set)
            //    set.RemotingFormat = SerializationFormat.Binary;

            return await ((ICompressAsync)this).CompressAsync(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(source, this.JsonSerializerOptions));
        }
        async Task<string> ICompressAsync.CompressToStringAsync<TValue>(TValue source)
        {
            byte[] compressedByte;

            compressedByte = await ((ICompressAsync)this).CompressAsync(source);

            return Convert.ToBase64String(compressedByte);
        }
        async Task<string> ICompressAsync.CompressToStringAsync(string source)
        {
            byte[] compressedByte;

            compressedByte = await ((ICompressAsync)this).CompressAsync(UTF8Encoding.Default.GetBytes(source));

            return Convert.ToBase64String(compressedByte);
        }

        byte[] IDecompress.Decompress(byte[] source)
        {
            byte[] decompressedByte;
            int readBytes;

            using (MemoryStream sourceMemoryStream = new(source))
            {
                using (MemoryStream resultMemoryStream = new())
                {
                    using (Stream stream = new GZipStream(sourceMemoryStream, CompressionMode.Decompress))
                    {
                        sourceMemoryStream.Seek(0, 0);
                        decompressedByte = new byte[source.Length];

                        while (true)
                        {
                            readBytes = stream.Read(decompressedByte, 0, decompressedByte.Length);

                            if (readBytes < 1)
                                break;

                            resultMemoryStream.Write(decompressedByte, 0, readBytes);
                        }

                        stream.Close();
                    }

                    decompressedByte = new byte[resultMemoryStream.Length];

                    resultMemoryStream.Seek(0, 0);
                    resultMemoryStream.Read(decompressedByte, 0, decompressedByte.Length);
                    resultMemoryStream.Close();
                }
                sourceMemoryStream.Close();
            }

            return decompressedByte;
        }
        TValue IDecompress.Decompress<TValue>(byte[] source)
        {
            byte[] decompressedByte;

            decompressedByte = ((IDecompress)this).Decompress(source);

            var utf8Reader = new System.Text.Json.Utf8JsonReader(decompressedByte);

            TValue? value = System.Text.Json.JsonSerializer.Deserialize<TValue>(ref utf8Reader, this.JsonSerializerOptions);

            if (value != null)
                return value;
            else
                throw new MetaFrmException("Object is null.");
        }
        TValue IDecompress.DecompressFromString<TValue>(string source)
        {
            return ((IDecompress)this).Decompress<TValue>(Convert.FromBase64String(source));
        }
        string IDecompress.DecompressFromString(string source)
        {
            byte[] decompressedByte;

            decompressedByte = ((IDecompress)this).Decompress(Convert.FromBase64String(source));

            return Encoding.Default.GetString(decompressedByte);
        }

        async Task<byte[]> IDecompressAsync.DecompressAsync(byte[] source)
        {
            byte[] decompressedByte;
            int readBytes;

            using (MemoryStream sourceMemoryStream = new(source))
            {
                using (MemoryStream resultMemoryStream = new())
                {
                    using (Stream stream = new GZipStream(sourceMemoryStream, CompressionMode.Decompress))
                    {
                        sourceMemoryStream.Seek(0, 0);
                        decompressedByte = new byte[source.Length];

                        while (true)
                        {
                            readBytes = await stream.ReadAsync(decompressedByte.AsMemory(0, decompressedByte.Length));

                            if (readBytes < 1)
                                break;

                            await resultMemoryStream.WriteAsync(decompressedByte.AsMemory(0, readBytes));
                        }

                        stream.Close();
                    }

                    decompressedByte = new byte[resultMemoryStream.Length];

                    resultMemoryStream.Seek(0, 0);
                    readBytes = await resultMemoryStream.ReadAsync(decompressedByte.AsMemory(0, decompressedByte.Length));
                    resultMemoryStream.Close();
                }
                sourceMemoryStream.Close();
            }

            return decompressedByte;
        }
        async Task<TValue> IDecompressAsync.DecompressAsync<TValue>(byte[] source)
        {
            byte[] decompressedByte;

            decompressedByte = await ((IDecompressAsync)this).DecompressAsync(source);

            using MemoryStream memoryStream = new();
            await memoryStream.WriteAsync(decompressedByte.AsMemory(0, decompressedByte.Length));
            memoryStream.Position = 0;

            TValue? value = await System.Text.Json.JsonSerializer.DeserializeAsync<TValue>(memoryStream, this.JsonSerializerOptions);

            if (value != null)
                return value;
            else
                throw new MetaFrmException("Object is null.");
        }
        async Task<TValue> IDecompressAsync.DecompressFromStringAsync<TValue>(string source)
        {
            return await ((IDecompressAsync)this).DecompressAsync<TValue>(Convert.FromBase64String(source));
        }
        async Task<string> IDecompressAsync.DecompressFromStringAsync(string source)
        {
            byte[] decompressedByte;

            decompressedByte = await ((IDecompressAsync)this).DecompressAsync(Convert.FromBase64String(source));

            return Encoding.Default.GetString(decompressedByte);
        }
    }
}