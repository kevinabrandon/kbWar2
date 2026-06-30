#nullable disable
namespace kbWar.Core;

/// <summary>
/// Simulates a card hand.
/// </summary>
public class kbCardHand
{
    protected List<kbPlayingCard> m_Cards = new List<kbPlayingCard>();
    protected Random m_Rand = null;

    public kbCardHand() { m_Rand = new Random(); }
    public kbCardHand(Random r) { m_Rand = r; }

    public int Count => m_Cards.Count;

    public void Clear() => m_Cards.Clear();

    public void AddToBottom(kbPlayingCard pc) => m_Cards.Add(pc);

    public void AddToTop(kbPlayingCard pc) => m_Cards.Insert(0, pc);

    public void AddTo(kbPlayingCard pc, int index) => m_Cards.Insert(index, pc);

    public kbPlayingCard DrawFromBottom()
    {
        kbPlayingCard pc = m_Cards.Last();
        m_Cards.RemoveAt(m_Cards.Count - 1);
        return pc;
    }

    public kbPlayingCard DrawFromTop()
    {
        kbPlayingCard pc = m_Cards.First();
        m_Cards.RemoveAt(0);
        return pc;
    }

    public kbPlayingCard DrawFrom(int index)
    {
        kbPlayingCard pc = m_Cards[index];
        m_Cards.RemoveAt(index);
        return pc;
    }

    public bool Contains(kbPlayingCard pc) =>
        m_Cards.Any(w => w.suit == pc.suit && w.rank == pc.rank);

    public int GetIndexOf(kbPlayingCard pc) =>
        m_Cards.FindIndex(w => w.suit == pc.suit && w.rank == pc.rank);

    public kbPlayingCard this[int i] => m_Cards[i];

    /// <summary>
    /// Shuffles by swapping each position with a random position (matches original behaviour).
    /// </summary>
    public void Shuffle()
    {
        int n = m_Cards.Count;
        for (int i = 0; i < n; i++)
        {
            int iSwap = (int)(m_Rand.NextDouble() * n);
            kbPlayingCard swap = m_Cards[i];
            m_Cards[i] = m_Cards[iSwap];
            m_Cards[iSwap] = swap;
        }
    }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        foreach (kbPlayingCard pc in m_Cards) sb.AppendLine(pc.ToString());
        return sb.ToString();
    }
}

/// <summary>
/// A card hand that starts as a new ordered 52-card deck (Bicycle brand order).
/// </summary>
public class kb52CardDeck : kbCardHand
{
    public kb52CardDeck(Random r) : base(r) { CreateNew52CardDeck(); }
    public kb52CardDeck() : base() { CreateNew52CardDeck(); }

    private void CreateNew52CardDeck()
    {
        m_Cards.Clear();
        foreach (kbPlayingCard.Suit s in Enum.GetValues(typeof(kbPlayingCard.Suit)))
        {
            if (s == kbPlayingCard.Suit.Hearts || s == kbPlayingCard.Suit.Clubs)
            {
                m_Cards.Add(new kbPlayingCard(s, kbPlayingCard.Rank.Ace));
                for (int iVal = 2; iVal < 14; iVal++)
                    m_Cards.Add(new kbPlayingCard(s, (kbPlayingCard.Rank)iVal));
            }
            else
            {
                for (int iVal = 13; iVal >= 2; iVal--)
                    m_Cards.Add(new kbPlayingCard(s, (kbPlayingCard.Rank)iVal));
                m_Cards.Add(new kbPlayingCard(s, kbPlayingCard.Rank.Ace));
            }
        }
    }
}
