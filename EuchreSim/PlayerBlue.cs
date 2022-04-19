using System.Collections.Generic;
using System;
using System.Linq;

namespace EuchreSim
{
    public class PlayerBlue : IPlayer
    {
        private int bottomsCount = 0;
        private int _firstPlayerCount = 0;
        private Dictionary<Position, int> postionCount;

        public byte ID { get; set; }
        public string Name { get; set; }
        public Team Team { get; set; }
        public PlayerBlue(string name, Team team, byte Id)
        {
            Name = name;
            Team = team;
            ID = Id;
            postionCount = new Dictionary<Position, int>();
            postionCount.Add(Position.Dealer, 0);
            postionCount.Add(Position.LeftOfDealer, 0);
            postionCount.Add(Position.DealerPartner, 0);
            postionCount.Add(Position.RightOfDealer, 0);
        }

        //public bool CallIt(Card upCard, Position dealer)
        //{
        //    var score = GetHandScore(upCard.Suit);
        //    var result = score > 23;
        //    Console.WriteLine($"{Name} callit:{result} score:{score} up{upCard}");

        //    return result;
        //}

        public void TakeBottoms(List<Card> bottomCards)
        {
            Console.WriteLine($"PreBottoms:{string.Join(",", _hand)}"); 
            bottomsCount++;
            var badCards = _hand.Where(c => c.Rank == Rank.Nine || c.Rank == Rank.Ten).Take(3).ToList() ;
            _hand.RemoveAll(c => badCards.Contains(c));

            _hand.AddRange(bottomCards);

            Console.WriteLine($"PostBottoms:{string.Join(",", _hand)}");
        }

        public CallResponse CallIt(Card upCard, Position dealer)
        {
            if (dealer == Position.LeftOfDealer)
            {
                _firstPlayerCount++;
            }
            else
            {
                Console.WriteLine($"{Name}:{dealer}");
            }

            var score = GetHandScore(upCard.Suit);

            if(score > 23)
            {
                //Console.WriteLine($"{Name} callit:{CallResponse.CallIt} score:{score} up{upCard}");
                return CallResponse.CallIt;
            }

            if (_hand.Where(c => c.Suit != upCard.Suit && (c.Rank == Rank.Nine || c.Rank == Rank.Ten)).Count() >= 3)
            {
                Console.WriteLine($"{Name} callit:{CallResponse.Bottoms}");
                bottomsCount++;
                return CallResponse.Bottoms;
            }

            return CallResponse.Pass;
        }

        private int GetHandScore(Suit suit)
        {
            int total = 0;

            foreach (var card in _hand)
            {
                total += GetCardScore(card, suit);
            }
            return total;
        }

        private int GetCardScore(Card card, Suit suit)
        {
            if (card.Suit == suit)
            {
                switch (card.Rank)
                {
                    case Rank.Nine:
                        return 6;
                    case Rank.Ten:
                        return 7;
                    case Rank.Jack:
                        return 12;
                    case Rank.Queen:
                        return 8;
                    case Rank.King:
                        return 9;
                    case Rank.Ace:
                        return 10;
                };
            }
            else
            {
                switch (card.Rank)
                {
                    case Rank.Nine:
                        return 0;
                    case Rank.Ten:
                        return 1;
                    case Rank.Jack:
                        if (IsLeft(card.Suit, suit))
                            return 11;
                        else
                            return 2;
                    case Rank.Queen:
                        return 3;
                    case Rank.King:
                        return 6;
                    case Rank.Ace:
                        return 9;
                };
            }

            return -10;
        }

        private bool IsLeft(Suit cardSuit, Suit trumpSuit)
        {
            switch (trumpSuit)
            {
                case Suit.Clubs:
                    return cardSuit == Suit.Spades;
                case Suit.Spades:
                    return cardSuit == Suit.Clubs;
                case Suit.Diamonds:
                    return cardSuit == Suit.Hearts;
                case Suit.Hearts:
                    return cardSuit == Suit.Diamonds;
            }
            return false;
        }

        public void RecordGame(bool won, GameOutcome outcome)
        {
            throw new NotImplementedException();
        }

        public void RecordTurn(Dictionary<Position, Card> cards)
        {
            throw new NotImplementedException();
        }

        private List<Card> _hand;
        private List<Card> _played;
        public void SetHand(List<Card> cards, Position position)
        {
            postionCount[position]++;

            _hand = cards;
            _played = new List<Card>();

            var text = string.Join(",", cards.OrderBy(c => c.Suit).ThenBy(c => c.Rank).Select(c => c.ToString()));
            //Console.WriteLine($"{Name} - {text}");
        }

        public /*Suit?*/CallItOption CallItAny(Position dealer, Suit suitCantPick)
        {
            Dictionary<Suit, int> suitValues = new Dictionary<Suit, int>();

            if (Suit.Clubs != suitCantPick)
                suitValues.Add(Suit.Clubs, GetHandScore(Suit.Clubs));

            if (Suit.Spades != suitCantPick)
                suitValues.Add(Suit.Spades, GetHandScore(Suit.Spades));

            if (Suit.Hearts != suitCantPick)
                suitValues.Add(Suit.Hearts, GetHandScore(Suit.Hearts));

            if (Suit.Diamonds != suitCantPick)
                suitValues.Add(Suit.Diamonds, GetHandScore(Suit.Diamonds));

            var bestSuit = suitValues.OrderByDescending(s => s.Value).First();


            var result = bestSuit.Value > 23;
            //Console.WriteLine($"{Name} callitAny:{result} bestSuit:{bestSuit.Key} score:{bestSuit.Value}");
            if (result)
            {
                return new CallItOption
                {
                    Suit = bestSuit.Key,
                    CallIt = CallResponse.CallIt
                };
            }

            return new CallItOption
            {
                CallIt = CallResponse.Pass
            }; 
        }

