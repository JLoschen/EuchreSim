using System;

namespace EuchreSim
{
    public enum Suit
    {
        Hearts,
        Diamonds,
        Spades,
        Clubs
    }

    public enum Rank
    {
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
        Ace = 14
    }

    public enum Team
    {
        Red,
        Blue
    }

    public enum HandResult
    {
        DealerPass,
        Set,
        Win,
        WinAll,
        LonerAll
    }

    public class Card
    {
        public Suit Suit { get; set; }
        public Rank Rank { get; set; }

        //public enum Rank
        //{
        //    Nine = 9,
        //    Ten = 10,
        //    Jack = 11,
        //    Queen = 12,
        //    King = 13,
        //    Ace = 14,
        //    Left = 15,
        //    Right = 16
        //}
        public Suit RealSuit { get; set; }
        public int RealRank { get; set; }
        public override string ToString() =>  $"{GetRankText(Rank)}{GetSuitText(Suit)}";

        public void UpdateJack(Suit trump)
        {
            if (Rank != Rank.Jack)
            {
                RealRank = (int)Rank;
                RealSuit = Suit;
            }
            else
            {
                switch (Suit)
                {
                    case Suit.Clubs:
                        if(trump == Suit.Clubs)
                        {
                            RealRank = 16;
                            RealSuit = Suit;
                        }
                        else if(trump == Suit.Spades)
                        {
                            RealRank = 15;
                            RealSuit = Suit.Spades;
                        }
                        break;
                    case Suit.Spades:
                        if (trump == Suit.Spades)
                        {
                            RealRank = 16;
                            RealSuit = Suit;
                        }
                        else if (trump == Suit.Clubs)
                        {
                            RealRank = 15;
                            RealSuit = Suit.Clubs;
                        }
                        break;
                    case Suit.Hearts:
                        if (trump == Suit.Hearts)
                        {
                            RealRank = 16;
                            RealSuit = Suit;
                        }
                        else if (trump == Suit.Diamonds)
                        {
                            RealRank = 15;
                            RealSuit = Suit.Diamonds;
                        }
                        break;
                    case Suit.Diamonds:
                        if (trump == Suit.Diamonds)
                        {
                            RealRank = 16;
                            RealSuit = Suit;
                        }
                        else if (trump == Suit.Hearts)
                        {
                            RealRank = 15;
                            RealSuit = Suit.Hearts;
                        }
                        break;
                }
            }
        }

        private string GetRankText(Rank rank)
        {
            switch (rank)
            {
                case Rank.Nine:
                    return "9";
                case Rank.Ten:
                    return "10";
                case Rank.Jack:
                    return "J";
                case Rank.Queen:
                    return "Q";
                case Rank.King:
                    return "K";
                case Rank.Ace:
                    return "A";
            }
            return "error";
        }

        private string GetSuitText(Suit suit)
        {
            switch (suit)
            {
                case Suit.Clubs:
                    return "♣";
                case Suit.Spades:
                    return "♠";
                case Suit.Diamonds:
                    return "♦";
                case Suit.Hearts:
                    return "♥";
            }
            return "bad";
        }

    }
}
