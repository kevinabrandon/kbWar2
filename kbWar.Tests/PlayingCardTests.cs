using kbWar.Core;

namespace kbWar.Tests;

public class PlayingCardTests
{
    [Fact]
    public void Rank_deuce_is_lowest_ace_is_highest()
    {
        var deuce = new kbPlayingCard(kbPlayingCard.Suit.Hearts, kbPlayingCard.Rank.Deuce);
        var ace   = new kbPlayingCard(kbPlayingCard.Suit.Spades, kbPlayingCard.Rank.Ace);
        Assert.True((int)ace.rank > (int)deuce.rank);
    }

    [Fact]
    public void Rank_values_are_2_through_14()
    {
        Assert.Equal(2,  (int)kbPlayingCard.Rank.Deuce);
        Assert.Equal(14, (int)kbPlayingCard.Rank.Ace);
    }

    [Fact]
    public void ToString_includes_rank_and_suit()
    {
        var card = new kbPlayingCard(kbPlayingCard.Suit.Hearts, kbPlayingCard.Rank.Queen);
        Assert.Contains("Queen", card.ToString());
        Assert.Contains("Hearts", card.ToString());
    }

    [Fact]
    public void Suit_and_rank_properties_round_trip()
    {
        var card = new kbPlayingCard(kbPlayingCard.Suit.Clubs, kbPlayingCard.Rank.King);
        Assert.Equal(kbPlayingCard.Suit.Clubs, card.suit);
        Assert.Equal(kbPlayingCard.Rank.King, card.rank);
    }
}
