namespace kbWar.Core;

/// <summary>
/// Simulates a playing card. Immutable.
/// </summary>
public class kbPlayingCard
{
    public enum Suit { Hearts, Clubs, Diamonds, Spades }
    public enum Rank
    {
        Deuce = 2, Trey = 3, Four = 4, Five = 5, Six = 6, Seven = 7,
        Eight = 8, Nine = 9, Ten = 10, Jack = 11, Queen = 12, King = 13, Ace = 14
    }

    private readonly Suit m_Suit;
    private readonly Rank m_Rank;

    public kbPlayingCard(Suit s, Rank r) { m_Suit = s; m_Rank = r; }

    public Suit suit => m_Suit;
    public Rank rank => m_Rank;

    public override string ToString() => String.Format("{0,5} of {1,-7}", rank, suit);
}
