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

// Bắt đầu duyệt từ root có depth bằng 0
PrintTree(nodeA, 0);

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
class TreeNode
{
    public string Name { get; set; }

    public List<TreeNode> Children { get; set; } = new List<TreeNode>();

    public TreeNode(string name)
    {
        Name = name;
    }
}