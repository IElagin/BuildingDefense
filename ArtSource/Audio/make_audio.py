"""BuildingDefense sounds (2026-10-21): Kenney CC0 clips mixed with ffmpeg plus local synthesis.

Usage: python3 ArtSource/Audio/make_audio.py
  -> Assets/_Project/Art/Audio/SFX/SFX_<SoundId>[_n].wav (48 kHz mono 16-bit)
  -> Assets/_Project/Art/Audio/Ambient/AMB_<AmbientId>.wav (seamless 32 s loops, 24 kHz mono: wind needs no more)
Kenney packs (CC0, kenney.nl, downloaded 2026-10-21) are unpacked in ArtSource/Library/Kenney (not in Git):
  sci-fi-sounds.zip    sha256 119340f351a5098ad814f78719438c0da355a9ce8a4c8a3af6a8d48aa3d49e04
  impact-sounds.zip    sha256 029d734af1582474edf3a694d1b0cebc97c1c152f2f39fa34d4c2bafc5de77f8
  interface-sounds.zip sha256 f2193d072726d6758a5f7871b2dcc54dcce0d5c35c6f0a62f92549b327c81232
Synthesis (pure Python, fixed seeds; building blocks from FinalProject ArtSource/Tools/sfx_synth.py): bug spawn and
death, wave alarm, win and defeat stingers, desert wind with a distant hive drone.
"""
import math
import random
import struct
import subprocess
import tempfile
import wave
from pathlib import Path

RATE = 48000
ROOT = Path(__file__).resolve().parents[2]
KENNEY = ROOT / "ArtSource/Library/Kenney"
SFX = ROOT / "Assets/_Project/Art/Audio/SFX"
AMBIENT = ROOT / "Assets/_Project/Art/Audio/Ambient"


# ---------- synthesis building blocks ----------

def silence(seconds):
    return [0.0] * int(seconds * RATE)


def mix(*tracks):
    out = [0.0] * max(len(t) for t in tracks)
    for t in tracks:
        for i, v in enumerate(t):
            out[i] += v
    return out


def offset(track, seconds):
    return silence(seconds) + track


def gain(track, g):
    return [v * g for v in track]


def env_exp(n, attack, decay):
    a = max(1, int(attack * RATE))
    return [i / a if i < a else math.exp(-(i - a) / (decay * RATE)) for i in range(n)]


def apply(track, envelope):
    return [v * e for v, e in zip(track, envelope)]


def sine(freq_fn, seconds):
    out, ph = [], 0.0
    for i in range(int(seconds * RATE)):
        f = freq_fn(i / RATE) if callable(freq_fn) else freq_fn
        ph += 2 * math.pi * f / RATE
        out.append(math.sin(ph))
    return out


def noise(seconds, rng):
    return [rng.uniform(-1, 1) for _ in range(int(seconds * RATE))]


def lowpass(track, cutoff_fn):
    out, y = [], 0.0
    for i, x in enumerate(track):
        fc = cutoff_fn(i / RATE) if callable(cutoff_fn) else cutoff_fn
        y += (1 - math.exp(-2 * math.pi * fc / RATE)) * (x - y)
        out.append(y)
    return out


def highpass(track, cutoff):
    return [x - l for x, l in zip(track, lowpass(track, cutoff))]


def bandpass(track, low_cut, high_cut):
    return lowpass(highpass(track, low_cut), high_cut)


def bell(freq, seconds, decay, ratios=(1.0, 2.0, 2.76, 5.4), amps=(1.0, 0.45, 0.3, 0.12)):
    n = int(seconds * RATE)
    out = [0.0] * n
    for r, a in zip(ratios, amps):
        d = decay / (1 + 0.6 * (r - 1))
        w = 2 * math.pi * freq * r / RATE
        for i in range(n):
            out[i] += a * math.sin(w * i) * math.exp(-i / RATE / d)
    return out


