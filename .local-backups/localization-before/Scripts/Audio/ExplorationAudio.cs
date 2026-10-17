using UnityEngine;
namespace Nations.Audio
{
    // Original deterministic synthesis: all oscillators complete integer cycles per loop.
    [DefaultExecutionOrder(-75)]
    public sealed class ExplorationAudio : MonoBehaviour
    {
        public float MusicVolume { get; private set; }
        public float SfxVolume { get; private set; }
        public bool MusicMuted { get; private set; }
        public bool SfxMuted { get; private set; }
        AudioSource music, effects;
        AudioClip pad, click, open, close, select;
        float velocity;
        const int Rate = 24000;
        void Awake()
        {
            MusicVolume = PlayerPrefs.GetFloat("Nations.Audio.Music", 0.38f);
            SfxVolume = PlayerPrefs.GetFloat("Nations.Audio.Sfx", 0.65f);
            MusicMuted = PlayerPrefs.GetInt("Nations.Audio.MusicMute", 0) != 0;
            SfxMuted = PlayerPrefs.GetInt("Nations.Audio.SfxMute", 0) != 0;
            music = gameObject.AddComponent<AudioSource>(); music.playOnAwake = false; music.loop = true; music.volume = 0;
            effects = gameObject.AddComponent<AudioSource>(); effects.playOnAwake = false;
            pad = AudioClip.Create("Nations • Quiet orbit (original)", Rate * 24, 2, Rate, true, FillMusic);
            music.clip = pad; music.Play();
            click = Tone("Soft touch", 0.07f, 740, 520);
            open = Tone("Menu arrival", 0.22f, 330, 660);
            close = Tone("Menu departure", 0.18f, 540, 270);
            select = Tone("Discovery", 0.36f, 440, 880);
        }
        long sample;
        static readonly float[] Notes = { 130.833333f, 196, 261.625f, 293.666667f, 392, 523.25f };
        void FillMusic(float[] data)
        {
            for (int i = 0; i < data.Length; i += 2)
            {
                double t = (sample++ % (Rate * 24)) / (double)Rate;
                double left = 0, right = 0;
                for (int n = 0; n < Notes.Length; n++)
                {
                    double frequency = System.Math.Round(Notes[n] * 24) / 24;
                    double envelope = 0.45 + 0.3 * System.Math.Sin(2 * System.Math.PI * t / 24 + n);
                    double wave = System.Math.Sin(2 * System.Math.PI * frequency * t) * envelope * 0.035;
                    left += wave * (n % 2 == 0 ? 1 : 0.7); right += wave * (n % 2 == 0 ? 0.7 : 1);
                }
                data[i] = (float)left; data[i + 1] = (float)right;
            }
        }
        static AudioClip Tone(string name, float duration, float from, float to)
        {
            var data = new float[(int)(Rate * duration)]; double phase = 0;
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)data.Length;
                phase += 2 * System.Math.PI * Mathf.Lerp(from, to, t) / Rate;
                data[i] = (float)System.Math.Sin(phase) * Mathf.Sin(Mathf.PI * t) * Mathf.Exp(-5 * t) * 0.25f;
            }
            var clip = AudioClip.Create(name, data.Length, 1, Rate, false); clip.SetData(data, 0); return clip;
        }
        void Update() => music.volume = Mathf.SmoothDamp(music.volume, MusicMuted ? 0 : MusicVolume, ref velocity, 0.25f, Mathf.Infinity, Time.unscaledDeltaTime);
        public void SetMusic(float value) { MusicVolume = Mathf.Clamp01(value); PlayerPrefs.SetFloat("Nations.Audio.Music", MusicVolume); }
        public void SetSfx(float value) { SfxVolume = Mathf.Clamp01(value); PlayerPrefs.SetFloat("Nations.Audio.Sfx", SfxVolume); }
        public void ToggleMusic() { MusicMuted = !MusicMuted; PlayerPrefs.SetInt("Nations.Audio.MusicMute", MusicMuted ? 1 : 0); Save(); }
        public void ToggleSfx() { SfxMuted = !SfxMuted; PlayerPrefs.SetInt("Nations.Audio.SfxMute", SfxMuted ? 1 : 0); Save(); }
        public void Play(int kind = 0) { if (!SfxMuted) effects.PlayOneShot(kind == 1 ? open : kind == 2 ? close : kind == 3 ? select : click, SfxVolume); }
        public void Save() => PlayerPrefs.Save();
        void OnApplicationPause(bool paused) { if (paused) Save(); }
        void OnDestroy() { Save(); if (music) music.Stop(); Destroy(pad); Destroy(click); Destroy(open); Destroy(close); Destroy(select); }
    }
}
