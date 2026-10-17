using System.IO;
using Nations.Audio;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Nations.Editor
{
    [InitializeOnLoad]
    public static class AudioPackSetup
    {
        const string Root = "Assets/_Nations/Audio/";
        static AudioPackSetup() { EditorApplication.update += Poll; }
        static void Poll()
        {
            const string path="Temp/nations-audio-command";
            if(EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(path))return;
            string command=File.ReadAllText(path).Trim();File.Delete(path);
            if(command=="seam" && !EditorApplication.isPlaying)CheckSeam();
            if(command=="setup" && !EditorApplication.isPlaying)Apply();
            if(command=="test" && EditorApplication.isPlaying)Object.FindAnyObjectByType<ExplorationAudio>().StartCoroutine(AudioPackChecks.Run());
        }
        [MenuItem("Nations/Exploration/Wire prepared audio pack")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)return;
            foreach(string file in Directory.GetFiles(Root,"*.wav",SearchOption.AllDirectories))
            {
                var importer=(AudioImporter)AssetImporter.GetAtPath(file);
                bool music=file.Contains("/Music/");
                importer.forceToMono=false;importer.loadInBackground=music;
                var settings=importer.defaultSampleSettings;
                settings.loadType=music?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat=music?AudioCompressionFormat.Vorbis:AudioCompressionFormat.PCM;
                settings.quality=0.85f;settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;
                settings.preloadAudioData=!music;
                importer.defaultSampleSettings=settings;
                importer.SetOverrideSampleSettings("Android",settings);
                importer.SetOverrideSampleSettings("iOS",settings);
                importer.SaveAndReimport();
            }
            var audio=Object.FindAnyObjectByType<ExplorationAudio>();
            Undo.RecordObject(audio,"Wire prepared audio pack");
            audio.MusicClip=Load("Music/Nations_Quiet_Horizons_LOOP_80s.wav");
            audio.TapClip=Load("SFX/01_UI_Tap.wav");audio.MenuOpenClip=Load("SFX/02_Menu_Open.wav");
            audio.MenuCloseClip=Load("SFX/03_Menu_Close.wav");audio.CountryClip=Load("SFX/04_Country_Discovery.wav");
            audio.LanguageClip=Load("SFX/05_Language_Change.wav");audio.ToggleClip=Load("SFX/06_Setting_Toggle.wav");
            audio.EarthReadyClip=Load("SFX/07_Earth_Ready.wav");audio.ConnectionClip=Load("SFX/08_Connection_Notice.wav");
            EditorUtility.SetDirty(audio);EditorSceneManager.MarkSceneDirty(audio.gameObject.scene);
            EditorSceneManager.SaveScene(audio.gameObject.scene);AssetDatabase.SaveAssets();
            File.WriteAllText("Temp/nations-audio-result","Nine clips imported and wired in existing scene.");
        }
        static void CheckSeam()
        {
            const string temporary="Assets/__NationsAudioLoopCheck.wav";
            if(File.Exists(temporary))throw new System.InvalidOperationException("Temporary validation path already exists");
            try
            {
                File.Copy(Root+"Music/Nations_Quiet_Horizons_LOOP_80s.wav",temporary);
                AssetDatabase.ImportAsset(temporary,ImportAssetOptions.ForceSynchronousImport);
                var importer=(AudioImporter)AssetImporter.GetAtPath(temporary);
                var settings=((AudioImporter)AssetImporter.GetAtPath(Root+"Music/Nations_Quiet_Horizons_LOOP_80s.wav")).GetOverrideSampleSettings("Android");
                settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.preloadAudioData=true;
                importer.defaultSampleSettings=settings;importer.forceToMono=false;importer.SaveAndReimport();
                var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(temporary);clip.LoadAudioData();
                float[] first=new float[9600],last=new float[9600];
                if(!clip.GetData(first,0)||!clip.GetData(last,clip.samples-4800))throw new System.Exception("Unable to decode loop check");
                float jump=0,adjacent=0;
                for(int c=0;c<2;c++)jump=Mathf.Max(jump,Mathf.Abs(first[c]-last[last.Length-2+c]));
                for(int i=2;i<first.Length;i++)adjacent=Mathf.Max(adjacent,Mathf.Abs(first[i]-first[i-2]),Mathf.Abs(last[i]-last[i-2]));
                File.WriteAllText("Documentation/audio-loop-check.txt",$"Decoded Vorbis quality 0.85, stereo {clip.frequency} Hz, {clip.samples} frames. Seam delta={jump:F6}; maximum neighboring sample delta={adjacent:F6}.\n"+
                    (jump<=adjacent*2+0.001f?"PASS seam jump within neighboring waveform slope tolerance. Technical check only; not a listening test.":"REVIEW seam exceeds waveform tolerance."));
            }
            finally { AssetDatabase.DeleteAsset(temporary); }
        }
        static AudioClip Load(string path)=>AssetDatabase.LoadAssetAtPath<AudioClip>(Root+path);
    }
}
