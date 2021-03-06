# Content
This document describes the specification for game assets.
## Beatmap assets
Each song should be in its own directory under `beatmap_assets`. The directory should be the title of the song as displayed in-game. Within each song's directory, there should be a `beatmap.txt` file with the song's beatmap in [Custom Beatmap Festival](https://www.reddit.com/r/CustomBeatmapFestival/comments/55adlk/custom_beatmap_festival_download_links_and_older/) format, one 256x256 png named `cover.png`, which is the cover art of the song, as well as a 1080p or 720p background video named `video.mp4`. The video is not to be included in Git as it is likely over the maximum 100MB filesize that Github allows. Attempting to commit a video file that is too large will result in the push failing. For audio-only beatmaps, a `song.mp3` file can be substituted instead of `video.mp4`.

### Beatmap videos
As beatmap background videos cannot be committed through Git due to file size limitations, they can instead be found [here](https://drive.google.com/drive/folders/1x5RCdZQPFd2duLyq7ta2aVfGLLFKGvFZ?usp=sharing).

## Fonts
Custom fonts should be added in this folder as `.ttf` files, then loaded into the Monogame content pipeline in a `.spritefont` file.

## Notes
Note sprites should be 526x526 `.png` images.

## Stars
Note sprites should be 512x512 `.png` images.

## UI
UI element image sizes may vary depending on the UI element, but they should be sufficiently sized `.png` images that can be downscaled if necessary.