        public Card PlayCard(List<PlayedCard> cards, Suit trump, Team callingTeam)
        {
            Card card = null;
            if (cards.Count == 0)
            {
                card = Lead(trump, callingTeam);
                //Console.WriteLine($"{Name}{Team} Lead {card}");
            }
            else
            {
                card = FollowSuit(cards, trump, callingTeam);
            }
            _played.Add(card);
            return card;
        }

        private Card FollowSuit(List<PlayedCard> cards, Suit trump, Team callingTeam)
        {
            var unPlayed = _hand.Except(_played);

            var unPlayedText = string.Join(",", unPlayed);

            var suitToFollow = cards.First();
            
            if (suitToFollow == null)
            {
                Console.WriteLine("Problems!!");
            }
            var leadSuit = suitToFollow.Card.RealSuit;

            var options = unPlayed.Where(c => c.Suit == leadSuit).ToList();
            if (options.Count == 1)
            {
                var card = options.First();
                //Console.WriteLine($"{Name}{Team} Follow suit {card} (only option)-{unPlayedText}");
                return card;
            }
            else if (options.Count > 1)
            {
                if (leadSuit != trump)
                {
                    //if someone already trumped it lay lowest option
                    if (cards.Any(c => c.Card.RealSuit == trump))
                    {
                        var offSuit = options.OrderBy(o => o.RealRank).First();
                        //Console.WriteLine($"{Name}{Team} Follow suit {offSuit} (Lay off trumped)-{unPlayedText}");
                        return offSuit;
                    }
                    else//not trumped, have a shot at taking 
                    {
                        var highestInSuit = cards.Where(c => c.Card.RealSuit == leadSuit).Max(c => c.Card.RealRank);

                        if (options.Any(c => c.RealRank > highestInSuit))
                        {
                            var higherCard = options.OrderByDescending(o => o.RealRank).First();
                            //Console.WriteLine($"{Name}{Team} Follow suit {higherCard} (taking higher)-{unPlayedText}");
                            return higherCard;
                        }
                    }
                }
                var card = options.FirstOrDefault();
                //Console.WriteLine($"{Name}{Team} Follow suit {card} (Guessing???)-{unPlayedText}");
                return card;
            }

            //can't follow suit, but can you trump?
            var trumpCards = unPlayed.Where(c => c.RealSuit == trump).OrderBy(c => c.RealRank).ToList();
            if (trumpCards.Any())
            {
                var card = trumpCards.First();
                //Console.WriteLine($"{Name}{Team} Follow suit {card} (Trump it!!!)-{unPlayedText}");
                return card;
            }

            //can't trump and can't follow suit, lay off
            //TODO intelligence here
            var layoff = unPlayed.First();
            //Console.WriteLine($"{Name}{Team} Follow suit {layoff} (laying off???)-{unPlayedText}");
            return layoff;
        }

        private Card Lead(Suit trump, Team callingTeam)
        {
            var unPlayed = _hand.Except(_played);

            if (Team == callingTeam)
            {
                var trumpCard = unPlayed.FirstOrDefault(c => c.RealSuit == trump);
                if (trumpCard != null)
                    return trumpCard;
            }

            var ace = unPlayed.FirstOrDefault(c => c.RealSuit != trump && c.Rank == Rank.Ace);
            if (ace != null)
                return ace;

            var king = unPlayed.FirstOrDefault(c => c.RealSuit != trump && c.Rank == Rank.King);
            if (king != null)
                return king;

            return unPlayed.FirstOrDefault();

        }

        public void OrderedUp(Card card)
        {
            _hand.Add(card);
            var nonTrump = _hand.Where(c => c.RealSuit != card.Suit && c.Rank != Rank.Ace);

            var groupedSuits = nonTrump.GroupBy(c => c.RealSuit).OrderBy(g => g.ToList().Count);

            foreach (var suitGroup in groupedSuits)
            {
                var list = suitGroup.ToList();
                var cardToRemove = list.OrderBy(c => c.RealRank).First();
                //Console.WriteLine($"{Name}-{Team} exchanging {cardToRemove} for {card}");

                _hand.Remove(cardToRemove);
                return;
            }

            if (_hand.Count == 6)
            {
                Console.WriteLine("Problem!!!!");
                var first = _hand.First(c => c != card);
                _hand.Remove(first);
            }
        }

        public void PrintStats()
        {
            Console.WriteLine($"{Name} Total Bottoms:{bottomsCount} First:{_firstPlayerCount} {postionCount[Position.Dealer]}-{postionCount[Position.LeftOfDealer]}-{postionCount[Position.DealerPartner]}-{postionCount[Position.RightOfDealer]}");
        }
    }
}
