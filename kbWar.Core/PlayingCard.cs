namespace kbWar.Core;

public enum Suit { Clubs, Diamonds, Hearts, Spades }

public enum Rank
{
    Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten,
    Jack, Queen, King, Ace
}

public sealed class PlayingCard : IComparable<PlayingCard>
{
    public Suit Suit { get; }
    public Rank Rank { get; }

    public PlayingCard(Suit suit, Rank rank)
    {
        Suit = suit;
        Rank = rank;
    }

    public int CompareTo(PlayingCard? other) =>
        other is null ? 1 : Rank.CompareTo(other.Rank);

    public override string ToString() => $"{Rank} of {Suit}";
}
