using System.Diagnostics;

namespace EasySolitaire.Engine;

/// <summary>
/// Checks whether a deal can be won, so kids don't get stuck on an impossible game.
///
/// Because the deck can be turned over again and again, every card in the deck/turned pile can be
/// brought to the top for free. The search therefore treats all of them as one pool of playable cards.
/// Every solution it finds is a real, legal game - it only ever misses some winnable deals.
/// </summary>
public static class Solver
{
    internal enum Kind { Run, ColumnHome, PoolHome, PoolColumn }

    internal readonly record struct Move(Kind Kind, int A, int B, int C);

    public static bool IsWinnable(GameState start, int maxNodes, out int nodesUsed)
    {
        var state = start.Clone();
        state.Stock.AddRange(state.Waste);
        state.Waste.Clear();

        var seen = new HashSet<ulong>();
        int nodes = 0;
        bool ok = Search(state, seen, ref nodes, maxNodes, null);
        nodesUsed = nodes;
        return ok;
    }

    /// <summary>Same search, but also returns the winning moves (used by the tests to replay a solution).</summary>
    internal static List<(Move Move, Card PoolCard)>? FindSolution(GameState start, int maxNodes)
    {
        var state = start.Clone();
        state.Stock.AddRange(state.Waste);
        state.Waste.Clear();

        var path = new List<(Move, Card)>();
        int nodes = 0;
        if (!Search(state, new HashSet<ulong>(), ref nodes, maxNodes, path)) return null;
        path.Reverse();
        return path;
    }

    private static bool Search(GameState s, HashSet<ulong> seen, ref int nodes, int maxNodes, List<(Move, Card)>? path)
    {
        if (++nodes > maxNodes) return false;

        AutoHome(s);
        if (s.IsWon) return true;
        if (!seen.Add(Hash(s))) return false;

        foreach (var move in GenerateMoves(s))
        {
            var next = s.Clone();
            Apply(next, move);
            if (Search(next, seen, ref nodes, maxNodes, path))
            {
                path?.Add((move, move.Kind is Kind.PoolHome or Kind.PoolColumn ? s.Stock[move.A] : default));
                return true;
            }
            if (nodes > maxNodes) return false;
        }
        return false;
    }

    private static List<Move> GenerateMoves(GameState s)
    {
        var moves = new List<Move>();
        var pool = s.Stock;
        int n = s.Columns.Length;

        // 1. Move a whole face-up run so that a hidden card gets turned over.
        for (int i = 0; i < n; i++)
        {
            var col = s.Columns[i];
            if (col.FaceDown == 0) continue;
            var head = col.Cards[col.FaceDown];
            bool triedEmpty = false;
            for (int j = 0; j < n; j++)
            {
                if (j == i || !s.CanGoOnColumn(head, j)) continue;
                if (s.Columns[j].IsEmpty) { if (triedEmpty) continue; triedEmpty = true; }
                moves.Add(new Move(Kind.Run, i, col.FaceDown, j));
            }
        }

        // 2. Last card of a column goes home.
        for (int i = 0; i < n; i++)
            if (!s.Columns[i].IsEmpty && s.CanGoHome(s.Columns[i].Top))
                moves.Add(new Move(Kind.ColumnHome, i, 0, 0));

        // 3. A pool card goes home.
        for (int p = 0; p < pool.Count; p++)
            if (s.CanGoHome(pool[p]))
                moves.Add(new Move(Kind.PoolHome, p, 0, 0));

        // 4. Empty a column by moving all of it onto another column.
        for (int i = 0; i < n; i++)
        {
            var col = s.Columns[i];
            if (col.IsEmpty || col.FaceDown != 0) continue;
            for (int j = 0; j < n; j++)
                if (j != i && !s.Columns[j].IsEmpty && s.CanGoOnColumn(col.Cards[0], j))
                    moves.Add(new Move(Kind.Run, i, 0, j));
        }

        // 5. A pool card goes onto a column.
        for (int p = 0; p < pool.Count; p++)
        {
            bool triedEmpty = false;
            for (int j = 0; j < n; j++)
            {
                if (!s.CanGoOnColumn(pool[p], j)) continue;
                if (s.Columns[j].IsEmpty) { if (triedEmpty) continue; triedEmpty = true; }
                moves.Add(new Move(Kind.PoolColumn, p, j, 0));
            }
        }

        return moves;
    }

