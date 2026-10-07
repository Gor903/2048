using UnityEngine;

namespace Tilevault.Game.Services
{
    /// <summary>
    /// Sound effects are synthesised at startup rather than shipped as files —
    /// a handful of short tones cost nothing in the bundle and need no licence
    /// trail. Everything routes through <see cref="Enabled"/> so the Settings
    /// toggle is the only thing that can silence the game.
    /// </summary>
    public sealed class AudioService
    {
        const int SampleRate = 44100;

        readonly AudioSource source;
        readonly SaveService save;

        AudioClip move;
        AudioClip spawn;
        AudioClip button;
        AudioClip win;
        AudioClip gameOver;
        AudioClip[] merges;   // one per exponent, so bigger merges sound bigger

        public AudioService(AudioSource source, SaveService save)
        {
            this.source = source;
            this.save = save;
            Build();
        }

        public bool Enabled
        {
            get => save.Data.soundOn;
            set
            {
                save.Data.soundOn = value;
                save.Save();
            }
        }

        void Build()
        {
            move = Blip("move", 220f, 0.055f, 26f, 0.18f);
            spawn = Blip("spawn", 440f, 0.05f, 30f, 0.12f);
            button = Blip("button", 330f, 0.045f, 32f, 0.16f);

            merges = new AudioClip[12];
            for (int i = 0; i < merges.Length; i++)
            {
                // Rising through a pentatonic-ish ramp keeps a long merge chain musical.
                float freq = 262f * Mathf.Pow(1.1225f, i);
                merges[i] = Blip($"merge{i}", freq, 0.12f, 14f, 0.22f);
            }

            win = Arpeggio("win", new[] { 523f, 659f, 784f, 1047f }, 0.11f, 0.22f);
            gameOver = Arpeggio("over", new[] { 392f, 330f, 262f, 196f }, 0.14f, 0.2f);
        }

        /// <summary>A decaying sine with a touch of second harmonic.</summary>
        static AudioClip Blip(string name, float frequency, float seconds, float decay, float gain)
        {
            int count = Mathf.CeilToInt(SampleRate * seconds);
            var data = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                float envelope = Mathf.Exp(-decay * t);
                float wave = Mathf.Sin(2f * Mathf.PI * frequency * t)
                             + 0.3f * Mathf.Sin(4f * Mathf.PI * frequency * t);
                data[i] = wave * envelope * gain;
            }

            var clip = AudioClip.Create(name, count, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip Arpeggio(string name, float[] frequencies, float noteSeconds, float gain)
        {
            int perNote = Mathf.CeilToInt(SampleRate * noteSeconds);
            var data = new float[perNote * frequencies.Length];

            for (int n = 0; n < frequencies.Length; n++)
            for (int i = 0; i < perNote; i++)
            {
                float t = (float)i / SampleRate;
                float envelope = Mathf.Exp(-9f * t);
                data[n * perNote + i] =
                    Mathf.Sin(2f * Mathf.PI * frequencies[n] * t) * envelope * gain;
            }

            var clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        void Play(AudioClip clip, float volume = 1f)
        {
            if (!Enabled || clip == null || source == null) return;
            source.PlayOneShot(clip, volume);
        }

        public void PlayMove() => Play(move, 0.7f);
        public void PlaySpawn() => Play(spawn, 0.5f);
        public void PlayButton() => Play(button, 0.6f);
        public void PlayWin() => Play(win);
        public void PlayGameOver() => Play(gameOver);

        public void PlayMerge(int resultValue)
        {
            int index = Mathf.Clamp(Mathf.RoundToInt(Mathf.Log(resultValue, 2f)) - 2, 0, merges.Length - 1);
            Play(merges[index], 0.8f);
        }
    }
}
