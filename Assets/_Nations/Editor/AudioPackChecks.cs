using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Nations.Audio;
using Nations.UI;
using Nations.Localization;
using UnityEngine;
namespace Nations.Editor
{
    public static class AudioPackChecks
    {
        static void Check(bool value,string message){if(!value)throw new Exception(message);}
        public static IEnumerator Run()
        {
            var a=UnityEngine.Object.FindAnyObjectByType<ExplorationAudio>();
            var ui=UnityEngine.Object.FindAnyObjectByType<ExplorationUI>();
            var sources=a.GetComponents<AudioSource>();var music=sources[0];
            var log=new System.Text.StringBuilder();
            Check(sources.Length==2,"Exactly two sources");
            Check(music.clip==a.MusicClip&&music.loop&&music.spatialBlend==0&&sources[1].spatialBlend==0,"Music and 2D sources");
            Check(a.MusicClip.channels==2&&Mathf.Abs(a.MusicClip.length-80)<0.01f,"80 second stereo music");
            foreach(var clip in new[]{a.TapClip,a.MenuOpenClip,a.MenuCloseClip,a.CountryClip,a.LanguageClip,a.ToggleClip,a.EarthReadyClip,a.ConnectionClip})Check(clip,"Missing clip");
            log.AppendLine("PASS nine clip references, 80s stereo looping music, two 2D sources");
            float start=music.time;ui.SetMenu(true);yield return new WaitForSecondsRealtime(.4f);ui.SetMenu(false);
            string language=Locale.Language;Locale.SetLanguage(language=="uz"?"en":"uz");a.LanguageChanged();
            yield return new WaitForSecondsRealtime(.4f);Locale.SetLanguage(language);
            Check(music.time>start,"Music not restarted by UI/language");
            for(int i=0;i<4;i++){a.Play(i);yield return new WaitForSecondsRealtime(.6f);}
            a.SettingToggled();yield return new WaitForSecondsRealtime(.6f);
            log.AppendLine("PASS menu/language continuity and Play(0..3)/specific hooks invoked; technical playback only");
            float mv=a.MusicVolume,sv=a.SfxVolume;bool mm=a.MusicMuted,sm=a.SfxMuted;
            if(mm)a.ToggleMusic();if(sm)a.ToggleSfx();
            a.SetMusic(.21f);a.SetSfx(.43f);yield return new WaitForSecondsRealtime(3);
            Check(Mathf.Abs(music.volume-.21f)<.01f&&Mathf.Abs(sources[1].volume-.43f)<.01f,"Independent volumes");
            a.ToggleMusic();yield return new WaitForSecondsRealtime(3);
            Check(music.volume<.01f&&sources[1].volume>.4f,"Independent music mute");
            a.ToggleMusic();a.ToggleSfx();yield return new WaitForSecondsRealtime(.5f);
            Check(sources[1].volume<.01f,"SFX mute");a.ToggleSfx();
            a.SetMusic(mv);a.SetSfx(sv);if(mm)a.ToggleMusic();if(sm)a.ToggleSfx();a.Save();
            log.AppendLine("PASS independent volume/mute controls; original preferences restored");
            // Seek near the seam to exercise the actual imported streaming decoder and loop wrap.
            music.time=78;yield return new WaitForSecondsRealtime(3);
            Check(music.isPlaying&&music.time<3,"Streaming loop wraps without stopping");
            log.AppendLine("PASS imported Vorbis streaming loop wraps and continues (not a listening assessment)");
            var runtime=a.GetComponent<GlobeRuntime>();
            Check(runtime.Ready&&runtime.LoadProgress>0,"Real Earth ready");
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            Check((bool)typeof(ExplorationAudio).GetField("earthAnnounced",flags).GetValue(a),"Earth-ready event reached audio");
            Check((bool)typeof(GlobeRuntime).GetField("contentAnnounced",flags).GetValue(runtime),"Real visible tile triggered notification");
            var notify=typeof(GlobeRuntime).GetMethod("NotifyFailure",flags);
            notify.Invoke(runtime,null);
            float deadline=(float)typeof(ExplorationAudio).GetField("nextNotice",flags).GetValue(a);
            notify.Invoke(runtime,null);a.ConnectionNotice();
            Check(deadline==(float)typeof(ExplorationAudio).GetField("nextNotice",flags).GetValue(a),"Failure transition and cooldown suppress duplicates");
            log.AppendLine("PASS first visible real-tile event reached audio; simulated failure hook/cooldown suppress duplicates; credentials unchanged");
            File.WriteAllText("Documentation/audio-pack-checks.txt",log.ToString());
        }
    }
}
