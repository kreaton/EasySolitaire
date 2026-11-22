namespace EasySolitaire.Engine;

/// <summary>One column on the table. The first <see cref="FaceDown"/> cards are face down.</summary>
public sealed class Column
{
    public List<Card> Cards { get; } = new();
    public int FaceDown { get; set; }

    public bool IsEmpty => Cards.Count == 0;
    public Card Top => Cards[^1];
    public int FaceUpCount => Cards.Count - FaceDown;

    public Column Clone()
    {
        var copy = new Column { FaceDown = FaceDown };
        copy.Cards.AddRange(Cards);
        return copy;
    }
}

public enum MoveKind { WasteToColumn, WasteToHome, ColumnToColumn, ColumnToHome }

/// <summary>What just happened - used to give kids a friendly explanation.</summary>
/// <param name="Onto">The card it was placed on (null when it went home or into an empty column).</param>
/// <param name="Revealed">A hidden card that was turned face up by this move, if any.</param>
public sealed record MoveInfo(MoveKind Kind, Card Card, Card? Onto, int Count, Card? Revealed);

public enum DrawResult { Nothing, Drew, Recycled }

/// <summary>
/// The whole table: columns, deck (stock), turned cards (waste) and the four home piles.
/// Draw-one Klondike with unlimited passes through the deck.
/// </summary>
public sealed class GameState
{
    public int MaxRank { get; }
    public Column[] Columns { get; }

    /// <summary>Face-down deck. The LAST element is the next card to be turned.</summary>
    public List<Card> Stock { get; } = new();

    /// <summary>Turned cards. The LAST element is the one on top (the playable one).</summary>
    public List<Card> Waste { get; } = new();

    /// <summary>Highest rank on each home pile, indexed by (int)Suit. 0 = empty.</summary>
    public int[] Home { get; } = new int[4];

    public GameState(int maxRank, int columns)
    {
        MaxRank = maxRank;
        Columns = new Column[columns];
        for (int i = 0; i < columns; i++) Columns[i] = new Column();
    }

    // ---------- Queries ----------

    public bool IsWon
    {
        get
        {
            foreach (var h in Home) if (h != MaxRank) return false;
            return true;
        }
    }

    public Card? WasteTop => Waste.Count > 0 ? Waste[^1] : null;

    /// <summary>A card goes home when it is the next one up for its own suit/animal.</summary>
    public bool CanGoHome(Card card) => Home[(int)card.Suit] == card.Rank - 1;

    /// <summary>
    /// A card goes on a column when it is one lower and the other color than the column's last card.
    /// An empty column only takes the highest card (King / 10 / 6).
    /// </summary>
    public bool CanGoOnColumn(Card card, int column)
    {
        var col = Columns[column];
        if (col.IsEmpty) return card.Rank == MaxRank;
        var top = col.Top;
        return top.Rank == card.Rank + 1 && top.IsRed != card.IsRed;
    }

    public int FirstColumnFor(Card card)
    {
        for (int i = 0; i < Columns.Length; i++)
            if (CanGoOnColumn(card, i)) return i;
        return -1;
    }

    public bool FitsAnywhere(Card card) => CanGoHome(card) || FirstColumnFor(card) >= 0;

    // ---------- Moves ----------

    /// <summary>Turn one card from the deck. With an empty deck, flip the turned cards back over.</summary>
    public DrawResult Draw()
    {
        if (Stock.Count > 0)
        {
            var card = Stock[^1];
            Stock.RemoveAt(Stock.Count - 1);
            Waste.Add(card);
            return DrawResult.Drew;
        }
        if (Waste.Count > 0)
        {
            for (int i = Waste.Count - 1; i >= 0; i--) Stock.Add(Waste[i]);
            Waste.Clear();
            return DrawResult.Recycled;
        }
        return DrawResult.Nothing;
    }