def finish(track, peak_db=-3.0, fade=0.004):
    n, f = len(track), int(fade * RATE)
    for i in range(min(f, n)):
        track[i] *= i / f
        track[n - 1 - i] *= i / f
    peak = max(abs(v) for v in track) or 1.0
    return [v * 10 ** (peak_db / 20) / peak for v in track]


def write_wav(path, track):
    path.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(path), "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(RATE)
        w.writeframes(b"".join(struct.pack("<h", int(max(-1.0, min(1.0, v)) * 32767)) for v in track))


def save_sfx(name, track):
    write_wav(SFX / f"{name}.wav", track)
    print(f"{name}.wav {len(track) / RATE:.2f}s (synth)")


# ---------- synthesized sounds ----------

def bug_spawn(rng):
    # dirt scratching out of the burrow, then a short chitter
    scratch = []
    for _ in range(7):
        burst = apply(bandpass(noise(0.05, rng), 900, 4500), env_exp(int(0.05 * RATE), 0.002, 0.012))
        scratch.append(offset(burst, rng.uniform(0.0, 0.32)))
    chitter_len = 0.22
    clicks = [0.5 + 0.5 * math.copysign(1, math.sin(2 * math.pi * 38 * i / RATE)) for i in range(int(chitter_len * RATE))]
    chitter = apply(sine(lambda t: 2600 + 500 * math.sin(2 * math.pi * 9 * t), chitter_len), clicks)
    chitter = offset(apply(chitter, env_exp(len(chitter), 0.01, 0.07)), 0.3)
    return finish(mix(*scratch, gain(chitter, 0.35)), -9)


def bug_death(rng):
    # chitin crunch with a short squeak falling away
    crunch = apply(bandpass(noise(0.12, rng), 700, 4200), env_exp(int(0.12 * RATE), 0.0005, 0.025))
    crack = apply(highpass(noise(0.04, rng), 3500), env_exp(int(0.04 * RATE), 0.0003, 0.006))
    f0 = rng.uniform(2200, 2900)
    squeak = apply(sine(lambda t: f0 * math.exp(-t * 9), 0.16), env_exp(int(0.16 * RATE), 0.004, 0.05))
    return finish(mix(crunch, gain(crack, 0.6), gain(squeak, 0.28)), -8)


def wave_alarm(rng):
    # base siren: three two-tone pulses, a filtered square
    out = []
    for k in range(3):
        for f in (440.0, 554.4):
            n = int(0.24 * RATE)
            tone = [math.copysign(1, math.sin(2 * math.pi * f * i / RATE)) for i in range(n)]
            tone = lowpass(tone, 1300)
            out += apply(tone, [min(1.0, i / (0.01 * RATE)) * min(1.0, (n - i) / (0.02 * RATE)) for i in range(n)])
    # a square wave is dense: a low peak keeps it level with the other cues
    return finish(out, -14)


def win(rng):
    notes = [392.0, 523.25, 659.25, 783.99, 1046.5]
    parts = [offset(bell(f, 1.6, 0.7), 0.1 * k) for k, f in enumerate(notes)]
    pad = apply(mix(sine(261.63, 1.8), gain(sine(392.0, 1.8), 0.6)), env_exp(int(1.8 * RATE), 0.25, 0.7))
    return finish(mix(*parts, gain(pad, 0.35)), -3)


def defeat(rng):
    notes = [392.0, 311.13, 261.63, 196.0]
    parts = [offset(bell(f, 1.6, 0.8, ratios=(1.0, 2.0, 3.0), amps=(1.0, 0.3, 0.15)), 0.2 * k) for k, f in enumerate(notes)]
    drone = apply(sine(lambda t: 98 - 18 * t, 2.2), env_exp(int(2.2 * RATE), 0.3, 0.9))
    return finish(mix(*parts, gain(drone, 0.5)), -4)


