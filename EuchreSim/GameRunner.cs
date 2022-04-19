using System;
using System.Collections.Generic;
using System.Linq;

namespace EuchreSim
{
    public class GameRunner
    {

        public GameRunner()
        { }

        public void Run()
        {
            Console.WriteLine("Running!");

            var josh = new PlayerRed("Josh", Team.Red, 3);
            var dad = new PlayerBlue("Dad", Team.Blue, 0);
            //var dad = new PlayerRed("Dad", Team.Blue, 0);
            var colleen = new PlayerRed("Colleen", Team.Red, 1);
            var mom = new PlayerBlue("Mom", Team.Blue, 2);
            //var mom = new PlayerRed("Mom", Team.Blue, 2);

            var players = new List<IPlayer> { /*josh,*/ dad, colleen, mom, josh };

            short redScore = 0;
            short blueScore = 0;

            int gameCount = 0;
            while (redScore < 20000 && blueScore < 20000)
            {
                players = new List<IPlayer> { players[1], players[2], players[3], players[0] };

                var result = DealHand(players);
                gameCount++;
                if (players[0].Team == Team.Red)
                {
                    switch (result)
                    {
                        case HandResult.DealerPass:
                            break;
                        case HandResult.Set:
                            blueScore += 2;
                            break;
                        case HandResult.Win:
                            redScore++;
                            break;
                        case HandResult.WinAll:
                            redScore += 2;
                            break;
                        case HandResult.LonerAll:
                            redScore += 4;
                            break;
                    }
                }
                else
                {
                    switch (result)
                    {
                        case HandResult.DealerPass:
                            break;
                        case HandResult.Set:
                            redScore += 2;
                            break;
                        case HandResult.Win:
                            blueScore++;
                            break;
                        case HandResult.WinAll:
                            blueScore += 2;
                            break;
                        case HandResult.LonerAll:
                            blueScore += 4;
                            break;
                    }
                }
            }
            
            foreach(var t in players)
            {
                t.PrintStats();
            }

            if(redScore > blueScore)
            {
                Console.WriteLine($"Red Team wins {redScore} to {blueScore}. Games:{gameCount}");
            }
            else
            {
                Console.WriteLine($"Blue Team wins {blueScore} to {redScore}. Games:{gameCount}");
            }
        }

        private HandResult DealHand(List<IPlayer> players)
        {
            var deck = GetDeck();

            players[0].SetHand(deck.Take(5).ToList(), Position.LeftOfDealer);
            players[1].SetHand(deck.Skip(5).Take(5).ToList(), Position.DealerPartner);
            players[2].SetHand(deck.Skip(10).Take(5).ToList(), Position.RightOfDealer);
            players[3].SetHand(deck.Skip(15).Take(5).ToList(), Position.Dealer);

            var dealer = deck.Skip(20).Take(5).ToList();

            var up = dealer[0];
            //Console.WriteLine($"Up card is {up}");
            var bottoms = new List<Card>() { dealer[1], dealer[2], dealer[3] };

            var bottomsText = string.Join(",", bottoms);
            //Console.WriteLine($"Bottoms:{bottomsText}");

            var teamCall = CallTrump(dealer, players, deck);

            if (teamCall != null)
            {
                var result = RunGame(players, teamCall.Suit, teamCall.Team);
                //Console.WriteLine($"Calling Team:{teamCall.Team} result:{result}");
                return result;
            }
            else
            {
                Console.WriteLine($"Dealer Pass");
                return HandResult.DealerPass;
            }
        }

        private TeamCall CallTrump(List<Card> dealerCards, List<IPlayer> players, List<Card> deck)
        {
            var up = dealerCards[0];
            for (int i = 0; i < 4; i++)
            {
                var position = (Position)i;
                
                var response = players[i].CallIt(up, position);
                if (response == CallResponse.CallIt)
                {
                    deck.ForEach(c => c.UpdateJack(up.Suit));
                    //Console.WriteLine($"{players[i].Name} of {players[i].Team} ordered up {up} to {players[0].Name}");
                    players[3].OrderedUp(up);

                    return new TeamCall
                    {
                        Suit = up.Suit,
                        Team = players[i].Team
                    };
                }
                else if(response == CallResponse.Bottoms)
                {
                    var bottoms = new List<Card>() { dealerCards[1], dealerCards[2], dealerCards[3] };
                    players[i].TakeBottoms(bottoms);
                }
            }

            for (int i = 0; i < 4; i++)
            {
                var any = players[i].CallItAny((Position)i, up.Suit);
                //if (any != null)
                if (any.CallIt == CallResponse.CallIt)
                {
                    deck.ForEach(c => c.UpdateJack(/*any.Value*/any.Suit));
                    //Console.WriteLine($"{players[i].Name} of {players[i].Team} Called it {up.Suit}");
                    return new TeamCall
                    {
                        Suit = any.Suit/*.Value*/,
                        Team = players[i].Team
                    };
                }
            }
            return null;
        }

