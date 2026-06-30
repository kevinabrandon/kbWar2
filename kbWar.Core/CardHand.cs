namespace kbWar.Core;

public class CardHand
{
    private readonly Queue<PlayingCard> _cards = new();

    public int Count => _cards.Count;
    public bool IsEmpty => _cards.Count == 0;

    public void Add(PlayingCard card) => _cards.Enqueue(card);

    public void AddRange(IEnumerable<PlayingCard> cards)
    {
        foreach (var c in cards) _cards.Enqueue(c);
    }

    public PlayingCard Draw() => _cards.Dequeue();

    public IReadOnlyList<PlayingCard> ToList() => _cards.ToList();

    public override string ToString() => $"{Count} cards";
}
