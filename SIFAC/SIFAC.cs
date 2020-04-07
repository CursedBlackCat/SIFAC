using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;

namespace SIFAC {
    public class Note {
        public float position; // Position of the note in seconds
        public int lane; // 0 through 8, left to right
        public Boolean isMultiple;
        public Boolean isHold;
        public Boolean isRelease;
        public float releaseNoteSpawnTime;
        public float parentNoteSpawnTime;
        public Boolean hasStar;
        public Boolean hasSpawned;
        public Note(float pos, int lan, Boolean multiple, Boolean hold, Boolean release, float releaseNoteTime, float parentNoteTime, Boolean star) {
            position = pos;
            lane = lan;
            isMultiple = multiple;
            isHold = hold;
            isRelease = release;
            releaseNoteSpawnTime = releaseNoteTime;
            parentNoteSpawnTime = parentNoteTime;
            hasStar = star;
            hasSpawned = false;
        }
    }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class SIFAC : Game {
        Texture2D noteTexture;
        Texture2D hitMarkerTexture;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Vector2[] hitMarkerPositions = new Vector2[9];
        Vector2 noteSpawnPosition;
        KeyboardState previousState;
        Video bgVideo;
        VideoPlayer bgVideoPlayer;
        Boolean playVideo = true;
        Note[] beatmap;

        public SIFAC() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            // TODO: Add your initialization logic here
            // graphics.PreferredBackBufferWidth = 1920;
            // graphics.PreferredBackBufferHeight = 1080;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            // TODO: Fine-tune hit marker positions
            float[] xOffsets = {
                graphics.PreferredBackBufferWidth / 2.4615f , // L4 and R4
                graphics.PreferredBackBufferWidth / 2.8235f , // L3 and R3
                graphics.PreferredBackBufferWidth / 3.84f , // L2 and R2
                graphics.PreferredBackBufferWidth / 6.8571f , // L1 and R1

            };
            float[] yOffsets = {
                graphics.PreferredBackBufferHeight / 1.44f , // L4 and R4
                graphics.PreferredBackBufferHeight / 2.16f , // L3 and R3
                graphics.PreferredBackBufferHeight / 4.32f , // L2 and R2
                graphics.PreferredBackBufferHeight / 21.6f , // L1 and R1

            };
            hitMarkerPositions[0] = new Vector2(graphics.PreferredBackBufferWidth / 2 - xOffsets[0], graphics.PreferredBackBufferHeight / 2 - yOffsets[0]);
            hitMarkerPositions[1] = new Vector2(graphics.PreferredBackBufferWidth / 2 - xOffsets[1], graphics.PreferredBackBufferHeight / 2 - yOffsets[1]);
            hitMarkerPositions[2] = new Vector2(graphics.PreferredBackBufferWidth / 2 - xOffsets[2], graphics.PreferredBackBufferHeight / 2 - yOffsets[2]);
            hitMarkerPositions[3] = new Vector2(graphics.PreferredBackBufferWidth / 2 - xOffsets[3], graphics.PreferredBackBufferHeight / 2 - yOffsets[3]);
            hitMarkerPositions[4] = new Vector2(graphics.PreferredBackBufferWidth / 2,graphics.PreferredBackBufferHeight / 2);
            hitMarkerPositions[5] = new Vector2(graphics.PreferredBackBufferWidth / 2 + xOffsets[3], graphics.PreferredBackBufferHeight / 2 - yOffsets[3]);
            hitMarkerPositions[6] = new Vector2(graphics.PreferredBackBufferWidth / 2 + xOffsets[2], graphics.PreferredBackBufferHeight / 2 - yOffsets[2]);
            hitMarkerPositions[7] = new Vector2(graphics.PreferredBackBufferWidth / 2 + xOffsets[1], graphics.PreferredBackBufferHeight / 2 - yOffsets[1]);
            hitMarkerPositions[8] = new Vector2(graphics.PreferredBackBufferWidth / 2 + xOffsets[0], graphics.PreferredBackBufferHeight / 2 - yOffsets[0]);
            base.Initialize();
            previousState = Keyboard.GetState();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            noteTexture = Content.Load<Texture2D>("Note");
            hitMarkerTexture = Content.Load<Texture2D>("HitMarker");
            bgVideo = Content.Load<Video>("believe_again");
            bgVideoPlayer = new VideoPlayer();

            //Load the beatmap
            string[] lines = System.IO.File.ReadAllLines(@"C:\Users\darre\source\repos\SIFAC\SIFAC\Beatmaps\believe_again.txt");
            beatmap = new Note[lines.Length];
            for (int i = 0; i < lines.Length; i++) {
                string[] data = lines[i].Split('/');
                int lane = -1;
                switch (data[1]) {
                    case "L4":
                        lane = 0;
                        break;
                    case "L3":
                        lane = 1;
                        break;
                    case "L2":
                        lane = 2;
                        break;
                    case "L1":
                        lane = 3;
                        break;
                    case "C":
                        lane = 4;
                        break;
                    case "R1":
                        lane = 5;
                        break;
                    case "R2":
                        lane = 6;
                        break;
                    case "R3":
                        lane = 7;
                        break;
                    case "R4":
                        lane = 8;
                        break;
                }
                beatmap[i] = new Note(float.Parse(data[0]), lane, bool.Parse(data[2]), bool.Parse(data[3]), bool.Parse(data[4]), float.Parse(data[5]), float.Parse(data[6]), bool.Parse(data[7]));
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            // TODO: Add your update logic here
            if (playVideo & bgVideoPlayer.State == MediaState.Stopped) {
                bgVideoPlayer.Volume = 0.2f;
                bgVideoPlayer.Play(bgVideo);
                playVideo = false;
            }
            var kstate = Keyboard.GetState();

            if (kstate.IsKeyDown(Keys.A) & !previousState.IsKeyDown(Keys.A)) {
                Console.WriteLine("L4");
            }
            if (kstate.IsKeyDown(Keys.S) & !previousState.IsKeyDown(Keys.S)) {
                Console.WriteLine("L3");
            }
            if (kstate.IsKeyDown(Keys.D) & !previousState.IsKeyDown(Keys.D)) {
                Console.WriteLine("L2");
            }
            if (kstate.IsKeyDown(Keys.F) & !previousState.IsKeyDown(Keys.F)) {
                Console.WriteLine("L1");
            }
            if (kstate.IsKeyDown(Keys.Space) & !previousState.IsKeyDown(Keys.Space)) {
                Console.WriteLine("C");
            }
            if (kstate.IsKeyDown(Keys.J) & !previousState.IsKeyDown(Keys.J)) {
                Console.WriteLine("R1");
            }
            if (kstate.IsKeyDown(Keys.K) & !previousState.IsKeyDown(Keys.K)) {
                Console.WriteLine("R2");
            }
            if (kstate.IsKeyDown(Keys.L) & !previousState.IsKeyDown(Keys.L)) {
                Console.WriteLine("R3");
            }
            if (kstate.IsKeyDown(Keys.OemSemicolon) & !previousState.IsKeyDown(Keys.OemSemicolon)) {
                Console.WriteLine("R4");
            }
            if (kstate.IsKeyDown(Keys.Escape) & !previousState.IsKeyDown(Keys.Escape)) {
                Console.WriteLine("Red");
                Exit();
            }
            if (kstate.IsKeyDown(Keys.Enter) & !previousState.IsKeyDown(Keys.Enter)) {
                Console.WriteLine("Blue");
            }
            base.Update(gameTime);
            previousState = kstate;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            spriteBatch.Draw(
                bgVideoPlayer.GetTexture(),
                new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                Color.White
                );

            double currentVideoPosition = bgVideoPlayer.PlayPosition.TotalSeconds;
            // Console.WriteLine(currentVideoPosition);
            foreach (Note note in beatmap) {
                //if (note.position <= currentVideoPosition + 2f & !note.hasSpawned){
                if (note.position <= currentVideoPosition & !note.hasSpawned) {
                    // Console.WriteLine(note.lane);
                    spriteBatch.Draw(noteTexture,
                    hitMarkerPositions[note.lane],
                    null,
                    Color.White,
                    0f,
                    new Vector2(hitMarkerTexture.Width / 2, hitMarkerTexture.Height / 2 - graphics.PreferredBackBufferHeight),
                    0.35f,
                    SpriteEffects.None,
                    0f);

                    note.hasSpawned = true;
                }
            }

            for (int i = 0; i < 9; i++) {
                spriteBatch.Draw(hitMarkerTexture,
                hitMarkerPositions[i],
                null,
                new Color(Color.White, 175),
                0f,
                new Vector2(hitMarkerTexture.Width / 2, hitMarkerTexture.Height / 2 - graphics.PreferredBackBufferHeight),
                0.35f,
                SpriteEffects.None,
                0f);
            }
         
            // spriteBatch.Draw(noteTexture, notePosition, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
