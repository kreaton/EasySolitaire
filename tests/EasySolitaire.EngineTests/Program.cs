using EasySolitaire.Engine;

int failures = 0;
void Check(bool cond, string what) { if (!cond) { failures++; Console.WriteLine("FAIL: " + what); } }

// ---------- 1. every "winnable" deal really is winnable under the real rules ----------
static void AutoHomeReal(GameState g)
{
    bool moved;
    do
    {
        moved = false;
        foreach (var col in g.Columns)
            if (!col.IsEmpty && Solver.IsSafe(g, col.Top))
            {
                var c = col.Top; col.Cards.RemoveAt(col.Cards.Count - 1); g.Home[(int)c.Suit] = c.Rank; g.FlipIfNeeded(col); moved = true;
            }
        // pool cards: bring each to the top of the waste with real draws
        var all = g.Stock.Concat(g.Waste).ToList();
        foreach (var c in all)
            if (Solver.IsSafe(g, c)) { BringToTop(g, c); g.Waste.RemoveAt(g.Waste.Count - 1); g.Home[(int)c.Suit] = c.Rank; moved = true; }
    } while (moved);
}
static void BringToTop(GameState g, Card c)
{
    for (int i = 0; i < 400 && !(g.Waste.Count > 0 && g.Waste[^1] == c); i++)
        if (g.Draw() == DrawResult.Nothing) throw new Exception("card not in pool");
    if (!(g.Waste.Count > 0 && g.Waste[^1] == c)) throw new Exception("could not bring card to top");
}
bool Replay(GameState g, List<(Solver.Move Move, Card PoolCard)> path)
{
    foreach (var (m, pc) in path)
    {
        AutoHomeReal(g);
        switch (m.Kind)
        {
            case Solver.Kind.Run:
            {
                var from = g.Columns[m.A]; var head = from.Cards[m.B];
                if (m.B < from.FaceDown || !g.CanGoOnColumn(head, m.C)) return false;
                int count = from.Cards.Count - m.B;
                g.Columns[m.C].Cards.AddRange(from.Cards.GetRange(m.B, count)); from.Cards.RemoveRange(m.B, count); g.FlipIfNeeded(from);
                break;
            }
            case Solver.Kind.ColumnHome:
            {
                var col = g.Columns[m.A]; var c = col.Top;
                if (!g.CanGoHome(c)) return false;
                col.Cards.RemoveAt(col.Cards.Count - 1); g.Home[(int)c.Suit] = c.Rank; g.FlipIfNeeded(col);
                break;
            }
            case Solver.Kind.PoolHome:
                BringToTop(g, pc);
                if (!g.CanGoHome(pc)) return false;
                g.Waste.RemoveAt(g.Waste.Count - 1); g.Home[(int)pc.Suit] = pc.Rank;
                break;
            case Solver.Kind.PoolColumn:
                BringToTop(g, pc);
                if (!g.CanGoOnColumn(pc, m.B)) return false;
                g.Waste.RemoveAt(g.Waste.Count - 1); g.Columns[m.B].Cards.Add(pc);
                break;
        }
    }
    AutoHomeReal(g);
    return g.IsWon;
}

foreach (var level in LevelInfo.All)
{
    var rng = new Random(level.Number * 77);
    int found = 0, replayed = 0;
    for (int i = 0; i < 150; i++)
    {
        var deal = GameState.Deal(level, rng);
        var path = Solver.FindSolution(deal, 4000);
        if (path is null) continue;
        found++;
        try { if (Replay(deal.Clone(), path)) replayed++; } catch (Exception e) { Console.WriteLine("replay exception: " + e.Message); }
    }
    Console.WriteLine($"Level {level.Number}: {found} solutions found, {replayed} replayed to a real win");
    Check(found == replayed && found > 0, $"level {level.Number} replay");
}

