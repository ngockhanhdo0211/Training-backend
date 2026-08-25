TreeNode nodeA = new TreeNode("A: Bình luận gốc");
TreeNode nodeB = new TreeNode("B: Phản hồi A");
TreeNode nodeC = new TreeNode("C: Phản hồi A");
TreeNode nodeD = new TreeNode("D: Phản hồi B");
TreeNode nodeE = new TreeNode("E: Phản hồi B");
TreeNode nodeF = new TreeNode("F: Phản hồi C");

// Thiết lập quan hệ cha - con
nodeA.Children.Add(nodeB);
nodeA.Children.Add(nodeC);

nodeB.Children.Add(nodeD);
nodeB.Children.Add(nodeE);

nodeC.Children.Add(nodeF);

// 1. In cây theo cấu trúc phân cấp
Console.WriteLine("Cấu trúc cây:");
PrintTree(nodeA, 0);

// 2. Flatten bằng đệ quy
List<TreeNode> flattenedNodes = new();
HashSet<TreeNode> visitedNodes = new();
FlattenRecursive(nodeA, flattenedNodes, visitedNodes);

PrintNodes("Flatten bằng đệ quy:", flattenedNodes);

// 3. Flatten không dùng đệ quy, thay call stack bằng Stack<TreeNode>
List<TreeNode> iterativeNodes = FlattenIterative(nodeA);

PrintNodes("Flatten bằng Stack:", iterativeNodes);

// Hai thuật toán phải tạo ra cùng thứ tự node.
bool sameOrder = flattenedNodes.SequenceEqual(iterativeNodes);
Console.WriteLine($"\nHai kết quả giống nhau: {sameOrder}");

static void PrintTree(TreeNode node, int depth)
{
    // Mỗi depth được thụt vào 4 dấu cách
    string indentation = new string(' ', depth * 4);

    Console.WriteLine($"{indentation}- {node.Name}");

    foreach (TreeNode child in node.Children)
    {
        PrintTree(child, depth + 1);
    }
}

static void FlattenRecursive(
    TreeNode node,
    List<TreeNode> result,
    HashSet<TreeNode> visited)
{
    // Không xử lý lại node đã được duyệt
    if (!visited.Add(node))
    {
        return;
    }

    // Đưa node hiện tại vào danh sách trước
    result.Add(node);

    // Tiếp tục flatten toàn bộ cây con
    foreach (TreeNode child in node.Children)
    {
        FlattenRecursive(child, result, visited);
    }
}

static List<TreeNode> FlattenIterative(TreeNode root)
{
    List<TreeNode> result = new();
    HashSet<TreeNode> visited = new();
    Stack<TreeNode> stack = new();

    stack.Push(root);

    while (stack.Count > 0)
    {
        TreeNode current = stack.Pop();

        // Bỏ qua node đã duyệt để tránh lặp vô hạn nếu dữ liệu có cycle.
        if (!visited.Add(current))
        {
            continue;
        }

        result.Add(current);

        // Stack là LIFO. Push từ phải sang trái để khi Pop vẫn duyệt
        // các node con theo thứ tự từ trái sang phải như bản đệ quy.
        for (int i = current.Children.Count - 1; i >= 0; i--)
        {
            stack.Push(current.Children[i]);
        }
    }

    return result;
}

static void PrintNodes(string title, IEnumerable<TreeNode> nodes)
{
    Console.WriteLine($"\n{title}");

    foreach (TreeNode node in nodes)
    {
        Console.WriteLine(node.Name);
    }
}

class TreeNode
{
    public string Name { get; set; }

    public List<TreeNode> Children { get; } = new();

    public TreeNode(string name)
    {
        Name = name;
    }
}