    /// <summary>
    /// Click on the turned card: it goes home first when it can,
    /// otherwise to the LEFTMOST column it fits on. Null = nowhere to go.
    /// </summary>
    public MoveInfo? PlayWaste()
    {
        if (Waste.Count == 0) return null;
        var card = Waste[^1];

        if (CanGoHome(card))
        {
            Waste.RemoveAt(Waste.Count - 1);
            Home[(int)card.Suit] = card.Rank;
            return new MoveInfo(MoveKind.WasteToHome, card, null, 1, null);
        }

        int to = FirstColumnFor(card);
        if (to >= 0)
        {
            Card? onto = Columns[to].IsEmpty ? null : Columns[to].Top;
            Waste.RemoveAt(Waste.Count - 1);
            Columns[to].Cards.Add(card);
            return new MoveInfo(MoveKind.WasteToColumn, card, onto, 1, null);
        }

        return null;
    }

    /// <summary>
    /// Click on a face-up card in a column. The card (with every card lying on it) moves to another
    /// place it fits: home first (only possible for the last card), otherwise the next column
    /// to the right that takes it, wrapping around. Null = nowhere to go.
    /// </summary>
    public MoveInfo? PlayColumn(int column, int index)
    {
        var from = Columns[column];
        if (index < from.FaceDown || index >= from.Cards.Count) return null;

        var head = from.Cards[index];
        bool isLast = index == from.Cards.Count - 1;

        if (isLast && CanGoHome(head))
        {
            from.Cards.RemoveAt(index);
            Home[(int)head.Suit] = head.Rank;
            return new MoveInfo(MoveKind.ColumnToHome, head, null, 1, FlipIfNeeded(from));
        }

        for (int step = 1; step < Columns.Length; step++)
        {
            int to = (column + step) % Columns.Length;
            if (!CanGoOnColumn(head, to)) continue;

            var dest = Columns[to];
            // Pointless: a run that already fills a whole column just hopping into an empty one.
            if (dest.IsEmpty && index == 0) continue;

            Card? onto = dest.IsEmpty ? null : dest.Top;
            int count = from.Cards.Count - index;
            dest.Cards.AddRange(from.Cards.GetRange(index, count));
            from.Cards.RemoveRange(index, count);
            return new MoveInfo(MoveKind.ColumnToColumn, head, onto, count, FlipIfNeeded(from));
        }

        return null;
    }

    /// <summary>When the last face-up card leaves, turn the next hidden card over.</summary>
    public Card? FlipIfNeeded(Column column)
    {
        if (column.Cards.Count > 0 && column.FaceDown == column.Cards.Count)
        {
            column.FaceDown--;
            return column.Cards[^1];
        }
        return null;
    }

    // ---------- Copy / deal ----------

    public GameState Clone()
    {
        var copy = new GameState(MaxRank, Columns.Length);
        for (int i = 0; i < Columns.Length; i++) copy.Columns[i] = Columns[i].Clone();
        copy.Stock.AddRange(Stock);
        copy.Waste.AddRange(Waste);
        Array.Copy(Home, copy.Home, Home.Length);
        return copy;
    }

    /// <summary>Classic deal: column n gets n cards, only the last one face up. The rest is the deck.</summary>
    public static GameState Deal(LevelInfo level, Random rng)
    {
        var deck = new List<Card>(level.MaxRank * 4);
        for (int s = 0; s < 4; s++)
            for (int r = 1; r <= level.MaxRank; r++)
                deck.Add(new Card((Suit)s, r));

        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (deck[i], deck[j]) = (deck[j], deck[i]);
        }

        var state = new GameState(level.MaxRank, level.Columns);
        int next = 0;
        for (int c = 0; c < level.Columns; c++)
        {
            for (int k = 0; k <= c; k++) state.Columns[c].Cards.Add(deck[next++]);
            state.Columns[c].FaceDown = c;
        }
        while (next < deck.Count) state.Stock.Add(deck[next++]);
        return state;
    }
}
