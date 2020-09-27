using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace YanivDesktop{
    internal class Player{
        // Personal Details
        public string PlayerName { get; set; }
        public int PlayerNumber { get; }
        public int Score { get; set; }

        // Game Details
        public List<Card> Cards { get; }
        public List<Card> ClubsCards { get; }
        public List<Card> DiamondCards { get; }
        public List<Card> HeartsCards { get; }
        public List<Card> SpadesCards { get; }
        public List<Card> JokerCards { get; }
        public List<Card> PickedCards { get; private set; }
        public List<List<Card>> OrganizedCards { get; }

        public Player(int number, string name) {
            PlayerNumber = number;
            PlayerName = name;
            Score = 0;
            Cards = new List<Card>();
            ClubsCards = new List<Card>();
            DiamondCards = new List<Card>();
            HeartsCards = new List<Card>();
            SpadesCards = new List<Card>();
            PickedCards = new List<Card>();
            JokerCards = new List<Card>();
            OrganizedCards = new List<List<Card>>() {
                ClubsCards, HeartsCards, SpadesCards, DiamondCards
            };
        }

        public void SetCards(List<Card> cards) {
            foreach (Card card in cards) {
                Cards.Add(card);
                card.CardState = CardState.NONE;
                switch (card.CardShape) {
                    case Shapes.CLUBS:
                        ClubsCards.Add(card);
                        break;
                    case Shapes.DIAMONDS:
                        DiamondCards.Add(card);
                        break;
                    case Shapes.HEARTS:
                        HeartsCards.Add(card);
                        break;
                    case Shapes.SPADES:
                        SpadesCards.Add(card);
                        break;
                    case Shapes.JOKER:
                        JokerCards.Add(card);
                        break;
                }
            }
        }



        public int CardSum() { return Card.CardSum(Cards); }

        public void PickCard(Card card) {
            if (card.Picked) {
                card.Picked = false;
                PickedCards.Remove(card);
                PickedCards.Sort();
                card.CardState = CardState.PUT_DOWN;
            } else {
                card.Picked = true;
                PickedCards.Sort();
                if (card.CardShape == Shapes.JOKER) {
                    if (PickedCards.Count > 0)
                        card.CardValue = PickedCards[PickedCards.Count - 1].CardValue + 1;
                    else card.CardValue = 0;
                }

                PickedCards.Add(card);
                card.CardState = CardState.LIFT;
            }
        }

        public void ResetPlayer() {
            Cards.Clear();
            ClubsCards.Clear();
            DiamondCards.Clear();
            HeartsCards.Clear();
            SpadesCards.Clear();
            JokerCards.Clear();
            PickedCards.Clear();
        }

        public List<Card> Play(Card newCard) {
            var thrownCards = new List<Card>(PickedCards);
            //   thrownCards.Sort();

            foreach (var pCard in PickedCards) {
                Cards.Remove(pCard);
                ClubsCards.Remove(pCard);
                SpadesCards.Remove(pCard);
                HeartsCards.Remove(pCard);
                DiamondCards.Remove(pCard);
                JokerCards.Remove(pCard);
                pCard.Picked = false;

            }

            PickedCards.Clear();
            SetCards(new List<Card> {newCard});
            return thrownCards;
        }

        public void ScorePlayer(bool penalty, bool caller) {
            if(!caller || penalty)
                Score += CardSum();
            if (penalty) Score += 30;
            if (Score == 50) Score = 0;
            else if (Score % 50 == 0) Score /= 2;
        }

        public void PickCards(List<Card> cards) {
            PickedCards = cards;
            foreach (var card in cards)
                card.Picked = true;
        }
    }


}
