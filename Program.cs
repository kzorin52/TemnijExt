﻿using Leaf.xNet;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TemnijExt
{
    internal static class Program
    {
        private static void Main()
        {
            Console.WriteLine(FileCl.Load(@"D:\Downloads\Windows10_InsiderPreview_Client_x64_ru-ru_21354.iso").Hashes.GetSHA256());
        }
    }

    public static class Ext
    {
        /// <summary>
        /// Распаковывает архив в указанную директорию
        /// </summary>
        /// <param name="file">Файл (для получения <c>FileCl.Load()</c>)</param>
        /// <param name="path">Путь до директории</param>
        public static void UnpackArchive(this FileCl file, string path, bool overwrite = false)
        {
            var archive = ArchiveFactory.Open(file.Path);
            foreach (var entry in archive.Entries)
                entry.WriteToDirectory(path, new ExtractionOptions() { ExtractFullPath = true, Overwrite = overwrite });
        }
        public static void ToArchive(this FileCl[] files, string archivePath, ArchiveType type = ArchiveType.Zip)
        {
            using (var zip = File.OpenWrite(archivePath))
            using (var zipWriter = WriterFactory.Open(zip, type, CompressionType.Deflate))
            {
                foreach (var filePath in files)
                {
                    zipWriter.Write(Path.GetFileName(filePath.Path), filePath.Path);
                }
            }
        }

        public static string Raw(this HttpResponse resp)
        {
            var raw = "";
            raw += "HTTP " + ((int)resp.StatusCode) + " " + resp.StatusCode + Environment.NewLine;

            var headers = resp.EnumerateHeaders();
            while (headers.MoveNext())
                raw += $"{headers.Current.Key}={headers.Current.Value}{Environment.NewLine}";
            raw += Environment.NewLine;
            raw += resp.ToString();

            return raw;
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.Shuffle(new Random());
        }
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (rng == null) throw new ArgumentNullException("rng");

            return source.ShuffleIterator(rng);
        }
        private static IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source, Random rng)
        {
            var buffer = source.ToList();
            for (int i = 0; i < buffer.Count; i++)
            {
                int j = rng.Next(i, buffer.Count);
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }
    }

    public class FileCl
    {
        #region FIELDS

        /// <summary>
        /// Путь до файла (только получение)
        /// </summary>
        public string Path { get; }

        public HashesCl Hashes;

        #endregion

        #region METHODS

        /// <summary>
        /// Создаёт новый объект типа FileCl (внутр.)
        /// </summary>
        /// <param name="path">Путь</param>
        internal FileCl(string path)
        {
            Hashes = new HashesCl(path);
            Path = path;
        }

        /// <summary>
        /// Загружает файл
        /// </summary>
        /// <param name="path">Путь до файла</param>
        /// <returns>Новый объект типа FileCl</returns>
        public static FileCl Load(string path)
            => new FileCl(path);

        /// <summary>
        /// Создаёт файл
        /// </summary>
        /// <param name="path">Путь до создаваемого файла</param>
        /// <param name="content">Контент файла (строка)</param>
        /// <returns>Новый объект типа FileCl</returns>
        public static FileCl Create(string path, string content = null)
        {
            File.Create(path);
            if (content != null)
                File.WriteAllText(path, content);
            return Load(path);
        }

        /// <summary>
        /// Создаёт файл
        /// </summary>
        /// <param name="path">Путь до создаваемого файла</param>
        /// <param name="content">Контент файла (массив байтов)</param>
        /// <returns>Новый объект типа FileCl</returns>
        public static FileCl Create(string path, byte[] content = null)
        {
            File.Create(path);
            if (content != null)
                File.WriteAllBytes(path, content);
            return Load(path);
        }

        public string GetBase64String() => Convert.ToBase64String(GetBytes());
        public void SetBase64String(string base64) => SetBytes(Convert.FromBase64String(base64));

        public byte[] GetBytes() => File.ReadAllBytes(Path);
        public void SetBytes(byte[] bytes) => File.WriteAllBytes(Path, bytes);
        public void SetBytes(IEnumerable<byte> bytes) => File.WriteAllBytes(Path, bytes.ToArray());

        public string GetContent() => File.ReadAllText(Path);
        public void SetContent(string content) => File.WriteAllText(Path, content);

        public string[] GetLines() => File.ReadAllLines(Path);
        public void SetLines(string[] lines) => File.WriteAllLines(Path, lines);
        public void SetLines(IEnumerable<string> lines) => File.WriteAllLines(Path, lines);

        public override string ToString() => GetContent();
        public bool Equals(FileCl obj) => Hashes.GetCRC32() == obj.Hashes.GetCRC32();

        #region Appends

        public void AppendText(string text) => File.AppendAllText(Path, text);
        public void AppendBytes(byte[] bytes)
        {
            using (var stream = new FileStream(Path, FileMode.Append))
                stream.Write(bytes, 0, bytes.Length);
        }
        public void AppendBytes(IEnumerable<byte> bytes)
        {
            using (var stream = new FileStream(Path, FileMode.Append))
                stream.Write(bytes.ToArray(), 0, bytes.Count());
        }
        public void AppendLines(string[] lines) => File.AppendAllLines(Path, lines);
        public void AppendLines(IEnumerable<string> lines) => File.AppendAllLines(Path, lines);

        #endregion

        #region GZip

        public string CompressB64() => GZip.Compress(GetContent());
        public string DecompressB64(string b64) => GZip.Decompress(b64);

        public byte[] Compress() => GZip.Compress(GetBytes());
        public byte[] Decompress(byte[] bytes) => GZip.Decompress(bytes);
        public byte[] Decompress(IEnumerable<byte> bytes) => Decompress(bytes.ToArray());

        #endregion

        #endregion

        #region CLASSES

        public class HashesCl
        {
            public HashesCl(string path)
            {
                Path = path;
            }

            public string Path { get; internal set; }

            /// <summary>
            /// Получение MD5 хеш-суммы файла
            /// Очень медленно, не советую использовать.
            /// </summary>
            /// <returns>Хеш-сумма</returns>
            public string GetMD5()
            {
                using (var md5 = MD5.Create())
                using (var stream = File.OpenRead(Path))
                    return Convert.ToBase64String(md5.ComputeHash(stream));
            }

            /// <summary>
            /// Получение CRC32-суммы файла
            /// Вроде побыстрее MD5 >_<
            /// </summary>
            /// <returns>Хеш-сумма</returns>
            public string GetCRC32()
            {
                using (var crc32 = HashAlgorithm.Create())
                using (var stream = File.OpenRead(Path))
                    return Convert.ToBase64String(crc32.ComputeHash(stream));
            }

            /// <summary>
            /// Получение SHA512-суммы файла
            /// Оч медленно :/
            /// </summary>
            /// <returns>Хеш-сумма</returns>
            public string GetSHA512()
            {
                using (var sha512 = SHA512.Create())
                using (var stream = File.OpenRead(Path))
                    return Convert.ToBase64String(sha512.ComputeHash(stream));
            }

            /// <summary>
            /// Получение SHA1-суммы файла
            /// Норм скорость, но медленнее CRC32
            /// </summary>
            /// <returns>Хеш-сумма</returns>
            public string GetSHA256()
            {
                using (var sha = SHA256.Create())
                using (var stream = File.OpenRead(Path))
                    return Convert.ToBase64String(sha.ComputeHash(stream));
            }
        }

        #endregion
    }

    public static class GZip
    {
        /// <summary>
        /// Декомпрессинг из Base64-строки
        /// </summary>
        /// <param name="input">Base64</param>
        public static string Decompress(string input)
        {
            byte[] compressed = Convert.FromBase64String(input);
            byte[] decompressed = Decompress(compressed);
            return Encoding.UTF8.GetString(decompressed);
        }

        /// <summary>
        /// Компресс обычной строки
        /// </summary>
        /// <param name="input">Строка</param>
        /// <returns>Base64-строка</returns>
        public static string Compress(string input)
        {
            byte[] encoded = Encoding.UTF8.GetBytes(input);
            byte[] compressed = Compress(encoded);
            return Convert.ToBase64String(compressed);
        }

        /// <summary>
        /// Декомпресс из байтов
        /// </summary>
        /// <param name="input">Скомпрессированные байты</param>
        /// <returns>Декомпрессированные байты</returns>
        public static byte[] Decompress(byte[] input)
        {
            using (var source = new MemoryStream(input))
            {
                byte[] lengthBytes = new byte[4];
                source.Read(lengthBytes, 0, 4);

                var length = BitConverter.ToInt32(lengthBytes, 0);
                using (var decompressionStream = new GZipStream(source,
                    CompressionMode.Decompress))
                {
                    var result = new byte[length];
                    decompressionStream.Read(result, 0, length);
                    return result;
                }
            }
        }

        /// <summary>
        /// Компресс массива байтов
        /// </summary>
        /// <param name="input">Декомпрессированные байтыСкомпрессиров</param>
        /// <returns>Скомпрессированные байты</returns>
        public static byte[] Compress(byte[] input)
        {
            using (var result = new MemoryStream())
            {
                var lengthBytes = BitConverter.GetBytes(input.Length);
                result.Write(lengthBytes, 0, 4);

                using (var compressionStream = new GZipStream(result,
                    CompressionMode.Compress))
                {
                    compressionStream.Write(input, 0, input.Length);
                    compressionStream.Flush();

                }
                return result.ToArray();
            }
        }
    }
}