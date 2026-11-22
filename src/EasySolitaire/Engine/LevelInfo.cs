namespace EasySolitaire.Engine;

/// <param name="Number">1, 2 or 3.</param>
/// <param name="MaxRank">Highest card in the deck (6, 10 or 13 = King).</param>
/// <param name="Columns">Number of columns on the table.</param>
public sealed record LevelInfo(int Number, int MaxRank, int Columns)
{
    public static readonly LevelInfo[] All =
    [
        new(1, 6, 3),   // cards 1-6,  24 cards, 3 columns
        new(2, 10, 5),  // cards 1-10, 40 cards, 5 columns
        new(3, 13, 7),  // cards 1-K,  52 cards, 7 columns (classic)
    ];

    public string RangeText => MaxRank == 13 ? "1–K" : $"1–{MaxRank}";

    public static LevelInfo FromNumber(int number) =>
        All.FirstOrDefault(l => l.Number == number) ?? All[0];
}