        HandResult RunGame(List<IPlayer> players, Suit trump, Team callingTeam)
        {
            byte callingTeamTricks = 0;
            byte leadPosition = 0;
            for(int i = 0; i < 5; i++)
            {
                var winningCard = RunTrick(players, trump, callingTeam, leadPosition);

                if (winningCard.Player.Team == callingTeam)
                {
                    callingTeamTricks++;
                }
                leadPosition = winningCard.Player.ID;
            }

            Console.WriteLine($"Hand over, {callingTeam} won {callingTeamTricks} tricks");
            switch (callingTeamTricks)
            {
                case 0:
                case 1:
                case 2:
                    return HandResult.Set;
                case 3:
                case 4:
                    return HandResult.Win;
                case 5:
                    return HandResult.WinAll;
                default:
                    return HandResult.LonerAll;
            }
            
        }

        private PlayedCard RunTrick(List<IPlayer> players, Suit trump, Team callingTeam, byte leadPosition)
        {
            var cards = new List<PlayedCard>();
            byte order = 0;
            for(int i = leadPosition; i < leadPosition + 4; i++)
            {
                var player = players[i % 4];
                var card = player.PlayCard(cards, trump, callingTeam);
                PlayedCard pCard = new PlayedCard
                {
                    Card = card,
                    Order = order,
                    Player = player,
                };
                cards.Add(pCard);
                order++;
            }

            var winningCard = GetWinningCard(cards, trump);
            Console.WriteLine($"---{winningCard.Player.Name}-{winningCard.Player.Team}-{winningCard.Card}-won the trick");
            return winningCard;
        }

        private PlayedCard GetWinningCard(List<PlayedCard> fourCards, Suit trump)
        {
            var trumpCards = fourCards.Where(c => c.Card.RealSuit == trump).OrderByDescending(c => c.Card.RealRank);
            if (trumpCards.Any())
                return trumpCards.First();

            var leadCard = fourCards.First(c => c.Order == 0);
            var suitCards = fourCards.Where(c => c.Card.RealSuit == leadCard.Card.RealSuit).OrderByDescending(c => c.Card.RealRank);

            return suitCards.First();
        }

        private static Random rng = new Random();
        public List<Card> GetDeck()
        {
            var deck = new List<Card>()
            {
                new Card{ Suit = Suit.Clubs, Rank = Rank.Nine },
                new Card{ Suit = Suit.Clubs, Rank = Rank.Ten },
                new Card{ Suit = Suit.Clubs, Rank = Rank.Jack },
                new Card{ Suit = Suit.Clubs, Rank = Rank.Queen },
                new Card{ Suit = Suit.Clubs, Rank = Rank.King },
                new Card{ Suit = Suit.Clubs, Rank = Rank.Ace },
                new Card{ Suit = Suit.Hearts, Rank = Rank.Nine  },
                new Card{ Suit = Suit.Hearts, Rank = Rank.Ten  },
                new Card{ Suit = Suit.Hearts, Rank = Rank.Jack  },
                new Card{ Suit = Suit.Hearts, Rank = Rank.Queen },
                new Card{ Suit = Suit.Hearts, Rank = Rank.King  },
                new Card{ Suit = Suit.Hearts, Rank = Rank.Ace  },
                new Card{ Suit = Suit.Spades, Rank = Rank.Nine  },
                new Card{ Suit = Suit.Spades, Rank = Rank.Ten  },
                new Card{ Suit = Suit.Spades, Rank = Rank.Jack  },
                new Card{ Suit = Suit.Spades, Rank = Rank.Queen },
                new Card{ Suit = Suit.Spades, Rank = Rank.King  },
                new Card{ Suit = Suit.Spades, Rank = Rank.Ace },
                new Card{ Suit = Suit.Diamonds, Rank = Rank.Nine  },
                new Card{ Suit = Suit.Diamonds, Rank = Rank.Ten  },
                new Card{ Suit = Suit.Diamonds, Rank = Rank.Jack  },
                new Card{ Suit = Suit.Diamonds, Rank = Rank.Queen },
                new Card{ Suit = Suit.Diamonds, Rank = Rank.King  },
                new Card{ Suit = Suit.Diamonds, Rank = Rank.Ace  },
            };

            return deck.OrderBy(d => rng.Next()).ToList();
        }
    }

    public class TeamCall
    {
        public Suit Suit { get; set; }
        public Team Team { get; set; }
    }

    public class PlayedCard
    {
        public Card Card { get; set; }
        public IPlayer Player { get; set; }
        public byte Order { get; set; }
    }
}