def wind_loop(seconds, rng, drone):
    """Desert wind: filtered noise with slow gusts; optional distant hive drone. Seamless: the tail fades into the head."""
    fade = 2.0
    n = int((seconds + fade) * RATE)
    raw = noise(seconds + fade, rng)
    gust = lambda t: 0.55 + 0.25 * math.sin(2 * math.pi * t / 7.3) + 0.2 * math.sin(2 * math.pi * t / 2.9 + 1.3)
    wind = lowpass(highpass(raw, 90), lambda t: 350 + 650 * gust(t))
    wind = [v * (0.6 + 0.4 * gust(i / RATE)) for i, v in enumerate(wind)]
    layers = [wind]
    if drone:
        hum = mix(sine(55.0, seconds + fade), gain(sine(82.4, seconds + fade), 0.5), gain(sine(110.6, seconds + fade), 0.25))
        trem = [0.6 + 0.4 * math.sin(2 * math.pi * 0.23 * i / RATE) for i in range(n)]
        layers.append(gain(apply(hum, trem), 0.18))
        for _ in range(int(seconds / 3)):
            chirp = apply(sine(lambda t, f=rng.uniform(1800, 2600): f + 300 * math.sin(2 * math.pi * 11 * t), 0.18),
                          env_exp(int(0.18 * RATE), 0.02, 0.05))
            layers.append(offset(gain(lowpass(chirp, 2200), 0.05), rng.uniform(0.0, seconds)))
    track = mix(*layers)[:n]
    body, tail = track[: int(seconds * RATE)], track[int(seconds * RATE):]
    f = len(tail)
    for i in range(f):
        k = i / f
        body[i] = body[i] * k + tail[i] * (1 - k)
    peak = max(abs(v) for v in body) or 1.0
    return [v * 10 ** (-6 / 20) / peak for v in body]


# ---------- Kenney clips ----------

def kenney(pack, name):
    return KENNEY / pack / "Audio" / f"{name}.ogg"


def ffmpeg_mix(out_name, layers, peak_db=-3.0, max_seconds=None):
    """layers: (path, delay seconds, gain). Mixed to 48 kHz mono, optionally cut with a fade-out, normalized to the peak."""
    args = ["ffmpeg", "-y", "-loglevel", "error"]
    for path, _, _ in layers:
        args += ["-i", str(path)]
    chains = []
    for k, (_, delay, g) in enumerate(layers):
        ms = int(delay * 1000)
        chains.append(f"[{k}:a]aformat=sample_rates={RATE}:channel_layouts=mono,adelay={ms}|{ms},volume={g}[a{k}]")
    inputs = "".join(f"[a{k}]" for k in range(len(layers)))
    cut = f",atrim=0:{max_seconds},afade=t=out:st={max_seconds - 0.25}:d=0.25" if max_seconds else ""
    graph = ";".join(chains) + f";{inputs}amix=inputs={len(layers)}:normalize=0{cut}[m]"
    with tempfile.TemporaryDirectory() as tmp:
        raw = Path(tmp) / "mix.wav"
        subprocess.run(args + ["-filter_complex", graph, "-map", "[m]", "-ac", "1", "-ar", str(RATE), str(raw)], check=True)
        with wave.open(str(raw)) as w:
            frames = w.readframes(w.getnframes())
        track = [s / 32768 for s in struct.unpack(f"<{len(frames) // 2}h", frames)]
    write_wav(SFX / f"{out_name}.wav", finish(track, peak_db))
    print(f"{out_name}.wav {len(track) / RATE:.2f}s (Kenney)")


