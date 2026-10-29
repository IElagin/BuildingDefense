# Music from SYNTX (Suno)

Generated 2026-10-29 by the agent in the user's SYNTX web account (syntx.ai → Audio → Suno), the existing subscription: model Suno, version 6, Simple mode, Instrumental mode on. Each request costs 10 tokens and returns two alternatives. Originals (MP3, 48 kHz stereo, ~185 kbit/s, downloaded from the result URLs on `r2.syntx.ai`) are kept byte for byte in `ArtSource/Library/SYNTX`, which is not in Git: masters stay out of the public repository (Studio Audio Content Pipeline); the game ships only the cuts in `Assets/_Project/Art/Audio/Ambient`. The offer allows commercial use of generated material (Studio [SYNTX](../../../../Studio/Systems/UnityProduction/knowledge/40-Tooling/SYNTX.md)).

| File | Request | Duration | SHA-256 | Choice |
|---|---|---|---|---|
| `menu_v1.mp3` | menu | 3:33.6 | `b0c75fdb404e145fa8b461523c8bfa92a977ee382115b381957fa421d22c6d27` | **menu** (user, 2026-10-29) → `AMB_Menu.mp3`, first 209 s, stream copy (the last 4 s are the fade-out) |
| `menu_v2.mp3` | menu | 3:24.0 | `27aa78dda9f9c523e65e2704f690ca11ed090fe37c1eac19cda0b68c9b98c697` | — |
| `battle_v1.mp3` | battle 1 | 2:28.0 | `11d759c9d40ca32cce5dccb4d08a42222fd5329bfc299ab0ddf9d675cd4f1a1d` | rejected: too energetic for a strategy game |
| `battle_v2.mp3` | battle 1 | 2:48.8 | `0fd7f44271bacddc10c443a30c075e8363ec48be09b5db2bbbf16b048145de7d` | rejected: same |
| `hive_v1.mp3` | battle 2 (hive) | 2:19.9 | `911873fff445b0d4998e80aed5f66356e9f1ad2488aacebb74b675239fbd8cd2` | **battle 1st** → `AMB_Gameplay_1.mp3`, without the last 1 s |
| `tactical_v1.mp3` | battle 3 (tactical) | 2:32.8 | `21e228bd14a56250d94935e662f9718836c91d23f5d057f3df31c2b63a6805bd` | **battle 2nd** → `AMB_Gameplay_2.mp3`, without the last 2 s (the near-silent first 1.5 s stays: cutting the head with stream copy breaks the MP3 length header) |
| `hive_v2.mp3` | battle 2 (hive) | 2:10.0 | `d4b5a6372ad86439d367b1c53c6c8a0691c3cf390604e44ec96e3e80da09898a` | **battle 3rd** → `AMB_Gameplay_3.mp3`, without the last 2 s |
| `tactical_v2.mp3` | battle 3 (tactical) | 2:44.0 | `81b83b4979dfbe8746c83d30db6da745dc58eba85c49d091283a1ff4c3457b6a` | **battle 4th** → `AMB_Gameplay_4.mp3`, without the last 2 s (the near-silent first 1.5 s stays: cutting the head with stream copy breaks the MP3 length header) |

The user (2026-10-29) liked all four hive / tactical tracks and asked for them all in the battle, played in turn, with the order set in the config; the default order alternates hive and tactical. Cuts are `ffmpeg -ss … -to … -c copy` (no re-encoding). Spent: 40 tokens (705.3 → 665.3).

## Prompts

**menu:** Calm atmospheric sci-fi ambient for a game main menu. Warm analog synth pads, soft slow arpeggio, gentle sub bass, light shimmering bells, hopeful and cozy mood, a small domed outpost on an alien plateau at dusk. Slow tempo around 80 BPM, steady even dynamics with no big drops or climaxes, smooth loopable ending. Instrumental only, no vocals.

**battle 1:** Driving sci-fi action music for a top-down base defense game, waves of alien bugs attacking a glowing dome. Pulsing synth bass ostinato, punchy electronic drums, tense staccato synth strings, bright arpeggiated lead motif, focused and energetic but not harsh. Tempo around 120 BPM, steady intensity, mid-range left clear for sound effects, smooth loopable ending. Instrumental only, no vocals.

**battle 2 (hive):** Dark atmospheric underscore for a slow strategy game: a lone dome outpost besieged by a swarm of alien insects. Low droning synth pads, deep slow pulse, eerie organic textures like chittering, clicking mandibles, rustling wings and distant hive hum, sparse muted percussion, minimal haunting melody. Tense and ominous but calm, no heavy drums, no drops, steady low intensity that sits under sound effects, around 85 BPM, loopable. Instrumental only, no vocals.

**battle 3 (tactical):** Tense tactical background music for a base defense strategy game against waves of giant alien bugs. Restrained organic percussion like wooden clicks, shakers and low taiko pulses, insectoid rattles and buzzing textures, dark cello and synth drone, a simple repeating motif that builds slight suspense. Mid-low energy, focused and thoughtful, not epic, not aggressive, no rock or EDM drums, around 95 BPM, loopable. Instrumental only, no vocals.
