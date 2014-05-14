using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SillyGame.View;

namespace SillyGame.Model
{
    class Player2
    {
        // Animation representing the player
        public Animation Player2Animation;

        // Position of the Player relative to the upper left side of the screen
        public Vector2 Position;

        // State of the player
        public bool Active;

        // Amount of hit points that player has
        public int Health;

        // Get the width of the player ship
        public int Width
        {
            get { return Player2Animation.FrameWidth; }
        }

        // Get the height of the player ship
        public int Height
        {
            get { return Player2Animation.FrameHeight; }
        }


        // Initialize the player
        public void Initialize(Animation animation, Vector2 position)
        {
            Player2Animation = animation;

            // Set the starting position of the player around the middle of the screen and to the back
            Position = position;

            // Set the player to be active
            Active = true;

            // Set the player health
            Health = 100;
        }

        // Update the player animation
        public void Update(GameTime gameTime)
        {
            Player2Animation.Position = Position;
            Player2Animation.Update(gameTime);
        }

        // Draw the player
        public void Draw(SpriteBatch spriteBatch)
        {
            Player2Animation.Draw(spriteBatch);
        }
    }
}
