#nullable disable
namespace kbWar.Core;

/// <summary>
/// Simulates a game of War for 2–52 players.
/// </summary>
public class kbCardGameWar
{
    public enum GameState { eNotStarted, eCurrentlyPlaying, eInfiniteLoop, eOverWithWinner }

    public const int MaxNumberOfPlayers = 52;

    public struct GameCounters
    {
        public int nTurns;
        public int nTies;
        public int nTotalWars;
        public int nSingleWars;
        public int nDoubleWars;
        public int nTripleWars;
        public int nQuadrupleWars;
        public int nQuintupleWars;
        public int nSextupleWars;
        public int nSeptupleWars;
    }

    private GameState m_State;
    private kb52CardDeck m_Deck;
    private kbCardHand[] m_Players;
    private kbCardHand[] m_ThrownCards;
    private List<kbPlayingCard> m_ThrownOrder;
    private kbCardHand m_MostRecentlyWonCards;
    private List<int> m_MostRecentWinners;
    private GameCounters m_Counters;
    private bool m_bShuffleRecentlyWonCards = true;
    private bool m_bLegacyPotOrder = false;
    private Random m_Rand;

    public kbCardGameWar() : this(2, new Random()) { }
    public kbCardGameWar(Random r) : this(2, r) { }
    public kbCardGameWar(int nPlayers) : this(nPlayers, new Random()) { }

    public kbCardGameWar(int nPlayers, Random r)
    {
        m_Rand = r;
        Restart(nPlayers);
    }

    public void Restart(int nPlayers = 2)
    {
        if (nPlayers < 2 || nPlayers > MaxNumberOfPlayers)
            throw new Exception(nPlayers + " Players! Number of players must be between 2 and " + MaxNumberOfPlayers + ".");

        m_Deck = new kb52CardDeck(m_Rand);
        m_Players = new kbCardHand[nPlayers];
        m_ThrownCards = new kbCardHand[nPlayers];
        for (int i = 0; i < nPlayers; i++)
        {
            m_Players[i] = new kbCardHand(m_Rand);
            m_ThrownCards[i] = new kbCardHand(m_Rand);
        }
        m_ThrownOrder = new List<kbPlayingCard>();
        m_MostRecentlyWonCards = new kb52CardDeck(m_Rand);
        m_MostRecentWinners = new List<int>();
        m_State = GameState.eNotStarted;
        m_Counters = new GameCounters();
    }

    public bool ShuffleRecentlyWonCards
    {
        get => m_bShuffleRecentlyWonCards;
        set => m_bShuffleRecentlyWonCards = value;
    }

    /// <summary>
    /// When true, the winner picks up the pot the way the original 2015 implementation
    /// did: all thrown cards in one pile in table order (interleaved between players),
    /// picked up in reverse. When false (default), the pot is grouped per player, which
    /// is how this class has assembled it since the multiplayer rewrite. Irrelevant when
    /// ShuffleRecentlyWonCards is on, but without shuffling the game is deterministic
    /// after the deal, and the pickup order changes the infinite-loop rate dramatically
    /// (roughly 10% legacy vs 42% grouped for 2 players).
    /// </summary>
    public bool LegacyPotOrder
    {
        get => m_bLegacyPotOrder;
        set => m_bLegacyPotOrder = value;
    }

    public GameState State => m_State;

    public int Winner
    {
        get
        {
            for (int i = 0; i < m_Players.Length; i++)
                if (m_Players[i].Count == 52) return i;
            return -1;
        }
    }

    public GameCounters Counters => m_Counters;

    public void ShuffleDeck() => m_Deck.Shuffle();

    public void Deal()
    {
        if (m_Deck.Count == 0) return;
        int count = 0;
        while (m_Deck.Count > 0)
        {
            int iPlayer = count % m_Players.Length;
            m_Players[iPlayer].AddToTop(m_Deck.DrawFromTop());
            count++;
        }
        m_State = GameState.eCurrentlyPlaying;
    }

    public int PlayTillFinished()
    {
        if (m_State == GameState.eOverWithWinner || m_State == GameState.eInfiniteLoop) return Winner;
        if (m_State == GameState.eNotStarted) { ShuffleDeck(); Deal(); }
        while (m_State == GameState.eCurrentlyPlaying) NewTurn();
        return Winner;
    }

