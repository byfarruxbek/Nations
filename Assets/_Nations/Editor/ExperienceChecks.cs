using System;
using System.IO;
using Nations.Audio;
using Nations.UI;
using Nations.Globe;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

namespace Nations.Editor
{
    public static class ExperienceChecks
    {
        static void Check(bool condition,string message) { if(!condition) throw new Exception("NATIONS CHECK: "+message); }
        public static void Run()
        {
            var report=new System.Text.StringBuilder();
            try
            {
                Check(EditorApplication.isPlaying,"Must run in Play Mode");
                var ui=UnityEngine.Object.FindAnyObjectByType<ExplorationUI>();var runtime=UnityEngine.Object.FindAnyObjectByType<GlobeRuntime>();
                Check(ui && ui.Countries && ui.Countries.Data!=null,"Country data initialized");
                var countries=ui.Countries;
                Check(countries.Data.countries[countries.FindCountry(64.5,41.5)].name=="Uzbekistan","Uzbekistan geographic hit test");
                Check(countries.Data.countries[countries.FindCountry(2,47)].name=="France","France geographic hit test");
                Check(countries.FindCountry(-140,0)==-1,"Ocean is not a country");
                report.AppendLine("PASS geographic hit testing: Uzbekistan, France, Pacific Ocean");
                ui.SetMenu(true);Check(ui.MenuOpen && ui.Input.InteractionBlocked,"Modal input lock");
                var menu=ui.transform.Find("Exploration interface/Safe area/Menu").GetComponent<RectTransform>();
                Canvas.ForceUpdateCanvases();
                var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,menu.TransformPoint(menu.rect.center))};
                var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
                Check(hits.Count>0,"Menu button receives UI raycasts (run after first rendered frame)");
                ui.SetMenu(false);Check(!ui.Input.InteractionBlocked,"Resume releases input lock");
                report.AppendLine("PASS menu open/close and EventSystem hit testing");
                bool oldBorders=countries.Borders,oldLabels=countries.Labels;
                countries.SetBorders(false);countries.SetLabels(false);Check(!countries.Borders&&!countries.Labels,"Layer switches");
                countries.SetBorders(oldBorders);countries.SetLabels(oldLabels);report.AppendLine("PASS layer visibility preferences");
                var sources=ui.Audio.GetComponents<AudioSource>();Check(sources.Length==2,"Exactly two audio sources");
                Check(sources[0].isPlaying&&sources[0].loop&&sources[0].clip!=null,"Music playback");ui.Audio.Play(3);
                Check(sources[1].isPlaying,"Selection SFX playback");
                report.AppendLine("PASS music and SFX AudioSource playback (not human listening)");
                float mv=ui.Audio.MusicVolume,sv=ui.Audio.SfxVolume;ui.Audio.SetMusic(0.27f);ui.Audio.SetSfx(0.61f);ui.Audio.Save();
                Check(Mathf.Approximately(PlayerPrefs.GetFloat("Nations.Audio.Music"),0.27f),"Saved music level");
                ui.Audio.SetMusic(mv);ui.Audio.SetSfx(sv);ui.Audio.Save();report.AppendLine("PASS volume persistence storage; restart check separate");
                Check(runtime.Tileset.ionAssetID==2275207&&runtime.GoogleActive,"Working tileset binding");
                Check(runtime.Tileset.showCreditsOnScreen,"Cesium on-screen credits");Check(!runtime.SetupGlobe.activeSelf,"Setup ellipsoid disabled");
                Check(!runtime.Status.Contains("add a map key"),"No stale setup text");report.AppendLine("PASS tileset binding, credits enabled, setup disabled");
                var mesh=countries.canvasRenderer.GetMesh();report.AppendLine("Boundary mesh vertices="+mesh.vertexCount);
                report.AppendLine("Streaming ready="+runtime.Ready+" progress="+runtime.LoadProgress+" rendered tile meshes="+runtime.Tileset.GetComponentsInChildren<MeshRenderer>().Length);
                report.AppendLine("Game view="+Screen.width+"x"+Screen.height+" safe area="+Screen.safeArea);
                var originalSafe=GlobeSafeArea.Current;
                GlobeSafeArea.TestOverride=new Rect(40,60,Screen.width-80,Screen.height-130);ui.ApplyLayout();Canvas.ForceUpdateCanvases();
                var bounds=new Vector3[4];menu.GetWorldCorners(bounds);
                Check(GlobeSafeArea.Current.Contains((Vector2)bounds[0])&&GlobeSafeArea.Current.Contains((Vector2)bounds[2]),"Menu lies inside simulated notched safe area");
                GlobeSafeArea.TestOverride=null;ui.ApplyLayout();
                report.AppendLine("PASS simulated safe-area menu containment; real device cutout testing outstanding");
                report.AppendLine("Credit safe-area binding="+runtime.GetComponent<CreditSafeArea>().CreditsPositioned);
                report.AppendLine("PASS Android settings="+PlayerSettings.Android.targetArchitectures+" orientation="+PlayerSettings.defaultInterfaceOrientation);
            }
            catch(Exception ex){GlobeSafeArea.TestOverride=null;report.AppendLine("FAIL "+ex.GetType().Name+": "+ex.Message);}
            Directory.CreateDirectory("Documentation");File.WriteAllText("Documentation/experience-checks.txt",report.ToString());
        }
    }
}
