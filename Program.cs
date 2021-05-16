using Leaf.xNet;
using Newtonsoft.Json.Linq;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Writers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TemnijExt
{
    public static class Ext
    {
        #region Archives

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
        /// <summary>
        /// Массив файлов в архив
        /// </summary>
        /// <param name="files">Массив файлов</param>
        /// <param name="archivePath">Путь до нового архива</param>
        /// <param name="type">Тип архива</param>
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

        /// <summary>
        /// Компресс массива байтов
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>Скомпрессированные байты</returns>
        public static byte[] Compress(this byte[] bytes) => GZip.Compress(bytes);
        /// <summary>
        /// Декомпресс массива байтов
        /// </summary>
        /// <param name="bytes">Скомпрессированные байты</param>
        /// <returns>Декомпрессированный массив байтов</returns>
        public static byte[] Decompress(this byte[] bytes) => GZip.Decompress(bytes);

        #endregion
        #region xNet

        /// <summary>
        /// Получить Raw-ответ (beta)
        /// </summary>
        /// <param name="resp">Ответ сервера</param>
        /// <returns>Raw-ответ</returns>
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

        #endregion
        #region Collections

        /// <summary>
        /// Перемешать коллекцию
        /// </summary>
        /// <typeparam name="T">Тип</typeparam>
        /// <param name="source">Коллекция</param>
        /// <returns>Перемешанная коллекция</returns>
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

        #endregion
    }

    public class FileCl
    {
        #region FIELDS

        /// <summary>
        /// Путь до файла (только получение)
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Работа с хешами
        /// </summary>
        public HashesCl Hashes;

        /// <summary>
        /// Инфа файла
        /// </summary>
        public FileInfo Info { get; internal set; }

        /// <summary>
        /// Шифрование файла
        /// </summary>
        public CryptingCl Crypting;

        #endregion
        #region METHODS

        #region Base Methods

        /// <summary>
        /// Создаёт новый объект типа FileCl (внутр.)
        /// </summary>
        /// <param name="path">Путь</param>
        internal FileCl(string path)
        {
            Hashes = new HashesCl(path);
            Info = new FileInfo(path);
            Path = path;
            Crypting = new CryptingCl(path) { file = this };
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

        #endregion
        #region Base File Methods

        /// <summary>
        /// Удаляет файл
        /// </summary>
        public void Delete() => File.Delete(Path);
        /// <summary>
        /// Копирует файл
        /// </summary>
        /// <param name="newPath">Местоположение копии файла (с расширением и именем!)</param>
        public void Copy(string newPath) => File.Copy(Path, newPath);
        /// <summary>
        /// Перемещает файл
        /// </summary>
        /// <param name="newPath">Новое местоположение файла (с расширением и именем!)</param>
        public void Move(string newPath)
        {
            File.Move(Path, newPath);
            Path = newPath;
        }
        /// <summary>
        /// Переименовать
        /// </summary>
        /// <param name="newName">Новое имя (с расширением)</param>
        public void Rename(string newName) => Info.MoveTo(Info.Directory.FullName + "\\" + newName);

        public void Run() => Process.Start(Path);

        #endregion

        #region Content (Base64)

        /// <summary>
        /// Получить Base64 файла
        /// </summary>
        /// <returns>Base64</returns>
        public string GetBase64String() => Convert.ToBase64String(GetBytes());
        /// <summary>
        /// Устанавливает контент файла из base64 строки
        /// </summary>
        /// <param name="base64">Base64-строка</param>
        public void SetBase64String(string base64) => SetContent(Convert.FromBase64String(base64));

        #endregion
        #region Content (Bytes)

        /// <summary>
        /// Получить массив байтов файла
        /// </summary>
        /// <returns>Массив байтов файла</returns>
        public byte[] GetBytes() => File.ReadAllBytes(Path);
        /// <summary>
        /// Установить массив байтов в файл
        /// </summary>
        /// <param name="bytes">Байты</param>
        public void SetContent(byte[] bytes) => File.WriteAllBytes(Path, bytes);
        /// <summary>
        /// Установить массив байтов в файл
        /// </summary>
        /// <param name="bytes">Байты</param>
        public void SetContent(IEnumerable<byte> bytes) => File.WriteAllBytes(Path, bytes.ToArray());

        #endregion
        #region Content (string)

        /// <summary>
        /// Получить контент файла, как <c>string</c>
        /// </summary>
        /// <returns>Контент файла</returns>
        public string GetContent() => File.ReadAllText(Path);
        /// <summary>
        /// Записать строку, как весь файл
        /// </summary>
        public void SetContent(string content) => File.WriteAllText(Path, content);

        #endregion
        #region Content (Lines)

        /// <summary>
        /// Получить все строки файла
        /// </summary>
        /// <returns>Все строки файла</returns>
        public string[] GetLines() => File.ReadAllLines(Path);
        /// <summary>
        /// Записать как все строки файла
        /// </summary>
        /// <param name="lines"></param>
        public void SetContent(string[] lines) => File.WriteAllLines(Path, lines);
        /// <summary>
        /// Записать как все строки файла
        /// </summary>
        /// <param name="lines"></param>
        public void SetContent(IEnumerable<string> lines) => File.WriteAllLines(Path, lines);

        #endregion

        #region Overrides

        public override string ToString() => GetContent();
        public bool Equals(FileCl obj) => Hashes.GetCRC32() == obj.Hashes.GetCRC32();

        #endregion

        #region Appends

        /// <summary>
        /// Дописать текст
        /// </summary>
        /// <param name="text"></param>
        public void Append(string text) => File.AppendAllText(Path, text);

        /// <summary>
        /// Добавить байты
        /// </summary>
        /// <param name="bytes"></param>
        public void Append(byte[] bytes)
        {
            using (var stream = new FileStream(Path, FileMode.Append))
                stream.Write(bytes, 0, bytes.Length);
        }
        /// <summary>
        /// Добавить байты
        /// </summary>
        /// <param name="bytes"></param>
        public void Append(IEnumerable<byte> bytes) => Append(bytes.ToArray());

        /// <summary>
        /// Добавить строки
        /// </summary>
        /// <param name="lines"></param>
        public void Append(string[] lines) => File.AppendAllLines(Path, lines);
        /// <summary>
        /// Добавить строки
        /// </summary>
        /// <param name="lines"></param>
        public void Append(IEnumerable<string> lines) => File.AppendAllLines(Path, lines);

        #endregion

        #region GZip

        /// <summary>
        /// Компрессирование с помощью GZip
        /// </summary>
        /// <returns>Скомпрессированный файл в base64</returns>
        public string CompressB64() => GZip.Compress(GetContent());
        /// <summary>
        /// Декомпрессирование из base64 c помощью GZip
        /// </summary>
        /// <param name="b64">Base64</param>
        /// <returns>Декомпрессированный контент</returns>
        public string DecompressB64(string b64) => GZip.Decompress(b64);

        /// <summary>
        /// Компрессирование с помощью GZip
        /// (рекомендую с байтами (то есть этот метод), а не с base64)
        /// </summary>
        /// <returns>Скомпрессированный файл</returns>
        public byte[] Compress() => GZip.Compress(GetBytes());
        /// <summary>
        /// Декомпрессирование c помощью GZip
        /// </summary>
        /// <param name="bytes">Скомпрессированный массив байтов</param>
        /// <returns>Декомпрессированный массив байтов</returns>
        public byte[] Decompress(byte[] bytes) => GZip.Decompress(bytes);
        /// <summary>
        /// Декомпрессирование c помощью GZip
        /// </summary>
        /// <param name="bytes">Скомпрессированный массив байтов</param>
        /// <returns>Декомпрессированный массив байтов</returns>
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
        public class CryptingCl
        {
            #region FIELDS

            internal FileCl file;
            internal string Path { get; set; }

            #endregion
            #region METHODS

            public CryptingCl(string path) =>
                Path = path;

            public void EncryptFile(string key) => file.SetContent(Encrypt(key));
            public byte[] Encrypt(string key) => Cryptor.Get(key).Encrypt(file.Path);

            public void DecryptFIle(string key) => file.SetContent(Decrypt(key));
            public byte[] Decrypt(string key) => Cryptor.Get(key).Decrypt(file.Path);

            #endregion
        }

        #endregion
    }

    #region Extensions

    public static class AsJSON
    {
        public static JObject Serialize(this FileCl file) => JObject.Parse(file.GetContent());
    }

    #endregion

    #region Extend Classes

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
    /// <summary>
    /// Crypto Module
    /// </summary>
    public class Cryptor
    {
        private readonly int[] _key;
        private readonly int[] _inversedKey;

        public static Cryptor Get(string key) => new Cryptor(key);

        public Cryptor(string key)
        {
            var indexPairs = key
                .Select((chr, idx1) => new { chr, idx1 })
                .OrderBy(arg => arg.chr)
                .Select((arg, idx2) =>
                    new
                    {
                        arg.idx1,
                        idx2
                    })
                .ToArray();

            _key = indexPairs
                .OrderBy(arg => arg.idx1)
                .Select(arg => arg.idx2)
                .ToArray();

            _inversedKey = indexPairs
                .OrderBy(arg => arg.idx2)
                .Select(arg => arg.idx1)
                .ToArray();
        }

        public byte[] Encrypt(string sourceFile) =>
            EncryptDecrypt(sourceFile, _key);

        public byte[] Decrypt(string sourceFile) =>
            EncryptDecrypt(sourceFile, _inversedKey);

        private static byte[] EncryptDecrypt(string sourceFile, int[] key)
        {
            var keyLength = key.Length;
            var buffer1 = new byte[keyLength];
            var buffer2 = new byte[keyLength];
            using (var source = new FileStream(sourceFile, FileMode.Open))
            using (var destination = new MemoryStream())
            {
                while (true)
                {
                    var read = source.Read(buffer1, 0, keyLength);
                    if (read == 0)
                        return destination.ToArray();
                    else if (read < keyLength)
                        for (int i = read; i < keyLength; i++)
                            buffer1[i] = (byte)' ';

                    for (var i = 0; i < keyLength; i++)
                    {
                        var idx = (i / keyLength * keyLength) + key[i % keyLength];
                        buffer2[idx] = buffer1[i];
                    }

                    destination.Write(buffer2, 0, keyLength);
                }
            }
        }
    }

    #endregion
}
