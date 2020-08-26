using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

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
        SpriteFont reglisseFillFont;
        SpriteFont multicoloreFont;
        SpriteFont delfinoFont;
        KeyboardState previousState;
        List<PlayableSong> songs = new List<PlayableSong>();
        PlayableSong currentSong;
        Difficulty currentDifficulty;

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
        Texture2D[] starTextures = new Texture2D[9]; // For convenience of spritebatch drawing
        Texture2D scoreGaugeBase;
        Texture2D scoreGaugeD;
        Texture2D scoreGaugeC;
        Texture2D scoreGaugeB;
        Texture2D scoreGaugeA;
        Texture2D scoreGaugeS;
        Texture2D scoreGaugeFrame;
        Texture2D progressGaugeBase;
        Texture2D progressGaugeRing;
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
        Texture2D resultTexture;
        Texture2D noteCountBaseTexture;
        Texture2D scoreDisplayBaseTexture;
        Texture2D songTitleBaseTexture;
        Texture2D[] accuracyLabelTextures = new Texture2D[5]; // // accuracyLabelTextures[0] is perfect, 1 is great, 2 is good, 3 is bad. 4 is miss
        Texture2D maxComboTexture;
        Texture2D fullComboResultTexture;
        Texture2D allPerfectResultTexture;
        Texture2D starYellowTexture;
        Texture2D starOrangeTexture;

        /*GOODBYE SCREEN VARIABLES*/

        /* CONFIG VARIABLES */
        // Timing tolerences, in seconds. Hitting a note at its time + or - each of these values gets the corresponding accuracy rating.
        readonly double perfectTolerance = 0.1;
        readonly double greatTolerance = 0.2;
        readonly double goodTolerance = 0.4;
        readonly double badTolerance = 0.8;
        // Anything not hit after the bad tolerance time is a miss.

        // Note speed, represented by seconds from spawn to note hit position.
        float noteSpeed = 1f; 

        // Timing offset setting, in seconds.
        // If too large, notes will be too early. If too small, notes will be too late.
        double timeOffset = 0;     

        // Autoplay, for debug purposes
        Boolean autoplay = true;

        // Fullscreen 1080p vs 720p flag for debugging. Game is intended to be played fullscreen at 1080p.
        Boolean fullscreen = false;

        // Whether to show framerate counter or not
        Boolean showFPS = true;

        // Vsync flag
        Boolean vsyncEnabled = true;

        // Volume of note hit sound effect
        float noteHitVolume = 0.1f;

        String beatmapDirectoryPath = @"C:\Users\darre\source\repos\SIFAC\SIFAC\Content\beatmap_assets\";
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

            // Enable vsync if setting is on
            graphics.SynchronizeWithVerticalRetrace = vsyncEnabled;

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
            progressGaugeBase = Content.Load<Texture2D>("live_ui/progress_gauge/Progress_Gauge_Base");
            progressGaugeRing = Content.Load<Texture2D>("live_ui/progress_gauge/Progress_Gauge_Ring");
            starTextures[0] = starL;
            starTextures[1] = starO;
            starTextures[2] = starV;
            starTextures[3] = starE;
            starTextures[4] = starL;
            starTextures[5] = starI;
            starTextures[6] = starV;
            starTextures[7] = starE;
            starTextures[8] = starExclamation;

            resultTexture = Content.Load<Texture2D>("results_ui/Result");
            noteCountBaseTexture = Content.Load<Texture2D>("results_ui/Note_Count_Base");
            scoreDisplayBaseTexture = Content.Load<Texture2D>("results_ui/Score_Display_Base");
            songTitleBaseTexture = Content.Load<Texture2D>("results_ui/Song_Title_Base");
            accuracyLabelTextures[0] = Content.Load<Texture2D>("results_ui/note_labels/Perfect");
            accuracyLabelTextures[1] = Content.Load<Texture2D>("results_ui/note_labels/Great");
            accuracyLabelTextures[2] = Content.Load<Texture2D>("results_ui/note_labels/Good");
            accuracyLabelTextures[3] = Content.Load<Texture2D>("results_ui/note_labels/Bad");
            accuracyLabelTextures[4] = Content.Load<Texture2D>("results_ui/note_labels/Miss");
            maxComboTexture = Content.Load<Texture2D>("results_ui/Max_Combo");
            fullComboResultTexture = Content.Load<Texture2D>("results_ui/Full_Combo");
            allPerfectResultTexture = Content.Load<Texture2D>("results_ui/All_Perfect");
            starYellowTexture = Content.Load<Texture2D>("results_ui/Star_Yellow");
            starOrangeTexture = Content.Load<Texture2D>("results_ui/Star_Orange");

            // Initialize the VideoPlayer
            bgVideoPlayer = new VideoPlayer();

            // Load the note hit sounds
            hitSoundEffects[0] = Content.Load<SoundEffect>("sounds/hit_perfect");
            hitSoundEffects[1] = Content.Load<SoundEffect>("sounds/hit_great");
            hitSoundEffects[2] = Content.Load<SoundEffect>("sounds/hit_good");
            hitSoundEffects[3] = Content.Load<SoundEffect>("sounds/hit_bad");

            // Load fonts
            defaultFont = Content.Load<SpriteFont>("fonts/DefaultFont");
            reglisseFillFont = Content.Load<SpriteFont>("fonts/ReglisseFill");
            multicoloreFont = Content.Load<SpriteFont>("fonts/Multicolore");
            delfinoFont = Content.Load<SpriteFont>("fonts/Delfino");


            // TODO use a for loop of some sort to dynamically load all beatmaps

            String[] beatmapDirectories = Directory.GetDirectories(beatmapDirectoryPath);

            /*foreach (String paths in beatmapDirectories) {
                String songName = paths.Split('\\').Last();
                Video video = Content.Load<Video>("beatmap_assets/" + songName + "/video");
                Texture2D cover = Content.Load<Texture2D>("beatmap_assets/" + songName + "/cover");
                PlayableSong song = new PlayableSong(songName, cover, video);

                Note[] beatmap = LoadBeatmap(paths + "\\beatmap_challenge.txt");   
                song.addDifficulty(Difficulty.Challenge, beatmap, 14, new int[] { 40000, 70000, 90000, 100000, 150000, 250000 }, 11f / 30f);
                songs.Add(song);

            }*/

            // Load Believe Again
            Video video = Content.Load<Video>("beatmap_assets/Believe Again/video");
            Texture2D cover = Content.Load<Texture2D>("beatmap_assets/Believe Again/cover");
            Note[] beatmap = LoadBeatmap(@"C:\Users\darre\source\repos\SIFAC\SIFAC\Content\beatmap_assets\Believe Again\beatmap.txt");

            PlayableSong song = new PlayableSong("Believe Again", cover, video);
            song.addDifficulty(Difficulty.Challenge, beatmap, 14, new int [] { 40000, 70000, 90000, 100000, 150000, 250000 }, 11f/30f); // TODO adjust target scores as appropriate

            songs.Add(song); 

            // Load Jump up HIGH!!
            video = Content.Load<Video>("beatmap_assets/Jump up HIGH!!/video");
            cover = Content.Load<Texture2D>("beatmap_assets/Jump up HIGH!!/cover");
            beatmap = LoadBeatmap(@"C:\Users\darre\source\repos\SIFAC\SIFAC\Content\beatmap_assets\Jump up HIGH!!\beatmap.txt");

            song = new PlayableSong("Jump up HIGH!!", cover, video);
            song.addDifficulty(Difficulty.Challenge, beatmap, 12, new int[] { 40000, 70000, 90000, 100000, 150000, 250000 }, -1f/12f); // TODO adjust target scores as appropriate

            songs.Add(song); // TODO adjust target scores as appropriate

            // Load the calibration beatmap
            Song audio = Content.Load<Song>("beatmap_assets/Calibration/song");
            cover = Content.Load<Texture2D>("beatmap_assets/Calibration/cover");
            beatmap = LoadBeatmap(@"C:\Users\darre\source\repos\SIFAC\SIFAC\Content\beatmap_assets\Calibration\beatmap.txt");

            song = new PlayableSong("Calibration", cover, audio);
            song.addDifficulty(Difficulty.Easy, beatmap, 1, new int[] { 1000, 2000, 3000, 4000, 5000, 6000 });
            songs.Add(song);

            // Load the hold calibration beatmap
            beatmap = LoadBeatmap(@"C:\Users\darre\source\repos\SIFAC\SIFAC\Content\beatmap_assets\Hold Note Calibration\beatmap.txt");

            song = new PlayableSong("Hold Note Calibration", cover, audio);
            song.addDifficulty(Difficulty.Easy, beatmap, 1, new int[] { 1000, 2000, 3000, 4000, 5000, 6000 });
            songs.Add(song);

            // Add placeholder song
            song = new PlayableSong("Placeholder", cover, audio);
            song.addDifficulty(Difficulty.Easy, new Note[0], 1, new int[] { 1000, 2000, 3000, 4000, 5000, 6000 });
            songs.Add(song);

            currentSong = songs[1]; // TODO remove this after debugging result screen
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() {
            Content.Unload();
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
            var kstate = Keyboard.GetState();
            if (kstate.IsKeyDown(Keys.Enter) & !previousState.IsKeyDown(Keys.Enter)) {
                currentGameState = GameState.NesicaCheckScreen;
            }
            previousState = kstate;

        }

        void UpdateNesicaCheckScreen(GameTime gameTime) {
            
        }

        void UpdateGroupSelectScreen(GameTime gameTime) {
            
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
                currentDifficulty = Difficulty.Challenge; // TODO Add an option to select this in the menu
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
                Exit();
            }
            if (kstate.IsKeyDown(Keys.Enter) & !previousState.IsKeyDown(Keys.Enter)) {
                
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
                Exit();
            }
            if (!kstate.IsKeyDown(Keys.Enter) & previousState.IsKeyDown(Keys.Enter)) {

            }
            previousState = kstate;

            double currentAudioPosition = 0;
            if (currentSong.type == PlayableSongType.Video) {
                currentAudioPosition = bgVideoPlayer.PlayPosition.TotalSeconds;
            } else {
                currentAudioPosition = MediaPlayer.PlayPosition.TotalSeconds;
            }

            PlayableSongDifficulty currentPlayableDifficulty = currentSong.getDifficulty(currentDifficulty);

            foreach (Note note in currentPlayableDifficulty.beatmap) {
                //AUTOPLAY CODE
                if (autoplay && !note.hasResolved && note.position <= currentAudioPosition + timeOffset) {
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
                if (!note.hasResolved && note.position <= currentAudioPosition + timeOffset - badTolerance) {
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
            if (score >= currentPlayableDifficulty.targetScores[5]) {
                rank = ScoreRank.SSS;
            } else if (score >= currentPlayableDifficulty.targetScores[4]) {
                rank = ScoreRank.SS;
            } else if (score >= currentPlayableDifficulty.targetScores[3]) {
                rank = ScoreRank.S;
            } else if (score >= currentPlayableDifficulty.targetScores[2]) {
                rank = ScoreRank.A;
            } else if (score >= currentPlayableDifficulty.targetScores[1]) {
                rank = ScoreRank.B;
            } else if (score >= currentPlayableDifficulty.targetScores[0]) {
                rank = ScoreRank.C;
            } else {
                rank = ScoreRank.D;
            }

            // Detect if song is over
            if ((currentSong.type == PlayableSongType.Video && bgVideoPlayer.State == MediaState.Stopped) || (currentSong.type == PlayableSongType.Music && MediaPlayer.State == MediaState.Stopped)) {
                currentGameState = GameState.ResultScreen;
            }
        }

        void UpdateResultScreen(GameTime gameTime) {
            var kstate = Keyboard.GetState();
            // Detect key down
            if (kstate.IsKeyDown(Keys.A) & !previousState.IsKeyDown(Keys.A)) {
                // Button isn't active here
            }
            if (kstate.IsKeyDown(Keys.S) & !previousState.IsKeyDown(Keys.S)) {
                // Button isn't active here
            }
            if (kstate.IsKeyDown(Keys.D) & !previousState.IsKeyDown(Keys.D)) {
                // Button isn't active here
            }
            if (kstate.IsKeyDown(Keys.F) & !previousState.IsKeyDown(Keys.F)) {
                // Button isn't active here
            }
            if (kstate.IsKeyDown(Keys.Space) & !previousState.IsKeyDown(Keys.Space)) {
                // Button isn't active here
            }
            if (kstate.IsKeyDown(Keys.J) & !previousState.IsKeyDown(Keys.J)) {
                // Button isn't active here
            }
            if (kstate.IsKeyDown(Keys.K) & !previousState.IsKeyDown(Keys.K)) {
                /// Button isn't active here
            }
            if (kstate.IsKeyDown(Keys.L) & !previousState.IsKeyDown(Keys.L)) {
                // Button isn't active here
            }
            if (kstate.IsKeyDown(Keys.OemSemicolon) & !previousState.IsKeyDown(Keys.OemSemicolon)) {
                // Button isn't active here
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
                foreach (Note note in currentSong.getDifficulty(currentDifficulty).beatmap) {
                    note.hasSpawned = false;
                    note.hasResolved = false;
                }

                // Go back to song select screen
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

            if (showFPS) {
                spriteBatch.Begin();
                drawFPSCounter(gameTime);
                spriteBatch.End();
            }
        }

        /// <summary>
        /// Draw method for the title screen. Called when game is on the title screen and should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        void DrawTitleScreen(GameTime gameTime) {
            GraphicsDevice.Clear(Color.White);
        }

        /// <summary>
        /// Draw method for the NESiCA check select screen. Called when game is on the NESiCA check screen and should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        void DrawNesicaCheckScreen(GameTime gameTime) {
            GraphicsDevice.Clear(Color.White);
        }

        /// <summary>
        /// Draw method for the group select screen. Called when game is on the group select screen and should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        void DrawGroupSelectScreen(GameTime gameTime) {
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
            GraphicsDevice.Clear(Color.White);
        }

        /// <summary>
        /// Draw method for the live screen. Called when the user is currently playing a live and the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        void DrawLiveScreen(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            PlayableSongDifficulty currentPlayableDifficulty = currentSong.getDifficulty(currentDifficulty);

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

            // Draw combo counter
            Vector2 stringMeasure = reglisseFillFont.MeasureString(combo + " COMBO");
            spriteBatch.DrawString(reglisseFillFont,
                combo + " COMBO",
                new Vector2(graphics.PreferredBackBufferWidth / 2, 600),
                new Color(222, 117, 199),
                0f,
                new Vector2(stringMeasure.X / 2, stringMeasure.Y / 2),
                1f,
                SpriteEffects.None,
                0f);

            // Draw the notes
            foreach (Note note in currentPlayableDifficulty.beatmap) {
                if (note.position <= currentAudioPosition + noteSpeed && !note.hasResolved) {
                    float[] coordinates = CalculateNoteCoordinates(currentAudioPosition, note);
                    float noteSize = 0.35f - (float)((note.position - currentAudioPosition) * 0.30f);

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
                scorePercent = (float)score / currentPlayableDifficulty.targetScores[0];
            } else if (rank == ScoreRank.C) { 
                scorePercent = (float)(score - currentPlayableDifficulty.targetScores[0]) / (currentPlayableDifficulty.targetScores[1] - currentPlayableDifficulty.targetScores[0]);
            } else if (rank == ScoreRank.B) {
                scorePercent = (float)(score - currentPlayableDifficulty.targetScores[1]) / (currentPlayableDifficulty.targetScores[2] - currentPlayableDifficulty.targetScores[1]);
            } else if (rank == ScoreRank.A) {
                scorePercent = (float)(score - currentPlayableDifficulty.targetScores[2]) / (currentPlayableDifficulty.targetScores[3] - currentPlayableDifficulty.targetScores[2]);
            } else {
                scorePercent = (float)(score - currentPlayableDifficulty.targetScores[3]) / (currentPlayableDifficulty.targetScores[4] - currentPlayableDifficulty.targetScores[3]);
            }

            if (scorePercent > 1) {
                scorePercent = 1;
            }

            // Draw the score gauge
            spriteBatch.Draw(gaugeTexture,
                    Vector2.Zero,
                    new Rectangle(0, 0, baseWidth + (int) (scorePercent * (gaugeTexture.Width - baseWidth)), gaugeTexture.Height),
                    Color.White,
                    0f,
                    Vector2.Zero,
                    0.30f,
                    SpriteEffects.None,
                    0f);

            // Draw the score gauge frame
            spriteBatch.Draw(scoreGaugeFrame,
                    Vector2.Zero,
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    0.30f,
                    SpriteEffects.None,
                    0f);

            // Draw the progress gauge base
            spriteBatch.Draw(progressGaugeBase,
                    new Vector2(graphics.PreferredBackBufferWidth / 2, 150),
                    null,
                    Color.White,
                    0f,
                    new Vector2(progressGaugeBase.Width / 2, progressGaugeBase.Height / 2),
                    0.25f,
                    SpriteEffects.None,
                    0f);

            // Draw the static progress gauge ring (Monogame doesn't support circular progress bars/circle "slices", may implement this in another way later)
            spriteBatch.Draw(progressGaugeRing,
                    new Vector2(graphics.PreferredBackBufferWidth / 2, 150),
                    null,
                    Color.White,
                    0f,
                    new Vector2(progressGaugeRing.Width / 2, progressGaugeRing.Height / 2),
                    0.25f,
                    SpriteEffects.None,
                    0f);
            spriteBatch.End();

        }

        /// <summary>
        /// Draw method for the live result screen. Called when game is on the live result screen and should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        void DrawResultScreen(GameTime gameTime) {
            GraphicsDevice.Clear(new Color(19, 232, 174));

            PlayableSongDifficulty currentPlayableDifficulty = currentSong.getDifficulty(currentDifficulty);

            spriteBatch.Begin();

            // Draw the result title text
            spriteBatch.Draw(resultTexture,
                    Vector2.Zero,
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    graphics.PreferredBackBufferHeight / 4000f,
                    SpriteEffects.None,
                    0f);

            // Display the song title
            spriteBatch.Draw(songTitleBaseTexture,
                    new Vector2(0, graphics.PreferredBackBufferHeight / 7.2f),
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    graphics.PreferredBackBufferHeight / 1800f,
                    SpriteEffects.None,
                    0f);

            spriteBatch.DrawString(delfinoFont,
                currentSong.title,
                new Vector2(graphics.PreferredBackBufferHeight / (720f / 180), graphics.PreferredBackBufferHeight / (720f / 115)),
                Color.White,
                0,
                Vector2.Zero,
                graphics.PreferredBackBufferHeight / 1800f,
                SpriteEffects.None,
                0f);

            // Display the song album art
            int squareSize = graphics.PreferredBackBufferHeight / 6;
            Texture2D albumArtBaseSquare = new Texture2D(graphics.GraphicsDevice, squareSize, squareSize);

            Color[] data = new Color[squareSize * squareSize];
            for (int i = 0; i < data.Length; ++i) {
                data[i] = new Color(206, 69, 123);
            }
            albumArtBaseSquare.SetData(data);

            spriteBatch.Draw(albumArtBaseSquare,
                new Vector2(graphics.PreferredBackBufferHeight / 24, graphics.PreferredBackBufferHeight / 9),
                Color.White);

            float offset = (squareSize - (graphics.PreferredBackBufferHeight / 1800f * currentSong.coverArt.Width)) / 2f; // Used to center the album art
            spriteBatch.Draw(currentSong.coverArt,
                    new Vector2(graphics.PreferredBackBufferHeight / 24 + offset, graphics.PreferredBackBufferHeight / 9 + offset),
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    graphics.PreferredBackBufferHeight / 1800f,
                    SpriteEffects.None,
                    0f);

            // Display beatmap difficulty  
            int orangeStars = 0;
            int yellowStars = currentPlayableDifficulty.starDifficulty;
            float starScale = graphics.PreferredBackBufferHeight / 9600f;
            if (currentPlayableDifficulty.starDifficulty > 10) {
                orangeStars = currentPlayableDifficulty.starDifficulty % 10;
                yellowStars = currentPlayableDifficulty.starDifficulty - orangeStars;
            }

            for (int i = 0; i < yellowStars; i++) {
                spriteBatch.Draw(starYellowTexture,
                    new Vector2(graphics.PreferredBackBufferHeight / (720f / 270) + (starScale * starYellowTexture.Width * i), graphics.PreferredBackBufferHeight / (720f / 170)),
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    starScale,
                    SpriteEffects.None,
                    0f);
            }

            for (int i = 0; i < orangeStars; i++) {
                spriteBatch.Draw(starOrangeTexture,
                    new Vector2(graphics.PreferredBackBufferHeight / (720f / 270) + (starScale * starOrangeTexture.Width * i), graphics.PreferredBackBufferHeight / (720f / 170)),
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    starScale,
                    SpriteEffects.None,
                    0f);
            }


            // Display score
            spriteBatch.Draw(scoreDisplayBaseTexture,
                    new Vector2(graphics.PreferredBackBufferHeight / (720 / (375 * 0.4f)), graphics.PreferredBackBufferHeight / (720f / 250)),
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    graphics.PreferredBackBufferHeight / 1800f,
                    SpriteEffects.None,
                    0f);

            // Draw score text
            DrawTextWithOutline(multicoloreFont,
                score.ToString(),
                new Vector2(graphics.PreferredBackBufferHeight / (720f / 630) - (graphics.PreferredBackBufferWidth / 3200f) * multicoloreFont.MeasureString(score.ToString()).X, graphics.PreferredBackBufferHeight / (720f / 275)),
                new Color(192, 78, 127),
                Color.White,
                Vector2.Zero,
                graphics.PreferredBackBufferWidth / 2800f,
                3f);

            // Draw high score text
            DrawTextWithOutline(multicoloreFont,
                score.ToString(), // TODO update this with actual high score
                new Vector2(graphics.PreferredBackBufferHeight / (720f / 630) - (graphics.PreferredBackBufferWidth / 3800f) * multicoloreFont.MeasureString(score.ToString()).X, graphics.PreferredBackBufferHeight / (720f / 320)),
                new Color(232, 132, 202),
                Color.White,
                Vector2.Zero,
                graphics.PreferredBackBufferWidth / 3400f,
                3f);

            // Display Perfects/Greats/Goods/Bads/Misses
            for (int i = 0; i < 5; i++) {
                spriteBatch.Draw(noteCountBaseTexture,
                    new Vector2(graphics.PreferredBackBufferHeight / (720 / (375 * 0.4f)), graphics.PreferredBackBufferHeight / (720f / (450 + i * 45))),
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    graphics.PreferredBackBufferHeight / 1800f,
                    SpriteEffects.None,
                    0f);
                spriteBatch.Draw(accuracyLabelTextures[i],
                    new Vector2(graphics.PreferredBackBufferHeight / (720 / (375 * 0.4f)) + graphics.PreferredBackBufferHeight / 72, graphics.PreferredBackBufferHeight / (720f / (450 + i * 45)) + graphics.PreferredBackBufferHeight / (720f/ 7)),
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    graphics.PreferredBackBufferHeight / 1800f,
                    SpriteEffects.None,
                    0f);

                String noteCount = "ERROR";
                switch (i) {
                    case 0:
                        noteCount = perfects.ToString();
                        break;
                    case 1:
                        noteCount = greats.ToString();
                        break;
                    case 2:
                        noteCount = goods.ToString();
                        break;
                    case 3:
                        noteCount = bads.ToString();
                        break;
                    case 4:
                        noteCount = misses.ToString();
                        break;
                }

                DrawTextWithOutline(multicoloreFont,
                    noteCount,
                    new Vector2(graphics.PreferredBackBufferHeight / (720f / 425) - (graphics.PreferredBackBufferWidth / 3800f) * multicoloreFont.MeasureString(noteCount.ToString()).X, graphics.PreferredBackBufferHeight / (720f / (450 + i * 45)) + graphics.PreferredBackBufferHeight / (720f / 7)),
                    new Color(135, 140, 146),
                    Color.White,
                    Vector2.Zero,
                    graphics.PreferredBackBufferWidth / 3800f,
                    3f);
            }

            // Display max combo label
            spriteBatch.Draw(maxComboTexture,
                new Vector2(graphics.PreferredBackBufferHeight / (720f / 580), graphics.PreferredBackBufferHeight / (720f / 480) + graphics.PreferredBackBufferHeight / (720f / 7)),
                null,
                Color.White,
                0f,
                new Vector2(maxComboTexture.Width / 2, maxComboTexture.Height / 2),
                graphics.PreferredBackBufferHeight / 1800f,
                SpriteEffects.None,
                0f);

            // Display max combo
            Vector2 maxComboTextSize = multicoloreFont.MeasureString(maxCombo.ToString());
            float maxComboTextScale = graphics.PreferredBackBufferWidth / 1100f;
            maxComboTextSize.X *= maxComboTextScale;
            maxComboTextSize.Y *= maxComboTextScale;

            DrawTextWithOutline(multicoloreFont,
                maxCombo.ToString(),
                new Vector2(graphics.PreferredBackBufferHeight / (720f / 600) + (maxComboTextSize.X / 2f), graphics.PreferredBackBufferHeight / (720f / 640)),
                new Color(158, 42, 170),
                Color.White,
                maxComboTextSize,
                maxComboTextScale,
                3f);

            // Display Full Combo/All Perfect text if appropriate
            if (perfects == currentPlayableDifficulty.beatmap.Length) {
                spriteBatch.Draw(allPerfectResultTexture,
                    new Vector2(graphics.PreferredBackBufferHeight / (720f / 580), graphics.PreferredBackBufferHeight / (720f / 620) + graphics.PreferredBackBufferHeight / (720f / 7)),
                    null,
                    Color.White,
                    0f,
                    new Vector2(allPerfectResultTexture.Width / 2, allPerfectResultTexture.Height / 2),
                    graphics.PreferredBackBufferHeight / 1800f,
                    SpriteEffects.None,
                    0f);
            } else if (maxCombo == currentPlayableDifficulty.beatmap.Length) {
                spriteBatch.Draw(fullComboResultTexture,
                    new Vector2(graphics.PreferredBackBufferHeight / (720f / 565), graphics.PreferredBackBufferHeight / (720f / 620) + graphics.PreferredBackBufferHeight / (720f / 7)),
                    null,
                    Color.White,
                    0f,
                    new Vector2(maxComboTexture.Width / 2, maxComboTexture.Height / 2),
                    graphics.PreferredBackBufferHeight / 1800f,
                    SpriteEffects.None,
                    0f);
            }
            spriteBatch.End();
        }

        /// <summary>
        /// Draw method for the goodbye screen. Called when game is on the goodbye screen and should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        void DrawGoodbyeScreen(GameTime gameTime) {
            GraphicsDevice.Clear(Color.White);
        }

        /// <summary>
        /// Loads beatmap data from a txt file in modified CustomBeatmapFestival format into an array of Notes.
        /// </summary>
        /// <param name="filepath">The path to the txt file.</param>
        /// <returns>An array of Notes in the beatmap.</returns>
        private Note[] LoadBeatmap(String filepath) {
            string[] lines = File.ReadAllLines(filepath);
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
                beatmap[i] = new Note(float.Parse(data[0]), lane, bool.Parse(data[2]), bool.Parse(data[3]), bool.Parse(data[4]), float.Parse(data[5]), float.Parse(data[6]), bool.Parse(data[7]), null);
            }
            Array.Sort(beatmap, delegate (Note x, Note y) { return x.position.CompareTo(y.position); });

            for (int i = 0; i < beatmap.Length; i++) {
                Note note = beatmap[i];
                Note nextNote;
                if (i + 1 >= beatmap.Length) {
                    nextNote = new Note(0f, 0, false, false, false, 0f, 0f, false, null); // Dummy note
                } else {
                    nextNote = beatmap[i + 1];
                }
                if (note.hasResolved && !note.isHold) {
                    continue;
                }

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
            }


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
            float xCoord;
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
                foreach (Note note in currentSong.getDifficulty(currentDifficulty).beatmap) {
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

            foreach (Note note in currentSong.getDifficulty(currentDifficulty).beatmap) {
                if (!note.hasResolved && ((down && !note.isRelease) || (!down && note.isRelease))) {
                    if (note.lane == lane && Math.Abs(note.position - currentAudioPosition) <= badTolerance) {
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

        /// <summary>
        /// Draws a framerate counter at the top right of the screen.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private void drawFPSCounter(GameTime gameTime) {
            int fps = (int)(1 / gameTime.ElapsedGameTime.TotalSeconds);
            spriteBatch.DrawString(defaultFont,
                fps.ToString() + " FPS",
                new Vector2(graphics.PreferredBackBufferWidth - 50, 10),
                // new Color(38, 252, 5),
                Color.Red,
                0,
                Vector2.Zero,
                0.3f,
                SpriteEffects.None,
                0f);
        }
    
        /// <summary>
        /// Draws text with an border/stroke. Must be called after SpriteBatch.Begin() and before SpriteBatch.End().
        /// </summary>
        /// <param name="font">The SpriteFont to use.</param>
        /// <param name="text">The text to be drawn.</param>
        /// <param name="position">A Vector2 with the coordinates of where the text should be drawn.</param>
        /// <param name="textColour">The colour of the text to be drawn.</param>
        /// <param name="outlineColour">The colour of the text border.</param>
        /// <param name="origin">The origin to use when drawing the text.</param>
        /// <param name="scale">The scale at which the text should be drawn.</param>
        /// <param name="outlineThickness">The thickness of the outline, in px.</param>
        private void DrawTextWithOutline(SpriteFont font, String text, Vector2 position, Color textColour, Color outlineColour, Vector2 origin, float scale, float outlineThickness) {
            // Draw outline
            spriteBatch.DrawString(font, text, position + new Vector2(outlineThickness * scale, outlineThickness * scale), outlineColour, 0, origin, scale, SpriteEffects.None, 1f);
            spriteBatch.DrawString(font, text, position + new Vector2(-outlineThickness * scale, outlineThickness * scale), outlineColour, 0, origin, scale, SpriteEffects.None, 1f);
            spriteBatch.DrawString(font, text, position + new Vector2(-outlineThickness * scale, -outlineThickness * scale), outlineColour, 0, origin, scale, SpriteEffects.None, 1f);
            spriteBatch.DrawString(font, text, position + new Vector2(outlineThickness * scale, -outlineThickness * scale), outlineColour, 0, origin, scale, SpriteEffects.None, 1f);

            // Draw text
            spriteBatch.DrawString(font, text, position, textColour, 0, origin, scale, SpriteEffects.None, 0f);
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
        public Note(float pos, int lan, Boolean multiple, Boolean hold, Boolean release, float releaseNoteTime, float parentNoteTime, Boolean star, Texture2D texture) {
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
            this.texture = texture;
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
        public PlayableSongDifficulty[] difficulties = new PlayableSongDifficulty[8]; // 0-4 = Easy-Challenge, 5 = Combo, 6 = Plus, 7 = Switch
        public PlayableSongType type;

        /// <summary>
        /// Defines a PlayableSong with a background video, using the video's audio track as the beatmap's song source.
        /// </summary>
        /// <param name="title">The title of the song.</param>
        /// <param name="coverArt">The cover art of the song.</param>
        /// <param name="video">The video for the song.</param>
        public PlayableSong(string title, Texture2D coverArt, Video video) {
            this.title = title;
            this.coverArt = coverArt;
            backgroundMv = video;
            type = PlayableSongType.Video;
        }

        /// <summary>
        /// Defines a PlayableSong with no background video.
        /// </summary>
        /// <param name="title">The title of the song.</param>
        /// <param name="coverArt">The cover art of the song.</param>
        /// <param name="song">The song's audio file.</param>
        public PlayableSong(string title, Texture2D coverArt, Song song) {
            this.title = title;
            this.coverArt = coverArt;
            music = song;
            type = PlayableSongType.Music;
        }

        /// <summary>
        /// Defines and adds a playable difficulty beatmap with no time offset to this song.
        /// </summary>
        /// <param name="difficulty"></param>
        /// <param name="beatmap"></param>
        /// <param name="targetScores"></param>
        public void addDifficulty(Difficulty difficulty, Note[] beatmap, int starDifficulty, int[] targetScores) {
            PlayableSongDifficulty songDifficulty = new PlayableSongDifficulty(beatmap, starDifficulty, targetScores);

            switch (difficulty){
                case Difficulty.Easy:
                    this.difficulties[0] = songDifficulty;
                    break;
                case Difficulty.Normal:
                    this.difficulties[1] = songDifficulty;
                    break;
                case Difficulty.Hard:
                    this.difficulties[2] = songDifficulty;
                    break;
                case Difficulty.Extreme:
                    this.difficulties[3] = songDifficulty;
                    break;
                case Difficulty.Challenge:
                    this.difficulties[4] = songDifficulty;
                    break;
                case Difficulty.Plus:
                    this.difficulties[5] = songDifficulty;
                    break;
                case Difficulty.Combo:
                    this.difficulties[6] = songDifficulty;
                    break;
                case Difficulty.Switch:
                    this.difficulties[7] = songDifficulty;
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Defines and adds a playable difficulty beatmap with the specified time offset to this song.
        /// </summary>
        /// <param name="difficulty"></param>
        /// <param name="beatmap"></param>
        /// <param name="targetScores"></param>
        /// <param name="offset"></param>
        public void addDifficulty(Difficulty difficulty, Note[] beatmap, int starDifficulty, int[] targetScores, float offset) {
            PlayableSongDifficulty songDifficulty = new PlayableSongDifficulty(beatmap, starDifficulty, targetScores, offset);

            switch (difficulty) {
                case Difficulty.Easy:
                    this.difficulties[0] = songDifficulty;
                    break;
                case Difficulty.Normal:
                    this.difficulties[1] = songDifficulty;
                    break;
                case Difficulty.Hard:
                    this.difficulties[2] = songDifficulty;
                    break;
                case Difficulty.Extreme:
                    this.difficulties[3] = songDifficulty;
                    break;
                case Difficulty.Challenge:
                    this.difficulties[4] = songDifficulty;
                    break;
                case Difficulty.Plus:
                    this.difficulties[5] = songDifficulty;
                    break;
                case Difficulty.Combo:
                    this.difficulties[6] = songDifficulty;
                    break;
                case Difficulty.Switch:
                    this.difficulties[7] = songDifficulty;
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Returns the PlayableSongDifficulty object for a given difficulty if it exists, or null otherwise.
        /// </summary>
        /// <param name="difficulty">The difficulty to be fetched.</param>
        /// <returns>The PlayableSongDifficulty object for a given difficulty if it exists, or null otherwise.</returns>
        public PlayableSongDifficulty getDifficulty(Difficulty difficulty) {
            switch (difficulty) {
                case Difficulty.Easy:
                    return (this.difficulties[0]);
                case Difficulty.Normal:
                    return (this.difficulties[1]);
                case Difficulty.Hard:
                    return (this.difficulties[2]);
                case Difficulty.Extreme:
                    return (this.difficulties[3]);
                case Difficulty.Challenge:
                    return (this.difficulties[4]);
                case Difficulty.Plus:
                    return (this.difficulties[5]);
                case Difficulty.Combo:
                    return (this.difficulties[6]);
                case Difficulty.Switch:
                    return (this.difficulties[7]);
                default:
                    throw new InvalidOperationException();
            }
        }
    }

    /// <summary>
    /// Represents a difficulty for a PlayableSong.
    /// </summary>
    public class PlayableSongDifficulty {
        public Note[] beatmap;
        public int starDifficulty;
        public int[] targetScores; // targetScores[0] is C score, 1 is B, 2 is A ... 5 is SSS
        public float beatmapTimeOffset; // Time offset of all notes in beatmap, in seconds. Corresponds to the number of seconds of silence at the beginning of the video before the audio starts. Used for manually aligned audio/video tracks.

        /// <summary>
        /// Defines a PlayableSongDifficulty with no beatmap time offset.
        /// </summary>
        /// <param name="beatmap">The beatmap of note timing data.</param>
        /// <param name="starDifficulty">The star difficulty of this beatmap.</param>
        /// <param name="targetScores">The target scores for C, B, A, S, SS, and SSS, in that order in the array.</param>
        public PlayableSongDifficulty(Note[] beatmap, int starDifficulty, int[] targetScores) {
            this.beatmap = beatmap;
            this.starDifficulty = starDifficulty;
            this.targetScores = targetScores;
            beatmapTimeOffset = 0;
        }

        /// <summary>
        /// Defines a PlayableSongDifficulty with a beatmap time offset.
        /// </summary>
        /// <param name="beatmap">The beatmap of note timing data.</param>
        /// <param name="starDifficulty">The star difficulty of this beatmap.</param>
        /// <param name="targetScores">The target scores for C, B, A, S, SS, and SSS, in that order in the array.</param>
        /// <param name="beatmapTimeOffset">The time offset for the beatmap, in seconds. Corresponds to the number of seconds of silence in the beginning of the video before the song starts.</param>
        public PlayableSongDifficulty(Note[] beatmap, int starDifficulty, int[] targetScores, float beatmapTimeOffset) {
            this.beatmap = beatmap;
            this.starDifficulty = starDifficulty;
            this.targetScores = targetScores;
            this.beatmapTimeOffset = beatmapTimeOffset;

            foreach (Note note in this.beatmap) {
                note.position += beatmapTimeOffset;
                if (note.isHold) {
                    note.releaseNoteSpawnTime += beatmapTimeOffset;
                } else if (note.isRelease) {
                    note.parentNoteSpawnTime += beatmapTimeOffset;
                }
            }
        }
    }

    /// <summary>
    /// The various beatmap difficulties and modes.
    /// </summary>
    public enum Difficulty {
        Easy,
        Normal,
        Hard,
        Extreme,
        Challenge,
        Plus,
        Combo,
        Switch
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

    /// <summary>
    /// The possible score ranks that can be achieved by the end of a song.
    /// </summary>
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