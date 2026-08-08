# BÁO CÁO KẾT QUẢ THỰC TẬP TUẦN 1
**Người thực hiện:** Đỗ Ngọc Khánh
**Chủ đề:** C# & Hiệu năng xử lý dữ liệu (Focus: CPU/Memory Bottleneck)

---

## TỔNG QUAN MỤC TIÊU TUẦN 1
- Hiểu sâu bản chất về quản lý bộ nhớ (Stack vs Heap), Value Type vs Reference Type.
- Nắm vững cơ chế hoạt động của Garbage Collector và cách tối ưu hóa để tránh tràn RAM (OutOfMemory) khi xử lý tập dữ liệu lớn.
- Phân biệt rõ ràng giữa **Đa luồng (Multi-threading / CPU-Bound)** và **Bất đồng bộ (Async/Await / I/O-Bound)**.
- Thực hành đo lường hiệu năng (Benchmark) qua các bài toán xử lý file log khổng lồ.

---

## CHI TIẾT CÁC CÔNG VIỆC ĐÃ THỰC HIỆN

### Day 1 & Day 2: Xử lý Memory & Tối ưu bằng Đa luồng (Parallel.ForEach)
**1. Vấn đề đặt ra:**
Khi cần đọc và phân tích một file log có kích thước lớn (ví dụ hàng triệu dòng, dung lượng GB), nếu sử dụng các phương pháp thông thường như `File.ReadAllLines()`, toàn bộ dữ liệu sẽ được đẩy vào RAM (Heap). Điều này dẫn đến cạn kiệt bộ nhớ và ứng dụng bị crash.

**2. Giải pháp kỹ thuật:**
- **Sử dụng Deferred Execution (Thực thi trì hoãn):** Thay vì nạp toàn bộ dữ liệu, em áp dụng `File.ReadLines()`. Phương thức này trả về `IEnumerable<string>`, hoạt động theo cơ chế lazy load (đọc đến đâu giải phóng bộ nhớ đến đó), giúp mức tiêu thụ RAM luôn ở mức cực thấp, bất kể kích thước file.
- **Tối ưu tốc độ (CPU Bottleneck):** Em tiến hành chia nhỏ công việc cho các Core của CPU bằng `Parallel.ForEach`. 
- **Đảm bảo Thread-Safe:** Khi nhiều luồng cùng đếm từ và ghi vào một Dictionary, hiện tượng Race Condition (xung đột dữ liệu) sẽ xảy ra. Để giải quyết, em đã sử dụng `ConcurrentDictionary<string, int>` và hàm `AddOrUpdate()` có hỗ trợ lock mức độ thấp để đảm bảo an toàn luồng.

**3. Kết quả Benchmark (Project: LogAnalyzer):**
- Đếm từ với file log 1 triệu dòng (60MB).
- **Chạy tuần tự (Sequential - 1 luồng):** Thời gian thực thi là **1331 ms**.
- **Chạy đa luồng (Parallel - Đa nhân):** Thời gian thực thi giảm xuống còn **841 ms** (tốc độ cải thiện rõ rệt ~40%).

---

### Day 3: Phân tích chuyên sâu Async/Await vs ThreadPool
Em đã dành thời gian nghiên cứu và phân biệt rạch ròi 2 khái niệm thường bị nhầm lẫn:
- **Multi-threading (Sử dụng Task.Run, Parallel):** Chỉ nên dùng cho các tác vụ **CPU-Bound** (ví dụ: mã hóa, xử lý hình ảnh, tính toán số liệu lớn). Nó mượn nhiều nhân CPU để làm việc cùng lúc, giúp giảm thời gian chạy.
- **Async/Await (I/O-Bound):** Dùng khi đọc/ghi ổ cứng, gọi Database, hoặc gọi API bên ngoài. Quá trình này CPU không tính toán mà chỉ chờ đợi. Dùng từ khóa `await` không làm truy vấn DB nhanh hơn, nhưng nó giúp **giải phóng Thread** trả về cho ThreadPool. 
- **Ý nghĩa thực tiễn:** Điều này cực kỳ quan trọng khi xây dựng WebAPI. Nhờ Async/Await, API có thể chịu tải hàng ngàn request cùng lúc mà không bị cạn kiệt Thread (Thread Starvation).

---

### Day 4: SOLID, Exception Handling & Logging hiệu năng
- **Exception Handling:** Việc ném (`throw`) và bắt (`catch`) exception trong C# cực kỳ đắt đỏ về mặt CPU. Em đã thực hành nguyên tắc: **Không dùng try-catch bên trong các vòng lặp hàng triệu lần** để bắt lỗi logic (vd: ép kiểu sai). Thay vào đó, dùng `TryParse` hoặc kiểm tra điều kiện `if-else`. Chỉ bọc `try-catch` ở lớp ngoài cùng (Middleware) để bắt các lỗi hệ thống không lường trước được.
- **SOLID:** Chia nhỏ các module: Hàm tạo file log, hàm phân tích, hàm in kết quả được tách biệt (Single Responsibility), giúp code dễ bảo trì và test.

---

### Day 5: Mini Project - Log Analyzer Tool (Kết hợp Async & Concurrent)
**1. Bài toán tổng hợp:**
Xây dựng một Tool phân tích log có khả năng đọc và đếm số lượng lỗi (ERROR, WARN, FATAL...) từ **5 file log độc lập**, mỗi file chứa 200,000 dòng (tổng 1 triệu dòng record).

**2. Kiến trúc áp dụng:**
- Sử dụng **`await Task.WhenAll(tasks)`** để kích hoạt đọc 5 file cùng lúc. Hệ điều hành sẽ xử lý I/O đồng thời mà không bắt CPU phải chờ từng file một.
- Trong mỗi file, kết hợp sử dụng **`await foreach`** với **`File.ReadLinesAsync()`** (tính năng của C# 8+). Dữ liệu được kéo lên từ ổ cứng dưới dạng luồng bất đồng bộ (Async Stream).
- Cả 5 Task đều ghi kết quả đếm chung vào một **`ConcurrentDictionary`** toàn cục để tổng hợp số liệu.

**3. Kết quả Benchmark cuối cùng:**
```text
=== DAY 5: LOG ANALYZER TOOL (ASYNC/AWAIT & CONCURRENT) ===
[Info] Tìm thấy 5 file log (tổng 1 triệu record). Bắt đầu phân tích...

[Result] Phân tích hoàn tất trong 306 ms!
Thống kê số lượng log theo từng loại:
- FATAL      : 200,350 dòng
- ERROR      : 200,092 dòng
- INFO       : 200,064 dòng
- WARN       : 199,893 dòng
- DEBUG      : 199,601 dòng
```
**=> Đánh giá:** Nhờ áp dụng triệt để kiến thức I/O Bound (Async) và Thread-Safe (Concurrent), thời gian xử lý 1 triệu record từ nhiều nguồn khác nhau đã được ép xuống **chỉ còn hơn 0.3 giây (~306 ms)**, nhanh hơn rất nhiều so với phương pháp thông thường.

---
**KẾT LUẬN TUẦN 1:** 
Tuần 1 đã giúp em thay đổi tư duy từ việc "viết code cho chạy được" sang "viết code thấu hiểu hệ thống". Nắm chắc cách điều khiển Memory, ThreadPool và I/O Async là nền tảng vững chắc để sang Tuần 2 em có thể tối ưu các câu query phức tạp của Entity Framework Core, xử lý bài toán N+1 và bùng nổ dữ liệu.
