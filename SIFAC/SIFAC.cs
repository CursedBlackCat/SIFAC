using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

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

    public class BeatmapParseException : Exception {
        public BeatmapParseException() {

        }

        public BeatmapParseException(string message) {

        }
    }

    public class PlayableSong {
        // TODO start moving assets for the beatmap here
        public string title;
        public Texture2D coverArt;
        public Song music = null;
        public Video backgroundMv = null;
        public Note[] beatmap;
        public PlayableSongType type;

        /// <summary>
        /// Constructs a PlayableSong with a background video, using the video as the beatmap's song source.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="cover"></param>
        /// <param name="v"></param>
        /// <param name="map"></param>
        public PlayableSong(string title, Texture2D cover, Video v, Note[] map) {
            this.title = title;
            coverArt = cover;
            backgroundMv = v;
            beatmap = map;
            type = PlayableSongType.Video;
        }

        /// <summary>
        /// Constructs a PlayableSong with no background video.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="cover"></param>
        /// <param name="v"></param>
        /// <param name="map"></param>
        public PlayableSong(string title, Texture2D cover, Song s, Note[] map) {
            this.title = title;
            coverArt = cover;
            music = s;
            beatmap = map;
            type = PlayableSongType.Music;
        }
    }

    public enum GameState {
        TitleScreen,
        NesicaCheckScreen,
        GroupSelectScreen,
        SongSelectScreen, // Also handles difficulty selection
        LivePreparationScreen,
        LiveScreen,
        ResultScreen,
        GoodbyeScreen
    }

    public enum NoteAccuracy {
        Perfect,
        Great,
        Good,
        Bad,
        Miss,
        None // Returns when you attempt to hit a note before it comes into range
    }

    public enum PlayableSongType {
        Video,
        Music
    }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class SIFAC : Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        GameState currentGameState = GameState.SongSelectScreen;

        Texture2D tooltipL3L4;
        Texture2D[] mainTooltips = new Texture2D[5]; //0 to 4, left to right. These are the main 5 tooltips on the song select screen spanning L2 through R2.
        Texture2D tooltipR3R4;
        Texture2D[] mainHighlightedTooltips = new Texture2D[5];
        int highlightedMenuElement = 2;
        int songSelectPage = 0; // Each page has 5 songs, and page n is indices n*5 through n*5+4 of songs
        PlayableSong[] menuChoices = new PlayableSong[5]; // Currently displayed song choices. 0 to 4, left to right. Index 0 corresonds to L2, 1 to L1, etc, and 4 to R2.

        Texture2D noteTexture;
        Texture2D noteMultiBlueTexture;
        Texture2D noteMultiOrangeTexture;
        Texture2D noteReleaseTexture;
        Texture2D noteReleaseMultiBlueTexture;
        Texture2D noteReleaseMultiOrangeTexture;
        Texture2D hitMarkerTexture;

        SpriteFont defaultFont;

        Vector2[] hitMarkerPositions = new Vector2[9];
        float[] xOffsets = new float[4];
        float[] yOffsets = new float[4];
        float radiusH; // This is set in Initialize()
        float radiusV; // This is set in Initialize()
        Vector2 noteSpawnPosition;
        Boolean lastMultiWasBlue = false; // Used to toggle between orange and blue multi notes

        KeyboardState previousState;

        List<PlayableSong> songs = new List<PlayableSong>();
        PlayableSong currentSong;

        VideoPlayer bgVideoPlayer;
        Boolean playVideo = true;

        SoundEffect[] hitSoundEffects = new SoundEffect[4]; // hitSoundEffects[0] is perfect, 1 is great, 2 is good, 3 is bad

        int combo = 0;
        int maxCombo = 0;
        int perfects = 0;
        int greats = 0;
        int goods = 0;
        int bads = 0;
        int misses = 0;


        /* CONFIG */
        // Timing tolerences, in seconds. Hitting a note at its time + or - each of these values gets the corresponding accuracy rating.
        readonly double perfectTolerance = 0.1;
        readonly double greatTolerance = 0.2;
        readonly double goodTolerance = 0.4;
        readonly double badTolerance = 0.8;
        readonly double missTolerance = 1.5; // Not hitting a note after this much time elapses after its hit time will count as a miss

        float noteSpeed = 1f; // Note speed, represented by seconds from spawn to note hit position.

        // Timing offset setting, in seconds.
        // Use a timeOffset value of -0.05 for playing and -0.25 for autoplay. These values aren't perfect.
        // If too large, notes will be too early. If too small, notes will be too late.
        double timeOffset = 0;     

        // Autoplay, for debug purposes
        Boolean autoplay = false;

        // Fullscreen 1080p vs 720p flag for debugging. Game is intended to be play fullscreen at 1080p.
        Boolean fullscreen = false;
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
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
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
            hitMarkerTexture = Content.Load<Texture2D>("notes/HitMarker");

            // Initialize the VideoPlayer
            bgVideoPlayer = new VideoPlayer();

            // Initialize the MediaPlayer

            // Load the note hit sounds
            hitSoundEffects[0] = Content.Load<SoundEffect>("sounds/hit_perfect");
            hitSoundEffects[1] = Content.Load<SoundEffect>("sounds/hit_great");
            hitSoundEffects[2] = Content.Load<SoundEffect>("sounds/hit_good");
            hitSoundEffects[3] = Content.Load<SoundEffect>("sounds/hit_bad");

            // Load fonts
            defaultFont = Content.Load<SpriteFont>("fonts/DefaultFont");

            // Load beatmap assets
            // TODO use a for loop of some sort to dynamically load all beatmaps
            Video video = Content.Load<Video>("beatmap_assets/Believe Again/video");
            Texture2D cover = Content.Load<Texture2D>("beatmap_assets/Believe Again/cover");

            // Load the notes
            // TODO put the beatmap loading code into its own helper method
            string[] lines = System.IO.File.ReadAllLines(@"C:\Users\darre\source\repos\SIFAC\SIFAC\Beatmaps\believe_again.txt");
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

            songs.Add(new PlayableSong("Believe Again", cover, video, beatmap));

            // Load the calibration beatmap
            lines = System.IO.File.ReadAllLines(@"C:\Users\darre\source\repos\SIFAC\SIFAC\Content\beatmap_assets\Calibration\beatmap.txt");
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
                    default:
                        throw new BeatmapParseException("Invalid note lane " + data[1]);
                }
                beatmap[i] = new Note(float.Parse(data[0]), lane, bool.Parse(data[2]), bool.Parse(data[3]), bool.Parse(data[4]), float.Parse(data[5]), float.Parse(data[6]), bool.Parse(data[7]));
            }
            Array.Sort(beatmap, delegate (Note x, Note y) { return x.position.CompareTo(y.position); });

            songs.Add(new PlayableSong("Calibration", Content.Load<Texture2D>("beatmap_assets/Calibration/cover"), Content.Load<Song>("beatmap_assets/Calibration/song"), beatmap));

            // Load the hold calibration beatmap
            lines = System.IO.File.ReadAllLines(@"C:\Users\darre\source\repos\SIFAC\SIFAC\Content\beatmap_assets\Hold Note Calibration\beatmap.txt");
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
                    default:
                        throw new BeatmapParseException("Invalid note lane " + data[1]);
                }
                beatmap[i] = new Note(float.Parse(data[0]), lane, bool.Parse(data[2]), bool.Parse(data[3]), bool.Parse(data[4]), float.Parse(data[5]), float.Parse(data[6]), bool.Parse(data[7]));
            }
            Array.Sort(beatmap, delegate (Note x, Note y) { return x.position.CompareTo(y.position); });

            songs.Add(new PlayableSong("Hold Note Calibration", Content.Load<Texture2D>("beatmap_assets/Hold Note Calibration/cover"), Content.Load<Song>("beatmap_assets/Hold Note Calibration/song"), beatmap));

            // Add placeholder songs
            songs.Add(new PlayableSong("Placeholder 3", Content.Load<Texture2D>("beatmap_assets/Placeholder/cover"), Content.Load<Song>("beatmap_assets/Calibration/song"), new Note[0]));
            songs.Add(new PlayableSong("Placeholder 4", Content.Load<Texture2D>("beatmap_assets/Placeholder/cover"), Content.Load<Song>("beatmap_assets/Calibration/song"), new Note[0]));
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
                highlightedMenuElement = 0;
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
                } else if (playVideo & bgVideoPlayer.State == MediaState.Stopped) {
                    bgVideoPlayer.Volume = 0.2f;
                    bgVideoPlayer.Play(currentSong.backgroundMv);
                    playVideo = false;
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
                Console.WriteLine("Red Button Pressed");
                Exit();
            }
            if (kstate.IsKeyDown(Keys.Enter) & !previousState.IsKeyDown(Keys.Enter)) {
                Console.WriteLine("Blue Button Pressed");
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
                Console.WriteLine("Red Button Released");
                Exit();
            }
            if (!kstate.IsKeyDown(Keys.Enter) & previousState.IsKeyDown(Keys.Enter)) {
                Console.WriteLine("Blue Button Released");
            }
            previousState = kstate;

            if (currentSong.type == PlayableSongType.Video) { // Beatmap audio source is from video
                foreach (Note note in currentSong.beatmap) {
                    //AUTOPLAY CODE
                    if (autoplay && !note.hasResolved && note.position <= bgVideoPlayer.PlayPosition.TotalSeconds + timeOffset) {
                        // Console.WriteLine("Auto");
                        note.result = NoteAccuracy.Perfect;
                        hitSoundEffects[0].Play(0.2f, 0f, 0f);
                        note.hasResolved = true;
                        perfects++;
                    }
                    //END AUTOPLAY CODE

                    if (note.result != NoteAccuracy.None) {
                        note.hasResolved = true;
                    }
                    if (!note.hasResolved && note.position <= bgVideoPlayer.PlayPosition.TotalSeconds - missTolerance) { //TODO factor in time offset
                        Console.WriteLine("Miss");
                        note.result = NoteAccuracy.Miss;
                        note.hasResolved = true;
                        combo = 0;
                        misses++;
                    }
                    if (bgVideoPlayer.State == MediaState.Stopped) {
                        currentGameState = GameState.ResultScreen;
                    }
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

                    if (note.result != NoteAccuracy.None) {
                        note.hasResolved = true;
                    }
                    if (!note.hasResolved && note.position <= MediaPlayer.PlayPosition.TotalSeconds - missTolerance) { //TODO factor in time offset
                        Console.WriteLine("Miss");
                        note.result = NoteAccuracy.Miss;
                        note.hasResolved = true;
                        misses++;
                    }
                }

                if (MediaPlayer.State == MediaState.Stopped) {
                    currentGameState = GameState.ResultScreen;
                }
            }
        }

        void UpdateResultScreen(GameTime gameTime) {
            // TODO
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
                // TODO

                // Reset the results
                combo = 0;
                maxCombo = 0;
                perfects = 0;
                greats = 0;
                goods = 0;
                bads = 0;
                misses = 0;

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

        void DrawTitleScreen(GameTime gameTime) {
            // TODO
            GraphicsDevice.Clear(Color.White);
        }

        void DrawNesicaCheckScreen(GameTime gameTime) {
            // TODO
            GraphicsDevice.Clear(Color.White);
        }

        void DrawGroupSelectScreen(GameTime gameTime) {
            // TODO
            GraphicsDevice.Clear(Color.White);
        }

        void DrawSongSelectScreen(GameTime gameTime) {
            // TODO
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

        void DrawLivePreparationScreen(GameTime gameTime) {
            // TODO
            GraphicsDevice.Clear(Color.White);
        }

        void DrawLiveScreen(GameTime gameTime) {
            GraphicsDevice.Clear(Color.White);

            spriteBatch.Begin();

            if (currentSong.backgroundMv != null) {
                spriteBatch.Draw(
                    bgVideoPlayer.GetTexture(),
                    new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                    Color.White
                    );
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

            double currentAudioPosition;
            if (currentSong.type == PlayableSongType.Video) {
                currentAudioPosition = bgVideoPlayer.PlayPosition.TotalSeconds;
            } else {
                currentAudioPosition = MediaPlayer.PlayPosition.TotalSeconds;
            }
            
            Note previousNote = new Note(-1, -1, false, false, false, 0, 0, false);
            foreach (Note note in currentSong.beatmap) {
                if (note.hasResolved) {
                    continue;
                }
                if (note.position <= currentAudioPosition + noteSpeed && !note.hasResolved) {
                    float[] coordinates = CalculateNoteCoordinates(currentAudioPosition, note);
                    float noteSize = 0.35f - (float)((note.position - currentAudioPosition) * 0.15f);

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
            spriteBatch.End();
        }

        void DrawResultScreen(GameTime gameTime) {
            // TODO
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

        void DrawGoodbyeScreen(GameTime gameTime) {
            // TODO
            GraphicsDevice.Clear(Color.White);
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
        /// <param name="down">Whether the button is being pushed or released. Should be true if pushed, otherwise false if released (used for hold note releases).</param>
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
                            Console.WriteLine("Perfect (early by " + diff + ")");
                            hitSoundEffects[0].Play(0.2f, 0f, 0f);
                            note.result = NoteAccuracy.Perfect;
                            note.hasResolved = true;
                            perfects++;
                            if (!note.isRelease && ++combo > maxCombo) {
                                    maxCombo = combo;
                            }
                            return NoteAccuracy.Perfect;
                        } else if (Math.Abs(diff) <= greatTolerance) {
                            Console.WriteLine("Great (early by " + diff + ")");
                            hitSoundEffects[1].Play(0.2f, 0f, 0f);
                            note.result = NoteAccuracy.Great;
                            note.hasResolved = true;
                            greats++;
                            if (!note.isRelease && ++combo > maxCombo) {
                                maxCombo = combo;
                            }
                            combo++;
                            return NoteAccuracy.Great;
                        } else if (Math.Abs(diff) <= goodTolerance) {
                            Console.WriteLine("Good (early by " + diff + ")");
                            hitSoundEffects[2].Play(0.2f, 0f, 0f);
                            note.result = NoteAccuracy.Good;
                            note.hasResolved = true;
                            goods++;
                            combo = 0;
                            return NoteAccuracy.Good;
                        } else if (Math.Abs(diff) <= badTolerance) {
                            Console.WriteLine("Bad (early by " + diff + ")");
                            hitSoundEffects[3].Play(0.2f, 0f, 0f);
                            note.result = NoteAccuracy.Bad;
                            note.hasResolved = true;
                            bads++;
                            combo = 0;
                            return NoteAccuracy.Bad;
                        }
                    }
                }
            }
            return NoteAccuracy.None;
        }
    }
}
