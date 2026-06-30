using kbWar.Core;

namespace kbWar.Tests;

public class CardHandTests
{
    [Fact]
    public void New_hand_is_empty()
    {
        var hand = new CardHand();
        Assert.True(hand.IsEmpty);
        Assert.Equal(0, hand.Count);
    }

    [Fact]
    public void Add_increases_count()
    {
        var hand = new CardHand();
        hand.Add(new PlayingCard(Suit.Clubs, Rank.Ace));
        Assert.Equal(1, hand.Count);
        Assert.False(hand.IsEmpty);
    }

    [Fact]
    public void Draw_returns_cards_in_FIFO_order()
    {
        var hand = new CardHand();
        var first = new PlayingCard(Suit.Clubs, Rank.Two);
        var second = new PlayingCard(Suit.Hearts, Rank.Three);
        hand.Add(first);
        hand.Add(second);

        Assert.Same(first, hand.Draw());
        Assert.Same(second, hand.Draw());
    }

    [Fact]
    public void AddRange_adds_all_cards()
    {
        var hand = new CardHand();
        var cards = new List<PlayingCard>
        {
            new(Suit.Clubs, Rank.Two),
            new(Suit.Diamonds, Rank.Three),
            new(Suit.Hearts, Rank.Four),
        };
        hand.AddRange(cards);
        Assert.Equal(3, hand.Count);
    }

    [Fact]
    public void ToList_reflects_current_order()
    {
        var hand = new CardHand();
        var c1 = new PlayingCard(Suit.Spades, Rank.Ace);
        var c2 = new PlayingCard(Suit.Clubs, Rank.King);
        hand.Add(c1);
        hand.Add(c2);

        var list = hand.ToList();
        Assert.Equal(2, list.Count);
        Assert.Same(c1, list[0]);
        Assert.Same(c2, list[1]);
    }
}
