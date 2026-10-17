using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nations.Localization
{
    [Serializable] public sealed class LocalizedEntry { public string key, en, uz, ru; }
    [Serializable] public sealed class LocalizationCatalog { public LocalizedEntry[] entries; }
    public static class Locale
    {
        public const string PreferenceKey = "Nations.Language";
        static Dictionary<string, LocalizedEntry> entries;
        static string language;
        static Font font;
        public static event Action Changed;
        public static string Language { get { EnsureLoaded(); return language; } }
        public static Font Font => font ? font : font = Resources.Load<Font>("Fonts/NotoSans-Regular");
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() { entries=null;language=null;font=null;Changed=null; }
        static void EnsureLoaded()
        {
            if(entries!=null)return;
            language=PlayerPrefs.GetString(PreferenceKey,"uz");
            if(language!="uz"&&language!="ru"&&language!="en")language="uz";
            entries=new Dictionary<string,LocalizedEntry>(1024,StringComparer.Ordinal);
            var source=Resources.Load<TextAsset>("NationsLocalization");
            if(!source){Debug.LogWarning("NATIONS: Localization catalog is missing.");return;}
            foreach(var item in JsonUtility.FromJson<LocalizationCatalog>(source.text).entries)entries[item.key]=item;
        }
        public static string Get(string key,string fallback=null)
        {
            EnsureLoaded();
            if(string.IsNullOrEmpty(key))return "";
            if(!entries.TryGetValue(key,out var item))return fallback??key;
            string value=language=="uz"?item.uz:language=="ru"?item.ru:item.en;
            return string.IsNullOrEmpty(value)?item.en??fallback??key:value;
        }
        public static void SetLanguage(string value)
        {
            EnsureLoaded();if(value!="uz"&&value!="en"&&value!="ru")return;
            language=value;PlayerPrefs.SetString(PreferenceKey,value);PlayerPrefs.Save();Changed?.Invoke();
        }
    }
}
