using kbWar.Core;

namespace kbWar.Tests;

public class PlayingCardTests
{
    [Fact]
    public void Rank_comparison_ace_beats_two()
    {
        var ace = new PlayingCard(Suit.Spades, Rank.Ace);
        var two = new PlayingCard(Suit.Hearts, Rank.Two);
        Assert.True(ace.CompareTo(two) > 0);
        Assert.True(two.CompareTo(ace) < 0);
    }

    [Fact]
    public void Same_rank_compares_equal()
    {
        var a = new PlayingCard(Suit.Spades, Rank.King);
        var b = new PlayingCard(Suit.Clubs, Rank.King);
        Assert.Equal(0, a.CompareTo(b));
    }

    [Fact]
    public void ToString_includes_rank_and_suit()
    {
        var card = new PlayingCard(Suit.Hearts, Rank.Queen);
        Assert.Equal("Queen of Hearts", card.ToString());
    }

    [Fact]
    public void CompareTo_null_returns_positive()
    {
        var card = new PlayingCard(Suit.Clubs, Rank.Two);
        Assert.True(card.CompareTo(null) > 0);
    }
}
