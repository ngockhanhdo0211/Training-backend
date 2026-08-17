int result = Sum(5);
Console.WriteLine($"Tổng từ 1 đến 5 là: {result}");
static int Sum(int n)
{
    // Base case
    if (n <= 0)
    {
        return 0;
    }
    // Recursive case
    return n + Sum(n - 1);
}