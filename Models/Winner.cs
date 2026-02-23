namespace lotteryapp.Models;

public class Winner
{
    public string DeptId { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PrizeName { get; set; } = string.Empty;
    public int WinOrder { get; set; }
}
