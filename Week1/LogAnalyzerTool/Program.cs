using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LogAnalyzerTool
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== DAY 5: LOG ANALYZER TOOL (ASYNC/AWAIT & CONCURRENT) ===");
            
            string logDirectory = "logs";
            int numberOfFiles = 5;
            int linesPerFile = 200_000;

            // 1. Chuẩn bị thư mục và sinh dummy logs
            PrepareDummyLogs(logDirectory, numberOfFiles, linesPerFile);

            string[] filePaths = Directory.GetFiles(logDirectory, "*.txt");
            Console.WriteLine($"\n[Info] Tìm thấy {filePaths.Length} file log. Bắt đầu phân tích...");

            // Khởi tạo từ điển an toàn đa luồng để đếm loại log (INFO, ERROR, WARN,...)
            var logTypeCounts = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            var stopwatch = Stopwatch.StartNew();

            // 2. Chạy Async/Await xử lý nhiều file cùng lúc (I/O Bound)
            // Thay vì dùng foreach đợi từng file, ta ném tất cả vào một List<Task>
            var tasks = new List<Task>();
            
            foreach (var file in filePaths)
            {
                // Thêm task xử lý file vào danh sách (chưa await ngay)
                tasks.Add(ProcessLogFileAsync(file, logTypeCounts));
            }

            // Await tất cả các task cùng hoàn thành
            await Task.WhenAll(tasks);

            stopwatch.Stop();

            // 3. In kết quả
            Console.WriteLine($"\n[Result] Phân tích hoàn tất trong {stopwatch.ElapsedMilliseconds} ms!");
            Console.WriteLine("Thống kê số lượng log theo từng loại:");
            
            // Sắp xếp giảm dần theo số lượng
            var sortedResults = logTypeCounts.OrderByDescending(x => x.Value);
            
            foreach (var kvp in sortedResults)
            {
                Console.WriteLine($"- {kvp.Key.PadRight(10)} : {kvp.Value:N0} dòng");
            }
            
            Console.WriteLine("\nHoàn thành! Nhấn phím bất kỳ để thoát...");
            Console.ReadLine();
        }

        // Hàm xử lý một file log bằng IAsyncEnumerable (NET 8)
        static async Task ProcessLogFileAsync(string filePath, ConcurrentDictionary<string, int> globalDictionary)
        {
            // Dùng File.ReadLinesAsync để đọc file bất đồng bộ, không khóa luồng khi chờ IO ổ cứng
            await foreach (string line in File.ReadLinesAsync(filePath))
            {
                // Giả định định dạng log: "2026-08-05 10:00:00 [ERROR] Some message"
                // Ta cần trích xuất chữ "ERROR" nằm trong dấu ngoặc vuông
                
                int startIndex = line.IndexOf('[');
                int endIndex = line.IndexOf(']');
                
                if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
                {
                    // Lấy loại log (INFO, ERROR, WARN...)
                    string logType = line.Substring(startIndex + 1, endIndex - startIndex - 1);

                    // Cộng dồn vào Dictionary một cách an toàn cho đa luồng
                    globalDictionary.AddOrUpdate(logType, 1, (key, oldValue) => oldValue + 1);
                }
            }
        }

        static void PrepareDummyLogs(string directory, int fileCount, int linesCount)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string[] types = { "ERROR", "INFO", "WARN", "DEBUG", "FATAL" };
            Random rand = new Random();

            for (int i = 1; i <= fileCount; i++)
            {
                string filePath = Path.Combine(directory, $"server_log_{i}.txt");
                
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"[Info] Đang tạo file {filePath}...");
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        for (int j = 0; j < linesCount; j++)
                        {
                            string type = types[rand.Next(types.Length)];
                            writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{type}] System module_{rand.Next(1, 10)} processed user_{rand.Next(1000)}");
                        }
                    }
                }
            }
        }
    }
}
