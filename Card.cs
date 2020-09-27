using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace YanivDesktop{

    public enum Shapes{ CLUBS, DIAMONDS, HEARTS, SPADES, JOKER }

    internal class Card : Sprite, IComparable<Card>{
        public Card(Shapes shape, int value, Texture2D texture) {
            CardShape = shape;
            CardValue = value;
            SpriteTexture = texture;
            spriteRectangle = new Rectangle(79 * CardValue, 123 * (int) CardShape, 79, 123);
            Picked = false;
            CardState = CardState.NONE;
        }

        public Card(Card other) {
            CardShape = other.CardShape;
            CardValue = other.CardValue;
            SpriteTexture = other.SpriteTexture;
            Picked = other.Picked;
            spriteVector = new Vector2(other.spriteVector.X, other.spriteVector.Y);
            spriteRectangle = new Rectangle(79 * CardValue, 123 * (int) CardShape, 79, 123);
        }

        public CardState CardState { get; set; }
        public int CardValue { get; set; }
        public Shapes CardShape { get; set; }
        public bool Picked { get; set; }

        public int CompareTo(Card other) { return CardValue - other.CardValue; }

        public static int CardSum(List<Card> listOfCards) {
            var sum = 0;
            foreach (var card in listOfCards.Where(card => card.CardShape != Shapes.JOKER)) {
                if (card.CardValue % 13 < 11) sum += (card.CardValue % 13 + 1);
                else sum += 10;
            }

            return sum;
        }


        /*
        public override string ToString()
        {
            return "    " +(CardValue + 1) + " " + CardShape;
        }*/
    }

}
