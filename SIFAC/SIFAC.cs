using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
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
        public NoteAccuracy result;
        public Boolean hasResolved;
        public Texture2D texture;
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
            hasResolved = false;
            result = NoteAccuracy.None;
        }
    }

    public enum NoteAccuracy {
        Perfect,
        Great,
        Good,
        Bad,
        Miss,
        None // Returns when you attempt to hit a note before it comes into range
    }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class SIFAC : Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D noteTexture;
        Texture2D noteMultiBlueTexture;
        Texture2D noteMultiOrangeTexture;
        Texture2D noteReleaseTexture;
        Texture2D noteReleaseMultiBlueTexture;
        Texture2D noteReleaseMultiOrangeTexture;
        Texture2D hitMarkerTexture;

        Vector2[] hitMarkerPositions = new Vector2[9];
        float[] xOffsets = new float[4];
        float[] yOffsets = new float[4];
        float radiusH; // This is set in Initialize()
        float radiusV; // This is set in Initialize()
        Vector2 noteSpawnPosition;
        Boolean lastMultiWasBlue = false; // Used to toggle between orange and blue multi notes

        KeyboardState previousState;

        Video bgVideo;
        VideoPlayer bgVideoPlayer;
        Boolean playVideo = true;

        Note[] beatmap;

        SoundEffect[] hitSoundEffects = new SoundEffect[4]; // hitSoundEffects[0] is perfect, 1 is great, 2 is good, 3 is bad

        int perfects = 0;
        int greats = 0;
        int goods = 0;
        int bads = 0;
        int misses = 0;


       /* CONFIG */
        float noteSpeed = 1f; // Note speed, represented by seconds from spawn to note hit position.

        // Timing tolerences, in seconds. Hitting a note at its time + or - each of these values gets the corresponding accuracy rating.
        double perfectTolerance = 0.2;
        double greatTolerance = 0.4;
        double goodTolerance = 0.6;
        double badTolerance = 1;
        double missTolerance = 1.5; // Not hitting a note after this much time elapses after its hit time will count as a miss

        // Timing offset setting, in seconds.
        double timeOffset = -0.25; // If too large, notes will be too early. If too small, notes will be too late.

        // Autoplay, for debug purposes
        Boolean autoplay = false;
        /* END CONFIG */

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
            // graphics.PreferredBackBufferWidth = 1920;
            // graphics.PreferredBackBufferHeight = 1080;
            // graphics.IsFullScreen = true;

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.IsFullScreen = false;

            graphics.ApplyChanges();

            radiusH = graphics.PreferredBackBufferWidth / 2.4615f;
            radiusV = graphics.PreferredBackBufferWidth / 2.4615f;

            noteSpawnPosition = new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2 - graphics.PreferredBackBufferHeight / 1.44f);

            for (int i = 0; i < 9; i++) {
                float x = (float)(noteSpawnPosition.X + radiusH * Math.Cos((i / 8f) * Math.PI));
                float y = (float)(noteSpawnPosition.Y + radiusV * (Math.Sin((i / 8f) * Math.PI)));
                Console.WriteLine((i / 8) * Math.PI);
                hitMarkerPositions[hitMarkerPositions.Length - i - 1] = new Vector2(x, y);
            }

            for (int i = 0; i < 4; i++) {
                xOffsets[i] = hitMarkerPositions[8 - i].X - noteSpawnPosition.X; // xOffsets[0] corresponds to L4 and R4. xOffsets[3] corresponds to L1 and R1
            }

            for (int i = 0; i < 4; i++) {
                yOffsets[i] = hitMarkerPositions[8 - i].Y - noteSpawnPosition.Y; // xOffsets[0] corresponds to L4 and R4. xOffsets[3] corresponds to L1 and R1
            }

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

            noteTexture = Content.Load<Texture2D>("Note");
            noteMultiBlueTexture = Content.Load<Texture2D>("notes/Note_Multi_Blue");
            noteMultiOrangeTexture = Content.Load<Texture2D>("notes/Note_Multi_Orange");
            noteReleaseTexture = Content.Load<Texture2D>("notes/Note_Hold_Release");
            noteReleaseMultiBlueTexture = Content.Load<Texture2D>("notes/Note_Hold_Release_Multi_Blue");
            noteReleaseMultiOrangeTexture = Content.Load<Texture2D>("notes/Note_Hold_Release_Multi_Orange");
            hitMarkerTexture = Content.Load<Texture2D>("HitMarker");
            bgVideo = Content.Load<Video>("believe_again");
            bgVideoPlayer = new VideoPlayer();

            hitSoundEffects[0] = Content.Load<SoundEffect>("sounds/hit_perfect");
            hitSoundEffects[1] = Content.Load<SoundEffect>("sounds/hit_great");
            hitSoundEffects[2] = Content.Load<SoundEffect>("sounds/hit_good");
            hitSoundEffects[3] = Content.Load<SoundEffect>("sounds/hit_bad");

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
            Array.Sort(beatmap, delegate (Note x, Note y) { return x.position.CompareTo(y.position); });
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
            if (playVideo & bgVideoPlayer.State == MediaState.Stopped) {
                bgVideoPlayer.Volume = 0.2f;
                bgVideoPlayer.Play(bgVideo);
                playVideo = false;
            }
           
            var kstate = Keyboard.GetState();
            // Detect key down
            if (kstate.IsKeyDown(Keys.A) & !previousState.IsKeyDown(Keys.A)) {
                judgeHit(0, true);
            }
            if (kstate.IsKeyDown(Keys.S) & !previousState.IsKeyDown(Keys.S)) {
                judgeHit(1, true);
            }
            if (kstate.IsKeyDown(Keys.D) & !previousState.IsKeyDown(Keys.D)) {
                judgeHit(2, true);
            }
            if (kstate.IsKeyDown(Keys.F) & !previousState.IsKeyDown(Keys.F)) {
                judgeHit(3, true);
            }
            if (kstate.IsKeyDown(Keys.Space) & !previousState.IsKeyDown(Keys.Space)) {
                judgeHit(4, true);
            }
            if (kstate.IsKeyDown(Keys.J) & !previousState.IsKeyDown(Keys.J)) {
                judgeHit(5, true);
            }
            if (kstate.IsKeyDown(Keys.K) & !previousState.IsKeyDown(Keys.K)) {
                judgeHit(6, true);
            }
            if (kstate.IsKeyDown(Keys.L) & !previousState.IsKeyDown(Keys.L)) {
                judgeHit(7, true);
            }
            if (kstate.IsKeyDown(Keys.OemSemicolon) & !previousState.IsKeyDown(Keys.OemSemicolon)) {
                judgeHit(8, true);
            }
            if (kstate.IsKeyDown(Keys.Escape) & !previousState.IsKeyDown(Keys.Escape)) {
                Console.WriteLine("Red Down");
                Exit();
            }
            if (kstate.IsKeyDown(Keys.Enter) & !previousState.IsKeyDown(Keys.Enter)) {
                Console.WriteLine("Blue Down");
            }

            // Detect key up
            if (!kstate.IsKeyDown(Keys.A) & previousState.IsKeyDown(Keys.A)) {
                judgeHit(0, false);
            }
            if (!kstate.IsKeyDown(Keys.S) & previousState.IsKeyDown(Keys.S)) {
                judgeHit(1, false);
            }
            if (!kstate.IsKeyDown(Keys.D) & previousState.IsKeyDown(Keys.D)) {
                judgeHit(2, false);
            }
            if (!kstate.IsKeyDown(Keys.F) & previousState.IsKeyDown(Keys.F)) {
                judgeHit(3, false);
            }
            if (!kstate.IsKeyDown(Keys.Space) & previousState.IsKeyDown(Keys.Space)) {
                judgeHit(4, false);
            }
            if (!kstate.IsKeyDown(Keys.J) & previousState.IsKeyDown(Keys.J)) {
                judgeHit(5, false);
            }
            if (!kstate.IsKeyDown(Keys.K) & previousState.IsKeyDown(Keys.K)) {
                judgeHit(6, false);
            }
            if (!kstate.IsKeyDown(Keys.L) & previousState.IsKeyDown(Keys.L)) {
                judgeHit(7, false);
            }
            if (!kstate.IsKeyDown(Keys.OemSemicolon) & previousState.IsKeyDown(Keys.OemSemicolon)) {
                judgeHit(8, false);
            }
            if (!kstate.IsKeyDown(Keys.Escape) & previousState.IsKeyDown(Keys.Escape)) {
                Console.WriteLine("Red Up");
                Exit();
            }
            if (!kstate.IsKeyDown(Keys.Enter) & previousState.IsKeyDown(Keys.Enter)) {
                Console.WriteLine("Blue Up");
            }

            foreach (Note note in beatmap) {
                //AUTOPLAY CODE
                if (autoplay && !note.hasResolved && note.position <= bgVideoPlayer.PlayPosition.TotalSeconds + timeOffset) {
                    // Console.WriteLine("Auto");
                    note.result = NoteAccuracy.Perfect;
                    hitSoundEffects[0].Play(0.2f, 0f, 0f);
                    note.hasResolved = true;
                }
                //END AUTOPLAY CODE

                if (note.result != NoteAccuracy.None) {
                    note.hasResolved = true;
                }
                if (!note.hasResolved && note.position <= bgVideoPlayer.PlayPosition.TotalSeconds - missTolerance) { //TODO factor in time offset
                    Console.WriteLine("Miss");
                    note.result = NoteAccuracy.Miss;
                    note.hasResolved = true;
                    misses++;
                }
            }

            if (bgVideoPlayer.State == MediaState.Stopped) {
                Console.WriteLine("Perfect: " + perfects);
                Console.WriteLine("Great: " + greats);
                Console.WriteLine("Good: " + goods);
                Console.WriteLine("Bad: " + bads);
                Console.WriteLine("Miss: " + misses);
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

            spriteBatch.Begin();
            spriteBatch.Draw(
                bgVideoPlayer.GetTexture(),
                new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                Color.White
                );

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

            double currentVideoPosition = bgVideoPlayer.PlayPosition.TotalSeconds;
            Note previousNote = new Note(-1, -1, false, false, false, 0, 0, false);
            foreach (Note note in beatmap) {
                if (note.position <= currentVideoPosition + noteSpeed && !note.hasResolved) {
                    float[] coordinates = calculateNoteCoordinates(currentVideoPosition, note);
                    float noteSize = 0.35f - (float)((note.position - currentVideoPosition)*0.15f);

                    // TODO figure out why orange and blue aren't working as expected
                    if (note.texture == null) {
                        if (note.isRelease & note.isMultiple) {
                            if (lastMultiWasBlue) {
                                note.texture = noteReleaseMultiOrangeTexture;
                                if (previousNote.position != note.position) {
                                    lastMultiWasBlue = false;
                                }
                            } else {
                                note.texture = noteReleaseMultiBlueTexture;
                                if (previousNote.position != note.position) {
                                    lastMultiWasBlue = true;
                                }
                            }
                        } else if (note.isMultiple) {
                            if (lastMultiWasBlue) {
                                note.texture = noteMultiOrangeTexture;
                                if (previousNote.position != note.position) {
                                    lastMultiWasBlue = false;
                                }
                            } else {
                                note.texture = noteMultiBlueTexture;
                                if (previousNote.position != note.position) {
                                    lastMultiWasBlue = true;
                                }
                            }
                        } else if (note.isRelease) {
                            note.texture = noteReleaseTexture;
                        } else {
                            note.texture = noteTexture;
                        }
                    }

                    // TODO hold note trails
                    /*if (note.isHold) {
                        Note releaseNote = getReleaseNote(note);
                        if (releaseNote.hasSpawned) {
                            spriteBatch.Draw(texture,
                                new Vector2(coordinates[0], coordinates[1]),
                                null,
                                new Color(Color.White, 175),
                                0f,
                                new Vector2(noteTexture.Width / 2, noteTexture.Height / 2 - graphics.PreferredBackBufferHeight),
                                noteSize,
                                SpriteEffects.None,
                                0f);
                        } else {
                            // Indefinite trail to note spawn position
                            
                        }
                    }*/

                    spriteBatch.Draw(note.texture,
                        new Vector2(coordinates[0], coordinates[1]),
                        null,
                        Color.White,
                        0f,
                        new Vector2(note.texture.Width / 2, note.texture.Height / 2 - graphics.PreferredBackBufferHeight),
                        noteSize,
                        SpriteEffects.None,
                        0f);
                    note.hasSpawned = true;
                }
                previousNote = note;
            }

            // spriteBatch.Draw(noteTexture, notePosition, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// This helper method calculates where a note should be drawn on the screen.
        /// </summary>
        /// <param name="songPosition">The current audio time, in seconds.</param>
        /// <param name="lane">The lane in which the note should be drawn.</param>
        /// <param name="note">The Note to be drawn.</param>
        /// <returns>Returns a float array of length 2, where the 0th element is the X coordinate of the note and the 1st element is the Y coordinate.</returns>
        private float[] calculateNoteCoordinates(double songPosition, Note note) {
            int lane = note.lane;

            // First, calculate the X coordinates
            float xCoord = 0;
            float deltaX = 0;
            // Determine the delta X
            switch (lane) {
                case 4: // C
                    // Special case, deltaX is 0 as x is constant.
                    break;
                case 3: // L1
                    deltaX = -xOffsets[3];
                    break;
                case 5: // R1
                    deltaX = +xOffsets[3];
                    break;
                case 2: // L2
                    deltaX = -xOffsets[2];
                    break;
                case 6: // R2
                    deltaX = xOffsets[2];
                    break;
                case 1: // L3
                    deltaX = -xOffsets[1];
                    break;
                case 7: // R3
                    deltaX = xOffsets[1];
                    break;
                case 0: // L4
                    deltaX = -xOffsets[0];
                    break;
                case 8: // R4
                    deltaX = xOffsets[0];
                    break;
            }
            // Determine the note's pixels per second given the globally set note speed in seconds
            float xPixelSpeed = deltaX / noteSpeed;

            // Determine the current time position between note spawn and note hit
            float timePosition = note.position - (float) songPosition;

            xCoord = hitMarkerPositions[lane].X - (xPixelSpeed * timePosition);

            float yCoord;
            float deltaY = 0;
            // Determine the delta Y
            switch (lane) {
                case 4: // C
                    deltaY = noteSpawnPosition.Y - hitMarkerPositions[4].Y;
                    break;
                case 3: // L1
                case 5: // R1
                    deltaY = noteSpawnPosition.Y - hitMarkerPositions[3].Y;
                    break;
                case 2: // L2
                case 6: // R2
                    deltaY = noteSpawnPosition.Y - hitMarkerPositions[2].Y;
                    break;
                case 1: // L3
                case 7: // R3
                    deltaY = noteSpawnPosition.Y - hitMarkerPositions[1].Y;
                    break;
                case 0: // L4
                case 8: // R4
                    deltaY = noteSpawnPosition.Y - hitMarkerPositions[0].Y;
                    break;
            }

            deltaY += 100;

            float yPixelSpeed = -deltaY / noteSpeed;
            yCoord = hitMarkerPositions[lane].Y - (yPixelSpeed * timePosition);

            float[] result = { xCoord, yCoord };
            return result;
        }

        /// <summary>
        /// Given a hold note, returns the release note for that hold note, or null if none is found.
        /// </summary>
        /// <param name="holdNote">The hold note for which the release note should be found.</param>
        /// <returns>The corresponding release note of the hold note.</returns>
        private Note getReleaseNote(Note holdNote) {
            if (holdNote.isHold) {
                foreach (Note note in beatmap) {
                    if (note.isRelease && note.lane == holdNote.lane && note.position > holdNote.position) {
                        return note;
                    }
                }
                return null;
            } else {
                throw new InvalidOperationException();
            }   
        }

        /// <summary>
        /// Judges the accuracy of a hit.
        /// </summary>
        /// <param name="lane">The lane in which the button was pressed.</param>
        /// <param name="down">Whether the button is being pushed or released. Should be true if pushed, otherwise false if released (used for hold note releases).</param>
        /// <returns></returns>
        private NoteAccuracy judgeHit(int lane, Boolean down) {
            foreach (Note note in beatmap) {
                if (down && !note.isRelease) {
                    if (note.lane == lane && note.position - bgVideoPlayer.PlayPosition.TotalSeconds <= badTolerance) {
                        double diff = note.position - bgVideoPlayer.PlayPosition.TotalSeconds + timeOffset;
                        if (Math.Abs(diff) <= perfectTolerance) {
                            Console.WriteLine("Perfect (early by " + diff + ")");
                            hitSoundEffects[0].Play(0.2f, 0f, 0f);
                            note.result = NoteAccuracy.Perfect;
                            note.hasResolved = true;
                            return NoteAccuracy.Perfect;
                        } else if (Math.Abs(diff) <= greatTolerance) {
                            Console.WriteLine("Great (early by " + diff + ")");
                            hitSoundEffects[1].Play(0.2f, 0f, 0f);
                            note.result = NoteAccuracy.Great;
                            note.hasResolved = true;
                            return NoteAccuracy.Great;
                        } else if (Math.Abs(diff) <= goodTolerance) {
                            Console.WriteLine("Good (early by " + diff + ")");
                            hitSoundEffects[2].Play(0.2f, 0f, 0f);
                            note.result = NoteAccuracy.Good;
                            note.hasResolved = true;
                            return NoteAccuracy.Good;
                        } else if (Math.Abs(diff) <= badTolerance) {
                            Console.WriteLine("Bad (early by " + diff + ")");
                            hitSoundEffects[3].Play(0.2f, 0f, 0f);
                            note.result = NoteAccuracy.Bad;
                            note.hasResolved = true;
                            return NoteAccuracy.Bad;
                        }
                    }
                } else if (!down && note.isRelease) {
                    // TODO this code is duplicated from above for now but needs to be separate because releases don't count for combo
                    if (note.lane == lane && note.position - bgVideoPlayer.PlayPosition.TotalSeconds <= badTolerance) {
                        double diff = note.position - bgVideoPlayer.PlayPosition.TotalSeconds + timeOffset;
                        if (Math.Abs(diff) <= perfectTolerance) {
                            Console.WriteLine("Perfect (early by " + diff + ")");
                            hitSoundEffects[0].Play(0.2f, 0f, 0f);
                            note.result = NoteAccuracy.Perfect;
                            note.hasResolved = true;
                            perfects++;
                            return NoteAccuracy.Perfect;
                        } else if (Math.Abs(diff) <= greatTolerance) {
                            Console.WriteLine("Great (early by " + diff + ")");
                            hitSoundEffects[1].Play(0.2f, 0f, 0f);
                            note.result = NoteAccuracy.Great;
                            note.hasResolved = true;
                            greats++;
                            return NoteAccuracy.Great;
                        } else if (Math.Abs(diff) <= goodTolerance) {
                            Console.WriteLine("Good (early by " + diff + ")");
                            hitSoundEffects[2].Play(0.2f, 0f, 0f);
                            note.result = NoteAccuracy.Good;
                            note.hasResolved = true;
                            goods++;
                            return NoteAccuracy.Good;
                        } else if (Math.Abs(diff) <= badTolerance) {
                            Console.WriteLine("Bad (early by " + diff + ")");
                            hitSoundEffects[3].Play(0.2f, 0f, 0f);
                            note.result = NoteAccuracy.Bad;
                            note.hasResolved = true;
                            bads++;
                            return NoteAccuracy.Bad;
                        }
                    }
                }
            }
            return NoteAccuracy.None;
        }
    }
}
