"""Gera os efeitos sonoros do Fuga na Baía por síntese (sem samples de terceiros).

Uso: python3 Tools/gen_sfx.py Assets/Audio/Sfx
"""
import os
import sys
import wave

import numpy as np

RATE = 44100
rng = np.random.default_rng(7)


def t(dur):
    return np.linspace(0, dur, int(RATE * dur), endpoint=False)


def env(n, attack=0.005, decay=6.0):
    x = np.arange(n) / RATE
    a = np.clip(x / attack, 0, 1)
    return a * np.exp(-decay * x)


def lowpass(x, alpha):
    y = np.empty_like(x)
    acc = 0.0
    for i, v in enumerate(x):
        acc += alpha * (v - acc)
        y[i] = acc
    return y


def tone(freq, dur, shape="sine", decay=6.0):
    tt = t(dur)
    ph = 2 * np.pi * freq * tt
    if shape == "square":
        w = np.sign(np.sin(ph)) * 0.6
    elif shape == "triangle":
        w = 2 / np.pi * np.arcsin(np.sin(ph))
    else:
        w = np.sin(ph) + 0.3 * np.sin(2 * ph)
    return w * env(len(tt), decay=decay)


def seq(notes, gap, dur, shape, decay):
    total = gap * (len(notes) - 1) + dur
    out = np.zeros(int(RATE * total))
    for i, f in enumerate(notes):
        s = tone(f, dur, shape, decay)
        start = int(RATE * gap * i)
        out[start:start + len(s)] += s
    return out


def noise(dur):
    return rng.uniform(-1, 1, int(RATE * dur))


def cannon():
    dur = 0.7
    tt = t(dur)
    freq = 110 * np.exp(-tt * 5) + 35
    boom = np.sin(2 * np.pi * np.cumsum(freq) / RATE) * env(len(tt), decay=6)
    rumble = lowpass(noise(dur), 0.05) * env(len(tt), decay=9) * 3
    return boom + rumble


def explosion():
    dur = 0.6
    return lowpass(noise(dur), 0.12) * env(int(RATE * dur), decay=7) * 2.2


def splash():
    dur = 0.35
    n = noise(dur)
    hp = n - lowpass(n, 0.2)
    return hp * env(len(n), attack=0.01, decay=10)


def chest():
    return seq([659, 988, 1319], 0.07, 0.35, "sine", 9)


def powerup():
    return seq([523, 659, 784, 1047], 0.06, 0.22, "square", 12) * 0.6


def shield_up():
    dur = 0.45
    tt = t(dur)
    f = 400 + 500 * tt / dur + 30 * np.sin(2 * np.pi * 18 * tt)
    return np.sin(2 * np.pi * np.cumsum(f) / RATE) * env(len(tt), decay=4) * 0.7


def shield_break():
    dur = 0.5
    tt = t(dur)
    f = 1400 * np.exp(-tt * 4)
    glass = np.sin(2 * np.pi * np.cumsum(f) / RATE) * env(len(tt), decay=8) * 0.5
    n = noise(dur)
    crack = (n - lowpass(n, 0.3)) * env(len(n), decay=14)
    return glass + crack


def wave_start():
    return seq([392, 523, 659, 784], 0.11, 0.45, "triangle", 5)


def game_over():
    return seq([392, 330, 262, 196], 0.22, 0.7, "triangle", 3)


def click():
    return tone(880, 0.06, "sine", 40) * 0.6


def write(path, data):
    peak = np.max(np.abs(data)) or 1.0
    pcm = (data / peak * 0.85 * 32767).astype(np.int16)
    with wave.open(path, "wb") as f:
        f.setnchannels(1)
        f.setsampwidth(2)
        f.setframerate(RATE)
        f.writeframes(pcm.tobytes())


def main(out):
    os.makedirs(out, exist_ok=True)
    sounds = {
        "cannon": cannon(), "explosion": explosion(), "splash": splash(), "chest": chest(),
        "powerup": powerup(), "shield_up": shield_up(), "shield_break": shield_break(),
        "wave_start": wave_start(), "game_over": game_over(), "click": click(),
    }
    for name, data in sounds.items():
        write(os.path.join(out, name + ".wav"), data)
        print(f"{name}.wav {len(data) / RATE:.2f}s")


if __name__ == "__main__":
    main(sys.argv[1])
