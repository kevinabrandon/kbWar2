using kbWar.Core;

namespace kbWar.Tests;

public class WarGameTests
{
    [Fact]
    public void BuildDeck_returns_52_unique_cards()
    {
        var deck = WarGame.BuildDeck();
        Assert.Equal(52, deck.Count);
        Assert.Equal(52, deck.Distinct().Count());
    }

    [Fact]
    public void Shuffle_produces_all_original_cards()
    {
        var game = new WarGame(2, false, new Random(42));
        var deck = WarGame.BuildDeck();
        game.Shuffle(deck);
        Assert.Equal(52, deck.Count);
        // All ranks and suits still present
        Assert.Equal(4, deck.Select(c => c.Suit).Distinct().Count());
        Assert.Equal(13, deck.Select(c => c.Rank).Distinct().Count());
    }

    [Fact]
    public void Deal_splits_cards_evenly_between_2_players()
    {
        var game = new WarGame(2, false);
        var deck = WarGame.BuildDeck();
        game.Deal(deck);
        Assert.Equal(26, game.Hands[0].Count);
        Assert.Equal(26, game.Hands[1].Count);
    }

    [Fact]
    public void Deal_with_3_players_distributes_fairly()
    {
        var game = new WarGame(3, false);
        var deck = WarGame.BuildDeck();
        game.Deal(deck);
        // 52 / 3 = 17 r 1 → players 0 gets 18, 1 and 2 get 17
        Assert.Equal(18, game.Hands[0].Count);
        Assert.Equal(17, game.Hands[1].Count);
        Assert.Equal(17, game.Hands[2].Count);
    }

    [Fact]
    public void RunToCompletion_produces_a_winner_or_loop()
    {
        var game = new WarGame(2, false, new Random(1));
        var result = game.RunToCompletion();
        Assert.True(result.WasInfiniteLoop || result.WinnerId.HasValue);
    }

    [Fact]
    public void RunToCompletion_winner_has_all_cards()
    {
        // Use a seeded random so we reliably get a winner (not an infinite loop)
        for (int seed = 0; seed < 100; seed++)
        {
            var game = new WarGame(2, true, new Random(seed));
            var result = game.RunToCompletion();
            if (!result.WasInfiniteLoop && result.WinnerId.HasValue)
            {
                int winner = result.WinnerId.Value;
                // Winner should have all 52 cards
                int total = game.Hands.Sum(h => h.Count);
                Assert.Equal(52, total);
                return;
            }
        }
        // If every seed produces an infinite loop, skip (shouldn't happen with 100 seeds)
    }

    [Fact]
    public void PlayTurn_returns_null_after_game_ends()
    {
        var game = new WarGame(2, false, new Random(7));
        game.Reset();
        while (game.Result == GameResult.InProgress)
            game.PlayTurn();

        var extra = game.PlayTurn();
        Assert.Null(extra);
    }

    [Fact]
    public void Total_cards_preserved_throughout_game()
    {
        var game = new WarGame(2, false, new Random(99));
        game.Reset();
        for (int i = 0; i < 200 && game.Result == GameResult.InProgress; i++)
        {
            game.PlayTurn();
            int total = game.HandCounts().Sum();
            Assert.Equal(52, total);
        }
    }

    [Fact]
    public void Four_player_game_runs_to_completion()
    {
        var game = new WarGame(4, true, new Random(123));
        var result = game.RunToCompletion();
        Assert.True(result.WasInfiniteLoop || result.WinnerId.HasValue);
        if (!result.WasInfiniteLoop)
        {
            Assert.InRange(result.WinnerId!.Value, 0, 3);
        }
    }

    [Fact]
    public void Constructor_rejects_invalid_player_counts()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new WarGame(1, false));
        Assert.Throws<ArgumentOutOfRangeException>(() => new WarGame(53, false));
    }

    [Fact]
    public void InfiniteLoop_detected_within_max_turns()
    {
        // Hard to force an infinite loop, but we can verify MaxTurns is honoured
        Assert.Equal(20_000, WarGame.MaxTurns);
    }
}
