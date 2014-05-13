using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SillyGame.Model;
using SillyGame.View;

namespace SillyGame.Controller
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        //Background textures for the various screens in the game
        Texture2D MainMenuScreenBackground;
        Texture2D GameOverScreenBackground;

        //Screen State variables to indicate what is the current screen
        bool GamePause;
        bool MainMenuScreenShown;
        bool GameScreenShown;
        bool GameOverScreenShown;

        //Directions(literally) for the thumbsticks
        bool N = false;
        bool NE = false;
        bool East = false;
        bool SE = false;
        bool South = false;
        bool SW = false;
        bool W = false;
        bool NW = false;

        //The index of the Player One controller
        PlayerIndex mPlayerOne;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Player player;

        // Keyboard states used to determine key presses
        KeyboardState currentKeyboardState;
        KeyboardState previousKeyboardState;

        // Gamepad states used to determine button presses
        GamePadState currentGamePadState;
        GamePadState previousGamePadState;
        GamePadState currentGamePadThumbSticks;
        GamePadState previousGamePadThumbSticks;

        // A movement speed for the player
        float playerMoveSpeed;

        // Image used to display the static background
        Texture2D mainBackground;

        // Parallaxing Layers
        ParallaxingBackground bgLayer1;
        ParallaxingBackground bgLayer2;

        // Enemies
        Texture2D enemyTexture;
        List<Enemy> enemies;

        // The rate at which the enemies appear
        TimeSpan enemySpawnTime;
        TimeSpan previousSpawnTime;

        // A random number generator
        Random random;

        Texture2D projectileTexture;
        List<Projectile> projectiles;

        // The rate of fire of the player laser
        TimeSpan fireTime;
        TimeSpan previousFireTime;

        Texture2D explosionTexture;
        List<Animation> explosions;

        // The sound that is played when a laser is fired
        SoundEffect laserSound;

        // The sound used when the player or an enemy dies
        SoundEffect explosionSound;

        // The music played during gameplay
        Song gameplayMusic;

        //Number that holds the player score
        int score;

        // The font used to display UI elements
        SpriteFont font;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {

            explosions = new List<Animation>();

            //Set player's score to zero
            score = 0;

            projectiles = new List<Projectile>();

            // Set the laser to fire every quarter second
            fireTime = TimeSpan.FromSeconds(.15f);

            // Initialize the enemies list
            enemies = new List<Enemy>();

            // Set the time keepers to zero
            previousSpawnTime = TimeSpan.Zero;

            // Used to determine how fast enemy respawns
            enemySpawnTime = TimeSpan.FromSeconds(1.0f);

            // Initialize our random number generator
            random = new Random();

            bgLayer1 = new ParallaxingBackground();
            bgLayer2 = new ParallaxingBackground();

            // Initialize the player class
            player = new Player();

            // Set a constant player move speed
            playerMoveSpeed = 6.0f;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load the player resources
            Animation playerAnimation = new Animation();
            Texture2D playerTexture = Content.Load<Texture2D>("Sprites/shipAnimation");
            playerAnimation.Initialize(playerTexture, Vector2.Zero, 115, 69, 8, 30, Color.White, 1f, true);

            Vector2 playerPosition = new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y
            + GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
            player.Initialize(playerAnimation, playerPosition);

            //Load the screen backgrounds
            MainMenuScreenBackground = Content.Load<Texture2D>("Images/mainMenu");
            GameOverScreenBackground = Content.Load<Texture2D>("Images/endMenu");

            //Initialize the screen state variables
            GamePause = false;
            GameOverScreenShown = false;
            GameScreenShown = false;
            MainMenuScreenShown = true;

            // Load the in-game backgrounds
            bgLayer1.Initialize(Content, "Images/bgLayer1", GraphicsDevice.Viewport.Width, -1);
            bgLayer2.Initialize(Content, "Images/bgLayer2", GraphicsDevice.Viewport.Width, -2);
            mainBackground = Content.Load<Texture2D>("Images/mainbackground");

            enemyTexture = Content.Load<Texture2D>("Sprites/mineAnimation");

            projectileTexture = Content.Load<Texture2D>("Sprites/laser");

            explosionTexture = Content.Load<Texture2D>("Sprites/explosion");

            // Load the music
            gameplayMusic = Content.Load<Song>("sound/gameMusic");

            // Load the laser and explosion sound effect
            laserSound = Content.Load<SoundEffect>("sound/laserFire");
            explosionSound = Content.Load<SoundEffect>("sound/explosion");

            // Start the music right away
            PlayMusic(gameplayMusic);

            // Load the score font
            font = Content.Load<SpriteFont>("gameFont");



        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private void AddProjectile(Vector2 position)
        {
            Projectile projectile = new Projectile();
            projectile.Initialize(GraphicsDevice.Viewport, projectileTexture, position);
            projectiles.Add(projectile);
        }

        private void AddExplosion(Vector2 position)
        {
            Animation explosion = new Animation();
            explosion.Initialize(explosionTexture, position, 134, 134, 12, 45, Color.White, 1f, false);
            explosions.Add(explosion);
        }

        private void AddEnemy()
        {
            // Create the animation object
            Animation enemyAnimation = new Animation();

            // Initialize the animation with the correct animation information
            enemyAnimation.Initialize(enemyTexture, Vector2.Zero, 47, 61, 8, 30, Color.White, 1f, true);

            // Randomly generate the position of the enemy
            Vector2 position = new Vector2(GraphicsDevice.Viewport.Width + enemyTexture.Width / 2, random.Next(100, GraphicsDevice.Viewport.Height - 100));

            // Create an enemy
            Enemy enemy = new Enemy();

            // Initialize the enemy
            enemy.Initialize(enemyAnimation, position);

            // Add the enemy to the active enemies list
            enemies.Add(enemy);
        }

        #region Update

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.T))
                this.Exit();

            // Save the previous state of the keyboard and game pad so we can determinesingle key/button presses
            previousGamePadState = currentGamePadState;
            previousKeyboardState = currentKeyboardState;

            // Read the current state of the keyboard and gamepad and store it
            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            if (GamePause == true)
            {

            }
            else if (GameScreenShown == true)
            {
                //Update the player
                UpdatePlayer(gameTime);


                // Update the parallaxing background
                bgLayer1.Update();
                bgLayer2.Update();

                // Update the enemies
                UpdateEnemies(gameTime);

                // Update the collision
                UpdateCollision();

                // Update the projectiles
                UpdateProjectiles();

                // Update the explosions
                UpdateExplosions(gameTime);
            }

            //Based on the screen state variables, call the
            //Update method associated with the current screen
            if (MainMenuScreenShown)
            {
                UpdateMainMenuScreen();
            }
            else if (GameScreenShown)
            {
                UpdateGameScreen();
            }
            else if (GameOverScreenShown)
            {
                UpdateGameOverMenuScreen();
            }
            if (GamePause)
            {
                UpdateGamePause();
            }
            base.Update(gameTime);

        }


        private void UpdateMainMenuScreen()
        {
            //Move back to the Controller detect screen if the player moves
            //back (using B) from the Title screen (this is typical game behavior
            //and is used to switch to a new player one controller)
            if (GamePad.GetState(mPlayerOne).Buttons.Start == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Enter) == true)
            {
                GameOverScreenShown = false;
                GameScreenShown = true;
                MainMenuScreenShown = false;
                player.Health = 100;
                return;
            }
        }

        private void UpdateGameScreen()
        {
            if (GamePad.GetState(mPlayerOne).Buttons.A == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.P) == true)
            {

                GamePause = true;
                return;
            }

            for (int aPlayer = 0; aPlayer < 4; aPlayer++)
            {
                if (player.Health <= 0)
                {
                    mPlayerOne = (PlayerIndex)aPlayer;
                    GameOverScreenShown = true;
                    GameScreenShown = false;
                    MainMenuScreenShown = false;
                    return;
                }
            }
        }

        private void UpdateGameOverMenuScreen()
        {
            if (GamePad.GetState(mPlayerOne).Buttons.Start == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.M) == true)
            {
                score = 0;
                GameOverScreenShown = false;
                GameScreenShown = false;
                MainMenuScreenShown = true;
                return;
            }
        }

        private void UpdateGamePause()
        {
            if (GamePad.GetState(mPlayerOne).Buttons.B == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.K) == true)
            {
                GamePause = false;
                //GameScreenShown = true;
                return;
            }
        }

        private void UpdateProjectiles()
        {
            // Update the Projectiles
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                projectiles[i].Update();

                if (projectiles[i].Active == false)
                {
                    projectiles.RemoveAt(i);
                }
            }
        }

        private void UpdateCollision()
        {
            // Use the Rectangle's built-in intersect function to 
            // determine if two objects are overlapping
            Rectangle rectangle1;
            Rectangle rectangle2;

            // Only create the rectangle once for the player
            rectangle1 = new Rectangle((int)player.Position.X,
            (int)player.Position.Y,
            player.Width,
            player.Height);

            // Do the collision between the player and the enemies
            for (int i = 0; i < enemies.Count; i++)
            {
                rectangle2 = new Rectangle((int)enemies[i].Position.X,
                (int)enemies[i].Position.Y,
                enemies[i].Width,
                enemies[i].Height);

                // Determine if the two objects collided with each
                // other
                if (rectangle1.Intersects(rectangle2))
                {
                    // Subtract the health from the player based on
                    // the enemy damage
                    player.Health -= enemies[i].Damage;

                    // Since the enemy collided with the player
                    // destroy it
                    enemies[i].Health = 0;

                    // If the player health is less than zero we died
                    if (player.Health <= 0)
                        player.Active = false;
                }

            }
            // Projectile vs Enemy Collision
            for (int i = 0; i < projectiles.Count; i++)
            {
                for (int j = 0; j < enemies.Count; j++)
                {
                    // Create the rectangles we need to determine if we collided with each other
                    rectangle1 = new Rectangle((int)projectiles[i].Position.X -
                    projectiles[i].Width / 2, (int)projectiles[i].Position.Y -
                    projectiles[i].Height / 2, projectiles[i].Width, projectiles[i].Height);

                    rectangle2 = new Rectangle((int)enemies[j].Position.X - enemies[j].Width / 2,
                    (int)enemies[j].Position.Y - enemies[j].Height / 2,
                    enemies[j].Width, enemies[j].Height);

                    // Determine if the two objects collided with each other
                    if (rectangle1.Intersects(rectangle2))
                    {
                        enemies[j].Health -= projectiles[i].Damage;
                        projectiles[i].Active = false;
                    }
                }
            }
        }

        private void UpdateExplosions(GameTime gameTime)
        {
            for (int i = explosions.Count - 1; i >= 0; i--)
            {
                explosions[i].Update(gameTime);
                if (explosions[i].Active == false)
                {
                    explosions.RemoveAt(i);
                }
            }
        }


        private void UpdateLeftStickDirection()
        {
            double testY = currentGamePadState.ThumbSticks.Left.Y;
            double testX = currentGamePadState.ThumbSticks.Left.X;

            String thumbStickDirectionTester = "Is it working yet???";

            if (GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X >= 0.5f)
            {
                if (GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y >= 0.5f)
                {
                    N = false;
                    NE = true;
                    East = false;
                    SE = false;
                    South = false;
                    SW = false;
                    W = false;
                    NW = false;
                }
                else if (GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y <= -0.5f)
                {
                    N = false;
                    NE = false;
                    East = false;
                    SE = true;
                    South = false;
                    SW = false;
                    W = false;
                    NW = false;
                }
                else
                {
                    N = false;
                    NE = false;
                    East = true;
                    SE = false;
                    South = false;
                    SW = false;
                    W = false;
                    NW = false;
                }
            }
            else if (GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X <= -0.5f)
            {
                if (GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y >= 0.5f)
                {
                    N = false;
                    NE = false;
                    East = false;
                    SE = false;
                    South = false;
                    SW = false;
                    W = false;
                    NW = true;
                }
                else if (GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y <= -0.5f)
                {
                    N = false;
                    NE = false;
                    East = false;
                    SE = false;
                    South = false;
                    SW = true;
                    W = false;
                    NW = false;
                }
                else
                {
                    N = false;
                    NE = false;
                    East = false;
                    SE = false;
                    South = false;
                    SW = false;
                    W = true;
                    NW = false;
                }
            }
            else if (GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y >= 0.5f && (GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X < 0.5f && GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X > -0.5f))
            {
                N = true;
                NE = false;
                East = false;
                SE = false;
                South = false;
                SW = false;
                W = false;
                NW = false;
            }
            else if (GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y <= -0.5f && (GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X < 0.5f && GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X > -0.5f))
            {
                N = false;
                NE = false;
                East = false;
                SE = false;
                South = true;
                SW = false;
                W = false;
                NW = false;
            }
            else
            {
                N = false;
                NE = false;
                East = false;
                SE = false;
                South = false;
                SW = false;
                W = false;
                NW = false;
            }
        }

        /// <summary>
        /// Updates the player based on the input from the user via gamepad or keyboard
        /// </summary>
        /// <param name="gameTime">The current time reference of the game</param>
        private void UpdatePlayer(GameTime gameTime)
        {
            player.Update(gameTime);

            // Get Thumbstick Controls
            player.Position.X += currentGamePadThumbSticks.ThumbSticks.Left.X * playerMoveSpeed;
            player.Position.Y -= currentGamePadThumbSticks.ThumbSticks.Left.Y * playerMoveSpeed;

            UpdateLeftStickDirection();

            // Use the Keyboard / Thumbsticks / Dpad
            if (currentKeyboardState.IsKeyDown(Keys.A) ||
            W == true ||
            currentGamePadState.DPad.Left == ButtonState.Pressed)
            {
                player.Position.X -= playerMoveSpeed;
            }

            if (currentKeyboardState.IsKeyDown(Keys.D) ||
            East == true ||
            currentGamePadState.DPad.Right == ButtonState.Pressed)
            {
                player.Position.X += playerMoveSpeed;
            }

            if (currentKeyboardState.IsKeyDown(Keys.W) ||
            N == true ||
            currentGamePadState.DPad.Up == ButtonState.Pressed)
            {
                player.Position.Y -= playerMoveSpeed;
            }
            if (NE == true)
            {
                player.Position.Y -= playerMoveSpeed;
                player.Position.X += playerMoveSpeed;
            }
            if (NW == true)
            {
                player.Position.Y -= playerMoveSpeed;
                player.Position.X -= playerMoveSpeed;
            }

            if (currentKeyboardState.IsKeyDown(Keys.S) ||
            South == true ||
            currentGamePadState.DPad.Down == ButtonState.Pressed)
            {
                player.Position.Y += playerMoveSpeed;
            }
            if (SE == true)
            {
                player.Position.Y += playerMoveSpeed;
                player.Position.X += playerMoveSpeed;
            }
            if (SW == true)
            {
                player.Position.Y += playerMoveSpeed;
                player.Position.X -= playerMoveSpeed;
            }

            // Make sure that the player does not go out of bounds
            player.Position.X = MathHelper.Clamp(player.Position.X, 0, GraphicsDevice.Viewport.Width - player.Width);
            player.Position.Y = MathHelper.Clamp(player.Position.Y, 0, GraphicsDevice.Viewport.Height - player.Height);

            if (currentKeyboardState.IsKeyDown(Keys.Space) || currentGamePadState.Buttons.LeftShoulder == ButtonState.Pressed)
            {
                        
                // Fire only every interval we set as the fireTime
                if (gameTime.TotalGameTime - previousFireTime > fireTime)
                {
                    // Reset our current time
                    previousFireTime = gameTime.TotalGameTime;

                    // Add the projectile, but add it to the front and center of the player
                    AddProjectile(player.Position + new Vector2(player.Width / 2, 0));

                    // Play the laser sound
                    laserSound.Play();


                    // If the Player is past the screen or its health reaches 0 then deactivate it
                    if (player.Health <= 0)
                    {
                        // By setting the Active flag to false, the game will remove this objet fromthe 
                        // active game list
                        player.Active = false;
                    }

                }
            }




        }

        private void UpdateEnemies(GameTime gameTime)
        {
            // Spawn a new enemy enemy every 1.5 seconds
            if (gameTime.TotalGameTime - previousSpawnTime > enemySpawnTime)
            {
                previousSpawnTime = gameTime.TotalGameTime;

                // Add an Enemy
                AddEnemy();
            }

            // Update the Enemies
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                enemies[i].Update(gameTime);

                if (enemies[i].Active == false)
                {

                    // If not active and health <= 0
                    if (enemies[i].Health <= 0)
                    {
                        // Add an explosion
                        AddExplosion(enemies[i].Position);

                        // Play the explosion sound
                        explosionSound.Play();

                        //Add to the player's score
                        score += enemies[i].Value;
                    }
                    enemies.RemoveAt(i);
                }
            }


        }
       
        #endregion Update

        #region Draw
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkRed);

            // Start drawing
            spriteBatch.Begin();


            //Based on the screen state variables, call the
            //Draw method associated with the current screen
            if (MainMenuScreenShown)
            {
                DrawMainMenuScreen();
                spriteBatch.DrawString(font, "To start game Press 'Enter' or 'start'", new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X + 175, GraphicsDevice.Viewport.TitleSafeArea.Y + 250), Color.White);
            }
            else if (GameScreenShown)
            {
                spriteBatch.Draw(mainBackground, Vector2.Zero, Color.White);

                // Draw the moving background
                bgLayer1.Draw(spriteBatch);
                bgLayer2.Draw(spriteBatch);

                // Draw the Enemies
                for (int i = 0; i < enemies.Count; i++)
                {
                    enemies[i].Draw(spriteBatch);
                }

                // Draw the Projectiles
                for (int i = 0; i < projectiles.Count; i++)
                {
                    projectiles[i].Draw(spriteBatch);
                }

                // Draw the explosions
                for (int i = 0; i < explosions.Count; i++)
                {
                    explosions[i].Draw(spriteBatch);
                }

                // Draw the Player
                player.Draw(spriteBatch);

            }
            else if (GameOverScreenShown)
            {
                DrawGameOverScreen();
                // Draw the score
                spriteBatch.DrawString(font, "Score: " + score, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X + 350, GraphicsDevice.Viewport.TitleSafeArea.Y + 200), Color.White);
            }



            if (GameScreenShown == true)
            {
                // Draw the score
                spriteBatch.DrawString(font, "Score: " + score, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y), Color.White);
                // Draw the player health
                spriteBatch.DrawString(font, "Health: " + player.Health, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 30), Color.White);
            }

            // Stop drawing
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawGameOverScreen()
        {
            //Draw all of the elements that are part of the Game Over screen
            spriteBatch.Draw(GameOverScreenBackground, Vector2.Zero, Color.White);
        }

        private void DrawMainMenuScreen()
        {   //Draw all of the elements that are part of the Main Menu screen
            spriteBatch.Draw(MainMenuScreenBackground, Vector2.Zero, Color.White);
        }

        #endregion Draw

        private void PlayMusic(Song song)
        {
            // Due to the way the MediaPlayer plays music,
            // we have to catch the exception. Music will play when the game is not tethered
            try
            {
                // Play the music
                MediaPlayer.Play(song);

                // Loop the currently playing song
                MediaPlayer.IsRepeating = true;
            }
            catch { }
        }
    }



}
