using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using ImageProcessor;

namespace ImageResizer
{
    internal class Program
    {
        private static string[] _extensions = { ".jpeg", ".jpg" };
        private const string OutputDirName = @"resized-images";

        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var path = string.Join(" ", args);

                // Либо папка либо файл
                var attr = File.GetAttributes(path);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    ProcessDirectory(path);
                }
                else
                {                    
                    Console.WriteLine($"Файл: {args[0]}");
                    if (!IsJpeg(path))
                    {
                        Console.WriteLine("Файл не является jpeg-картинкой!");
                    }
                    else
                    {
                        var outputDirPath = Path.Combine(Path.GetDirectoryName(path), OutputDirName);
                        SetupOutputDir(outputDirPath);
                        ProcessFiles(outputDirPath, path);
                    }
                }
            }
            else if (args.Length == 0)
            {
                // Обрабатываем все папки в текущей директории

                var cwd = Directory.GetCurrentDirectory();
                var dirs = Directory.GetDirectories(cwd);
                foreach (var dir in dirs)
                {
                    ProcessDirectory(dir);
                    Console.WriteLine();
                }
            }

            Thread.Sleep(2000);
        }

        private static void ProcessDirectory(string dirPath)
        {
            Console.WriteLine($"Директория: {dirPath}");

            var outputDirPath = Path.Combine(dirPath, OutputDirName);
            SetupOutputDir(outputDirPath);

            var files = Directory.GetFiles(dirPath)
                        .Where(IsJpeg)
                        .ToArray();

            ProcessFiles(outputDirPath, files);
        }

        private static void ProcessFiles(string outputDirPath, params string[] files)
        {           

            var sw = Stopwatch.StartNew();

            using (var imFactory = new ImageFactory())
            {
                var size = new Size(1920, 1080);
                for (var i = 0; i < files.Length; i++)
                {
                    var fileName = Path.GetFileName(files[i]);
                    var bytes = File.ReadAllBytes(files[i]);
                    var savePath = Path.Combine(outputDirPath, fileName);
                    imFactory
                        .Load(bytes)
                        .Resize(size)
                        .Save(savePath);

                    Console.WriteLine($"Обработано {(i + 1):00} из {files.Length}: {fileName}");
                }
            }

            sw.Stop();

            Console.WriteLine("Готово!");
            Console.WriteLine($"Время выполнения, сек: {sw.Elapsed.TotalSeconds}");
        }

        private static void SetupOutputDir(string outputDirPath)
        {
            var outputDirInfo = new DirectoryInfo(outputDirPath);
            if (!outputDirInfo.Exists)
                outputDirInfo.Create();
            else
                outputDirInfo.Delete(true);
        }

        private static bool IsJpeg(string filePath)
        {
            return _extensions.Any(ext => filePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }
    }
}