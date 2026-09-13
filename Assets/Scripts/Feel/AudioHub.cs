using UnityEngine;

namespace Veinfire
{
    public enum Sfx
    {
        Shoot,
        Hit,
        Kill,
        Hurt,
        Dash,
        Pickup,
        LevelUp,
        Ui,
        Boss,
        Death,
        Explosion
    }

    public sealed class AudioHub : MonoBehaviour
    {
        static AudioHub _i;
        AudioSource _music;
        AudioSource[] _sfx;
        int _sfxIndex;
        AudioClip[] _clips;
        float _shootGate;
        float _hitGate;
        float _killGate;
        float _pickupGate;
        float _musicMul = 0.32f;
        float _sfxMul = 1f;
        bool _ready;

        public static void Ensure()
        {
            if (_i == null)
            {
                var go = new GameObject("VeinfireAudio");
                DontDestroyOnLoad(go);
                _i = go.AddComponent<AudioHub>();
            }

            if (!_i._ready) _i.Build();
            BindListener();
            if (_i._music != null && !_i._music.isPlaying) _i._music.Play();
        }

        public static void EnsureListener()
        {
            if (_i == null) Ensure();
            else BindListener();
        }

        static void BindListener()
        {
            if (_i == null) return;
            var keep = _i.GetComponent<AudioListener>();
            if (keep == null) keep = _i.gameObject.AddComponent<AudioListener>();
            keep.enabled = true;
            var all = FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i] != keep)
                    all[i].enabled = false;
            }

            AudioListener.pause = false;
            AudioListener.volume = 1f;
        }

        void Build()
        {
            _ready = true;
            _music = gameObject.AddComponent<AudioSource>();
            Setup(_music, 0.32f);
            _music.loop = true;
            _music.clip = Synth.MusicLoop();
            _music.volume = _musicMul;
            _music.Play();

            _sfx = new AudioSource[12];
            for (var i = 0; i < _sfx.Length; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                Setup(src, 1f);
                _sfx[i] = src;
            }

            BakeClips();
        }

        static void Setup(AudioSource src, float volume)
        {
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            src.volume = volume;
            src.loop = false;
            src.mute = false;
            src.bypassEffects = true;
            src.bypassListenerEffects = true;
            src.bypassReverbZones = true;
            src.ignoreListenerPause = true;
            src.ignoreListenerVolume = true;
            src.priority = 32;
        }

        void BakeClips()
        {
            _clips = new AudioClip[11];
            _clips[(int)Sfx.Shoot] = Synth.NoiseBurst(0.06f, 2100f, 0.7f);
            _clips[(int)Sfx.Hit] = Synth.Click(1020f, 0.07f, 0.85f);
            _clips[(int)Sfx.Kill] = Synth.Thud(120f, 0.2f, 1f);
            _clips[(int)Sfx.Hurt] = Synth.Dissonant(0.22f, 0.9f);
            _clips[(int)Sfx.Dash] = Synth.Whoosh(0.16f, 0.85f);
            _clips[(int)Sfx.Pickup] = Synth.Arp(new[] { 523f, 659f, 784f }, 0.05f, 0.7f);
            _clips[(int)Sfx.LevelUp] = Synth.Arp(new[] { 392f, 523f, 659f, 784f, 1046f }, 0.07f, 0.8f);
            _clips[(int)Sfx.Ui] = Synth.Click(680f, 0.05f, 0.6f);
            _clips[(int)Sfx.Boss] = Synth.Thud(64f, 0.5f, 1f);
            _clips[(int)Sfx.Death] = Synth.Fall(320f, 70f, 0.6f, 0.95f);
            _clips[(int)Sfx.Explosion] = Synth.NoiseBurst(0.28f, 280f, 1f);
        }

        public static void Play(Sfx id, float volume = 1f)
        {
            Ensure();
            if (_i == null || _i._sfx == null || _i._clips == null) return;
            if (id == Sfx.Shoot)
            {
                if (Time.unscaledTime < _i._shootGate) return;
                _i._shootGate = Time.unscaledTime + 0.04f;
            }

            if (id == Sfx.Hit)
            {
                if (Time.unscaledTime < _i._hitGate) return;
                _i._hitGate = Time.unscaledTime + 0.028f;
            }

            if (id == Sfx.Kill)
            {
                if (Time.unscaledTime < _i._killGate) return;
                _i._killGate = Time.unscaledTime + 0.035f;
            }

            if (id == Sfx.Pickup)
            {
                if (Time.unscaledTime < _i._pickupGate) return;
                _i._pickupGate = Time.unscaledTime + 0.045f;
            }

            var clip = _i._clips[(int)id];
            if (clip == null) return;
            var src = _i._sfx[_i._sfxIndex];
            _i._sfxIndex = (_i._sfxIndex + 1) % _i._sfx.Length;
            src.Stop();
            src.clip = clip;
            src.pitch = Random.Range(0.94f, 1.06f);
            src.volume = Mathf.Clamp01(volume * Mathf.Max(0.45f, _i._sfxMul));
            src.Play();
        }

        public static void SetMix(float music, float sfx)
        {
            Ensure();
            _i._musicMul = 0.32f * Mathf.Clamp01(music);
            _i._sfxMul = Mathf.Clamp01(sfx);
            if (_i._sfxMul < 0.05f) _i._sfxMul = 1f;
            if (_i._music != null) _i._music.volume = _i._musicMul;
        }

        public static void SetMusic(bool on)
        {
            Ensure();
            if (_i._music == null) return;
            if (on && !_i._music.isPlaying) _i._music.Play();
            if (!on) _i._music.Stop();
        }
    }

    static class Synth
    {
        const int Rate = 44100;

        public static AudioClip Click(float hz, float dur, float amp)
        {
            return Render(dur, (t, n) =>
            {
                var env = Mathf.Exp(-t * 26f);
                return Mathf.Sin(2f * Mathf.PI * hz * t) * env * amp;
            });
        }

        public static AudioClip Thud(float hz, float dur, float amp)
        {
            return Render(dur, (t, n) =>
            {
                var env = Mathf.Exp(-t * 8f);
                var tone = Mathf.Sin(2f * Mathf.PI * hz * (1f - t * 0.6f) * t);
                var noise = (n - 0.5f) * 0.4f;
                return (tone + noise) * env * amp;
            });
        }

        public static AudioClip NoiseBurst(float dur, float filter, float amp)
        {
            var prev = 0f;
            return Render(dur, (t, n) =>
            {
                var env = Mathf.Exp(-t * (1f / Mathf.Max(0.02f, dur * 0.55f)));
                var raw = n * 2f - 1f;
                var a = Mathf.Clamp01(filter / 4000f);
                prev = prev + a * (raw - prev);
                return prev * env * amp;
            });
        }

        public static AudioClip Whoosh(float dur, float amp)
        {
            return Render(dur, (t, n) =>
            {
                var env = Mathf.Sin(Mathf.PI * t / dur);
                var raw = n * 2f - 1f;
                return raw * env * amp * 0.55f;
            });
        }

        public static AudioClip Dissonant(float dur, float amp)
        {
            return Render(dur, (t, n) =>
            {
                var env = Mathf.Exp(-t * 6f);
                return (Mathf.Sin(2f * Mathf.PI * 220f * t) + Mathf.Sin(2f * Mathf.PI * 233f * t)) * 0.5f * env * amp;
            });
        }

        public static AudioClip Arp(float[] notes, float step, float amp)
        {
            var dur = notes.Length * step;
            return Render(dur, (t, n) =>
            {
                var i = Mathf.Clamp(Mathf.FloorToInt(t / step), 0, notes.Length - 1);
                var local = t - i * step;
                var env = Mathf.Exp(-local * 18f);
                return Mathf.Sin(2f * Mathf.PI * notes[i] * t) * env * amp;
            });
        }

        public static AudioClip Fall(float from, float to, float dur, float amp)
        {
            return Render(dur, (t, n) =>
            {
                var hz = Mathf.Lerp(from, to, t / dur);
                var env = 1f - t / dur;
                return Mathf.Sin(2f * Mathf.PI * hz * t) * env * amp;
            });
        }

        public static AudioClip MusicLoop()
        {
            const float bpm = 96f;
            const int bars = 8;
            var beat = 60f / bpm;
            var dur = beat * 4f * bars;
            float[] bass = { 110f, 110f, 130.81f, 98f };
            float[] lead = { 220f, 261.63f, 246.94f, 196f, 220f, 174.61f, 196f, 164.81f };
            return Render(dur, (t, n) =>
            {
                var beatIndex = Mathf.FloorToInt(t / beat);
                var bar = (beatIndex / 4) % 4;
                var eighth = Mathf.FloorToInt(t / (beat * 0.5f));
                var bassHz = bass[bar];
                var leadHz = lead[eighth % lead.Length];
                var kick = beatIndex % 2 == 0 ? Mathf.Exp(-Mathf.Repeat(t, beat) * 14f) * Mathf.Sin(2f * Mathf.PI * 70f * t) : 0f;
                var hat = (eighth % 2 == 1) ? (n - 0.5f) * Mathf.Exp(-Mathf.Repeat(t, beat * 0.5f) * 40f) * 0.28f : 0f;
                var bassTone = Mathf.Sin(2f * Mathf.PI * bassHz * t) * 0.28f;
                var pad = Mathf.Sin(2f * Mathf.PI * bassHz * 2f * t) * 0.07f;
                var melody = Mathf.Sin(2f * Mathf.PI * leadHz * t) * 0.12f * (0.6f + 0.4f * Mathf.Sin(t * 0.7f));
                return (kick * 0.65f + hat + bassTone + pad + melody) * 0.95f;
            });
        }

        static AudioClip Render(float duration, System.Func<float, float, float> sample)
        {
            var count = Mathf.Max(256, Mathf.CeilToInt(duration * Rate));
            var data = new float[count];
            var rng = new System.Random(17);
            for (var i = 0; i < count; i++)
            {
                var t = i / (float)Rate;
                data[i] = Mathf.Clamp(sample(t, (float)rng.NextDouble()), -1f, 1f);
            }

            var clip = AudioClip.Create("vf", count, 1, Rate, false);
            clip.SetData(data, 0);
            clip.LoadAudioData();
            return clip;
        }
    }
}
