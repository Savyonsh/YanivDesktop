using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace YanivDesktop
{
    [Serializable]
    public class Sprite{
        public Texture2D SpriteTexture { get; set; }
        public Rectangle spriteRectangle;
        public Vector2 spriteVector;
        public bool IsVisible { get; set; }
        public Sprite() { }

        public Sprite(Sprite other) {
            SpriteTexture = other.SpriteTexture;
            spriteVector = other.spriteVector;
            spriteRectangle = other.spriteRectangle;
        }

        public Sprite(Texture2D texture2D) {
            SpriteTexture = texture2D;
            spriteVector = new Vector2();
            spriteRectangle = new Rectangle(0, 0, texture2D.Width, texture2D.Height);
            IsVisible = true;
        }

        public Sprite(Texture2D texture2D, Vector2 vector) {
            SpriteTexture = texture2D;
            spriteVector = vector;
            spriteRectangle = new Rectangle(0, 0, texture2D.Width, texture2D.Height);
            IsVisible = true;
        }

        public Sprite(Texture2D texture2D, Vector2 vector, Rectangle rectangle) {
            SpriteTexture = texture2D;
            spriteVector = vector;
            spriteRectangle = rectangle;
            IsVisible = true;
        }

        public bool MouseTouched(MouseState mouseCurrent, MouseState mousePrevious) {
            return mousePrevious.LeftButton == ButtonState.Pressed &&
                   mouseCurrent.LeftButton == ButtonState.Released &&
                   mouseCurrent.X > spriteVector.X &&
                   mouseCurrent.X <= spriteVector.X + spriteRectangle.Width &&
                   mouseCurrent.Y > spriteVector.Y &&
                   mouseCurrent.Y <= spriteVector.Y + spriteRectangle.Height;
        }

        public bool MouseHovered(MouseState mouseCurrent) {
            return mouseCurrent.X > spriteVector.X &&
                   mouseCurrent.X <= spriteVector.X + spriteRectangle.Width &&
                   mouseCurrent.Y > spriteVector.Y &&
                   mouseCurrent.Y <= spriteVector.Y + spriteRectangle.Height;
        }
    }
}