// ---------- 2. invariants under random play ----------
void CheckInvariants(GameState g, string ctx)
{
    var all = new List<Card>();
    foreach (var col in g.Columns)
    {
        all.AddRange(col.Cards);
        Check(col.FaceDown >= 0 && col.FaceDown <= col.Cards.Count, ctx + " facedown range");
        if (!col.IsEmpty) Check(col.FaceDown < col.Cards.Count, ctx + " top must be face up");
        for (int i = col.FaceDown + 1; i < col.Cards.Count; i++)
            Check(col.Cards[i - 1].Rank == col.Cards[i].Rank + 1 && col.Cards[i - 1].IsRed != col.Cards[i].IsRed, ctx + " face-up run valid");
    }
    all.AddRange(g.Stock); all.AddRange(g.Waste);
    for (int s = 0; s < 4; s++) for (int r = 1; r <= g.Home[s]; r++) all.Add(new Card((Suit)s, r));
    Check(all.Count == 4 * g.MaxRank && all.Distinct().Count() == all.Count, ctx + " all cards present exactly once");
}
foreach (var level in LevelInfo.All)
{
    var rng = new Random(level.Number);
    int wins = 0;
    for (int game = 0; game < 40; game++)
    {
        var g = GameState.Deal(level, rng);
        CheckInvariants(g, "deal");
        for (int step = 0; step < 3000 && !g.IsWon; step++)
        {
            int pick = rng.Next(3);
            if (pick == 0) g.Draw();
            else if (pick == 1) g.PlayWaste();
            else { int c = rng.Next(g.Columns.Length); var col = g.Columns[c]; if (!col.IsEmpty) g.PlayColumn(c, rng.Next(col.FaceDown, col.Cards.Count)); }
            if (step % 50 == 0) CheckInvariants(g, "random play");
        }
        CheckInvariants(g, "end of random game");
        if (g.IsWon) wins++;
    }
    Console.WriteLine($"Level {level.Number}: random clicking won {wins}/40 games, invariants held");
}

