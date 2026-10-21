# Audio (2026-10-21)

Not part of the course (it has no audio module): the Studio audio addition from the Architecture Starter, the same module as FinalProject. Sounds follow the sci-fi swarm direction: an orbital strike beam, land mines, chitin bugs, a metal dome reactor, desert wind.

## How it is wired

- **Settings in one place:** `Assets/_Project/Resources/Configs/Audio/AudioConfig.asset`. For every sound it holds the id (`SoundIds`), channel (UI / Gameplay / Ambient), clip variants, volume, pitch spread and cooldown. It also holds an ambient loop per `AmbientIds`, channel volumes and the ambient crossfade.
- **Service:** `IAudioService` (`Play`, `Stop`, `PlayAmbient`, `SetVolume`) is a project singleton, `Resources/Utilities/AudioService.prefab` (12 one-shot sources, two crossfading ambient sources). It is created in `ProjectContextRegistrations` after the configs are loaded.
- **Ambient:** the scene registrations start `Menu` and `Gameplay`. Each is one seamless 32 s loop that the service crossfades into itself.
- **Gameplay sounds:** `Gameplay/Features/AudioFeature/GameplaySoundsService` (gameplay scope) listens to entities and services:

  | Source | Event | Sound |
  |---|---|---|
  | Bugs | appear from a burrow | BugSpawn |
  | Bugs | killed | BugDeath |
  | Bugs | blow up at the dome | AcidBlast |
  | Dome | hit | DomeHit |
  | Dome | destroyed | DomeDestroyed |
  | Mines | placed | MinePlace |
  | Mines | blast | MineBlast |
  | `ClickExplosionService.Exploded` | orbital strike | OrbitalStrike |
  | `MinePlacementService.Denied` | refused mine click | MineDenied |
  | Stage number grows | wave starts | WaveStart |

  Views and prefabs have no audio code.
- **UI sounds** in presenters:
  - menu Play → UIOpen;
  - the start-wave button → UIClick;
  - the win and defeat popups → Win / Defeat when shown, UIClick on their buttons.
- **Cooldowns keep hordes listenable:** a strike that kills 30 bugs plays one death, not thirty.

## Sources

`ArtSource/Audio/make_audio.py` builds every clip into `Assets/_Project/Art/Audio` (SFX 48 kHz mono WAV, ambient 24 kHz mono WAV). `ArtSource/UnityAuthoring/AudioAuthoring.cs` then sets the import (SFX Vorbis 0.8, decompress on load; ambient streaming), the prefab and the config.

- **Kenney CC0** ([kenney.nl](https://kenney.nl), Creative Commons Zero; credit not required, given here with thanks), mixed and trimmed with ffmpeg:

  | Pack | sha256 | Used for |
  |---|---|---|
  | Sci-Fi Sounds 1.0 | `119340f3…49e04` | OrbitalStrike (laserLarge + lowFrequency_explosion), MineBlast (explosionCrunch), AcidBlast (slime), DomeDestroyed, the force-field layer of DomeHit |
  | Impact Sounds | `029d734a…77f8` | DomeHit (impactMetal_heavy), MinePlace (impactPlate_medium) |
  | Interface Sounds | `f2193d07…1232` | UIClick, UIOpen, MineDenied, the beep of MinePlace |

  The packs are unpacked in `ArtSource/Library/Kenney`, which is not in Git; full hashes are in the script header.
- **Local synthesis** (pure Python, fixed seeds; blocks from FinalProject's `sfx_synth.py`):
  - BugSpawn — dirt scratching and a chitter;
  - BugDeath — a chitin crunch with a falling squeak;
  - WaveStart — a two-tone base siren;
  - Win and Defeat stingers;
  - ambient Menu (desert wind) and Gameplay (wind plus a distant hive drone and rare chirps).

## Verified 2026-10-21

- Compile PASS, 0 new Console errors.
- Background Play is muted, so playback was checked through the service's last-play log and its playing sources:
  - the menu ambient crossfades into the gameplay ambient;
  - all 14 sound ids played at their events: wave start, spawn, strike and deaths, acid blast and dome hit, mine placed and refused, mine blast, defeat with dome destruction, restart click, win.
- Listening and levels are the user's review.
