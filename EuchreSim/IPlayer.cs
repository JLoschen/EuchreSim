using System.Collections.Generic;

namespace EuchreSim
{
    public interface IPlayer
    {
        byte ID { get; set; }
        string Name { get; set; }
        Team Team { get; set; }
        void SetHand(List<Card> cards, Position position);
        void RecordTurn(Dictionary<Position,Card> cards);
        CallResponse/*bool*/ CallIt(Card upCard, Position dealer);
        CallItOption/*Suit?*/ CallItAny(Position dealer, Suit suitCantPick);
        void RecordGame(bool won, GameOutcome outcome);
        Card PlayCard(List<PlayedCard> cards, Suit trump, Team callingTeam);
        void OrderedUp(Card card);
        void TakeBottoms(List<Card> bottomCards);
        void PrintStats();
    }

    public enum Position
    {
        LeftOfDealer,
        DealerPartner,
        RightOfDealer,
        Dealer,
    }

    public enum CallResponse
    {
        Pass,
        CallIt,
        CallItAlone,
        Bottoms
    }

    public class CallItOption
    {
        public CallResponse CallIt { get; set; }
        public Suit Suit { get; set; }
    }

    public enum GameOutcome
    {
        Set,
        Win,
        AllWin,
        LonerWin
    }
}