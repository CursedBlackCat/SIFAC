using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SIFAC {
    /// <summary>
    /// The main type for the game.
    /// </summary>
    public class SIFAC : Game {
        /*GLOBALLY USED VARIABLES*/
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GameState currentGameState = GameState.SongSelectScreen;
        SpriteFont defaultFont;
        KeyboardState previousState;
        List<PlayableSong> songs = new List<PlayableSong>();
        PlayableSong currentSong;

        /*TITLE SCREEN VARIABLES*/

        /*NESICA CHECK SCREEN VARIABLES*/

        /*GROUP SELECT SCREEN VARIABLES*/

        /*SONG SELECT SCREEN VARIABLES*/
        Texture2D tooltipL3L4;
        Texture2D[] mainTooltips = new Texture2D[5]; //0 to 4, left to right. These are the main 5 tooltips on the song select screen spanning L2 through R2.
        Texture2D tooltipR3R4;
        Texture2D[] mainHighlightedTooltips = new Texture2D[5];
        int highlightedMenuElement = 2;
        int songSelectPage = 0; // Each page has 5 songs, and page n is indices n*5 through n*5+4 of songs
        PlayableSong[] menuChoices = new PlayableSong[5]; // Currently displayed song choices. 0 to 4, left to right. Index 0 corresonds to L2, 1 to L1, etc, and 4 to R2.

        /*LIVE PREPARATION SCREEN VARIABLES*/

        /*LIVE SCREEN VARIABLES*/
        Texture2D noteTexture;
        Texture2D noteMultiBlueTexture;
        Texture2D noteMultiOrangeTexture;
        Texture2D noteReleaseTexture;
        Texture2D noteReleaseMultiBlueTexture;
        Texture2D noteReleaseMultiOrangeTexture;
        Texture2D noteStarTexture;
        Texture2D noteStarMultiBlueTexture;
        Texture2D noteStarMultiOrangeTexture;
        Texture2D hitMarkerTexture;
        Texture2D noteTrailTexture;
        Texture2D starL;
        Texture2D starO;
        Texture2D starV;
        Texture2D starE;
        Texture2D starI;
        Texture2D starExclamation;
        Texture2D starEmpty;
        Texture2D scoreGaugeBase;
        Texture2D scoreGaugeD;
        Texture2D scoreGaugeC;
        Texture2D scoreGaugeB;
        Texture2D scoreGaugeA;
        Texture2D scoreGaugeS;
        Texture2D scoreGaugeFrame;
        Texture2D[] starTextures = new Texture2D[9]; // For convenience of spritebatch drawing
        Vector2[] hitMarkerPositions = new Vector2[9];
        float[] xOffsets = new float[4];
        float[] yOffsets = new float[4];
        float radiusH; // This is set in Initialize()
        float radiusV; // This is set in Initialize()
        Vector2 noteSpawnPosition;
        List<NoteTrail> noteTrailPositions = new List<NoteTrail>();
        Boolean lastMultiWasBlue = false; // Used to toggle between orange and blue multi notes
        VideoPlayer bgVideoPlayer;
        SoundEffect[] hitSoundEffects = new SoundEffect[4]; // hitSoundEffects[0] is perfect, 1 is great, 2 is good, 3 is bad
        int combo = 0;
        int maxCombo = 0;
        int perfects = 0;
        int greats = 0;
        int goods = 0;
        int bads = 0;
        int misses = 0;
        int stars = 0;
        int score = 0;
        ScoreRank rank = ScoreRank.D;

        /*RESULT SCREEN VARIABLES*/

        /*GOODBYE SCREEN VARIABLES*/

        /* CONFIG VARIABLES */
        // Timing tolerences, in seconds. Hitting a note at its time + or - each of these values gets the corresponding accuracy rating.
        readonly double perfectTolerance = 0.1;
        readonly double greatTolerance = 0.2;
        readonly double goodTolerance = 0.4;
        readonly double badTolerance = 0.8;
        readonly double missTolerance = 1; // Not hitting a note after this much time elapses after its hit time will count as a miss

        float noteSpeed = 1f; // Note speed, represented by seconds from spawn to note hit position.

        // Timing offset setting, in seconds.
        // Use a timeOffset value of -0.05 for playing and -0.25 for autoplay. These values aren't perfect.
        // If too large, notes will be too early. If too small, notes will be too late.
        double timeOffset = 0;     

        // Autoplay, for debug purposes
        Boolean autoplay = true;

        // Fullscreen 1080p vs 720p flag for debugging. Game is intended to be played fullscreen at 1080p.
        Boolean fullscreen = false;

        float noteHitVolume = 0.1f;
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

            if (!fullscreen) {
                graphics.PreferredBackBufferWidth = 1280;
                graphics.PreferredBackBufferHeight = 720;
                graphics.IsFullScreen = false;
            } else {
                graphics.PreferredBackBufferWidth = 1920;
                graphics.PreferredBackBufferHeight = 1080;
                graphics.IsFullScreen = true;
            }

            graphics.ApplyChanges();

            radiusH = graphics.PreferredBackBufferWidth / 2.4615f;
            radiusV = graphics.PreferredBackBufferWidth / 2.4615f;

            noteSpawnPosition = new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2 - graphics.PreferredBackBufferHeight / 1.44f);

            for (int i = 0; i < 9; i++) {
                float x = (float)(noteSpawnPosition.X + radiusH * Math.Cos((i / 8f) * Math.PI));
                float y = (float)(noteSpawnPosition.Y + radiusV * Math.Sin((i / 8f) * Math.PI));
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
        /// Loads game assets. Called once per game.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load textures
            tooltipL3L4 = Content.Load<Texture2D>("ui/Tooltip_L3_L4");
            mainTooltips[0] = Content.Load<Texture2D>("ui/Tooltip_L2");
            mainTooltips[1] = Content.Load<Texture2D>("ui/Tooltip_L1");
            mainTooltips[2] = Content.Load<Texture2D>("ui/Tooltip_C");
            mainTooltips[3] = Content.Load<Texture2D>("ui/Tooltip_R1");
            mainTooltips[4] = Content.Load<Texture2D>("ui/Tooltip_R2");
            tooltipR3R4 = Content.Load<Texture2D>("ui/Tooltip_R3_R4");

            mainHighlightedTooltips[0] = Content.Load<Texture2D>("ui/Tooltip_L2_Highlighted");
            mainHighlightedTooltips[1] = Content.Load<Texture2D>("ui/Tooltip_L1_Highlighted");
            mainHighlightedTooltips[2] = Content.Load<Texture2D>("ui/Tooltip_C_Highlighted");
            mainHighlightedTooltips[3] = Content.Load<Texture2D>("ui/Tooltip_R1_Highlighted");
            mainHighlightedTooltips[4] = Content.Load<Texture2D>("ui/Tooltip_R2_Highlighted");

            noteTexture = Content.Load<Texture2D>("notes/Note");
            noteMultiBlueTexture = Content.Load<Texture2D>("notes/Note_Multi_Blue");
            noteMultiOrangeTexture = Content.Load<Texture2D>("notes/Note_Multi_Orange");
            noteReleaseTexture = Content.Load<Texture2D>("notes/Note_Hold_Release");
            noteReleaseMultiBlueTexture = Content.Load<Texture2D>("notes/Note_Hold_Release_Multi_Blue");
            noteReleaseMultiOrangeTexture = Content.Load<Texture2D>("notes/Note_Hold_Release_Multi_Orange");
            noteStarTexture = Content.Load<Texture2D>("notes/Note_Star");
            noteStarMultiBlueTexture = Content.Load<Texture2D>("notes/Note_Star_Multi_Blue");
            noteStarMultiOrangeTexture = Content.Load<Texture2D>("notes/Note_Star_Multi_Orange");
            hitMarkerTexture = Content.Load<Texture2D>("notes/HitMarker");
            noteTrailTexture = Content.Load<Texture2D>("notes/Note_Trail");
            starL = Content.Load<Texture2D>("live_ui/stars/Star_L");
            starO = Content.Load<Texture2D>("live_ui/stars/Star_O");
            starV = Content.Load<Texture2D>("live_ui/stars/Star_V");
            starE = Content.Load<Texture2D>("live_ui/stars/Star_E");
            starI = Content.Load<Texture2D>("live_ui/stars/Star_I");
            starExclamation = Content.Load<Texture2D>("live_ui/stars/Star_!");
            starEmpty = Content.Load<Texture2D>("live_ui/stars/Star_Empty");
            scoreGaugeBase = Content.Load<Texture2D>("live_ui/score_gauge/Score_Gauge_Base");
            scoreGaugeD = Content.Load<Texture2D>("live_ui/score_gauge/Score_Gauge_BelowC");
            scoreGaugeC = Content.Load<Texture2D>("live_ui/score_gauge/Score_Gauge_CtoB");
            scoreGaugeB = Content.Load<Texture2D>("live_ui/score_gauge/Score_Gauge_BtoA");
            scoreGaugeA = Content.Load<Texture2D>("live_ui/score_gauge/Score_Gauge_AtoS");
            scoreGaugeS = Content.Load<Texture2D>("live_ui/score_gauge/Score_Gauge_AboveS");
            scoreGaugeFrame = Content.Load<Texture2D>("live_ui/score_gauge/Score_Gauge_Frame");
            starTextures[0] = starL;
            starTextures[1] = starO;
            starTextures[2] = starV;
            starTextures[3] = starE;
            starTextures[4] = starL;
            starTextures[5] = starI;
            starTextures[6] = starV;
            starTextures[7] = starE;
            starTextures[8] = starExclamation;


            // Initialize the VideoPlayer
            bgVideoPlayer = new VideoPlayer();

            // Load the note hit sounds
            hitSoundEffects[0] = Content.Load<SoundEffect>("sounds/hit_perfect");
            hitSoundEffects[1] = Content.Load<SoundEffect>("sounds/hit_great");
            hitSoundEffects[2] = Content.Load<SoundEffect>("sounds/hit_good");
            hitSoundEffects[3] = Content.Load<SoundEffect>("sounds/hit_bad");

            // Load fonts
            defaultFont = Content.Load<SpriteFont>("fonts/DefaultFont");


            // TODO use a for loop of some sort to dynamically load all beatmaps

            // Load Believe Again
            Video video = Content.Load<Video>("beatmap_assets/Believe Again/video");
            Texture2D cover = Content.Load<Texture2D>("beatmap_assets/Believe Again/cover");
            Note[] beatmap = LoadBeatmap(@"C:\Users\darre\source\repos\SIFAC\SIFAC\Content\beatmap_assets\Believe Again\beatmap.txt");

            songs.Add(new PlayableSong("Believe Again", cover, video, beatmap, 11f/30f, 10000, 40000, 70000, 80000, 100000, 120000)); // TODO adjust target scores as appropriate

            // Load Jump up HIGH!!
            video = Content.Load<Video>("beatmap_assets/Jump up HIGH!!/video");
            cover = Content.Load<Texture2D>("beatmap_assets/Jump up HIGH!!/cover");
            beatmap = LoadBeatmap(@"C:\Users\darre\source\repos\SIFAC\SIFAC\Content\beatmap_assets\Jump up HIGH!!\beatmap.txt");
            songs.Add(new PlayableSong("Jump up HIGH!!", cover, video, beatmap, -1f/12f, 10000, 40000, 70000, 80000, 100000, 120000)); // TODO adjust target scores as appropriate

            // Load the calibration beatmap
            beatmap = LoadBeatmap(@"C:\Users\darre\source\repos\SIFAC\SIFAC\Content\beatmap_assets\Calibration\beatmap.txt");
            songs.Add(new PlayableSong("Calibration", Content.Load<Texture2D>("beatmap_assets/Calibration/cover"), Content.Load<Song>("beatmap_assets/Calibration/song"), beatmap, 0, 0, 0, 0, 0, 0));

            // Load the hold calibration beatmap
            beatmap = LoadBeatmap(@"C:\Users\darre\source\repos\SIFAC\SIFAC\Content\beatmap_assets\Hold Note Calibration\beatmap.txt");
            songs.Add(new PlayableSong("Hold Note Calibration", Content.Load<Texture2D>("beatmap_assets/Hold Note Calibration/cover"), Content.Load<Song>("beatmap_assets/Hold Note Calibration/song"), beatmap, 0, 0, 0, 0, 0, 0));

            // Add placeholder song
            songs.Add(new PlayableSong("Placeholder", Content.Load<Texture2D>("beatmap_assets/Placeholder/cover"), Content.Load<Song>("beatmap_assets/Calibration/song"), new Note[0], 0, 0, 0, 0, 0, 0));
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() {
            
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            base.Update(gameTime);

            switch (currentGameState) {
                case GameState.TitleScreen:
                    UpdateTitleScreen(gameTime);
                    break;
                case GameState.NesicaCheckScreen:
                    UpdateNesicaCheckScreen(gameTime);
                    break;
                case GameState.GroupSelectScreen:
                    UpdateGroupSelectScreen(gameTime);
                    break;
                case GameState.SongSelectScreen:
                    UpdateSongSelectScreen(gameTime);
                    break;
                case GameState.LivePreparationScreen:
                    UpdateLivePreparationScreen(gameTime);
                    break;
                case GameState.LiveScreen:
                    UpdateLiveScreen(gameTime);
                    break;
                case GameState.ResultScreen:
                    UpdateResultScreen(gameTime);
                    break;
                case GameState.GoodbyeScreen:
                    UpdateGoodbyeScreen(gameTime);
                    break;
            }
        }

        void UpdateTitleScreen(GameTime gameTime) {
            // TODO
        }

        void UpdateNesicaCheckScreen(GameTime gameTime) {
            // TODO
        }

        void UpdateGroupSelectScreen(GameTime gameTime) {
            // TODO
        }

        void UpdateSongSelectScreen(GameTime gameTime) {
            // Get the 5 songs on the current page
            for (int i = 0; i < 5; i++) {
                menuChoices[i] = songs[(songSelectPage * 5) + i];
            }

            var kstate = Keyboard.GetState();
            // Detect key down
            if (kstate.IsKeyDown(Keys.A) & !previousState.IsKeyDown(Keys.A)) {
                // TODO
            }
            if (kstate.IsKeyDown(Keys.S) & !previousState.IsKeyDown(Keys.S)) {
                // TODO
            }
            if (kstate.IsKeyDown(Keys.D) & !previousState.IsKeyDown(Keys.D)) {
                highlightedMenuElement = 0; //TODO play highlighted song's preview
            }
            if (kstate.IsKeyDown(Keys.F) & !previousState.IsKeyDown(Keys.F)) {
                highlightedMenuElement = 1;
            }
            if (kstate.IsKeyDown(Keys.Space) & !previousState.IsKeyDown(Keys.Space)) {
                highlightedMenuElement = 2;
            }
            if (kstate.IsKeyDown(Keys.J) & !previousState.IsKeyDown(Keys.J)) {
                highlightedMenuElement = 3;
            }
            if (kstate.IsKeyDown(Keys.K) & !previousState.IsKeyDown(Keys.K)) {
                highlightedMenuElement = 4;
            }
            if (kstate.IsKeyDown(Keys.L) & !previousState.IsKeyDown(Keys.L)) {
                // TODO
            }
            if (kstate.IsKeyDown(Keys.OemSemicolon) & !previousState.IsKeyDown(Keys.OemSemicolon)) {
                // TODO
            }
            if (kstate.IsKeyDown(Keys.Escape) & !previousState.IsKeyDown(Keys.Escape)) {
                Console.WriteLine("Red Button Pressed");
                Exit();
            }
            if (kstate.IsKeyDown(Keys.Enter) & !previousState.IsKeyDown(Keys.Enter)) {
                currentSong = menuChoices[highlightedMenuElement];
                if (currentSong.type == PlayableSongType.Music && MediaPlayer.State == MediaState.Stopped) {
                    MediaPlayer.Play(currentSong.music);
                } else if (bgVideoPlayer.State == MediaState.Stopped) {
                    bgVideoPlayer.Volume = 0.2f;
                    bgVideoPlayer.Play(currentSong.backgroundMv);
                }
                currentGameState = GameState.LiveScreen; // TODO add live preparation screen
            }
            previousState = kstate;
        }

        void UpdateLivePreparationScreen(GameTime gameTime) {
            // TODO
        }

        void UpdateLiveScreen(GameTime gameTime) {
            var kstate = Keyboard.GetState();
            // Detect key down
            if (kstate.IsKeyDown(Keys.A) & !previousState.IsKeyDown(Keys.A)) {
                JudgeHit(0, true);
            }
            if (kstate.IsKeyDown(Keys.S) & !previousState.IsKeyDown(Keys.S)) {
                JudgeHit(1, true);
            }
            if (kstate.IsKeyDown(Keys.D) & !previousState.IsKeyDown(Keys.D)) {
                JudgeHit(2, true);
            }
            if (kstate.IsKeyDown(Keys.F) & !previousState.IsKeyDown(Keys.F)) {
                JudgeHit(3, true);
            }
            if (kstate.IsKeyDown(Keys.Space) & !previousState.IsKeyDown(Keys.Space)) {
                JudgeHit(4, true);
            }
            if (kstate.IsKeyDown(Keys.J) & !previousState.IsKeyDown(Keys.J)) {
                JudgeHit(5, true);
            }
            if (kstate.IsKeyDown(Keys.K) & !previousState.IsKeyDown(Keys.K)) {
                JudgeHit(6, true);
            }
            if (kstate.IsKeyDown(Keys.L) & !previousState.IsKeyDown(Keys.L)) {
                JudgeHit(7, true);
            }
            if (kstate.IsKeyDown(Keys.OemSemicolon) & !previousState.IsKeyDown(Keys.OemSemicolon)) {
                JudgeHit(8, true);
            }
            if (kstate.IsKeyDown(Keys.Escape) & !previousState.IsKeyDown(Keys.Escape)) {
                // Console.WriteLine("Red Button Pressed");
                Exit();
            }
            if (kstate.IsKeyDown(Keys.Enter) & !previousState.IsKeyDown(Keys.Enter)) {
                // Console.WriteLine("Blue Button Pressed");
            }

            // Detect key up
            if (!kstate.IsKeyDown(Keys.A) & previousState.IsKeyDown(Keys.A)) {
                JudgeHit(0, false);
            }
            if (!kstate.IsKeyDown(Keys.S) & previousState.IsKeyDown(Keys.S)) {
                JudgeHit(1, false);
            }
            if (!kstate.IsKeyDown(Keys.D) & previousState.IsKeyDown(Keys.D)) {
                JudgeHit(2, false);
            }
            if (!kstate.IsKeyDown(Keys.F) & previousState.IsKeyDown(Keys.F)) {
                JudgeHit(3, false);
            }
            if (!kstate.IsKeyDown(Keys.Space) & previousState.IsKeyDown(Keys.Space)) {
                JudgeHit(4, false);
            }
            if (!kstate.IsKeyDown(Keys.J) & previousState.IsKeyDown(Keys.J)) {
                JudgeHit(5, false);
            }
            if (!kstate.IsKeyDown(Keys.K) & previousState.IsKeyDown(Keys.K)) {
                JudgeHit(6, false);
            }
            if (!kstate.IsKeyDown(Keys.L) & previousState.IsKeyDown(Keys.L)) {
                JudgeHit(7, false);
            }
            if (!kstate.IsKeyDown(Keys.OemSemicolon) & previousState.IsKeyDown(Keys.OemSemicolon)) {
                JudgeHit(8, false);
            }
            if (!kstate.IsKeyDown(Keys.Escape) & previousState.IsKeyDown(Keys.Escape)) {
                // Console.WriteLine("Red Button Released");
                Exit();
            }
            if (!kstate.IsKeyDown(Keys.Enter) & previousState.IsKeyDown(Keys.Enter)) {
                // Console.WriteLine("Blue Button Released");
            }
            previousState = kstate;

            // TODO refactor this code to have one foreach for both scenarios
            if (currentSong.type == PlayableSongType.Video) { // Beatmap audio source is from video
                foreach (Note note in currentSong.beatmap) {
                    //AUTOPLAY CODE
                    if (autoplay && !note.hasResolved && note.position <= bgVideoPlayer.PlayPosition.TotalSeconds + timeOffset) {
                        // Console.WriteLine("Perfect (Auto)");
                        note.result = NoteAccuracy.Perfect;
                        hitSoundEffects[0].Play(noteHitVolume, 0f, 0f);
                        note.hasResolved = true;
                        perfects++;
                        score += 300; // TODO adjust score as appropriate
                        if (++combo > maxCombo) {
                            maxCombo = combo;
                        }
                        if (note.hasStar) {
                            stars++;
                            // TODO play star note sound
                        }
                    }
                    //END AUTOPLAY CODE

                    // Code to handle currently visible hold trails
                    double currentAudioPosition;
                    if (currentSong.type == PlayableSongType.Video) {
                        currentAudioPosition = bgVideoPlayer.PlayPosition.TotalSeconds;
                    } else {
                        currentAudioPosition = MediaPlayer.PlayPosition.TotalSeconds;
                    }

                    if (note.isHold && !note.hasResolved && note.hasSpawned) {
                        float[] coords = CalculateNoteCoordinates(currentAudioPosition, note);
                        float removeTime = note.releaseNoteSpawnTime + ((float)currentAudioPosition - note.position);
                        NoteTrail trail = new NoteTrail(new Vector2(coords[0], coords[1]), 0.35f - (float)((note.position - currentAudioPosition) * 0.30f), (float)currentAudioPosition, removeTime, GetReleaseNote(note));
                        noteTrailPositions.Add(trail);
                    }

                    foreach (NoteTrail trail in noteTrailPositions.ToList()) {
                        if (trail.removeTime < currentAudioPosition || trail.releaseNote.hasResolved) {
                            noteTrailPositions.Remove(trail);
                        }
                    }

                    // Handle missed notes
                    if (!note.hasResolved && note.position <= bgVideoPlayer.PlayPosition.TotalSeconds + timeOffset - missTolerance) {
                        // Console.WriteLine("Miss");
                        note.result = NoteAccuracy.Miss;
                        note.hasResolved = true;
                        if (note.isHold) {
                            GetReleaseNote(note).hasResolved = true;
                            GetReleaseNote(note).result = NoteAccuracy.Miss;
                            misses++;
                        }
                        combo = 0;
                        misses++;
                    }                   
                }

                // Update score rank
                if (score >= currentSong.sssScore) {
                    rank = ScoreRank.SSS;
                } else if (score >= currentSong.ssScore) {
                    rank = ScoreRank.SS;
                } else if (score >= currentSong.sScore) {
                    rank = ScoreRank.S;
                } else if (score >= currentSong.aScore) {
                    rank = ScoreRank.A;
                } else if (score >= currentSong.bScore) {
                    rank = ScoreRank.B;
                } else if (score >= currentSong.cScore) {
                    rank = ScoreRank.C;
                } else {
                    rank = ScoreRank.D;
                }

                // Detect if song is over
                if (bgVideoPlayer.State == MediaState.Stopped) {
                    currentGameState = GameState.ResultScreen;
                }
            } else { // Beatmap audio source is from audio file
                foreach (Note note in currentSong.beatmap) {
                    //AUTOPLAY CODE
                    if (autoplay && !note.hasResolved && note.position <= MediaPlayer.PlayPosition.TotalSeconds + timeOffset) {
                        // Console.WriteLine("Auto");
                        note.result = NoteAccuracy.Perfect;
                        hitSoundEffects[0].Play(0.2f, 0f, 0f);
                        note.hasResolved = true;
                        perfects++;
                    }
                    //END AUTOPLAY CODE

                    // Code to handle currently visible hold trails
                    double currentAudioPosition;
                    if (currentSong.type == PlayableSongType.Video) {
                        currentAudioPosition = bgVideoPlayer.PlayPosition.TotalSeconds;
                    } else {
                        currentAudioPosition = MediaPlayer.PlayPosition.TotalSeconds;
                    }

                    if (note.isHold && !note.hasResolved && note.hasSpawned) {
                        float[] coords = CalculateNoteCoordinates(currentAudioPosition, note);
                        float removeTime = note.releaseNoteSpawnTime + ((float) currentAudioPosition - note.position);
                        if (removeTime < note.releaseNoteSpawnTime) {
                            NoteTrail trail = new NoteTrail(new Vector2(coords[0], coords[1]), 0.35f - (float)((note.position - currentAudioPosition) * 0.30f), (float) currentAudioPosition, removeTime, GetReleaseNote(note));
                            noteTrailPositions.Add(trail);
                        }   
                    }

                    foreach (NoteTrail trail in noteTrailPositions.ToList()) {
                        if (trail.removeTime < currentAudioPosition || trail.releaseNote.hasResolved) {
                            noteTrailPositions.Remove(trail);
                        }
                    }

                    // Handle misses
                    if (!note.hasResolved && note.position <= bgVideoPlayer.PlayPosition.TotalSeconds + timeOffset - missTolerance) {
                        Console.WriteLine("Miss");
                        note.result = NoteAccuracy.Miss;
                        note.hasResolved = true;
                        if (note.isHold) {
                            GetReleaseNote(note).hasResolved = true;
                            GetReleaseNote(note).result = NoteAccuracy.Miss;
                            misses++;
                        }
                        combo = 0;
                        misses++;
                    }
                }

                // Update score rank
                if(score >= currentSong.sssScore) {
                    rank = ScoreRank.SSS;
                } else if (score >= currentSong.ssScore) {
                    rank = ScoreRank.SS;
                } else if (score >= currentSong.sScore) {
                    rank = ScoreRank.S;
                } else if (score >= currentSong.aScore) {
                    rank = ScoreRank.A;
                } else if (score >= currentSong.bScore) {
                    rank = ScoreRank.B;
                } else if (score >= currentSong.cScore) {
                    rank = ScoreRank.C;
                } else {
                    rank = ScoreRank.D;
                }

                // Detect if song is over
                if (MediaPlayer.State == MediaState.Stopped) {
                    currentGameState = GameState.ResultScreen;
                }
            }
        }

        void UpdateResultScreen(GameTime gameTime) {
            var kstate = Keyboard.GetState();
            // Detect key down
            if (kstate.IsKeyDown(Keys.A) & !previousState.IsKeyDown(Keys.A)) {
                // TODO
            }
            if (kstate.IsKeyDown(Keys.S) & !previousState.IsKeyDown(Keys.S)) {
                // TODO
            }
            if (kstate.IsKeyDown(Keys.D) & !previousState.IsKeyDown(Keys.D)) {
                // TODO
            }
            if (kstate.IsKeyDown(Keys.F) & !previousState.IsKeyDown(Keys.F)) {
                // TODO
            }
            if (kstate.IsKeyDown(Keys.Space) & !previousState.IsKeyDown(Keys.Space)) {
                // TODO
            }
            if (kstate.IsKeyDown(Keys.J) & !previousState.IsKeyDown(Keys.J)) {
                // TODO
            }
            if (kstate.IsKeyDown(Keys.K) & !previousState.IsKeyDown(Keys.K)) {
                // TODO
            }
            if (kstate.IsKeyDown(Keys.L) & !previousState.IsKeyDown(Keys.L)) {
                // TODO
            }
            if (kstate.IsKeyDown(Keys.OemSemicolon) & !previousState.IsKeyDown(Keys.OemSemicolon)) {
                // TODO
            }
            if (kstate.IsKeyDown(Keys.Escape) & !previousState.IsKeyDown(Keys.Escape)) {
                Console.WriteLine("Red Button Pressed");
                Exit();
            }
            if (kstate.IsKeyDown(Keys.Enter) & !previousState.IsKeyDown(Keys.Enter)) {
                // Reset the results
                combo = 0;
                maxCombo = 0;
                perfects = 0;
                greats = 0;
                goods = 0;
                bads = 0;
                misses = 0;
                stars = 0;
                score = 0;
                rank = ScoreRank.D;

                // Reset the beatmap
                foreach (Note note in currentSong.beatmap) {
                    note.hasSpawned = false;
                    note.hasResolved = false;
                }

                currentGameState = GameState.SongSelectScreen;
            }
            previousState = kstate;
        }

        void UpdateGoodbyeScreen(GameTime gameTime) {
            // TODO
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            base.Draw(gameTime);
            switch (currentGameState) {
                case GameState.TitleScreen:
                    DrawTitleScreen(gameTime);
                    break;
                case GameState.NesicaCheckScreen:
                    DrawNesicaCheckScreen(gameTime);
                    break;
                case GameState.GroupSelectScreen:
                    DrawGroupSelectScreen(gameTime);
                    break;
                case GameState.SongSelectScreen:
                    DrawSongSelectScreen(gameTime);
                    break;
                case GameState.LivePreparationScreen:
                    DrawLivePreparationScreen(gameTime);
                    break;
                case GameState.LiveScreen:
                    DrawLiveScreen(gameTime);
                    break;
                case GameState.ResultScreen:
                    DrawResultScreen(gameTime);
                    break;
                case GameState.GoodbyeScreen:
                    DrawGoodbyeScreen(gameTime);
                    break;
            }
        }

        /// <summary>
        /// Draw method for the title screen. Called when game is on the title screen and should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        void DrawTitleScreen(GameTime gameTime) {
            // TODO
            GraphicsDevice.Clear(Color.White);
        }

        /// <summary>
        /// Draw method for the NESiCA check select screen. Called when game is on the NESiCA check screen and should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        void DrawNesicaCheckScreen(GameTime gameTime) {
            // TODO
            GraphicsDevice.Clear(Color.White);
        }

        /// <summary>
        /// Draw method for the group select screen. Called when game is on the group select screen and should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        void DrawGroupSelectScreen(GameTime gameTime) {
            // TODO
            GraphicsDevice.Clear(Color.White);
        }

        /// <summary>
        /// Draw method for the song select screen. Called when game is on the song select screen and should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        void DrawSongSelectScreen(GameTime gameTime) {
            // TODO make this look less sketchy, finish implementing screen
            GraphicsDevice.Clear(new Color(19, 232, 174));

            // Offsets to account for the slight curvature of the tooltips
            float[] xOffsets = { 
                graphics.PreferredBackBufferWidth / 27f,
                graphics.PreferredBackBufferWidth / 33f,
                graphics.PreferredBackBufferWidth / 33f,
                graphics.PreferredBackBufferWidth / 36f,
                graphics.PreferredBackBufferWidth / 44f
            };

            float[] yOffsets = {
                graphics.PreferredBackBufferHeight / 20f,
                graphics.PreferredBackBufferHeight / 25f,
                graphics.PreferredBackBufferHeight / 25f,
                graphics.PreferredBackBufferHeight / 25f,
                graphics.PreferredBackBufferHeight / 20f
            };

            spriteBatch.Begin();

            // Draw the tooltips for L2, L1, C, R1, and R2
            for (int i = 0; i < 5; i++) {
                float x = (graphics.PreferredBackBufferWidth / 2 - (graphics.PreferredBackBufferWidth / 3.2f)) + ((graphics.PreferredBackBufferWidth / 6.4f) * i);
                float y = graphics.PreferredBackBufferHeight / 5 * 4;
                if (highlightedMenuElement == i) {
                    // Draw base tooltip shape
                    spriteBatch.Draw(mainHighlightedTooltips[i],
                        new Vector2(x, y),
                        null,
                        Color.White,
                        0f,
                        new Vector2(mainHighlightedTooltips[i].Width / 2, mainHighlightedTooltips[i].Height / 2),
                        graphics.PreferredBackBufferWidth / 3840f, // Scale of 0.5 at intended 1080p
                        SpriteEffects.None,
                        0f);

                    // Draw album art
                    spriteBatch.Draw(menuChoices[i].coverArt,
                        new Vector2(x + xOffsets[i], y + yOffsets[i]),
                        null,
                        Color.White,
                        0f,
                        new Vector2(mainHighlightedTooltips[i].Width / 2, mainHighlightedTooltips[i].Height / 2),
                        graphics.PreferredBackBufferWidth / 3490.91f, // Scale of 0.55 at intended 1080p
                        SpriteEffects.None,
                        0f);

                    // Draw text
                    spriteBatch.DrawString(defaultFont,
                        menuChoices[i].title,
                        new Vector2(x - (mainHighlightedTooltips[i].Width * graphics.PreferredBackBufferWidth / 3840f / 2), y + (mainHighlightedTooltips[i].Height * graphics.PreferredBackBufferHeight / 3840f / 2)),
                        Color.Black,
                        0f,
                        new Vector2(0, 0),
                        0.35f,
                        SpriteEffects.None,
                        0f);
                } else {
                    // Draw base tooltip shape
                    spriteBatch.Draw(mainTooltips[i],
                        new Vector2(x, y),
                        null,
                        Color.White,
                        0f,
                        new Vector2(mainTooltips[i].Width / 2, mainTooltips[i].Height / 2),
                        graphics.PreferredBackBufferWidth / 3840f, // Scale of 0.5 at intended 1080p
                        SpriteEffects.None,
                        0f);

                    // Draw album art
                    spriteBatch.Draw(menuChoices[i].coverArt,
                        new Vector2(x + xOffsets[i], y + yOffsets[i]),
                        null,
                        Color.White,
                        0f,
                        new Vector2(mainHighlightedTooltips[i].Width / 2, mainHighlightedTooltips[i].Height / 2),
                        graphics.PreferredBackBufferWidth / 3490.91f, // Scale of 0.55 at intended 1080p
                        SpriteEffects.None,
                        0f);

                    // Draw text
                    spriteBatch.DrawString(defaultFont,
                        menuChoices[i].title,
                        new Vector2(x - (mainTooltips[i].Width * graphics.PreferredBackBufferWidth / 3840f / 2), y + (mainTooltips[i].Height * graphics.PreferredBackBufferHeight / 3840f / 2)),
                        Color.Black,
                        0f,
                        new Vector2(0, 0),
                        0.35f,
                        SpriteEffects.None,
                        0f);
                }   
            }
            spriteBatch.End();
        }

        /// <summary>
        /// Draw method for the live preparation screen. Called when game is on the live preparation screen and should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        void DrawLivePreparationScreen(GameTime gameTime) {
            // TODO
            GraphicsDevice.Clear(Color.White);
        }

        /// <summary>
        /// Draw method for the live screen. Called when the user is currently playing a live and the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        void DrawLiveScreen(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            // Draw the video frame
            if (currentSong.type == PlayableSongType.Video) {
                try {
                    spriteBatch.Draw(
                    bgVideoPlayer.GetTexture(),
                    new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                    Color.White
                    );
                } catch (InvalidOperationException) {
                    Console.WriteLine("Platform returned a null texture");
                }
            }
            
            // Draw the hit positions
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

            // Get current audio position (used for drawing notes and note trails)
            double currentAudioPosition;
            if (currentSong.type == PlayableSongType.Video) {
                currentAudioPosition = bgVideoPlayer.PlayPosition.TotalSeconds;
            } else {
                currentAudioPosition = MediaPlayer.PlayPosition.TotalSeconds;
            }

            // Draw the trails
            foreach (NoteTrail trail in noteTrailPositions) {
                spriteBatch.Draw(noteTrailTexture,
                    trail.position,
                    null,
                    Color.White,
                    0f,
                    new Vector2(noteTrailTexture.Width / 2, noteTrailTexture.Height / 2 - graphics.PreferredBackBufferHeight),
                    trail.scale,
                    SpriteEffects.None,
                    0f);
            }

            // Draw the notes
            for (int i = 0; i < currentSong.beatmap.Length; i++) {
                Note note = currentSong.beatmap[i];
                Note nextNote;
                if (i + 1 >= currentSong.beatmap.Length) {
                    nextNote = new Note(0f, 0, false, false, false, 0f, 0f, false);
                } else {
                    nextNote = currentSong.beatmap[i + 1];
                }
                if (note.hasResolved && !note.isHold) {
                    continue;
                }
                if (note.position <= currentAudioPosition + noteSpeed && !note.hasResolved) {
                    float[] coordinates = CalculateNoteCoordinates(currentAudioPosition, note);
                    float noteSize = 0.35f - (float)((note.position - currentAudioPosition) * 0.30f);

                    // TODO release note multis aren't rendering properly
                    // TODO this if statement hell is disgusting, refactor this
                    if (note.texture == null) {
                        if (note.hasStar && !note.isMultiple) {
                            note.texture = noteStarTexture;
                        } else if (note.hasStar && note.isMultiple) {
                            if (lastMultiWasBlue) {
                                note.texture = noteStarMultiOrangeTexture;
                                if (nextNote.position != note.position) {
                                    lastMultiWasBlue = false;
                                }
                            } else {
                                note.texture = noteStarMultiBlueTexture;
                                if (nextNote.position != note.position) {
                                    lastMultiWasBlue = true;
                                }
                            }
                        } else if (note.isRelease && note.isMultiple) {
                            if (lastMultiWasBlue) {
                                note.texture = noteReleaseMultiOrangeTexture;
                                if (nextNote.position != note.position) {
                                    lastMultiWasBlue = false;
                                }
                            } else {
                                note.texture = noteReleaseMultiBlueTexture;
                                if (nextNote.position != note.position) {
                                    lastMultiWasBlue = true;
                                }
                            }
                        } else if (note.isMultiple) {
                            if (lastMultiWasBlue) {
                                note.texture = noteMultiOrangeTexture;
                                if (nextNote.position != note.position) {
                                    lastMultiWasBlue = false;
                                }
                            } else {
                                note.texture = noteMultiBlueTexture;
                                if (nextNote.position != note.position) {
                                    lastMultiWasBlue = true;
                                }
                            }
                        } else if (note.isRelease) {
                            note.texture = noteReleaseTexture;
                        } else {
                            note.texture = noteTexture;
                        }
                    }

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
                } else if (note.isHold && note.hasResolved && !GetReleaseNote(note).hasResolved){
                    spriteBatch.Draw(note.texture,
                        hitMarkerPositions[note.lane],
                        null,
                        Color.White,
                        0f,
                        new Vector2(note.texture.Width / 2, note.texture.Height / 2 - graphics.PreferredBackBufferHeight),
                        0.35f,
                        SpriteEffects.None,
                        0f);
                }
            }

            // Draw the star gauge

            // Draw the star counter's empty stars
            for (int i = 0; i < 9; i++) {
                float x = graphics.PreferredBackBufferHeight + 75 * i;
                spriteBatch.Draw(starEmpty,
                    new Vector2(x, 50),
                    null,
                    Color.White,
                    0f,
                    new Vector2(starEmpty.Width / 2, starEmpty.Height / 2),
                    0.15f,
                    SpriteEffects.None,
                    0f);
            }
            
            // Draw any accumulated stars
            for (int i = 0; i < stars; i++) {
                float x = graphics.PreferredBackBufferHeight + 75 * i;
                spriteBatch.Draw(starTextures[i],
                    new Vector2(x, 50),
                    null,
                    Color.White,
                    0f,
                    new Vector2(starEmpty.Width / 2, starEmpty.Height / 2),
                    0.15f,
                    SpriteEffects.None,
                    0f);
            }

            // Draw the score gauge
            // Draw the base
            spriteBatch.Draw(scoreGaugeBase,
                    Vector2.Zero,
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    0.30f,
                    SpriteEffects.None,
                    0f);

            // Draw the gauge
            Texture2D gaugeTexture;
            int baseWidth; // The "0" x coordinate for each meter should be the top of the previous rank
            if (rank == ScoreRank.D) {
                gaugeTexture = scoreGaugeD;
                baseWidth = 0;
            } else if (rank == ScoreRank.C) {
                gaugeTexture = scoreGaugeC;
                baseWidth = scoreGaugeD.Width;
            } else if (rank == ScoreRank.B) {
                gaugeTexture = scoreGaugeB;
                baseWidth = scoreGaugeC.Width;
            } else if (rank == ScoreRank.A) {
                gaugeTexture = scoreGaugeA;
                baseWidth = scoreGaugeB.Width;
            } else {
                gaugeTexture = scoreGaugeS;
                baseWidth = scoreGaugeA.Width;
            }

            float scorePercent; // % of the way from current rank to next one

            if (rank == ScoreRank.D) {
                scorePercent = (float)score / currentSong.cScore;
            } else if (rank == ScoreRank.C) { 
                scorePercent = (float)(score - currentSong.cScore) / (currentSong.bScore - currentSong.cScore);
            } else if (rank == ScoreRank.B) {
                scorePercent = (float)(score - currentSong.bScore) / (currentSong.aScore - currentSong.bScore);
            } else if (rank == ScoreRank.A) {
                scorePercent = (float)(score - currentSong.aScore) / (currentSong.sScore - currentSong.aScore);
            } else {
                scorePercent = (float)(score - currentSong.sScore) / (currentSong.ssScore - currentSong.sScore);
            }

            if (scorePercent > 1) {
                scorePercent = 1;
            }

            spriteBatch.Draw(gaugeTexture,
                    Vector2.Zero,
                    new Rectangle(0, 0, baseWidth + (int) (scorePercent * (gaugeTexture.Width - baseWidth)), gaugeTexture.Height),
                    Color.White,
                    0f,
                    Vector2.Zero,
                    0.30f,
                    SpriteEffects.None,
                    0f);

            // Draw the frame
            spriteBatch.Draw(scoreGaugeFrame,
                    Vector2.Zero,
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    0.30f,
                    SpriteEffects.None,
                    0f);

            spriteBatch.End();
        }

        /// <summary>
        /// Draw method for the live result screen. Called when game is on the live result screen and should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        void DrawResultScreen(GameTime gameTime) {
            // TODO make this actually look legit
            GraphicsDevice.Clear(Color.White);
            spriteBatch.Begin();
            spriteBatch.DrawString(defaultFont, 
                                   "Perfect: " + perfects,
                                   new Vector2(200, 100),
                                   Color.Black
                                   );
            spriteBatch.DrawString(defaultFont,
                                   "Great: " + greats,
                                   new Vector2(200, 200),
                                   Color.Black
                                   );
            spriteBatch.DrawString(defaultFont,
                                   "Good: " + goods,
                                   new Vector2(200, 300),
                                   Color.Black
                                   );
            spriteBatch.DrawString(defaultFont,
                                   "Bad: " + bads,
                                   new Vector2(200, 400),
                                   Color.Black
                                   );
            spriteBatch.DrawString(defaultFont,
                                   "Miss: " + misses,
                                   new Vector2(200, 500),
                                   Color.Black
                                   );
            spriteBatch.DrawString(defaultFont,
                                   "Max Combo: " + maxCombo,
                                   new Vector2(200, 600),
                                   Color.Black
                                   );
            spriteBatch.End();
        }

        /// <summary>
        /// Draw method for the goodbye screen. Called when game is on the goodbye screen and should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        void DrawGoodbyeScreen(GameTime gameTime) {
            // TODO
            GraphicsDevice.Clear(Color.White);
        }

        /// <summary>
        /// Loads beatmap data from a txt file in CustomBeatmapFestival format into an array of Notes.
        /// </summary>
        /// <param name="filepath">The path to the txt file.</param>
        /// <returns>An array of Notes in the beatmap.</returns>
        private Note[] LoadBeatmap(String filepath) {
            string[] lines = System.IO.File.ReadAllLines(filepath);
            Note[] beatmap = new Note[lines.Length];
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
                    default:
                        throw new BeatmapParseException("Invalid note lane " + data[1]);
                }
                beatmap[i] = new Note(float.Parse(data[0]), lane, bool.Parse(data[2]), bool.Parse(data[3]), bool.Parse(data[4]), float.Parse(data[5]), float.Parse(data[6]), bool.Parse(data[7]));
            }
            Array.Sort(beatmap, delegate (Note x, Note y) { return x.position.CompareTo(y.position); });
            return beatmap;
        }

        /// <summary>
        /// This helper method calculates where a note should be drawn on the screen.
        /// </summary>
        /// <param name="songPosition">The current audio time, in seconds.</param>
        /// <param name="lane">The lane in which the note should be drawn.</param>
        /// <param name="note">The Note to be drawn.</param>
        /// <returns>Returns a float array of length 2, where the 0th element is the X coordinate of the note and the 1st element is the Y coordinate.</returns>
        private float[] CalculateNoteCoordinates(double songPosition, Note note) {
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

            deltaY += graphics.PreferredBackBufferHeight / 3.6f;

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
        private Note GetReleaseNote(Note holdNote) {
            if (holdNote.isHold) {
                foreach (Note note in currentSong.beatmap) {
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
        /// <param name="down">Whether the button is being pushed or released. True if pushed, otherwise false if released (used for hold note releases).</param>
        /// <returns></returns>
        private NoteAccuracy JudgeHit(int lane, Boolean down) {
            double currentAudioPosition;

            if (currentSong.type == PlayableSongType.Video) {
                currentAudioPosition = bgVideoPlayer.PlayPosition.TotalSeconds;
            } else {
                currentAudioPosition = MediaPlayer.PlayPosition.TotalSeconds;
            }

            foreach (Note note in currentSong.beatmap) {
                if (!note.hasResolved && ((down && !note.isRelease) || (!down && note.isRelease))) {
                    if (note.lane == lane && note.position - currentAudioPosition <= badTolerance) {
                        double diff = note.position - currentAudioPosition + timeOffset;
                        if (Math.Abs(diff) <= perfectTolerance) {
                            // Console.WriteLine("Perfect (early by " + diff + ")");
                            hitSoundEffects[0].Play(noteHitVolume, 0f, 0f);
                            note.result = NoteAccuracy.Perfect;
                            note.hasResolved = true;
                            perfects++;
                            score += 300; // TODO adjust score as appropriate and factor in "star mode"
                            if (++combo > maxCombo) {
                                maxCombo = combo;
                            }
                            if (note.hasStar) {
                                stars++;
                                // TODO play star sound
                            }
                            return NoteAccuracy.Perfect;
                        } else if (Math.Abs(diff) <= greatTolerance) {
                            // Console.WriteLine("Great (early by " + diff + ")");
                            hitSoundEffects[1].Play(noteHitVolume, 0f, 0f);
                            note.result = NoteAccuracy.Great;
                            note.hasResolved = true;
                            greats++;
                            score += 150; // TODO adjust score as appropriate and factor in "star mode"
                            if (++combo > maxCombo) {
                                maxCombo = combo;
                            }
                            if (note.hasStar) {
                                stars++;
                                // TODO play star sound
                            }
                            if (!down && note.isHold) { // TODO make sure this code doesn't intefere with short holds
                                Note releaseNote = GetReleaseNote(note);
                                releaseNote.hasResolved = true;
                                releaseNote.hasSpawned = true;
                                releaseNote.result = NoteAccuracy.Miss;
                                misses++;
                            }
                            return NoteAccuracy.Great;
                        } else if (Math.Abs(diff) <= goodTolerance) {
                            // Console.WriteLine("Good (early by " + diff + ")");
                            hitSoundEffects[2].Play(noteHitVolume, 0f, 0f);
                            note.result = NoteAccuracy.Good;
                            note.hasResolved = true;
                            goods++;
                            score += 100; // TODO adjust score as appropriate and factor in "star mode"
                            if (note.hasStar) {
                                stars++;
                                // TODO play star sound
                            }
                            if (!down && note.isHold) { // TODO make sure this code doesn't intefere with short holds
                                Note releaseNote = GetReleaseNote(note);
                                releaseNote.hasResolved = true;
                                releaseNote.hasSpawned = true;
                                releaseNote.result = NoteAccuracy.Miss;
                                misses++;
                            }
                            combo = 0;
                            return NoteAccuracy.Good;
                        } else if (Math.Abs(diff) <= badTolerance) {
                            // Console.WriteLine("Bad (early by " + diff + ")");
                            hitSoundEffects[3].Play(noteHitVolume, 0f, 0f);
                            note.result = NoteAccuracy.Bad;
                            note.hasResolved = true;
                            bads++;
                            score += 50; // TODO adjust score as appropriate and factor in "star mode"
                            if (note.hasStar) {
                                stars++;
                                // TODO play star sound
                            }
                            if (!down && note.isHold) { // TODO make sure this code doesn't intefere with short holds
                                Note releaseNote = GetReleaseNote(note);
                                releaseNote.hasResolved = true;
                                releaseNote.hasSpawned = true;
                                releaseNote.result = NoteAccuracy.Miss;
                                misses++;
                            }
                            combo = 0;
                            return NoteAccuracy.Bad;
                        }
                    }
                }
            }
            return NoteAccuracy.None;
        }
    }

    /// <summary>
    /// Represents a note in a beatmap.
    /// </summary>
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

    /// <summary>
    /// Represents a part of a trail behind a hold note.
    /// </summary>
    public class NoteTrail {
        public Vector2 position;
        public float scale;
        public float spawnTime;
        public float removeTime;
        public Note releaseNote;

        public NoteTrail(Vector2 position, float scale, float spawnTime, float removeTime, Note releaseNote) {
            this.position = position;
            this.scale = scale;
            this.spawnTime = spawnTime;
            this.removeTime = removeTime;
            this.releaseNote = releaseNote;
        }
    }

    /// <summary>
    /// Thrown when a beatmap data file is corrupt or has invalid data and cannot be parsed into a beatmap.
    /// </summary>
    public class BeatmapParseException : Exception {
        public BeatmapParseException() {

        }

        public BeatmapParseException(string message) {

        }
    }

    /// <summary>
    /// Represents a playable song with a beatmap, an audio track, and possibly a background video.
    /// </summary>
    public class PlayableSong {
        public string title;
        public Texture2D coverArt;
        public Song music = null;
        public Video backgroundMv = null;
        public Note[] beatmap;
        public PlayableSongType type;
        public float beatmapTimeOffset; // Time offset of all notes in beatmap, in seconds. Corresponds to the number of seconds of silence at the beginning of the video before the audio starts. Used for manually aligned audio/video tracks.
        public int cScore;
        public int bScore;
        public int aScore;
        public int sScore;
        public int ssScore;
        public int sssScore;

        /// <summary>
        /// Constructs a PlayableSong with a background video, using the video as the beatmap's song source.
        /// </summary>
        /// <param name="title">The title of the song.</param>
        /// <param name="cover">The cover art for the song.</param>
        /// <param name="v">The background video for the song. The song will use the audio track from the video file for the song.</param>
        /// <param name="map">The beatmap of the song.</param>
        /// <param name="cScore">The minimum score required for a C score rank.</param>
        /// <param name="bScore">The minimum score required for a B score rank.</param>
        /// <param name="aScore">The minimum score required for an A score rank.</param>
        /// <param name="sScore">The minimum score required for an S score rank.</param>
        /// <param name="ssScore">The minimum score required for an SS score rank.</param>
        /// <param name="sssScore">The minimum score required for an SSS score rank.</param>
        public PlayableSong(string title, Texture2D cover, Video v, Note[] map, int cScore, int bScore, int aScore, int sScore, int ssScore, int sssScore) {
            this.title = title;
            coverArt = cover;
            backgroundMv = v;
            beatmap = map;
            type = PlayableSongType.Video;
            this.cScore = cScore;
            this.bScore = bScore;
            this.aScore = aScore;
            this.sScore = sScore;
            this.ssScore = ssScore;
            this.sssScore = sssScore;
        }

        /// <summary>
        /// Constructs a PlayableSong with no background video.
        /// </summary>
        /// <param name="title">The title of the song.</param>
        /// <param name="cover">The cover art for the song.</param>
        /// <param name="s">The audio for the song.</param>
        /// <param name="map">The beatmap of the song.</param>
        /// <param name="cScore">The minimum score required for a C score rank.</param>
        /// <param name="bScore">The minimum score required for a B score rank.</param>
        /// <param name="aScore">The minimum score required for an A score rank.</param>
        /// <param name="sScore">The minimum score required for an S score rank.</param>
        /// <param name="ssScore">The minimum score required for an SS score rank.</param>
        /// <param name="sssScore">The minimum score required for an SSS score rank.</param>
        public PlayableSong(string title, Texture2D cover, Song s, Note[] map, int cScore, int bScore, int aScore, int sScore, int ssScore, int sssScore) {
            this.title = title;
            coverArt = cover;
            music = s;
            beatmap = map;
            type = PlayableSongType.Music;
            this.cScore = cScore;
            this.bScore = bScore;
            this.aScore = aScore;
            this.sScore = sScore;
            this.ssScore = ssScore;
            this.sssScore = sssScore;
        }
        /// <summary>
        /// Constructs a PlayableSong with a time offset and a background video, using the video as the beatmap's song source.
        /// </summary>
        /// <param name="title">The title of the song.</param>
        /// <param name="cover">The cover art for the song.</param>
        /// <param name="v">The background video for the song. The song will use the audio track from the video file for the song.</param>
        /// <param name="map">The beatmap of the song.</param>
        /// <param name="offset">The time offset, in seconds. Corresponds to the number of seconds of silence at the beginning of the video before the audio starts. Used for manually aligned audio/video tracks.</param>
        /// <param name="cScore">The minimum score required for a C score rank.</param>
        /// <param name="bScore">The minimum score required for a B score rank.</param>
        /// <param name="aScore">The minimum score required for an A score rank.</param>
        /// <param name="sScore">The minimum score required for an S score rank.</param>
        /// <param name="ssScore">The minimum score required for an SS score rank.</param>
        /// <param name="sssScore">The minimum score required for an SSS score rank.</param>
        public PlayableSong(string title, Texture2D cover, Video v, Note[] map, float offset, int cScore, int bScore, int aScore, int sScore, int ssScore, int sssScore) {
            this.title = title;
            coverArt = cover;
            backgroundMv = v;
            beatmap = map;
            type = PlayableSongType.Video;
            this.cScore = cScore;
            this.bScore = bScore;
            this.aScore = aScore;
            this.sScore = sScore;
            this.ssScore = ssScore;
            this.sssScore = sssScore;

            foreach (Note note in beatmap) {
                note.position += offset;
                if (note.isHold) {
                    note.releaseNoteSpawnTime += offset;
                } else if (note.isRelease) {
                    note.parentNoteSpawnTime += offset;
                }
            }
        }

        /// <summary>
        /// Constructs a PlayableSong with a time offset and no background video.
        /// </summary>
        /// <param name="title">The title of the song.</param>
        /// <param name="cover">The cover art for the song.</param>
        /// <param name="s">The audio for the song.</param>
        /// <param name="map">The beatmap of the song.</param>
        /// <param name="offset">The time offset, in seconds. Corresponds to the number of seconds of silence at the beginning of the video before the audio starts. Used for manually aligned audio/video tracks.</param>
        /// <param name="cScore">The minimum score required for a C score rank.</param>
        /// <param name="bScore">The minimum score required for a B score rank.</param>
        /// <param name="aScore">The minimum score required for an A score rank.</param>
        /// <param name="sScore">The minimum score required for an S score rank.</param>
        /// <param name="ssScore">The minimum score required for an SS score rank.</param>
        /// <param name="sssScore">The minimum score required for an SSS score rank.</param>
        public PlayableSong(string title, Texture2D cover, Song s, Note[] map, float offset, int cScore, int bScore, int aScore, int sScore, int ssScore, int sssScore) {
            this.title = title;
            coverArt = cover;
            music = s;
            beatmap = map;
            type = PlayableSongType.Music;
            this.cScore = cScore;
            this.bScore = bScore;
            this.aScore = aScore;
            this.sScore = sScore;
            this.ssScore = ssScore;
            this.sssScore = sssScore;

            foreach (Note note in beatmap) {
                note.position += offset;
                if (note.isHold) {
                    note.releaseNoteSpawnTime += offset;
                } else if (note.isRelease) {
                    note.parentNoteSpawnTime += offset;
                }
            }
        }
    }

    /// <summary>
    /// The various game states/screens possible during gameplay.
    /// </summary>
    public enum GameState {
        TitleScreen,
        NesicaCheckScreen,
        GroupSelectScreen,
        SongSelectScreen,
        LivePreparationScreen,
        LiveScreen,
        ResultScreen,
        GoodbyeScreen
    }

    /// <summary>
    /// The possible note hit judgement results.
    /// </summary>
    public enum NoteAccuracy {
        Perfect,
        Great,
        Good,
        Bad,
        Miss,
        None // Returns when you attempt to hit a note before it comes into range
    }

    /// <summary>
    /// The source types from which a PlayableSong acn pull its audio.
    /// </summary>
    public enum PlayableSongType {
        Video,
        Music
    }

    public enum ScoreRank {
        SSS,
        SS,
        S,
        A,
        B,
        C,
        D
    }
}