def kenney_sounds():
    sci, impact, ui = "sci-fi-sounds", "impact-sounds", "interface-sounds"
    ffmpeg_mix("SFX_UIClick", [(kenney(ui, "click_001"), 0, 1)], -8)
    ffmpeg_mix("SFX_UIOpen", [(kenney(ui, "confirmation_001"), 0, 1)], -6)
    ffmpeg_mix("SFX_MineDenied", [(kenney(ui, "error_004"), 0, 1)], -8)
    ffmpeg_mix("SFX_MinePlace", [(kenney(impact, "impactPlate_medium_000"), 0, 1), (kenney(ui, "tick_002"), 0.08, 0.6)], -5)
    for k, (laser, boom) in enumerate((("laserLarge_000", "lowFrequency_explosion_000"), ("laserLarge_003", "lowFrequency_explosion_001")), 1):
        ffmpeg_mix(f"SFX_OrbitalStrike_{k}", [(kenney(sci, laser), 0, 0.8), (kenney(sci, boom), 0.06, 1)], -3)
    for k, name in enumerate(("explosionCrunch_000", "explosionCrunch_002", "explosionCrunch_004"), 1):
        ffmpeg_mix(f"SFX_MineBlast_{k}", [(kenney(sci, name), 0, 1)], -3)
    for k, name in enumerate(("slime_000", "slime_001"), 1):
        ffmpeg_mix(f"SFX_AcidBlast_{k}", [(kenney(sci, name), 0, 1)], -6, max_seconds=0.9)
    for k, name in enumerate(("impactMetal_heavy_000", "impactMetal_heavy_002", "impactMetal_heavy_004"), 1):
        ffmpeg_mix(f"SFX_DomeHit_{k}", [(kenney(impact, name), 0, 1), (kenney(sci, "forceField_001"), 0, 0.25)], -6)
    ffmpeg_mix("SFX_DomeDestroyed", [(kenney(sci, "lowFrequency_explosion_001"), 0, 1), (kenney(sci, "explosionCrunch_003"), 0.05, 0.8)], -2)
    defense_sounds()


def defense_sounds():
    """Final assignment 2026-10-29: the turret and the puddle."""
    sci, impact, ui = "sci-fi-sounds", "impact-sounds", "interface-sounds"
    ffmpeg_mix("SFX_TurretPlace", [(kenney(impact, "impactMetal_medium_001"), 0, 1), (kenney(sci, "computerNoise_000"), 0.1, 0.5)], -5, max_seconds=0.8)
    for k, name in enumerate(("laserSmall_000", "laserSmall_002", "laserSmall_004"), 1):
        ffmpeg_mix(f"SFX_TurretShot_{k}", [(kenney(sci, name), 0, 1)], -9)
    ffmpeg_mix("SFX_PuddlePlace", [(kenney(sci, "slime_001"), 0, 1), (kenney(ui, "drop_002"), 0, 0.6)], -7, max_seconds=0.7)


def main():
    for old in list(SFX.glob("SFX_*.wav")):
        old.unlink()
    kenney_sounds()
    rng = random.Random(2026)
    for k in (1, 2):
        save_sfx(f"SFX_BugSpawn_{k}", bug_spawn(rng))
    for k in (1, 2, 3):
        save_sfx(f"SFX_BugDeath_{k}", bug_death(rng))
    save_sfx("SFX_WaveStart", wave_alarm(rng))
    save_sfx("SFX_Win", win(rng))
    save_sfx("SFX_Defeat", defeat(rng))

    # ambient: synthesized at 48 kHz, stored at 24 kHz (sample-exact resampling keeps the loop seamless)
    AMBIENT.mkdir(parents=True, exist_ok=True)
    for name, drone, seed in (("AMB_Menu", False, 11), ("AMB_Gameplay", True, 12)):
        loop = wind_loop(32.0, random.Random(seed), drone)
        half = [(loop[i] + loop[i + 1]) * 0.5 for i in range(0, len(loop) - 1, 2)]
        path = AMBIENT / f"{name}.wav"
        with wave.open(str(path), "wb") as w:
            w.setnchannels(1)
            w.setsampwidth(2)
            w.setframerate(RATE // 2)
            w.writeframes(b"".join(struct.pack("<h", int(max(-1.0, min(1.0, v)) * 32767)) for v in half))
        print(f"{name}.wav 32.00s loop (synth)")


if __name__ == "__main__":
    main()
