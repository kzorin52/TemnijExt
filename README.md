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
Потом с файлом можно делать что угодно: <br>
-	Копировать	<br>
-	Перемеcтить	<br>
-	Удалить	<br>
-	Переименовать
-	Дополнить строки<br>
-	-	Массивом байтов<br>
-	-	Текстом<br>
-	-	Строками<br>
-	Получить контент файла, как<br>
-	-	Массив байтов<br>
-	-	Строки<br>
-	-	Строка<br>
-	-	Base-64<br>
-	**Скомпрессировать файл** _с помощью GZip_<br>
-	**Декомпрессировать файл** _с помощью GZip_<br>
-	Зашифровать файл на пароль
-	Дешифровать
-	**Получить хеш-суммы**
-	- CRC32
-	- SHA-256
-	- SHA-512
- - MD5
-	Получить метаданные файла классом `FileInfo` 
<br>
Это правда удобно, вот посмотрите! <br>
```cs
var file = FileCl.Load(@"c:\file.exe");
var extension = file.Info.Extension;

file.Crypting.EncryptFile("My-super-secret-key");
file.Crypting.DecryptFile("My-super-secret-key");

file.Compress();
Console.WriteLine(file.Info.Length);
file.Decompress();
Console.WriteLine(file.Info.Length);

var hash = file.Hashes.GetCRC32();

file.Run();
```