    public GameState NewTurn()
    {
        if (m_State == GameState.eNotStarted) { ShuffleDeck(); Deal(); }
        if (m_State == GameState.eInfiniteLoop || m_State == GameState.eOverWithWinner) return m_State;

        List<int> players = new List<int>();
        for (int iPlayer = 0; iPlayer < m_Players.Length; iPlayer++)
            if (m_Players[iPlayer].Count > 0) players.Add(iPlayer);

        int nWars = 0;
        m_MostRecentWinners = ThrowDown(players, ref nWars);

        m_MostRecentlyWonCards.Clear();
        if (m_bLegacyPotOrder)
        {
            for (int i = m_ThrownOrder.Count - 1; i >= 0; i--)
                m_MostRecentlyWonCards.AddToBottom(m_ThrownOrder[i]);
            foreach (var cards in m_ThrownCards) cards.Clear();
        }
        else
        {
            foreach (var cards in m_ThrownCards)
                while (cards.Count > 0) m_MostRecentlyWonCards.AddToBottom(cards.DrawFromBottom());
        }
        m_ThrownOrder.Clear();

        if (m_bShuffleRecentlyWonCards) m_MostRecentlyWonCards.Shuffle();

        for (int iCard = 0; iCard < m_MostRecentlyWonCards.Count; iCard++)
            m_Players[m_MostRecentWinners[iCard % m_MostRecentWinners.Count]].AddToBottom(m_MostRecentlyWonCards[iCard]);

        UpdateCounters(nWars, m_MostRecentWinners);

        if (Winner >= 0) m_State = GameState.eOverWithWinner;
        else if (m_Counters.nTurns > 20000) m_State = GameState.eInfiniteLoop;
        else m_State = GameState.eCurrentlyPlaying;

        return m_State;
    }

    private void UpdateCounters(int nWars, List<int> players)
    {
        m_Counters.nTurns++;
        if (players.Count > 1) m_Counters.nTies++;
        m_Counters.nTotalWars += nWars;
        if (nWars == 1) m_Counters.nSingleWars++;
        else if (nWars == 2) m_Counters.nDoubleWars++;
        else if (nWars == 3) m_Counters.nTripleWars++;
        else if (nWars == 4) m_Counters.nQuadrupleWars++;
        else if (nWars == 5) m_Counters.nQuintupleWars++;
        else if (nWars == 6) m_Counters.nSextupleWars++;
        else if (nWars == 7) m_Counters.nSeptupleWars++;
    }

    private List<int> ThrowDown(List<int> players, ref int recursionCount)
    {
        foreach (int iPlayer in players)
        {
            kbPlayingCard thrown = m_Players[iPlayer].DrawFromTop();
            m_ThrownCards[iPlayer].AddToBottom(thrown);
            m_ThrownOrder.Add(thrown);
        }

        List<int> winningPlayers = new List<int>();
        int winningRank = 0;
        foreach (int iPlayer in players)
        {
            int rank = (int)m_ThrownCards[iPlayer][m_ThrownCards[iPlayer].Count - 1].rank;
            if (rank > winningRank) { winningRank = rank; winningPlayers.Clear(); winningPlayers.Add(iPlayer); }
            else if (rank == winningRank) winningPlayers.Add(iPlayer);
        }

        if (winningPlayers.Count == 1) return winningPlayers;

        // WAR
        List<int> playersReadyToThrowDown = new List<int>();
        foreach (int iPlayer in winningPlayers)
        {
            int nCardsToThrow = m_Players[iPlayer].Count;
            if (nCardsToThrow == 0) continue;
            else playersReadyToThrowDown.Add(iPlayer);
            if (nCardsToThrow > 4) nCardsToThrow = 4;
            for (int j = 0; j < nCardsToThrow - 1; j++)
            {
                kbPlayingCard thrown = m_Players[iPlayer].DrawFromTop();
                m_ThrownCards[iPlayer].AddToBottom(thrown);
                m_ThrownOrder.Add(thrown);
            }
        }

        if (playersReadyToThrowDown.Count == 0) return winningPlayers;
        else if (playersReadyToThrowDown.Count == 1) return playersReadyToThrowDown;
        else { recursionCount++; return ThrowDown(playersReadyToThrowDown, ref recursionCount); }
    }

    public string Deck => m_Deck.ToString();

    public string GetPlayer(int iHand) => m_Players[iHand].ToString();

    public int GetPlayerCount(int iHand) => m_Players[iHand].Count;

    public kbCardHand MostRecentlyWonCards
    {
        get
        {
            kbCardHand newHand = new kbCardHand(m_Rand);
            for (int i = 0; i < m_MostRecentlyWonCards.Count; i++)
                newHand.AddToBottom(m_MostRecentlyWonCards[i]);
            return newHand;
        }
    }

    public List<int> MostRecentWinners => m_MostRecentWinners.ToList();

    public int nPlayers => m_Players.Length;
}
