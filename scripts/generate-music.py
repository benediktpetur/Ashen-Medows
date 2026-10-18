"""Rebuild Ashen Meadow's original synthesized score. Requires numpy; no samples used."""
from pathlib import Path
import wave
import numpy as np

RATE = 22050
SECONDS = 32
OUT = Path(__file__).resolve().parents[1] / 'game/Audio/Music'
OUT.mkdir(parents=True, exist_ok=True)

def render(name, root, dark=0, pulse=0, warmth=0):
    size = RATE * SECONDS
    audio = np.zeros((size, 2), dtype=np.float64)
    rng = np.random.default_rng(sum(map(ord, name)))
    def note(start, duration, midi, gain, pluck=False, pan=0):
        offset = int(start * RATE)
        length = min(int(duration * RATE), size-offset)
        t = np.arange(length) / RATE
        hz = 440 * 2 ** ((midi-69)/12)
        env = (1-np.exp(-t*(45 if pluck else 1.8))) * np.exp(-t*(2 if pluck else .12))
        env *= np.minimum(1, (duration-t)/(.3 if pluck else 1.8)).clip(0,1)
        sound = (np.sin(2*np.pi*hz*t) + .22*np.sin(2*np.pi*hz*2.002*t) + .08*np.sin(2*np.pi*hz*3*t)) * env * gain
        audio[offset:offset+length,0] += sound * (.7-pan*.25)
        audio[offset:offset+length,1] += sound * (.7+pan*.25)
    # Eight-bar variations of the same home motif; restrained minor harmony.
    for bar, change in enumerate([0,-3,5,-5,0,5,-3,-5]):
        for interval in [0, 3 if dark else 7, 12]:
            note(bar*4, 7.5, root+change+interval, .055, pan=rng.uniform(-1,1))
        if warmth or bar%2 == 0:
            for j, step in enumerate([12,19,15,14] if dark else [12,19,16,14]):
                note(bar*4+j*.8, 2.5, root+change+step, .08 if warmth else .035, True, (-1)**j*.5)
        if pulse:
            for j in range(8):
                note(bar*4+j*.5, .45, root-12+(j%4==3)*7, .055*pulse, True)
    # Soft stereo echoes, with a tail fade so looping never clicks.
    for delay, gain in [(.23,.18),(.47,.12),(.79,.08)]:
        n=int(delay*RATE)
        audio[n:] += audio[:-n,::-1].copy()*gain
    fade=np.minimum(np.arange(size)/RATE/1.5, (size-1-np.arange(size))/RATE/2).clip(0,1)
    audio *= fade[:,None]
    peak=np.max(np.abs(audio))
    audio *= .65/max(.65,peak)
    with wave.open(str(OUT/(name+'.wav')), 'wb') as f:
        f.setnchannels(2); f.setsampwidth(2); f.setframerate(RATE)
        f.writeframes((audio*32767).astype('<i2').tobytes())
    # Imported streams loop in both editor and exported builds.
    if not (OUT/(name+'.wav.import')).exists(): (OUT/(name+'.wav.import')).write_text('''[remap]
importer="wav"
type="AudioStreamWAV"
uid=""
path=""

[deps]
source_file="res://Audio/Music/'''+name+'''.wav"

[params]
force/8_bit=false
force/mono=false
force/max_rate=false
edit/trim=false
edit/normalize=false
edit/loop_mode=2
edit/loop_begin=0
edit/loop_end=-1
compress/mode=2
''')
    print(name, round(peak,3))

for spec in [('meadow',45,0,0,1),('woodland',43,1,0,0),('outskirts',40,1,.5,0),
             ('city',38,1,.7,0),('depths',33,1,.4,0),('home',48,0,0,1),
             ('night',36,1,0,0),('combat',40,1,1.5,0),('foreman',36,1,1.7,0),
             ('wrecker',38,1,2,0),('bellkeeper',41,1,1.8,0),('broodheart',33,1,2,0),
             ('mourner',42,1,2.3,0),('colossus',31,1,1.2,0),('matriarch',35,1,2.5,0)]:
    render(*spec)
