using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        Console.Write("Введите путь к исходному файлу: ");
        string? sourceFilePath = Console.ReadLine();

        Console.Write("Введите путь для копирования: ");
        string? destinationFilePath = Console.ReadLine();

        Console.Write("Введите количество потоков для копирования: ");
        if (!int.TryParse(Console.ReadLine(), out int numberOfThreads) || numberOfThreads < 1)
        {
            Console.WriteLine("Некорректное количество потоков. Используется 1 поток.");
            numberOfThreads = 1;
        }

        await CopyFileAsync(sourceFilePath, destinationFilePath, numberOfThreads);
    }

    static async Task CopyFileAsync(string sourceFilePath, string destinationFilePath, int numberOfThreads)
    {
        if (!File.Exists(sourceFilePath))
        {
            Console.WriteLine("Исходный файл не существует.");
            return;
        }

        long fileSize = new FileInfo(sourceFilePath).Length;
        long chunkSize = fileSize / numberOfThreads;
        long remainingBytes = fileSize;

        using (FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
        {
            using (FileStream destinationStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write))
            {
                Task[] tasks = new Task[numberOfThreads];
                for (int i = 0; i < numberOfThreads; i++)
                {
                    long bytesToRead = (i == numberOfThreads - 1) ? remainingBytes : chunkSize;

                    tasks[i] = Task.Run(async () =>
                    {
                        byte[] buffer = new byte[4096];
                        long totalBytesRead = 0;

                        while (totalBytesRead < bytesToRead)
                        {
                            int bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length);
                            if (bytesRead == 0) break;

                            await destinationStream.WriteAsync(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;
                            long progress = totalBytesRead + (i * chunkSize);
                            DisplayProgress(fileSize, progress);
                        }
                    });

                    remainingBytes -= bytesToRead;
                }

                await Task.WhenAll(tasks);
                Console.WriteLine("\nКопирование завершено.");
            }
        }
    }

    static void DisplayProgress(long totalSize, long currentSize)
    {
        int percent = (int)((currentSize * 100) / totalSize);
        Console.CursorLeft = 0;
        Console.Write($"Прогресс: {percent}% ({currentSize} / {totalSize} байт)");
    }
}
