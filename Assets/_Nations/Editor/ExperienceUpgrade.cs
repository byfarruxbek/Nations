using System.IO;
using CesiumForUnity;
using Nations.Audio;
using Nations.Input;
using Nations.Navigation;
using Nations.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Nations.Editor
{
    [InitializeOnLoad]
    public static class ExperienceUpgrade
    {
        static ExperienceUpgrade() { EditorApplication.update += Poll; }
        static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            const string request = "Temp/nations-experience-command";
            if (!File.Exists(request)) return;
            string command=File.ReadAllText(request).Trim();File.Delete(request);
            if(command=="upgrade" && !EditorApplication.isPlaying) Apply();
            if(command=="play") { Application.runInBackground=true; EditorApplication.isPlaying=true; }
            if(command=="stop") EditorApplication.isPlaying=false;
            if(command=="report") Report();
            if(command=="test") Object.FindAnyObjectByType<ExplorationUI>().StartCoroutine(RuntimeChecks());
            if(command=="refresh") AssetDatabase.Refresh();
            if(command=="navigation") ExperienceNavigationChecks.Run();
            if(command=="localization") LocalizationChecks.Run();
            if(command=="language-write")
            {
                SessionState.SetString("Nations.Test.Language",Nations.Localization.Locale.Language);
                Nations.Localization.Locale.SetLanguage("ru");
            }
            if(command=="language-read")
            {
                bool passed=Nations.Localization.Locale.Language=="ru";
                File.WriteAllText("Documentation/localization-persistence.txt",passed?"PASS Russian language restored after Play Mode restart.":"FAIL language persistence");
                Nations.Localization.Locale.SetLanguage(SessionState.GetString("Nations.Test.Language","uz"));
            }
            if(command=="prefs-write")
            {
                var audio=Object.FindAnyObjectByType<ExplorationAudio>();
                SessionState.SetFloat("Nations.Test.Music",audio.MusicVolume);SessionState.SetFloat("Nations.Test.Sfx",audio.SfxVolume);
                audio.SetMusic(0.23f);audio.SetSfx(0.57f);audio.Save();
            }
            if(command=="prefs-read")
            {
                var audio=Object.FindAnyObjectByType<ExplorationAudio>();
                bool valid=Mathf.Approximately(audio.MusicVolume,0.23f)&&Mathf.Approximately(audio.SfxVolume,0.57f);
                File.WriteAllText("Documentation/experience-persistence.txt",valid?"PASS: separate music and SFX levels restored after Play Mode restart.":"FAIL: audio restart persistence");
                audio.SetMusic(SessionState.GetFloat("Nations.Test.Music",0.38f));audio.SetSfx(SessionState.GetFloat("Nations.Test.Sfx",0.65f));audio.Save();
            }
        }
        static System.Collections.IEnumerator RuntimeChecks()
        {
            yield return null;
            ExperienceChecks.Run();
        }
        [MenuItem("Nations/Exploration/Upgrade existing Globe")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying) return;
            var runtime=Object.FindAnyObjectByType<GlobeRuntime>();
            if(!runtime) throw new System.Exception("Open the existing Globe scene first.");
            Undo.RecordObject(runtime,"Integrate existing tileset");
            foreach(var tile in Object.FindObjectsByType<Cesium3DTileset>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))
                if(tile.tilesetSource==CesiumDataSource.FromCesiumIon && tile.ionAssetID==2275207)
                {runtime.Tileset=tile;Undo.RecordObject(tile,"Preserve on-screen credits");tile.showCreditsOnScreen=true;}
            if(runtime.SetupGlobe){Undo.RecordObject(runtime.SetupGlobe,"Disable overlapping setup globe");runtime.SetupGlobe.SetActive(false);}
            var audio=runtime.GetComponent<ExplorationAudio>();if(!audio)audio=Undo.AddComponent<ExplorationAudio>(runtime.gameObject);
            var ui=runtime.GetComponent<ExplorationUI>();if(!ui)ui=Undo.AddComponent<ExplorationUI>(runtime.gameObject);
            ui.Audio=audio;ui.Controller=Object.FindAnyObjectByType<GlobeCameraController>();ui.Input=Object.FindAnyObjectByType<GlobeInputManager>();
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait=true;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
            PlayerSettings.allowedAutorotateToLandscapeLeft=true;PlayerSettings.allowedAutorotateToLandscapeRight=true;
            EditorSceneManager.MarkSceneDirty(runtime.gameObject.scene);EditorSceneManager.SaveScene(runtime.gameObject.scene);AssetDatabase.SaveAssets();
            File.WriteAllText("Temp/nations-upgrade-result","Existing scene upgraded; working ion tileset bound; credentials unchanged.");
        }
        static void Report()
        {
            var runtime=Object.FindAnyObjectByType<GlobeRuntime>();var ui=Object.FindAnyObjectByType<ExplorationUI>();
            File.WriteAllText("Temp/nations-experience-report",$"Playing={EditorApplication.isPlaying}\nTime={Time.unscaledTime}\nFocused={Application.isFocused}\nRuntime={runtime!=null}\nGoogle={runtime && runtime.GoogleActive}\nReady={runtime && runtime.Ready}\nProgress={(runtime?runtime.LoadProgress:0)}\nActiveAsset={(runtime&&runtime.Tileset?runtime.Tileset.ionAssetID:0)}\nUI={ui!=null}\nCountries={(ui&&ui.Countries&&ui.Countries.Data!=null?ui.Countries.Data.countries.Length:0)}\nAudioSources={Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Length}\n");
        }
    }
}
