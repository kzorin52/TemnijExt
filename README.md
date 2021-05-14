# TemnijExt
Temnij Extensions &amp; FileCl Class

Полезные расширения и классы, а также класс-обёртка для файлов - `FileCl`.

Telegram - https://t.me/temnij52

# Что такое FileCl?
FileCl - это класс-обёртка для файлов.

Его синтаксис прост - 
```cs
var file = FileCl.Load(@"c:\file.exe");
```
или

```cs
var file = FileCl.Create(@"c:\file2.exe");
```

Потом с файлом можно делать что угодно: <br>
-	Копировать  `file.Copy(@"c:\file2.exe");`
-	Перемеcтить `file.Move(@"c:\file2.exe");`
-	Удалить `file.Delete();`
-	Переименовать `file.Rename("file3.exe");`
-	Дополнить строки
-	-	Массивом байтов `byte[] bytes = { 0x00, 0x01 }; file.Append(bytes);`
-	-	Текстом `file.Append("hello!");`
-	-	Строками `string[] lines = { "line", "line2" }; file.Append(lines);`
-	Получить контент файла, как
-	-	Массив байтов `file.GetBytes();`
-	-	Строки `file.GetLines();`
-	-	Строка `file.GetContent();` **или** `file.ToString()`
-	-	Base-64 `file.GetBase64String();`
-	**Скомпрессировать файл** _с помощью GZip_ `var compressedBytes = file.Compress();`
-	**Декомпрессировать файл** _с помощью GZip_ `var decompressedBytes = file.Decompress();`
-	Зашифровать файл на пароль `file.Crypting.EncryptFile("key");`
-	Дешифровать `file.Crypting.DecryptFile("key");`
-	**Получить хеш-суммы**
-	- CRC32 `file.Hashes.GetCRC32();`
-	- SHA-256 `file.Hashes.GetSHA256();`
-	- SHA-512 `file.Hashes.GetSHA512();`
- - MD5 `file.Hashes.GetMD5();`
-	Получить метаданные файла классом `FileInfo` (время создания, папку, содержащую файл, расширение и т.д.)
<br>
Это правда удобно, вот посмотрите!<br>

```cs
var file = FileCl.Load(@"c:\file.exe");
var extension = file.Info.Extension;

file.Crypting.EncryptFile("My-super-secret-key");
file.Crypting.DecryptFile("My-super-secret-key");

file.SetBytes(file.Compress());
Console.WriteLine(file.Info.Length);
file.SetBytes(file.Decompress());
Console.WriteLine(file.Info.Length);

var hash = file.Hashes.GetCRC32();

file.Run();
```
