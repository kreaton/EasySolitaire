namespace EasySolitaire.Engine;

/// <summary>Hearts and Diamonds are red, Spades and Clubs are black.</summary>
public enum Suit { Hearts = 0, Diamonds = 1, Spades = 2, Clubs = 3 }

public readonly record struct Card(Suit Suit, int Rank)
{
    public bool IsRed => Suit is Suit.Hearts or Suit.Diamonds;

    /// <summary>Unique id, handy as a Blazor @key.</summary>
    public int Id => (int)Suit * 100 + Rank;

    /// <summary>Unique number 0..51, used for bit masks and hashing.</summary>
    public int Bit => (int)Suit * 13 + (Rank - 1);
}

/// <summary>Everything about how cards are written on screen.</summary>
public static class CardText
{
    // The ︎ selector asks the font for the plain text glyph instead of the emoji one.
    public static string Symbol(Suit suit) => suit switch
    {
        Suit.Hearts => "♥︎",
        Suit.Diamonds => "♦︎",
        Suit.Spades => "♠︎",
        _ => "♣︎",
    };

    /// <summary>The cute animal that replaces each suit (2 red-bordered, 2 black-bordered).</summary>
    public static string Animal(Suit suit) => suit switch
    {
        Suit.Hearts => "\U0001F431",   // cat   (red border)
        Suit.Diamonds => "\U0001F436", // dog   (red border)
        Suit.Spades => "\U0001F43C",   // panda (black border)
        _ => "\U0001F438",             // frog  (black border)
    };

    public static string AnimalName(Suit suit) => suit switch
    {
        Suit.Hearts => "kat",
        Suit.Diamonds => "hund",
        Suit.Spades => "panda",
        _ => "frø",
    };

    public static string SuitName(Suit suit) => suit switch
    {
        Suit.Hearts => "hjerter",
        Suit.Diamonds => "ruder",
        Suit.Spades => "spar",
        _ => "klør",
    };

    /// <summary>Short text on the card. The big deck shows E B D K (es, bonde, dame, konge), the small levels show plain numbers.</summary>
    public static string Label(int rank, int maxRank) =>
        maxRank == 13
            ? rank switch { 1 => "E", 11 => "B", 12 => "D", 13 => "K", _ => rank.ToString() }
            : rank.ToString();

    /// <summary>Spoken name (indefinite), e.g. "5'er", "konge".</summary>
    public static string Name(int rank, int maxRank) =>
        maxRank == 13
            ? rank switch { 1 => "es", 11 => "bonde", 12 => "dame", 13 => "konge", _ => $"{rank}'er" }
            : $"{rank}'er";

    /// <summary>Definite form, e.g. "5'eren", "kongen", "esset".</summary>
    public static string DefiniteName(int rank, int maxRank) =>
        maxRank == 13
            ? rank switch { 1 => "esset", 11 => "bonden", 12 => "damen", 13 => "kongen", _ => $"{rank}'eren" }
            : $"{rank}'eren";

    /// <summary>"det" for es (et es), otherwise "den".</summary>
    public static string Article(int rank, int maxRank) =>
        maxRank == 13 && rank == 1 ? "det" : "den";
}
