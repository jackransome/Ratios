using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Ratios
{

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D logo;

        Texture2D blackSquare;

        Vector2 screenDimensions = new Vector2(1920, 1080);

        Sequencer sequencer = new Sequencer();

        MouseState mouseState;

        KeyboardState keyboardState;

        Vector2 zoomPoint;

        Vector2 scale;

        float xZoomFactor = 1;

        float yZoomFactor = 1;

        Vector2 cameraPosition = new Vector2(1920, 1080);

        int previousScrollValue = 0;

        Vector2 selectInit =  new Vector2(0, 0);

        bool selecting;

        ButtonState previousRightButton = ButtonState.Released;

        public Game1()
        {
            var rand = new System.Random();
            for (int i = 0; i < 96; i++)
            {
                sequencer.addNote((float)(16.35*System.Math.Pow(1.06, i)), 0, 10);
            }

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = (int)screenDimensions.X;
            graphics.PreferredBackBufferHeight = (int)screenDimensions.Y;
            graphics.ApplyChanges();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

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

            logo = this.Content.Load<Texture2D>("afx logo");

            blackSquare = new Texture2D(GraphicsDevice, 1, 1);
            blackSquare.SetData(new Color[] { Color.DarkBlue });


            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            mouseState = Mouse.GetState();
            keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
                Exit();
            
            if (keyboardState.IsKeyDown(Keys.LeftControl))
            {
                if (mouseState.ScrollWheelValue < previousScrollValue)
                {
                    if (xZoomFactor > 1)
                    {
                        xZoomFactor /= 1.25f;
                    }
                }
                else if (mouseState.ScrollWheelValue > previousScrollValue)
                {
                    if (xZoomFactor < 30)
                    {
                        xZoomFactor *= 1.25f;
                    }
                }
            } else
            {
                if (mouseState.ScrollWheelValue < previousScrollValue)
                {
                    if (yZoomFactor > 1)
                    {
                        yZoomFactor /= 1.25f;
                    }
                }
                else if (mouseState.ScrollWheelValue > previousScrollValue)
                {
                    if (yZoomFactor < 30)
                    {
                        yZoomFactor *= 1.25f;
                    }
                }
            }
            if (keyboardState.IsKeyDown(Keys.W) && cameraPosition.Y > screenDimensions.Y/2.0f)
            {
                cameraPosition.Y -= 20 / yZoomFactor;
            }
            if (keyboardState.IsKeyDown(Keys.A) && cameraPosition.X > screenDimensions.X/2.0f)
            {
                cameraPosition.X -= 20 / xZoomFactor;
            }
            if (keyboardState.IsKeyDown(Keys.S) && cameraPosition.Y < screenDimensions.Y * 1.5)
            {
                cameraPosition.Y += 20 / yZoomFactor;
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                cameraPosition.X += 20 / xZoomFactor;
            }
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                float frequency = (float)(System.Math.Pow(2, -15 * (((mouseState.Position.Y - zoomPoint.Y) / yZoomFactor - screenDimensions.Y + cameraPosition.Y + zoomPoint.Y) / screenDimensions.Y - 1)) + 4);
                int startTime = (int)((mouseState.Position.X - zoomPoint.X) / xZoomFactor - screenDimensions.X + cameraPosition.X + zoomPoint.X);
                if (startTime > 0 && frequency > 30 && frequency < 20000)
                {
                    sequencer.addNote(frequency, startTime, 10);
                }
            }
            if (mouseState.RightButton == ButtonState.Pressed && previousRightButton == ButtonState.Released)
            {
                selectInit.X = mouseState.X;
                selectInit.Y = mouseState.Y;
                selecting = true;
            }
            else if (mouseState.RightButton == ButtonState.Released && previousRightButton == ButtonState.Pressed)
            {
                selecting = false;
            }

            previousRightButton = mouseState.RightButton;
                previousScrollValue = mouseState.ScrollWheelValue;
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();

            scale = new Vector2(0.25f,0.25f);

            //spriteBatch.Draw(logo, position: Vector2.Zero, scale: scale);

            zoomPoint = new Vector2(screenDimensions.X/2, screenDimensions.Y / 2);
            for (int i = 0; i < sequencer.notes.Count; i++)
            {
                
                float x = sequencer.notes[i].startTime;
                //Applying x camera transform
                x += (screenDimensions.X - cameraPosition.X);
                // Applying x zoom
                x = ((xZoomFactor * (x - zoomPoint.X)) + zoomPoint.X);
                float y = (float)(screenDimensions.Y * (1 - System.Math.Log(sequencer.notes[i].frequency - 4, 2) * (1.0f / 15.0f)));
                //Applying y camera transform
                y += (screenDimensions.Y - cameraPosition.Y);
                // Applying y zoom
                y = ((yZoomFactor * (y - zoomPoint.Y)) + zoomPoint.Y);
                blackSquare.SetData(new Color[] { Color.Black });
                spriteBatch.Draw(blackSquare, new Rectangle((int)x, (int)y, (int)(sequencer.notes[i].duration * xZoomFactor), 2), Color.Black);
            }

            if (selecting)
            {
                blackSquare.SetData(new Color[] { Color.Green });
                if (mouseState.X > selectInit.X && mouseState.Y < selectInit.Y)
                {
                    spriteBatch.Draw(blackSquare, new Rectangle((int)selectInit.X, (int)mouseState.Y, (int)(mouseState.X - selectInit.X), -(int)(mouseState.Y - selectInit.Y)), Color.Green * 0.33f);
                } else if (mouseState.X < selectInit.X && mouseState.Y > selectInit.Y) {
                    spriteBatch.Draw(blackSquare, new Rectangle((int)mouseState.X, (int)selectInit.Y, -(int)(mouseState.X - selectInit.X), (int)(mouseState.Y - selectInit.Y)), Color.Green * 0.33f);
                }
                else
                {
                    spriteBatch.Draw(blackSquare, new Rectangle((int)selectInit.X, (int)selectInit.Y, (int)(mouseState.X - selectInit.X), (int)(mouseState.Y - selectInit.Y)), Color.Green * 0.33f);
                }
            }

            blackSquare.SetData(new Color[] { Color.White });
            spriteBatch.Draw(blackSquare, new Rectangle(mouseState.X, mouseState.Y, 2, 2), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
