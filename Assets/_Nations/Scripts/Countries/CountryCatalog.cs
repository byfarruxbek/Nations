using System;
using System.Collections.Generic;
using Nations.Localization;
using UnityEngine;
namespace Nations.Countries
{
    [Serializable] public sealed class CapitalInfo { public string key, role; }
    [Serializable] public sealed class CountryInfo
    {
        public string id, nameKey, flag, noteKey;
        public CapitalInfo[] capitals;
        public string[] continents;
    }
    [Serializable] public sealed class CountryMetadata { public CountryInfo[] countries; }
    public static class CountryCatalog
    {
        static Dictionary<string,CountryInfo> countries;
        public static CountryInfo Find(string id)
        {
            if(countries==null)
            {
                countries=new Dictionary<string,CountryInfo>(256,StringComparer.Ordinal);
                var source=Resources.Load<TextAsset>("NationsCountryMetadata");
                if(source)foreach(var item in JsonUtility.FromJson<CountryMetadata>(source.text).countries)countries[item.id]=item;
            }
            return id!=null&&countries.TryGetValue(id,out var value)?value:null;
        }
        public static string Name(Country country) => Locale.Get("country."+country.id,country.name);
        public static string Capitals(CountryInfo info)
        {
            if(info?.capitals==null||info.capitals.Length==0)return Locale.Get("data.unavailable");
            var result=new System.Text.StringBuilder();
            for(int i=0;i<info.capitals.Length;i++)
            {
                if(i>0)result.Append("\n");
                result.Append(Locale.Get(info.capitals[i].key));
                if(!string.IsNullOrEmpty(info.capitals[i].role))result.Append(" · ").Append(Locale.Get("capital.role."+info.capitals[i].role));
            }
            return result.ToString();
        }
        public static string Continents(CountryInfo info)
        {
            if(info?.continents==null||info.continents.Length==0)return Locale.Get("data.unavailable");
            var result=new System.Text.StringBuilder();
            for(int i=0;i<info.continents.Length;i++){if(i>0)result.Append(" / ");result.Append(Locale.Get("continent."+info.continents[i]));}
            return result.ToString();
        }
    }
}
