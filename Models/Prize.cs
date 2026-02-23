namespace lotteryapp.Models;

public class Prize
{
    public int DrawOrder { get; set; }
    public string PrizeName { get; set; } = string.Empty;
    public int Num { get; set; }

    public override string ToString() => PrizeName;
}