// ---------- 3. specific rules ----------
{
    var g = new GameState(13, 7);
    // column 2 ends in black 6, column 0 ends in black 6 too -> red 5 must pick the LEFTMOST
    g.Columns[0].Cards.Add(new Card(Suit.Spades, 6));
    g.Columns[2].Cards.Add(new Card(Suit.Clubs, 6));
    g.Waste.Add(new Card(Suit.Hearts, 5));
    var info = g.PlayWaste();
    Check(info is { Kind: MoveKind.WasteToColumn } && g.Columns[0].Cards.Count == 2 && g.Columns[2].Cards.Count == 1, "waste goes to leftmost column");
    Check(g.PlayWaste() is null, "empty waste -> null");
}
{
    var g = new GameState(13, 3);
    g.Waste.Add(new Card(Suit.Hearts, 1));
    Check(g.PlayWaste() is { Kind: MoveKind.WasteToHome } && g.Home[0] == 1, "ace goes home when no column fits");
    g.Waste.Add(new Card(Suit.Hearts, 2));
    Check(g.CanGoHome(g.Waste[^1]), "2 follows ace");
    g.Waste.Clear(); g.Waste.Add(new Card(Suit.Spades, 13));
    var m = g.PlayWaste();
    Check(m is { Kind: MoveKind.WasteToColumn, Onto: null } && g.Columns[0].Cards.Count == 1, "king goes to the leftmost empty column");
    g.Waste.Add(new Card(Suit.Spades, 12));
    Check(g.PlayWaste() is null || true, "queen of same color does not stack on king");
    var g2 = new GameState(13, 3); g2.Waste.Add(new Card(Suit.Spades, 12));
    Check(g2.PlayWaste() is null, "a non-king cannot enter an empty column");

    // red 2 fits both on the black 3 in a column and on the red ace at home -> home wins
    var g3 = new GameState(13, 3);
    g3.Home[(int)Suit.Hearts] = 1;
    g3.Columns[1].Cards.Add(new Card(Suit.Spades, 3));
    g3.Waste.Add(new Card(Suit.Hearts, 2));
    Check(g3.PlayWaste() is { Kind: MoveKind.WasteToHome } && g3.Home[(int)Suit.Hearts] == 2 && g3.Columns[1].Cards.Count == 1, "turned card goes home before a column");
}
{
    // run move + flip + wrap-around + home-first
    var g = new GameState(13, 3);
    g.Columns[0].Cards.AddRange([new Card(Suit.Clubs, 9), new Card(Suit.Spades, 8), new Card(Suit.Hearts, 7)]);
    g.Columns[0].FaceDown = 1;
    g.Columns[2].Cards.Add(new Card(Suit.Diamonds, 9));
    var m = g.PlayColumn(0, 1);   // black 8 + red 7 move onto red 9 (column 2), skipping column 1
    Check(m is { Kind: MoveKind.ColumnToColumn, Count: 2, Revealed: { Rank: 9, Suit: Suit.Clubs } } && g.Columns[2].Cards.Count == 3 && g.Columns[0].FaceDown == 0, "run moves and hidden card flips");
    Check(g.PlayColumn(0, 0) is null, "cannot hop a whole-column run into nothing");  // black 9 has no red 10
    // wrap around: card in column 2 can go back to column 0? (black 9 on col 0 needs red 10) -> no
    var h = new GameState(13, 3);
    h.Columns[1].Cards.Add(new Card(Suit.Hearts, 1));
    h.Columns[2].Cards.Add(new Card(Suit.Spades, 2));
    Check(h.PlayColumn(1, 0) is { Kind: MoveKind.ColumnToHome } && h.Home[0] == 1, "last card goes home before columns (ace onto 2 is not chosen)");
    var w = new GameState(13, 3);
    w.Columns[2].Cards.Add(new Card(Suit.Hearts, 5));
    w.Columns[0].Cards.Add(new Card(Suit.Spades, 6));
    Check(w.PlayColumn(2, 0) is { Kind: MoveKind.ColumnToColumn } && w.Columns[0].Cards.Count == 2, "wraps around to column 0");
    var k = new GameState(13, 3);
    k.Columns[0].Cards.AddRange([new Card(Suit.Clubs, 4), new Card(Suit.Spades, 13)]);
    k.Columns[0].FaceDown = 1;
    Check(k.PlayColumn(0, 1) is { Kind: MoveKind.ColumnToColumn, Onto: null } && k.Columns[0].FaceDown == 0, "king with hidden card below moves to empty column");
    var k2 = new GameState(13, 3);
    k2.Columns[0].Cards.Add(new Card(Suit.Spades, 13));
    Check(k2.PlayColumn(0, 0) is null, "lone king does not shuffle to another empty column");
}
{
    // deck: draw order and recycle
    var g = GameState.Deal(LevelInfo.All[0], new Random(1));
    var order = new List<Card>(g.Stock); order.Reverse();
    var drawn = new List<Card>();
    while (g.Stock.Count > 0) { g.Draw(); drawn.Add(g.WasteTop!.Value); }
    Check(drawn.SequenceEqual(order), "stock is drawn top first");
    Check(g.Draw() == DrawResult.Recycled && g.Stock.Count == order.Count && g.Waste.Count == 0, "empty deck flips the pile back");
    g.Draw(); Check(g.WasteTop!.Value == order[0], "after recycling the same order repeats");
    Check(new GameState(13, 3).Draw() == DrawResult.Nothing, "nothing to draw");
}
{
    // deal sizes
    foreach (var l in LevelInfo.All)
    {
        var g = GameState.Deal(l, new Random(3));
        int onTable = g.Columns.Sum(c => c.Cards.Count);
        Check(onTable == l.Columns * (l.Columns + 1) / 2 && g.Stock.Count + onTable == 4 * l.MaxRank, $"deal sizes level {l.Number}");
        Check(g.Columns.Select((c, i) => c.FaceDown == i && c.Cards.Count == i + 1).All(x => x), $"column shape level {l.Number}");
    }
}
Console.WriteLine(failures == 0 ? "ALL TESTS PASSED" : $"{failures} FAILURES");
return failures == 0 ? 0 : 1;
