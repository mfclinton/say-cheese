# Say Cheese

You're a secret police officer in a future where being unhappy is illegal. Each level gives you a description, and you scan the camera feeds for the people who match it.

- Play: [itch.io](https://unitedfailures.itch.io/say-cheese)
- Made: January to June 2024. We started it at Global Game Jam 2024.
- Team: [@mfclinton](https://github.com/mfclinton) (programming), [@MrAozora](https://github.com/MrAozora) (music, UI, and sound effects), [27mth](https://27mth.itch.io) (art), [Kiy0shi2099](https://kiy0shi2099.itch.io) (art)
- Engine: Unity, C#

This is an export of a private repo with only the code we wrote. Art, audio, the Unity project files, and third party plugins aren't included. The history is squashed into one commit.

## What I built

- I built the crowd generator. Everyone is put together from layered sprites for their hair, face, hat, clothes, and whatever they're holding. Each level bans a few specific pieces and the suspects wear them. Innocent people get re-rolled if they match by accident, and some are decoys who wear one banned piece on purpose.
- The crowd walks in lanes at different depths. People randomly switch lanes and speed up or slow down over the course of the level, and their walk animation keeps pace.
- All the shaders and post-processing. Losing turns on a full-screen glitch effect, the game and main menu each have their own look with bloom, vignette, film grain, and split toning, and people get a scan line over them when you hover. I also made the laser and fog.
- A surveillance eye follows your cursor and fires a laser at whoever you pick. When you lose, the camera swings back to the eye, a light swells, and the screen glitches out.
- The ending. After the last level, the game's camera starts drawing into a render texture on a computer monitor, the UI moves onto it too, and a second camera pulls back to reveal the monitor with matrix-style code rain.
- 10 levels across three maps that ramp up the crowd size, lanes, and decoys, with checkpoints and medals for time, accuracy, and mistakes.
