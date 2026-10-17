using System;
using System.IO;
using System.Collections.Generic;
using Nations.Countries;
using Nations.Localization;
using Nations.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Nations.Editor
{
    public static class LocalizationChecks
    {
        static void Check(bool value,string message){if(!value)throw new Exception(message);}
        [MenuItem("Nations/Exploration/Check localization in Play Mode")]
        public static void Run()
        {
            var log=new System.Text.StringBuilder();string previous=Locale.Language;
            try
            {
                Check(EditorApplication.isPlaying,"Run in Play Mode");
                var ui=UnityEngine.Object.FindAnyObjectByType<ExplorationUI>();Check(ui&&ui.Countries.Data!=null,"Interface initialized");
                var root=ui.transform.Find("Exploration interface/Safe area");
                Check(root.Find("byFarrukhbek").GetComponent<Text>().text=="byFarrukhbek","Creator credit exact text");
                var catalog=JsonUtility.FromJson<LocalizationCatalog>(Resources.Load<TextAsset>("NationsLocalization").text);
                var characters=new HashSet<char>();
                foreach(var e in catalog.entries)
                {
                    Check(!string.IsNullOrEmpty(e.en)&&!string.IsNullOrEmpty(e.uz)&&!string.IsNullOrEmpty(e.ru),"Translation coverage: "+e.key);
                    foreach(char c in e.en+e.uz+e.ru)if(!char.IsWhiteSpace(c))characters.Add(c);
                }
                foreach(char c in characters)Check(Locale.Font.HasCharacter(c),"Font missing U+"+((int)c).ToString("X4"));
                log.AppendLine("PASS complete catalog values and Noto Sans glyph coverage (Uzbek Latin/Cyrillic included)");
                string[] ids={"UZ","GB","RU","JP","BR","AU","ZA"};
                double[] lon={64.5,-0.1276,37.62,139.75,-47.88,149.13,28.19};
                double[] lat={41.5,51.50,55.75,35.68,-15.79,-35.28,-25.75};
                string[][] expected={new[]{"Tashkent"},new[]{"London"},new[]{"Moscow"},new[]{"Tokyo"},new[]{"Brasília"},new[]{"Canberra"},new[]{"Pretoria","Cape Town","Bloemfontein"}};
                foreach(string language in new[]{"uz","en","ru"})
                {
                    ui.SetMenu(true);
                    root.Find("Menu panel/Viewport/Settings/Language "+language).GetComponent<Button>().onClick.Invoke();
                    Check(Locale.Language==language,"Language button applied immediately");
                    Check(root.Find("Menu").GetComponentInChildren<Text>().text==Locale.Get("menu"),"Menu translated");
                    Check(root.Find("Menu panel/Close menu"),"Compact close button exists");
                    foreach(var text in root.GetComponentsInChildren<Text>(true))
                        Check(text.name!="@title"&&text.name!="@resume"&&text.name!="@scroll","Removed menu elements stay absent");
                    root.Find("Menu panel/Close menu").GetComponent<Button>().onClick.Invoke();
                    Check(!ui.MenuOpen&&!ui.Input.InteractionBlocked,"Close restores navigation");
                    for(int i=0;i<ids.Length;i++)
                    {
                        int picked=ui.Countries.FindCountry(lon[i],lat[i]);Check(picked>=0&&ui.Countries.Data.countries[picked].id==ids[i],ids[i]+" boundary identification");
                        Check(ui.Countries.SelectCountry(ids[i]),"Select "+ids[i]);var info=CountryCatalog.Find(ids[i]);
                        Check(Resources.Load<Texture2D>(info.flag),ids[i]+" bundled flag");
                        Check(info.capitals.Length==expected[i].Length,ids[i]+" capital count");
                        for(int j=0;j<expected[i].Length;j++)
                        {
                            string actual=Array.Find(catalog.entries,e=>e.key==info.capitals[j].key).en;
                            Check(actual==expected[i][j],ids[i]+" capital value");
                        }
                        bool countryVisible=false,capitalVisible=false;
                        foreach(var text in root.Find("Selected country").GetComponentsInChildren<Text>())
                        {
                            if(text.text==Locale.Get(info.nameKey))countryVisible=true;
                            if(text.text==CountryCatalog.Capitals(info))capitalVisible=true;
                            Check(text.font==Locale.Font,"Bundled font on card");
                        }
                        Check(countryVisible&&capitalVisible,ids[i]+" card updates in "+language);
                        log.AppendLine("PASS "+language+" "+ids[i]+": "+Locale.Get(info.nameKey)+" | "+CountryCatalog.Capitals(info).Replace('\n',';')+" | "+CountryCatalog.Continents(info));
                    }
                }
                Check(CountryCatalog.Find("RU").continents.Length==2,"Russia spans Europe and Asia");
                Check(CountryCatalog.Find("ZA").capitals[2].role=="judicial","South Africa capital roles");
                log.AppendLine("PASS simplified menu in all languages, close input unlock, flags, capitals and continents");
                log.AppendLine("Portrait resolution: "+ui.Controller.View.pixelWidth+"x"+ui.Controller.View.pixelHeight+". Physical Android touch not tested.");
                ui.Countries.SelectCountry("UZ");
            }
            catch(Exception ex){log.AppendLine("FAIL "+ex.Message);}
            finally{Locale.SetLanguage(previous);}
            File.WriteAllText("Documentation/localization-checks.txt",log.ToString());
        }
    }
}
