using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace LogAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = "dummy_log.txt";

            Console.WriteLine("=== DAY 1: LOG ANALYZER ===");
            
            // 1. Tạo file dummy khoảng 50MB nếu chưa có
            if (!File.Exists(filePath))
            {
                GenerateDummyLogFile(filePath, 1_000_000); // 1 triệu dòng
            }
            else
            {
                Console.WriteLine($"[Info] File {filePath} đã tồn tại, kích thước: {new FileInfo(filePath).Length / (1024 * 1024)} MB");
            }

            // 2. Chạy đếm từ tuần tự (Sequential)
            Console.WriteLine("\n[Running] Bắt đầu đếm từ (Sequential)...");
            var stopwatch = Stopwatch.StartNew();
            
            var wordCounts = CountWordsSequential(filePath);
            
            stopwatch.Stop();
            
            Console.WriteLine($"[Result] Đã đếm được {wordCounts.Count} từ khác nhau.");
            Console.WriteLine($"[Time] Thời gian chạy: {stopwatch.ElapsedMilliseconds} ms");
            
            // In ra top 5 từ xuất hiện nhiều nhất để kiểm tra
            Console.WriteLine("\nTop 5 từ xuất hiện nhiều nhất:");
            int count = 0;
            foreach (var kvp in wordCounts)
            {
                Console.WriteLine($"- {kvp.Key}: {kvp.Value}");
                count++;
                if (count >= 5) break;
            }
            
            // 3. Chạy đếm từ song song (Parallel)
            Console.WriteLine("\n[Running] Bắt đầu đếm từ (Parallel)...");
            var stopwatchParallel = Stopwatch.StartNew();
            
            var wordCountsParallel = CountWordsParallel(filePath);
            
            stopwatchParallel.Stop();
            
            Console.WriteLine($"[Result] Đã đếm được {wordCountsParallel.Count} từ khác nhau.");
            Console.WriteLine($"[Time] Thời gian chạy: {stopwatchParallel.ElapsedMilliseconds} ms");

            Console.WriteLine("\nHoàn thành! Nhấn phím bất kỳ để thoát...");
            Console.ReadLine();
        }

        static void GenerateDummyLogFile(string path, int lines)
        {
            Console.WriteLine($"[Info] Đang tạo file {path} với {lines} dòng...");
            var stopwatch = Stopwatch.StartNew();
            
            string[] words = { "ERROR", "INFO", "WARN", "DEBUG", "FATAL", "TIMEOUT", "SUCCESS", "FAILED", "NULL", "EXCEPTION" };
            Random rand = new Random();

            // Dùng StreamWriter để ghi file từng dòng (tiết kiệm RAM)
            using (StreamWriter writer = new StreamWriter(path))
            {
                for (int i = 0; i < lines; i++)
                {
                    // Tạo một dòng log ngẫu nhiên
                    string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{words[rand.Next(words.Length)]}] User{rand.Next(1, 1000)} action_{words[rand.Next(words.Length)]} in module_{rand.Next(1, 50)}";
                    writer.WriteLine(logLine);
                }
            }
            
            stopwatch.Stop();
            Console.WriteLine($"[Info] Tạo file xong! Kích thước: {new FileInfo(path).Length / (1024 * 1024)} MB. Thời gian: {stopwatch.ElapsedMilliseconds} ms");
        }

        static Dictionary<string, int> CountWordsSequential(string path)
        {
            var dictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // Dùng File.ReadLines (Deferred Execution) thay vì File.ReadAllLines
            foreach (string line in File.ReadLines(path))
            {
                // Cắt từ bằng dấu cách và một số ký tự đặc biệt
                string[] words = line.Split(new[] { ' ', '[', ']', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (string word in words)
                {
                    if (dictionary.ContainsKey(word))
                    {
                        dictionary[word]++;
                    }
                    else
                    {
                        dictionary[word] = 1;
                    }
                }
            }

            return dictionary;
        }

        static ConcurrentDictionary<string, int> CountWordsParallel(string path)
        {
            var dictionary = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // C# Parallel.ForEach sẽ tự động chia công việc cho các core của CPU
            Parallel.ForEach(File.ReadLines(path), line =>
            {
                string[] words = line.Split(new[] { ' ', '[', ']', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (string word in words)
                {
                    // AddOrUpdate an toàn với đa luồng (Thread-safe)
                    dictionary.AddOrUpdate(word, 1, (key, oldValue) => oldValue + 1);
                }
            });

            return dictionary;
        }
    }
}