    private static void Apply(GameState s, Move m)
    {
        switch (m.Kind)
        {
            case Kind.Run:
            {
                var from = s.Columns[m.A];
                int count = from.Cards.Count - m.B;
                s.Columns[m.C].Cards.AddRange(from.Cards.GetRange(m.B, count));
                from.Cards.RemoveRange(m.B, count);
                s.FlipIfNeeded(from);
                break;
            }
            case Kind.ColumnHome:
            {
                var col = s.Columns[m.A];
                var card = col.Top;
                col.Cards.RemoveAt(col.Cards.Count - 1);
                s.Home[(int)card.Suit] = card.Rank;
                s.FlipIfNeeded(col);
                break;
            }
            case Kind.PoolHome:
            {
                var card = s.Stock[m.A];
                s.Stock.RemoveAt(m.A);
                s.Home[(int)card.Suit] = card.Rank;
                break;
            }
            case Kind.PoolColumn:
            {
                var card = s.Stock[m.A];
                s.Stock.RemoveAt(m.A);
                s.Columns[m.B].Cards.Add(card);
                break;
            }
        }
    }

    /// <summary>Plays every "obviously good" home move: it can never block another card.</summary>
    internal static void AutoHome(GameState s)
    {
        bool moved;
        do
        {
            moved = false;
            foreach (var col in s.Columns)
            {
                if (col.IsEmpty || !IsSafe(s, col.Top)) continue;
                var card = col.Top;
                col.Cards.RemoveAt(col.Cards.Count - 1);
                s.Home[(int)card.Suit] = card.Rank;
                s.FlipIfNeeded(col);
                moved = true;
            }
            for (int p = s.Stock.Count - 1; p >= 0; p--)
            {
                var card = s.Stock[p];
                if (!IsSafe(s, card)) continue;
                s.Stock.RemoveAt(p);
                s.Home[(int)card.Suit] = card.Rank;
                moved = true;
            }
        } while (moved);
    }

    internal static bool IsSafe(GameState s, Card card)
    {
        if (!s.CanGoHome(card)) return false;
        if (card.Rank <= 2) return true;
        int need = card.Rank - 1;
        return card.IsRed
            ? s.Home[(int)Suit.Spades] >= need && s.Home[(int)Suit.Clubs] >= need
            : s.Home[(int)Suit.Hearts] >= need && s.Home[(int)Suit.Diamonds] >= need;
    }

    private static ulong Hash(GameState s)
    {
        ulong h = 14695981039346656037UL;
        foreach (var col in s.Columns)
        {
            Mix(ref h, 1000UL + (ulong)col.FaceDown);
            foreach (var c in col.Cards) Mix(ref h, (ulong)(c.Bit + 1));
        }
        foreach (var home in s.Home) Mix(ref h, 2000UL + (ulong)home);
        ulong mask = 0;
        foreach (var c in s.Stock) mask |= 1UL << c.Bit;
        Mix(ref h, mask);
        return h;
    }

    private static void Mix(ref ulong h, ulong value)
    {
        h ^= value;
        h *= 1099511628211UL;
        h ^= h >> 29;
    }
}

public static class DealGenerator
{
    /// <summary>
    /// Deals random games until the solver finds that one can be won (or the time budget is used up,
    /// in which case the last deal is returned anyway).
    /// </summary>
    public static GameState NewDeal(LevelInfo level, Random rng, int maxMilliseconds = 400, int nodeBudget = 4000)
    {
        var clock = Stopwatch.StartNew();
        GameState deal;
        do
        {
            deal = GameState.Deal(level, rng);
            if (Solver.IsWinnable(deal, nodeBudget, out _)) return deal;
        } while (clock.ElapsedMilliseconds < maxMilliseconds);
        return deal;
    }
}
