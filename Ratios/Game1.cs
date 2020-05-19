using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System;

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

        FileLoader fileLoader = new FileLoader();

        Sequencer sequencer = new Sequencer();

        MouseState mouseState;

        KeyboardState keyboardState;

        Vector2 zoomPoint;

        Vector2 scale;

        float xZoomFactor = 10;

        float yZoomFactor = 1;

        float minXZoomFactor = 10;

        float maxXZoomFactor = 800;

        float minYZoomFactor = 1;

        float maxYZoomFactor = 30;

        Vector2 cameraPosition = new Vector2(1920/2, 1080/2);

        int previousScrollValue = 0;

        Vector2 selectInit =  new Vector2(0, 0);

        Vector2 snappedMouse = new Vector2(0, 0);

        bool selecting = false;

        bool noteDrawing = false;

        ButtonState previousRightButton = ButtonState.Released;

        ButtonState previousLeftButton = ButtonState.Released;

        float bpm = 130;

        // Audio stuff
        private const int SampleRate = 44100;
        private const int ChannelsCount = 2;
        
        private DynamicSoundEffectInstance _instance;

        // On your class
        public const int SamplesPerBuffer = 500;
        private float[,] _workingBuffer;
        private byte[] _xnaBuffer;

        private double _time = 0.0;


        public Game1()
        {
            sequencer .attachFileLoader(fileLoader);

            var rand = new Random();
            //generating notes C1 to B8
            for (int i = 0; i < 84; i++)
            {
                sequencer.addNote((float)(32.70 * Math.Pow((float)Math.Pow(2, 1.0f / 12.0f), i)), i, 1, 1, "sample1");
            }

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            sequencer.bpm = bpm;

            fileLoader.loadSample("sample1", "untitled.wav");

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
            spriteBatch = new SpriteBatch(GraphicsDevice);

            logo = this.Content.Load<Texture2D>("afx logo");

            blackSquare = new Texture2D(GraphicsDevice, 1, 1);
            blackSquare.SetData(new Color[] { Color.DarkBlue });

            // audio stuff

            _instance = new DynamicSoundEffectInstance(SampleRate, (ChannelsCount == 2) ? AudioChannels.Stereo : AudioChannels.Mono);

            _workingBuffer = new float[ChannelsCount, SamplesPerBuffer];
            const int bytesPerSample = 2;
            _xnaBuffer = new byte[ChannelsCount * SamplesPerBuffer * bytesPerSample];

            _instance.Play();

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

            if (keyboardState.IsKeyDown(Keys.Space))
            {
                _time = 0;
            }

            if (keyboardState.IsKeyDown(Keys.LeftControl))
            {
                if (mouseState.ScrollWheelValue < previousScrollValue)
                {
                    if (xZoomFactor > minXZoomFactor)
                    {
                        xZoomFactor /= 1.25f;
                    }
                }
                else if (mouseState.ScrollWheelValue > previousScrollValue)
                {
                    if (xZoomFactor < maxXZoomFactor)
                    {
                        xZoomFactor *= 1.25f;
                    }
                }
            } else
            {
                if (mouseState.ScrollWheelValue < previousScrollValue)
                {
                    if (yZoomFactor > minYZoomFactor)
                    {
                        yZoomFactor /= 1.25f;
                    }
                }
                else if (mouseState.ScrollWheelValue > previousScrollValue)
                {
                    if (yZoomFactor < maxYZoomFactor)
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
            if (keyboardState.IsKeyDown(Keys.Delete))
            {
                //finding and deleting all selected notes
                for (int i = 0; i < sequencer.notes.Count; i++)
                {
                    if (sequencer.notes[i].selected)
                    {
                        sequencer.removeNote(i);
                        i--;
                    }
                }
            }
            if (mouseState.LeftButton == ButtonState.Pressed && previousLeftButton == ButtonState.Released)
            {
                noteDrawing = true;
                selecting = false;
                selectInit.X = snappedMouse.X;
                selectInit.Y = snappedMouse.Y;
                sequencer.deselectAll();
            }
            else if (mouseState.LeftButton == ButtonState.Released && previousLeftButton == ButtonState.Pressed)
            {
                noteDrawing = false;
                float frequency = getFreqFromY((int)selectInit.Y);
                float startTime = getStartTimeFromX((int)selectInit.X);
                if (startTime < 0)
                {
                    startTime = 0;
                }
                float duration = getStartTimeFromX((int)snappedMouse.X) - startTime;
                if (startTime >= 0 && frequency > 20 && frequency < 20000 && snappedMouse.X > selectInit.X)
                {
                    sequencer.addNote(frequency, startTime, duration, 1, "sample1");
                }
            }
            else if (mouseState.LeftButton == ButtonState.Released && mouseState.RightButton == ButtonState.Pressed && previousRightButton == ButtonState.Released)
            {
                selectInit.X = mouseState.X;
                selectInit.Y = mouseState.Y;
                selecting = true;
            }
            else if (mouseState.LeftButton == ButtonState.Released && mouseState.RightButton == ButtonState.Released && previousRightButton == ButtonState.Pressed)
            {
                //select notes inside the rect:
                for (int i = 0; i < sequencer.notes.Count; i++)
                {
                    if (!keyboardState.IsKeyDown(Keys.LeftShift))
                    {
                        sequencer.notes[i].setSelected(false);
                    }
                    
                    //checking startTime
                    if (mouseState.X < selectInit.X)
                    {
                        if (mouseState.X < getXFromStartTime(sequencer.notes[i].startTime) && getXFromStartTime(sequencer.notes[i].startTime) < selectInit.X)
                        {
                            //checking freq:
                            if (mouseState.Y < selectInit.Y)
                            {
                                if (mouseState.Y < getYFromFreq(sequencer.notes[i].frequency) && getYFromFreq(sequencer.notes[i].frequency) < selectInit.Y)
                                {
                                    sequencer.notes[i].setSelected(true);
                                }
                            }
                            else if (mouseState.Y > selectInit.Y)
                            {
                                if (mouseState.Y > getYFromFreq(sequencer.notes[i].frequency) && getYFromFreq(sequencer.notes[i].frequency) > selectInit.Y)
                                {
                                    sequencer.notes[i].setSelected(true);
                                }
                            }
                        }
                    }
                    else if (mouseState.X > selectInit.X)
                    {
                        if (mouseState.X > getXFromStartTime(sequencer.notes[i].startTime) && getXFromStartTime(sequencer.notes[i].startTime) > selectInit.X)
                        {
                            //checking freq:
                            if (mouseState.Y < selectInit.Y)
                            {
                                if (mouseState.Y < getYFromFreq(sequencer.notes[i].frequency) && getYFromFreq(sequencer.notes[i].frequency) < selectInit.Y)
                                {
                                    sequencer.notes[i].setSelected(true);
                                }
                            }
                            else if (mouseState.Y > selectInit.Y)
                            {
                                if (mouseState.Y > getYFromFreq(sequencer.notes[i].frequency) && getYFromFreq(sequencer.notes[i].frequency) > selectInit.Y)
                                {
                                    sequencer.notes[i].setSelected(true);
                                }
                            }
                        }
                    }
                }
                selecting = false;
            }

            //getting the snapped position of the mouse
            float tempX = getStartTimeFromX(mouseState.X);
            float tempY = mouseState.Y;

            float ratio = (float)Math.Pow(2, 1.0f / 12.0f);
            float baseFreq = 440;

            float xSnap = 1;

            snappedMouse.X = getXFromStartTime((int)(xSnap * Math.Round(tempX / xSnap)));
            snappedMouse.Y = getYFromFreq((float)(baseFreq * Math.Pow(ratio, Math.Round(Math.Log(getFreqFromY(mouseState.Y)/ baseFreq, ratio)))));

            previousLeftButton = mouseState.LeftButton;
            previousRightButton = mouseState.RightButton;
            previousScrollValue = mouseState.ScrollWheelValue;

            while (_instance.PendingBufferCount < 3)
            {
                SubmitBuffer();
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            spriteBatch.Begin();

            scale = new Vector2(0.25f,0.25f);

            //spriteBatch.Draw(logo, position: Vector2.Zero, scale: scale);

            zoomPoint = new Vector2(screenDimensions.X/2, screenDimensions.Y / 2);
            
            //drawing notes
            for (int i = 0; i < sequencer.notes.Count; i++)
            {
                int x = getXFromStartTime(sequencer.notes[i].startTime);
                int y = getYFromFreq(sequencer.notes[i].frequency);
                if (sequencer.notes[i].selected)
                {
                    blackSquare.SetData(new Color[] { Color.GreenYellow });
                    spriteBatch.Draw(blackSquare, new Rectangle(x, y, (int)(sequencer.notes[i].duration * xZoomFactor), 2), Color.GreenYellow);
                } else
                {
                    blackSquare.SetData(new Color[] { Color.Black });
                    spriteBatch.Draw(blackSquare, new Rectangle(x, y, (int)(sequencer.notes[i].duration * xZoomFactor), 2), Color.Black);
                }
            }

            //drawing the green selection rectangle
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
                //drawing the red bit when drawing a new note
            if (noteDrawing && getStartTimeFromX((int)selectInit.X) >= 0 && getFreqFromY((int)selectInit.Y) > 30 && getFreqFromY((int)selectInit.Y) < 20000 && snappedMouse.X > selectInit.X)
            {
                blackSquare.SetData(new Color[] { Color.Red });
                spriteBatch.Draw(blackSquare, new Rectangle((int)selectInit.X, (int)selectInit.Y, (int)(snappedMouse.X - selectInit.X), 2), Color.Red * 0.33f);
            }

            //drawing mouse cursor
            blackSquare.SetData(new Color[] { Color.Black });
            spriteBatch.Draw(blackSquare, new Rectangle(mouseState.X, mouseState.Y, 2, 2), Color.Black * 0.5f);

            //drawing snapped cursor
            blackSquare.SetData(new Color[] { Color.White });
            spriteBatch.Draw(blackSquare, new Rectangle((int)snappedMouse.X, (int)snappedMouse.Y, 2, 2), Color.White);

            //drawing the play line
            blackSquare.SetData(new Color[] { Color.Yellow });
            float u = (float)_time * (bpm / 60.0f);
            spriteBatch.Draw(blackSquare, new Rectangle((int)getXFromStartTime((float)_time*(bpm/60.0f)), 0, 2, (int)screenDimensions.Y), Color.Yellow);

            //drawing left border
            spriteBatch.Draw(blackSquare, new Rectangle(getXFromStartTime(0) - 2, 0, 2, (int)screenDimensions.Y), Color.Black);

            //drawing top border
            spriteBatch.Draw(blackSquare, new Rectangle(0, getYFromFreq(20), (int)screenDimensions.X, 2), Color.Black);

            //drawing bottom border
            spriteBatch.Draw(blackSquare, new Rectangle(0, getYFromFreq(20000), (int)screenDimensions.X, 2), Color.Black);

            //draw verticle lines
            for (int i = 0; i < 1000; i++)
            {
                blackSquare.SetData(new Color[] { Color.White });
                spriteBatch.Draw(blackSquare, new Rectangle(getXFromStartTime(i), 0, 2, (int)screenDimensions.Y), Color.White * 0.1f);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        float getFreqFromY(int _y)
        {
            return (float)(Math.Pow(2, -15 * (((_y - zoomPoint.Y) / yZoomFactor - screenDimensions.Y + cameraPosition.Y + zoomPoint.Y) / screenDimensions.Y - 1)) + 4);
        }

        int getYFromFreq(float _freq)
        {
            float y = (float)(screenDimensions.Y * (1 - Math.Log(_freq - 4, 2) * (1.0f / 15.0f)));
            //Applying y camera transform
            y += (screenDimensions.Y - cameraPosition.Y);
            // Applying y zoom
            y = ((yZoomFactor * (y - zoomPoint.Y)) + zoomPoint.Y);
            return (int)y;
        }

        float getStartTimeFromX(int _x)
        {
            return ((_x - zoomPoint.X) / xZoomFactor - screenDimensions.X + cameraPosition.X + zoomPoint.X);
        }

        int getXFromStartTime(float _startTime)
        {
            float x = _startTime;
            //Applying x camera transform
            x += (screenDimensions.X - cameraPosition.X);
            // Applying x zoom
            x = ((xZoomFactor * (x - zoomPoint.X)) + zoomPoint.X);
            return (int)x;
        }

        // from https://www.david-gouveia.com/creating-a-basic-synth-in-xna-part-ii
        //converts from working buffer to xna buffer
        private static void ConvertBuffer(float[,] from, byte[] to)
        {
            const int bytesPerSample = 2;
            int channels = from.GetLength(0);
            int bufferSize = from.GetLength(1);

            // Make sure the buffer sizes are correct
            System.Diagnostics.Debug.Assert(to.Length == bufferSize * channels * bytesPerSample, "Buffer sizes are mismatched.");

            for (int i = 0; i < bufferSize; i++)
            {
                for (int c = 0; c < channels; c++)
                {
                    // First clamp the value to the [-1.0..1.0] range
                    float floatSample = MathHelper.Clamp(from[c, i], -1.0f, 1.0f);

                    // Convert it to the 16 bit [short.MinValue..short.MaxValue] range
                    short shortSample = (short)(floatSample >= 0.0f ? floatSample * short.MaxValue : floatSample * short.MinValue * -1);

                    // Calculate the right index based on the PCM format of interleaved samples per channel [L-R-L-R]
                    int index = i * channels * bytesPerSample + c * bytesPerSample;

                    // Store the 16 bit sample as two consecutive 8 bit values in the buffer with regard to endian-ness
                    if (!BitConverter.IsLittleEndian)
                    {
                        to[index] = (byte)(shortSample >> 8);
                        to[index + 1] = (byte)shortSample;
                    }
                    else
                    {
                        to[index] = (byte)shortSample;
                        to[index + 1] = (byte)(shortSample >> 8);
                    }
                }
            }
        }
        private void FillWorkingBuffer()
        {
            for (int i = 0; i < SamplesPerBuffer; i++)
            {
                // Here is where you sample your wave function
                _workingBuffer[0, i] = sequencer.getDisplacement(false, _time); // Left Channel
                _workingBuffer[1, i] = sequencer.getDisplacement(true, _time); // Right Channel

                // Advance time passed since beginning
                // Since the amount of samples in a second equals the chosen SampleRate
                // Then each sample should advance the time by 1 / SampleRate
                _time += 1.0 / SampleRate;
            }
        }
        private void SubmitBuffer()
        {
            FillWorkingBuffer();
            ConvertBuffer(_workingBuffer, _xnaBuffer);
            _instance.SubmitBuffer(_xnaBuffer);
        }
    }
}
