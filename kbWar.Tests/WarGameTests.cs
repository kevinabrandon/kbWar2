using kbWar.Core;

namespace kbWar.Tests;

public class WarGameTests
{
    [Fact]
    public void Fresh_game_state_is_not_started()
    {
        var game = new kbCardGameWar(2, new Random(1));
        Assert.Equal(kbCardGameWar.GameState.eNotStarted, game.State);
    }

    [Fact]
    public void After_deal_state_is_currently_playing()
    {
        var game = new kbCardGameWar(2, new Random(1));
        game.ShuffleDeck();
        game.Deal();
        Assert.Equal(kbCardGameWar.GameState.eCurrentlyPlaying, game.State);
    }

    [Fact]
    public void Deal_distributes_all_52_cards_for_2_players()
    {
        var game = new kbCardGameWar(2, new Random(1));
        game.ShuffleDeck();
        game.Deal();
        Assert.Equal(26, game.GetPlayerCount(0));
        Assert.Equal(26, game.GetPlayerCount(1));
    }

    [Fact]
    public void Deal_distributes_cards_for_3_players()
    {
        var game = new kbCardGameWar(3, new Random(1));
        game.ShuffleDeck();
        game.Deal();
        int total = game.GetPlayerCount(0) + game.GetPlayerCount(1) + game.GetPlayerCount(2);
        Assert.Equal(52, total);
    }

    [Fact]
    public void PlayTillFinished_ends_with_winner_or_loop()
    {
        var game = new kbCardGameWar(2, new Random(1));
        game.ShuffleDeck();
        game.Deal();
        game.PlayTillFinished();
        Assert.True(
            game.State == kbCardGameWar.GameState.eOverWithWinner ||
            game.State == kbCardGameWar.GameState.eInfiniteLoop);
    }

    [Fact]
    public void Winner_has_all_52_cards()
    {
        // Try a few seeds to get a completing game
        for (int seed = 0; seed < 100; seed++)
        {
            var game = new kbCardGameWar(2, new Random(seed));
            game.ShuffleRecentlyWonCards = true;
            game.ShuffleDeck();
            game.Deal();
            game.PlayTillFinished();
            if (game.State == kbCardGameWar.GameState.eOverWithWinner)
            {
                Assert.Equal(52, game.GetPlayerCount(game.Winner));
                return;
            }
        }
    }

    [Fact]
    public void NewTurn_after_game_over_returns_same_state()
    {
        var game = new kbCardGameWar(2, new Random(7));
        game.ShuffleDeck();
        game.Deal();
        game.PlayTillFinished();
        var finalState = game.State;
        Assert.Equal(finalState, game.NewTurn());
    }

    [Fact]
    public void Turn_counter_increments_each_turn()
    {
        var game = new kbCardGameWar(2, new Random(42));
        game.ShuffleDeck();
        game.Deal();
        game.NewTurn();
        game.NewTurn();
        Assert.Equal(2, game.Counters.nTurns);
    }

    [Fact]
    public void MostRecentWinners_is_populated_after_turn()
    {
        var game = new kbCardGameWar(2, new Random(1));
        game.ShuffleDeck();
        game.Deal();
        game.NewTurn();
        Assert.NotEmpty(game.MostRecentWinners);
    }

    [Fact]
    public void Restart_resets_state()
    {
        var game = new kbCardGameWar(2, new Random(5));
        game.ShuffleDeck();
        game.Deal();
        game.NewTurn();
        game.Restart(2);
        Assert.Equal(kbCardGameWar.GameState.eNotStarted, game.State);
        Assert.Equal(0, game.Counters.nTurns);
    }

    [Fact]
    public void Constructor_rejects_invalid_player_counts()
    {
        Assert.Throws<Exception>(() => new kbCardGameWar(1, new Random()));
        Assert.Throws<Exception>(() => new kbCardGameWar(53, new Random()));
    }

    [Fact]
    public void Four_player_game_runs_to_completion()
    {
        var game = new kbCardGameWar(4, new Random(123));
        game.ShuffleRecentlyWonCards = true;
        game.ShuffleDeck();
        game.Deal();
        game.PlayTillFinished();
        Assert.True(
            game.State == kbCardGameWar.GameState.eOverWithWinner ||
            game.State == kbCardGameWar.GameState.eInfiniteLoop);
    }

    [Fact]
    public void Legacy_pot_order_conserves_all_52_cards()
    {
        var game = new kbCardGameWar(2, new Random(9));
        game.ShuffleRecentlyWonCards = false;
        game.LegacyPotOrder = true;
        game.ShuffleDeck();
        game.Deal();
        for (int i = 0; i < 50 && game.State == kbCardGameWar.GameState.eCurrentlyPlaying; i++)
        {
            game.NewTurn();
            Assert.Equal(52, game.GetPlayerCount(0) + game.GetPlayerCount(1));
        }
    }

    [Fact]
    public void Legacy_pot_order_loops_far_less_often_without_shuffling()
    {
        // The 2015 implementation interleaved the players' cards in the pot and
        // reversed them on pickup, which mixes the deck a little every war and
        // makes unending games much rarer (~10%) than the grouped pickup (~42%).
        int LoopCount(bool legacy)
        {
            var game = new kbCardGameWar(2, new Random(42));
            game.ShuffleRecentlyWonCards = false;
            game.LegacyPotOrder = legacy;
            int loops = 0;
            for (int i = 0; i < 300; i++)
            {
                game.Restart(2);
                game.ShuffleDeck();
                game.Deal();
                game.PlayTillFinished();
                if (game.State == kbCardGameWar.GameState.eInfiniteLoop) loops++;
            }
            return loops;
        }

        int legacyLoops = LoopCount(true);
        int groupedLoops = LoopCount(false);
        Assert.True(legacyLoops * 2 < groupedLoops,
            $"Expected legacy pot order to loop far less: legacy={legacyLoops}, grouped={groupedLoops}");
    }

    [Fact]
    public void Shuffle_off_can_produce_infinite_loop()
    {
        // Without shuffling, infinite loops are common — verify the heuristic works
        int loops = 0;
        for (int seed = 0; seed < 20; seed++)
        {
            var game = new kbCardGameWar(2, new Random(seed));
            game.ShuffleRecentlyWonCards = false;
            game.ShuffleDeck();
            game.Deal();
            game.PlayTillFinished();
            if (game.State == kbCardGameWar.GameState.eInfiniteLoop) loops++;
        }
        Assert.True(loops > 0, "Expected at least one infinite loop with no shuffling");
    }
}
