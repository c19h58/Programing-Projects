using System;
using System.IO;
using System.Linq;

namespace FileSystemInspector
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== FileSystemInspector ===");
            Console.Write("Введите путь к директории: ");
            string? path = Console.ReadLine();

            try
            {
            
                if (!Directory.Exists(path))
                {
                    throw new DirectoryNotFoundException($"Директория по пути '{path}' не найдена.");
                }

                DirectoryInfo rootDir = new DirectoryInfo(path);

                
                Console.WriteLine("\n--- Основная информация ---");
                Console.WriteLine($"Полный путь: {rootDir.FullName}");
                Console.WriteLine($"Дата создания: {rootDir.CreationTime}");
                Console.WriteLine($"Последнее изменение: {rootDir.LastWriteTime}");

                
                var subDirs = rootDir.GetDirectories();
                var files = rootDir.GetFiles();

                Console.WriteLine($"\nКоличество поддиректорий: {subDirs.Length}");
                Console.WriteLine($"Количество файлов в текущей папке: {files.Length}");

                Console.WriteLine("\n--- Список поддиректорий ---");
                foreach (var dir in subDirs)
                {
                    Console.WriteLine($"[{dir.CreationTime}] {dir.Name}");
                }

                Console.WriteLine("\n--- Список файлов ---");
                foreach (var file in files)
                {
                   Console.WriteLine($"{file.Name} | Размер: {file.Length} байт | Расширение: {file.Extension}"); 
                }

                
                if (files.Length > 0)
                {
                    var maxFile = files.OrderByDescending(f => f.Length).First();
                    var minFile = files.OrderBy(f => f.Length).First();
                    Console.WriteLine("\n--- Статистика по файлам ---");
                    Console.WriteLine($"Самый большой файл: {maxFile.Name} ({maxFile.Length} байт)");
                    Console.WriteLine($"Самый маленький файл: {minFile.Name} ({minFile.Length} байт)");
                    Console.WriteLine($"Средний размер файла: {files.Average(f => f.Length):F2} байт");
                }

                
                int totalFilesCount = CountFilesRecursive(rootDir);
                Console.WriteLine($"\nОбщее количество файлов (включая вложенные): {totalFilesCount}");

                
                Console.WriteLine("\n--- Структура каталогов ---");
                PrintDirectoryTree(rootDir, "");

            }
            
            catch (DirectoryNotFoundException ex) { Console.WriteLine($"Ошибка: {ex.Message}"); } 
            catch (UnauthorizedAccessException ex) { Console.WriteLine($"Ошибка доступа: {ex.Message}"); } 
            catch (ArgumentException ex) { Console.WriteLine($"Ошибка аргумента: {ex.Message}"); } 
            catch (Exception ex) { Console.WriteLine($"Произошла непредвиденная ошибка: {ex.Message}"); }

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        
        static int CountFilesRecursive(DirectoryInfo dir)
        {
            int count = dir.GetFiles().Length;
            foreach (var subDir in dir.GetDirectories())
            {
                try { count += CountFilesRecursive(subDir); }
                catch (UnauthorizedAccessException) { /* Пропускаем папки без доступа */ }
            }
            return count;
        }

      
        static void PrintDirectoryTree(DirectoryInfo dir, string indent)
        {
            Console.WriteLine($"{indent}└── {dir.Name}");
            string subIndent = indent + "    ";

            try
            {
                foreach (var file in dir.GetFiles())
                {
                    Console.WriteLine($"{subIndent}├── {file.Name}");
                }

                foreach (var subDir in dir.GetDirectories())
                {
                    PrintDirectoryTree(subDir, subIndent);
                }
            }
            catch (UnauthorizedAccessException) { /* Пропуск */ }
        }
    }
}