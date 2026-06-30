namespace kbWar.Core;

public enum GameResult { InProgress, Winner, InfiniteLoop }

public record WarRound(List<(int Player, List<PlayingCard> FaceDownCards, PlayingCard? FaceUpCard)> PlayerCards);

public class TurnResult
{
    public int TurnNumber { get; init; }
    public required List<(int Player, PlayingCard Card)> InitialDraw { get; init; }
    public List<WarRound>? WarRounds { get; init; }
    public int? TurnWinnerId { get; init; }
    public GameResult GameResult { get; init; }
    public int[] HandCounts { get; init; } = [];
    public int? GameWinnerId { get; init; }
}

public class SimulationResult
{
    public int TotalTurns { get; init; }
    public int? WinnerId { get; init; }
    public bool WasInfiniteLoop { get; init; }
}

public class WarGame
{
    public const int MaxTurns = 20_000;

    public int PlayerCount { get; }
    public bool ShuffleRecentlyWonCards { get; }

    private readonly CardHand[] _hands;
    private readonly Random _rng;
    private int _turn;
    private GameResult _result = GameResult.InProgress;
    private int? _gameWinnerId;

    public int Turn => _turn;
    public GameResult Result => _result;
    public int? GameWinnerId => _gameWinnerId;
    public IReadOnlyList<CardHand> Hands => _hands;

    public WarGame(int playerCount, bool shuffleRecentlyWonCards, Random? rng = null)
    {
        if (playerCount < 2 || playerCount > 52)
            throw new ArgumentOutOfRangeException(nameof(playerCount), "Must be 2–52");
        PlayerCount = playerCount;
        ShuffleRecentlyWonCards = shuffleRecentlyWonCards;
        _rng = rng ?? new Random();
        _hands = Enumerable.Range(0, playerCount).Select(_ => new CardHand()).ToArray();
    }

    public static List<PlayingCard> BuildDeck()
    {
        var deck = new List<PlayingCard>(52);
        foreach (Suit s in Enum.GetValues<Suit>())
            foreach (Rank r in Enum.GetValues<Rank>())
                deck.Add(new PlayingCard(s, r));
        return deck;
    }

    public void Shuffle(List<PlayingCard> deck)
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (deck[i], deck[j]) = (deck[j], deck[i]);
        }
    }

    public void Deal(List<PlayingCard> deck)
    {
        for (int i = 0; i < deck.Count; i++)
            _hands[i % PlayerCount].Add(deck[i]);
    }

    // Reset all hands and deal a fresh shuffled deck
    public void Reset()
    {
        for (int i = 0; i < PlayerCount; i++) _hands[i] = new CardHand();
        var deck = BuildDeck();
        Shuffle(deck);
        Deal(deck);
        _turn = 0;
        _result = GameResult.InProgress;
        _gameWinnerId = null;
    }

    public TurnResult? PlayTurn()
    {
        if (_result != GameResult.InProgress) return null;

        _turn++;

        if (_turn > MaxTurns)
        {
            _result = GameResult.InfiniteLoop;
            return new TurnResult
            {
                TurnNumber = _turn,
                InitialDraw = [],
                GameResult = _result,
                HandCounts = HandCounts()
            };
        }

        var active = ActivePlayers();
        if (active.Count <= 1)
        {
            _gameWinnerId = active.Count == 1 ? active[0] : null;
            _result = GameResult.Winner;
            return new TurnResult
            {
                TurnNumber = _turn,
                InitialDraw = [],
                GameWinnerId = _gameWinnerId,
                GameResult = _result,
                HandCounts = HandCounts()
            };
        }

        var pot = new List<PlayingCard>();
        List<WarRound>? warRounds = null;

        // Initial draw — each active player draws 1 face-up card
        var initialDraw = active
            .Select(p => (Player: p, Card: _hands[p].Draw()))
            .ToList();
        pot.AddRange(initialDraw.Select(d => d.Card));

        var contested = initialDraw; // current face-up cards being compared

        while (true)
        {
            var maxRank = contested.Max(d => d.Card.Rank);
            var leaders = contested.Where(d => d.Card.Rank == maxRank).ToList();

            if (leaders.Count == 1)
            {
                AwardPot(leaders[0].Player, pot);
                return BuildResult(leaders[0].Player, initialDraw, warRounds, pot);
            }

            // War: each leader draws up to 3 face-down + 1 face-up
            warRounds ??= [];
            var roundEntries = new List<(int Player, List<PlayingCard> FaceDownCards, PlayingCard? FaceUpCard)>();
            var nextContest = new List<(int Player, PlayingCard Card)>();

            foreach (var leader in leaders)
            {
                int p = leader.Player;
                if (_hands[p].IsEmpty) continue; // knocked out during war

                var faceDown = new List<PlayingCard>();
                for (int i = 0; i < 3 && !_hands[p].IsEmpty; i++)
                {
                    var c = _hands[p].Draw();
                    faceDown.Add(c);
                    pot.Add(c);
                }

                PlayingCard? faceUp = null;
                if (!_hands[p].IsEmpty)
                {
                    faceUp = _hands[p].Draw();
                    pot.Add(faceUp);
                    nextContest.Add((p, faceUp));
                }

                roundEntries.Add((p, faceDown, faceUp));
            }

            warRounds.Add(new WarRound(roundEntries));

            if (nextContest.Count == 0)
            {
                // All war participants ran out; give pot to first leader
                AwardPot(leaders[0].Player, pot);
                return BuildResult(leaders[0].Player, initialDraw, warRounds, pot);
            }

            if (nextContest.Count == 1)
            {
                AwardPot(nextContest[0].Player, pot);
                return BuildResult(nextContest[0].Player, initialDraw, warRounds, pot);
            }

            contested = nextContest;
        }
    }

    public SimulationResult RunToCompletion()
    {
        Reset();
        while (_result == GameResult.InProgress)
            PlayTurn();
        return new SimulationResult
        {
            TotalTurns = _turn,
            WinnerId = _gameWinnerId,
            WasInfiniteLoop = _result == GameResult.InfiniteLoop
        };
    }

    private void AwardPot(int winner, List<PlayingCard> pot)
    {
        var cards = ShuffleRecentlyWonCards ? ShuffledCopy(pot) : pot;
        _hands[winner].AddRange(cards);

        var remaining = ActivePlayers();
        if (remaining.Count == 1)
        {
            _gameWinnerId = remaining[0];
            _result = GameResult.Winner;
        }
        else if (remaining.Count == 0 || _turn >= MaxTurns)
        {
            _result = _turn >= MaxTurns ? GameResult.InfiniteLoop : GameResult.Winner;
        }
    }

    private TurnResult BuildResult(int turnWinner, List<(int Player, PlayingCard Card)> initialDraw,
        List<WarRound>? warRounds, List<PlayingCard> pot)
    {
        return new TurnResult
        {
            TurnNumber = _turn,
            InitialDraw = initialDraw,
            WarRounds = warRounds,
            TurnWinnerId = turnWinner,
            GameResult = _result,
            GameWinnerId = _gameWinnerId,
            HandCounts = HandCounts()
        };
    }

    private List<PlayingCard> ShuffledCopy(List<PlayingCard> cards)
    {
        var copy = new List<PlayingCard>(cards);
        Shuffle(copy);
        return copy;
    }

    private List<int> ActivePlayers() =>
        Enumerable.Range(0, PlayerCount).Where(i => !_hands[i].IsEmpty).ToList();

    public int[] HandCounts() => _hands.Select(h => h.Count).ToArray();
}
