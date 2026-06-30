using kbWar.Core;

namespace kbWar.Tests;

public class CardHandTests
{
    [Fact]
    public void New_hand_is_empty()
    {
        var hand = new kbCardHand();
        Assert.Equal(0, hand.Count);
    }

    [Fact]
    public void AddToBottom_increases_count()
    {
        var hand = new kbCardHand();
        hand.AddToBottom(new kbPlayingCard(kbPlayingCard.Suit.Clubs, kbPlayingCard.Rank.Ace));
        Assert.Equal(1, hand.Count);
    }

    [Fact]
    public void DrawFromTop_returns_LIFO_when_added_to_top()
    {
        var hand = new kbCardHand();
        var first  = new kbPlayingCard(kbPlayingCard.Suit.Clubs, kbPlayingCard.Rank.Deuce);
        var second = new kbPlayingCard(kbPlayingCard.Suit.Hearts, kbPlayingCard.Rank.Trey);
        hand.AddToTop(first);
        hand.AddToTop(second);   // second is now on top

        Assert.Same(second, hand.DrawFromTop());
        Assert.Same(first,  hand.DrawFromTop());
    }

    [Fact]
    public void DrawFromBottom_returns_last_added_to_bottom()
    {
        var hand = new kbCardHand();
        var a = new kbPlayingCard(kbPlayingCard.Suit.Clubs,    kbPlayingCard.Rank.Four);
        var b = new kbPlayingCard(kbPlayingCard.Suit.Diamonds, kbPlayingCard.Rank.Five);
        hand.AddToBottom(a);
        hand.AddToBottom(b);

        Assert.Same(b, hand.DrawFromBottom());
        Assert.Same(a, hand.DrawFromBottom());
    }

    [Fact]
    public void Indexer_returns_correct_card()
    {
        var hand = new kbCardHand();
        var c = new kbPlayingCard(kbPlayingCard.Suit.Spades, kbPlayingCard.Rank.Ace);
        hand.AddToBottom(c);
        Assert.Same(c, hand[0]);
    }

    [Fact]
    public void kb52CardDeck_has_52_cards()
    {
        var deck = new kb52CardDeck();
        Assert.Equal(52, deck.Count);
    }

    [Fact]
    public void kb52CardDeck_has_4_of_each_rank()
    {
        var deck = new kb52CardDeck();
        var cards = Enumerable.Range(0, 52).Select(i => deck[i]).ToList();
        foreach (kbPlayingCard.Rank r in Enum.GetValues(typeof(kbPlayingCard.Rank)))
            Assert.Equal(4, cards.Count(c => c.rank == r));
    }

    [Fact]
    public void Shuffle_preserves_all_cards()
    {
        var deck = new kb52CardDeck(new Random(42));
        deck.Shuffle();
        Assert.Equal(52, deck.Count);
    }
}
