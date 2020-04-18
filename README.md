# SIFAC - Project Northern Stage
School Idol Festival AC - Project Northern Stage (SIFAC-PNS for short) is an unofficial, fan-made version of the Japanese arcade game [School Idol Festival AC Next Stage (SIFAC NS)](http://www.lovelive-sifacns.jp). It is intended to be played on PC with the use of a specialized, custom-made controller (currently a work in progress).

## Project Statement
The goal of this project is to achieve a gameplay experience as similar to that of the official arcade machines as possible. This experience will be achieved through recreating the look and feel of the official arcade machines as faithfully to the original as reasonably possible. However, this recreation will obviously be limited by factors such as what official assets can be obtained and what must be recreated from scratch, as well as the quality of the custom controller when that is completed.

## Project Staff
This project is being worked on by:
* CursedBlackCat#7801 - Project lead, game client development, audiovisual asset recreation
* Crumbs#2341 - Controller design and build, audio asset recreation

## Project Status
### Game Client
The game client still has much work to be done. One beatmap is currently playable, as well as two debugging beatmaps.

To-do (based on program flow of official arcade machines):
- [ ] Title screen
  - [ ] Recreate "look and feel" of official game
- [ ] Group select
  - [ ] Display and select groups
  - [ ] Recreate "look and feel" of official game
- [ ] Song select
  - [x] Display song list
  - [x] Select song
  - [ ] Song difficulties
  - [ ] Song categories
  - [ ] Special beatmaps (Plus/Combo/Switch)
  - [ ] Recreate "look and feel" of official game
- [ ] Live preparation screen
  - [ ] Option menu
  - [ ] Recreate "look and feel" of official game
- [ ] Live gameplay
  - [x] Background video
  - [x] Note hit accuracy judgement
  - [x] Star notes
  - [ ] Scoring
- [ ] Result screen
  - [x] Track and show Perfect/Great/Good/Bad/Miss count
  - [x] Track and show max combo
  - [ ] Calculate and show score
  - [ ] Recreate "look and feel" of official game
- [ ] Goodbye screen
  - [ ] Recreate "look and feel" of official game

Long-term goals (subject to change):
- [ ] NFC reader integration for NESiCA cards
- [ ] Implementation of accounts (gacha, card collection, profiles, save max combo and scores, etc)

### Controller
The controller design is complete and is awaiting a physical build.

![3D model of controller](https://cdn.discordapp.com/attachments/617370153512075264/624073853873946642/unknown.png "3D model of controller")
![Front panel of controller](https://cdn.discordapp.com/attachments/617370153512075264/624120121761333248/unknown.png "Front panel of controller")
This design is intended to be used on top of a table or desk. The controller has a self-contained monitor as well as an Arduino unit. The monitor and the Arduino are to be plugged into a computer running the game client. The Arduino maps the eleven buttons to their respective keys on the keyboard, as specified below.

## Project Distribution and Usage
We currently have no plans to produce controllers for others. However, there is a chance we may do so in the future.

The game is playable, albeit with extreme difficulty, with a standard QWERTY keyboard. The nine main buttons map to `A S D F <space> J K L ;` respectively from left to right, the red button maps to `Escape`, and the blue button maps to `Enter/Return`. Please keep in mind that the controls are designed for our custom controller and not a keyboard, so the keyboard key mapping may be difficult to use.
