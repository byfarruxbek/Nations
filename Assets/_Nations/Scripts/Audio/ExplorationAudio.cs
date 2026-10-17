using UnityEngine;
namespace Nations.Audio
{
    [DefaultExecutionOrder(-75), DisallowMultipleComponent]
    public sealed class ExplorationAudio : MonoBehaviour
    {
        public AudioClip MusicClip;
        public AudioClip TapClip, MenuOpenClip, MenuCloseClip, CountryClip;
        public AudioClip LanguageClip, ToggleClip, EarthReadyClip, ConnectionClip;
        public float MusicVolume { get; private set; }
        public float SfxVolume { get; private set; }
        public bool MusicMuted { get; private set; }
        public bool SfxMuted { get; private set; }
        AudioSource music, effects;
        GlobeRuntime runtime;
        float velocity, effectsVelocity;
        float nextNotice = float.NegativeInfinity;
        bool earthAnnounced, paused, resumeMusic;
        const float NoticeInterval = 60;
        void Awake()
        {
            MusicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat("Nations.Audio.Music", 0.25f));
            SfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat("Nations.Audio.Sfx", 0.55f));
            MusicMuted = PlayerPrefs.GetInt("Nations.Audio.MusicMute", 0) != 0;
            SfxMuted = PlayerPrefs.GetInt("Nations.Audio.SfxMute", 0) != 0;
            music = gameObject.AddComponent<AudioSource>();
            effects = gameObject.AddComponent<AudioSource>();
            music.playOnAwake = effects.playOnAwake = false;
            music.spatialBlend = effects.spatialBlend = 0;
            music.loop = true; effects.loop = false;
            music.volume = 0; effects.volume = SfxMuted ? 0 : SfxVolume;
            music.clip = MusicClip;
            if (MusicClip) music.Play();
            else Debug.LogWarning("NATIONS: Music clip is not assigned.");
            runtime = GetComponent<GlobeRuntime>();
        }
        void OnEnable()
        {
            if (!runtime) return;
            runtime.FirstContentRendered += EarthReady;
            runtime.ConnectionFailed += ConnectionNotice;
        }
        void OnDisable()
        {
            if (!runtime) return;
            runtime.FirstContentRendered -= EarthReady;
            runtime.ConnectionFailed -= ConnectionNotice;
        }
        void Update()
        {
            music.volume = Mathf.SmoothDamp(music.volume, MusicMuted ? 0 : MusicVolume, ref velocity, 0.8f, Mathf.Infinity, Time.unscaledDeltaTime);
            effects.volume = Mathf.SmoothDamp(effects.volume, SfxMuted ? 0 : SfxVolume, ref effectsVelocity, 0.08f, Mathf.Infinity, Time.unscaledDeltaTime);
        }
        public void SetMusic(float value) { MusicVolume = Mathf.Clamp01(value); PlayerPrefs.SetFloat("Nations.Audio.Music", MusicVolume); }
        public void SetSfx(float value) { SfxVolume = Mathf.Clamp01(value); PlayerPrefs.SetFloat("Nations.Audio.Sfx", SfxVolume); }
        public void ToggleMusic() { MusicMuted = !MusicMuted; PlayerPrefs.SetInt("Nations.Audio.MusicMute", MusicMuted ? 1 : 0); Save(); }
        public void ToggleSfx() { SfxMuted = !SfxMuted; PlayerPrefs.SetInt("Nations.Audio.SfxMute", SfxMuted ? 1 : 0); Save(); }
        public void Play(int kind = 0) => OneShot(kind == 1 ? MenuOpenClip : kind == 2 ? MenuCloseClip : kind == 3 ? CountryClip : TapClip);
        public void LanguageChanged() => OneShot(LanguageClip);
        public void SettingToggled() => OneShot(ToggleClip);
        public void EarthReady()
        {
            if (earthAnnounced) return;
            earthAnnounced = true; OneShot(EarthReadyClip);
        }
        public void ConnectionNotice()
        {
            if (Time.unscaledTime < nextNotice) return;
            nextNotice = Time.unscaledTime + NoticeInterval; OneShot(ConnectionClip);
        }
        void OneShot(AudioClip clip) { if (clip && effects && !SfxMuted && !paused) effects.PlayOneShot(clip); }
        public void Save() => PlayerPrefs.Save();
        void OnApplicationPause(bool value)
        {
            if (paused == value) return;
            paused = value;
            if (value) { Save(); resumeMusic = music && music.isPlaying; if (music) music.Pause(); if (effects) effects.Pause(); }
            else { if (resumeMusic && music) music.UnPause(); if (effects) effects.UnPause(); }
        }
        void OnDestroy()
        {
            Save();
            // Only owned sources are transient. Imported clips belong to the AssetDatabase.
            if (music) { music.Stop(); Destroy(music); }
            if (effects) { effects.Stop(); Destroy(effects); }
        }
    }
}
