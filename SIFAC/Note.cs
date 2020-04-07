using System;

namespace Beatmap {
	public class Note {
		float position; // Position of the note in seconds
		int lane; // 1 through 9, left to right
		Boolean isMultiple;
		Boolean isHold;
		Boolean isRelease;
		float releaseNoteSpawnTime;
		float parentNoteSpawnTime;
		Boolean hasStar;
		public Note(float pos, int lan, Boolean multiple, Boolean hold, Boolean release, float releaseNoteTime, float parentNoteTime, Boolean star) {
			position = pos;
			lane = lan;
			isMultiple = multiple;
			isHold = hold;
			isRelease = release;
			releaseNoteSpawnTime = releaseNoteTime;
			parentNoteSpawnTime = parentNoteTime;
			hasStar = star;
		}
	}
}

