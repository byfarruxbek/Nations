using System;
using System.IO;
using Nations.UI;
using Nations.Globe;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Unity.Mathematics;

namespace Nations.Editor
{
    [InitializeOnLoad]
    public static class ExperienceNavigationChecks
    {
        static Mouse mouse;
        static InputSettings original;
        static ExplorationUI ui;
        static int stage;
        static double began, longitude, altitude;
        static Vector2 center, pick;
        static readonly System.Text.StringBuilder results=new System.Text.StringBuilder();
        static ExperienceNavigationChecks(){EditorApplication.update+=Tick;}
        public static void Run()
        {
            if(!EditorApplication.isPlaying||mouse!=null)return;
            ui=UnityEngine.Object.FindAnyObjectByType<ExplorationUI>();ui.SetMenu(false);ui.Controller.Initialize();
            original=InputSystem.settings;InputSystem.settings=UnityEngine.Object.Instantiate(original);
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            mouse=InputSystem.AddDevice<Mouse>();stage=0;began=EditorApplication.timeSinceStartup;results.Clear();
            center=new Vector2(ui.Controller.View.pixelWidth/2f,ui.Controller.View.pixelHeight/2f);
        }
        static void State(Vector2 position,bool down,float scroll=0)
        {
            var state=new MouseState{position=position,scroll=new Vector2(0,scroll)};if(down)state=state.WithButton(MouseButton.Left);InputSystem.QueueStateEvent(mouse,state);
        }
        static void Check(bool value,string message){if(!value)throw new Exception(message);results.AppendLine("PASS "+message);}
        static void Advance(){stage++;began=EditorApplication.timeSinceStartup;}
        static void Tick()
        {
            if(mouse==null)return;
            if(!EditorApplication.isPlaying){Finish();return;}
            try
            {
                double t=EditorApplication.timeSinceStartup-began;
                if(stage==0&&t>0.3){longitude=ui.Controller.Longitude;State(center,true);Advance();}
                else if(stage==1&&t>0.1){State(center+new Vector2(100,0),true);Advance();}
                else if(stage==2&&t>0.2){State(center+new Vector2(100,0),false);Advance();}
                else if(stage==3&&t>0.5)
                {
                    Check(Math.Abs(ui.Controller.Longitude-longitude)>0.1,"Input System mouse drag orbits globe");
                    ui.Controller.StopMotion();altitude=ui.Controller.Altitude;State(center,false,120);Advance();
                }
                else if(stage==4&&t>0.8)
                {
                    Check(ui.Controller.Altitude<altitude,"Mouse wheel zoom");ui.SetMenu(true);longitude=ui.Controller.Longitude;altitude=ui.Controller.Altitude;
                    State(center,true,120);Advance();
                }
                else if(stage==5&&t>0.15){State(center+new Vector2(150,100),true,120);Advance();}
                else if(stage==6&&t>0.3)
                {
                    State(center,false);Check(Math.Abs(ui.Controller.Longitude-longitude)<0.001&&Math.Abs(ui.Controller.Altitude-altitude)<1,"Menu blocks drag and scroll from moving globe");
                    ui.SetMenu(false);ui.Controller.Initialize();Advance();
                }
                else if(stage==7&&t>0.5)
                {
                    var ecef=GlobeMath.Surface(GlobeMath.Normal(64.5,41.5));
                    pick=ui.Controller.View.WorldToScreenPoint((Vector3)(float3)ui.Controller.Georeference.TransformEarthCenteredEarthFixedPositionToUnity(ecef));
                    State(pick,true);Advance();
                }
                else if(stage==8&&t>0.1){State(pick,false);Advance();}
                else if(stage==9&&t>0.3)
                {
                    Check(ui.Countries.Selected>=0&&ui.Countries.Data.countries[ui.Countries.Selected].name=="Uzbekistan","Mouse tap selects Uzbekistan from projected WGS84 position");Finish();
                }
            }
            catch(Exception ex){results.AppendLine("FAIL "+ex.Message);Finish();}
        }
        static void Finish()
        {
            if(mouse!=null){InputSystem.RemoveDevice(mouse);mouse=null;}
            if(original){var temporary=InputSystem.settings;InputSystem.settings=original;UnityEngine.Object.DestroyImmediate(temporary);original=null;}
            if(ui)ui.SetMenu(false);
            File.WriteAllText("Documentation/experience-navigation.txt",results.ToString());
        }
    }
}
