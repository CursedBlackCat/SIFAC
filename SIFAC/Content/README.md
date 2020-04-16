# Content
This document describes the specification for game assets.
## Beatmap assets
Each song should be in its own directory under `beatmap_assets`. The directory should be the title of the song as displayed in-game. Within each song's directory, there should be one 256x256 png named `cover.png`, which is the cover art of the song, as well as a 1080p or 720p background video named `video.mp4`. The video is not to be included in Git as it is likely over the maximum 100MB filesize that Github allows. Attempting to commit a video file that is too large will result in the push failing.