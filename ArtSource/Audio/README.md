# Audio (2026-10-21, updated 2026-10-29)

Not part of the course (it has no audio module): the Studio audio addition from the Architecture Starter, the same module as FinalProject. Sounds follow the sci-fi swarm direction: an orbital strike beam, land mines, turrets, chitin bugs and acid, a metal dome reactor; music is a calm synth menu theme and a dark hive / tactical battle playlist.

## How it is wired

- **Settings in one place:** `Assets/_Project/Resources/Configs/Audio/AudioConfig.asset`. For every sound it holds the id (`SoundIds`), channel (UI / Gameplay / Ambient), clip variants, volume, pitch spread and cooldown. It also holds a playlist per `AmbientIds`, channel volumes and the ambient crossfade.
- **Service:** `IAudioService` (`Play`, `Stop`, `PlayAmbient`, `SetVolume`) is a project singleton, `Resources/Utilities/AudioService.prefab` (12 one-shot sources, two crossfading ambient sources). It is created in `ProjectContextRegistrations` after the configs are loaded.
- **Music:** the scene registrations start `Menu` and `Gameplay`. The service plays the ambient's clips in the order of its `Clips` list, starting from the first one each time the ambient starts, and crossfades (2 s) into the next track before the current one ends; after the last it returns to the first. The menu has one track, the battle four. **To change the order,** reorder `Clips` of the `Gameplay` ambient in the Inspector; `AudioAuthoring` refills the list by file name (`AMB_Gameplay_1…4`), so after rerunning it either rename the files or reorder again.
- **Gameplay sounds** are systems in the entity recipes (`Gameplay/Features/AudioFeature`), each subscribed to its entity's events (curator's L8 review):

  | System | Event | Sound |
  |---|---|---|
  | `BomberSoundsSystem` | out of a burrow / orbital volley hit / killed / blew up at the dome | BugSpawn / OrbitalVolley / BugDeath / AcidBlast |
  | `SpitterSoundsSystem` | out of a burrow / orbital volley hit / spit leaves / killed | BugSpawn / OrbitalVolley / SpitterSpit / BugDeath |
  | `AcidSpitSoundsSystem` | the acid blob splashes | AcidSpitSplash |
  | `BuildingSoundsSystem` (dome) | hit / repaired / destroyed | DomeHit / DomeRepair / DomeDestroyed |
  | `MineSoundsSystem` | placed / blast | MinePlace / MineBlast |
  | `TurretSoundsSystem` | placed / shot | TurretPlace / TurretShot |
  | `PuddleSoundsSystem` | placed | PuddlePlace |
  | `ClickExplosionSoundSystem` | orbital strike | OrbitalStrike |

  A refused placement plays PlacementDenied, a new wave WaveStart. Views and prefabs have no audio code.
- **UI sounds** in presenters:
  - menu Play → UIOpen;
  - the start-wave button → UIClick;
  - the win and defeat popups → Win / Defeat when shown, UIClick on their buttons.
- **Cooldowns keep hordes listenable:** a strike that kills 30 bugs plays one death, not thirty; eight volley beams make a quick run of zaps (cooldown 0.09 s).

## Sources

`ArtSource/Audio/make_audio.py` builds every SFX clip into `Assets/_Project/Art/Audio/SFX` (48 kHz mono WAV). `ArtSource/UnityAuthoring/AudioAuthoring.cs` then sets the import (SFX Vorbis, decompress on load, mono; music streaming, stereo), the prefab and the config.

- **Music — Suno v6 through SYNTX** (2026-10-29, the user's subscription, instrumental): `Assets/_Project/Art/Audio/Ambient/AMB_Menu.mp3` and `AMB_Gameplay_1…4.mp3`, cut from the originals without re-encoding (silent heads and fade-out tails removed). Prompts, hashes and the user's choice: [SYNTX/README.md](SYNTX/README.md); the originals are in `ArtSource/Library/SYNTX` (not in Git).
- **Kenney CC0** ([kenney.nl](https://kenney.nl), Creative Commons Zero; credit not required, given here with thanks), mixed and trimmed with ffmpeg:

  | Pack | sha256 | Used for |
  |---|---|---|
  | Sci-Fi Sounds 1.0 | `119340f3…49e04` | OrbitalStrike (laserLarge + lowFrequency_explosion), OrbitalVolley (laserRetro + forceField), MineBlast (explosionCrunch), AcidBlast and the spit (slime), TurretShot (laserSmall), DomeDestroyed, the force-field layer of DomeHit |
  | Impact Sounds | `029d734a…77f8` | DomeHit (impactMetal_heavy), MinePlace (impactPlate_medium), TurretPlace, AcidSpitSplash (impactSoft) |
  | Interface Sounds | `f2193d07…1232` | UIClick, UIOpen, PlacementDenied, the beep of MinePlace, the drops of the spit and the puddle |

  The packs are unpacked in `ArtSource/Library/Kenney`, which is not in Git; full hashes are in the script header.
- **Local synthesis** (pure Python, fixed seeds; blocks from FinalProject's `sfx_synth.py`):
  - BugSpawn — dirt scratching and a chitter;
  - BugDeath — a chitin crunch with a falling squeak;
  - WaveStart — a two-tone base siren;
  - Win and Defeat stingers;
  - DomeRepair — a soft rising major arpeggio of bells.

## Verified

- 2026-10-21: all 14 first sound ids played at their events in background Play (checked through the service's last-play log, Play is muted).
- 2026-10-29: compile PASS, 0 new Console errors; the regenerated SFX are byte-identical except the three new clips; the music playlist and the new sounds — see the Implementation Report.
- Listening and levels are the user's review.
