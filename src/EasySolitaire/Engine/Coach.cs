namespace EasySolitaire.Engine;

/// <summary>Short, friendly sentences (in Danish) that explain WHY a move was allowed.</summary>
public static class Coach
{
    /// <summary>Definite adjective form: "den røde", "den sorte".</summary>
    private static string Color(Card c) => c.IsRed ? "røde" : "sorte";

    /// <summary>"den røde 5'er", "det sorte es".</summary>
    private static string Desc(Card c, int max) =>
        $"{CardText.Article(c.Rank, max)} {Color(c)} {CardText.Name(c.Rank, max)}";

    private static string Cap(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];

    public static string Explain(MoveInfo move, int maxRank, bool animals)
    {
        string text = move.Kind switch
        {
            MoveKind.WasteToHome or MoveKind.ColumnToHome =>
                $"Hjem med {Desc(move.Card, maxRank)}! Hver hjemmebunke har {(animals ? "sit eget dyr" : "sin egen kulør")} og tæller opad: 1, 2, 3…",

            _ when move.Onto is { } onto =>
                $"{Cap(Desc(move.Card, maxRank))} passer på {Desc(onto, maxRank)}: ét tal lavere og en anden farve.",

            _ => $"Kun {CardText.DefiniteName(maxRank, maxRank)} kan starte en tom kolonne.",
        };

        if (move.Count > 1) text += " Kortene ovenpå kom med!";
        if (move.Revealed is not null) text += " Et skjult kort blev vendt!";
        return text;
    }

    public static string CannotMove(bool fromDeckPile) => fromDeckPile
        ? "Det kort kan ikke komme nogen steder endnu. Vend et nyt kort fra bunken!"
        : "Det kort kan ikke flyttes endnu. Det skal have et kort, der er ét tal højere og har den anden farve.";
}
