using Nations.Audio;
using Nations.Localization;
using System.Collections.Generic;
using Nations.Countries;
using Nations.Input;
using Nations.Navigation;
using UnityEngine;
using UnityEngine.UI;

namespace Nations.UI
{
    [DefaultExecutionOrder(-50)]
    public sealed class ExplorationUI : MonoBehaviour
    {
        public GlobeCameraController Controller;
        public GlobeInputManager Input;
        public ExplorationAudio Audio;
        public CountryLayer Countries { get; private set; }
        public bool MenuOpen { get; private set; }
        RectTransform safe, panel, selection, content;
        GameObject shade;
        Text selectedName, capitalHeading, capitalNames, continentNames, countryNote, missingFlag;
        Text musicMute, sfxMute, borders, labels;
        RawImage flag;
        readonly Dictionary<Text,string> localized = new Dictionary<Text,string>();
        readonly Dictionary<string,Texture2D> flags = new Dictionary<string,Texture2D>();
        readonly List<UnityEngine.UI.Image> languageButtons = new List<UnityEngine.UI.Image>();
        Canvas canvas;
        readonly Color ink = new Color(0.035f, 0.065f, 0.10f, 0.96f);
        readonly Color accent = new Color(0.48f, 0.93f, 0.79f);
        void Awake()
        {
            var root = new GameObject("Exploration interface", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false); canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 2;
            var countryRect = Rect("Country layer", root.transform); Stretch(countryRect);
            Countries = countryRect.gameObject.AddComponent<CountryLayer>(); Countries.Controller = Controller; Countries.Input = Input;
            safe = Rect("Safe area", root.transform); Stretch(safe);
            Label("NATIONS", safe, new Vector2(16,-12), new Vector2(200,34), 18, TextAnchor.MiddleLeft);
            Label("byFarrukhbek", safe, new Vector2(17,-46), new Vector2(200,22), 11, TextAnchor.MiddleLeft).color = new Color(0.59f,0.71f,0.77f);
            var menu = Button("@menu", safe, new Vector2(-16,-12), new Vector2(76,48), () => SetMenu(!MenuOpen));
            menu.name="Menu"; menu.anchorMin = menu.anchorMax = menu.pivot = new Vector2(1,1);
            selection = Rect("Selected country", safe); selection.anchorMin = selection.anchorMax = new Vector2(0.5f,0); selection.pivot = new Vector2(0.5f,0);
            selection.anchoredPosition = new Vector2(0,110); selection.sizeDelta = new Vector2(360,190); Image(selection, ink);
            var flagRect=Rect("Country flag",selection);flagRect.anchoredPosition=new Vector2(16,-18);flagRect.sizeDelta=new Vector2(72,54);flag=flagRect.gameObject.AddComponent<RawImage>();flag.raycastTarget=false;
            missingFlag=Label("",selection,new Vector2(16,-18),new Vector2(72,54),10,TextAnchor.MiddleCenter);
            selectedName = Label("", selection, new Vector2(100,-12), new Vector2(244,62), 21, TextAnchor.MiddleLeft);
            selectedName.resizeTextForBestFit=true;selectedName.resizeTextMinSize=15;selectedName.resizeTextMaxSize=21;
            capitalHeading=Label("",selection,new Vector2(16,-80),new Vector2(328,22),11,TextAnchor.MiddleLeft);capitalHeading.color=accent;
            capitalNames=Label("",selection,new Vector2(16,-103),new Vector2(328,66),14,TextAnchor.UpperLeft);
            continentNames=Label("",selection,new Vector2(16,-169),new Vector2(328,26),13,TextAnchor.MiddleLeft);
            countryNote=Label("",selection,new Vector2(16,-198),new Vector2(328,36),10,TextAnchor.UpperLeft);countryNote.color=new Color(0.63f,0.73f,0.78f);
            selection.gameObject.SetActive(false);
            var blocker = Rect("Menu backdrop", safe); Stretch(blocker); blocker.offsetMin = new Vector2(0,100);
            Image(blocker, new Color(0,0,0,0.3f)); blocker.gameObject.AddComponent<Button>().onClick.AddListener(() => SetMenu(false)); shade = blocker.gameObject;
            panel = Rect("Menu panel", safe); panel.anchorMin = panel.anchorMax = panel.pivot = Vector2.one; panel.anchoredPosition = new Vector2(-16,-82); Image(panel, ink);
            var viewport = Rect("Viewport", panel); Stretch(viewport); viewport.gameObject.AddComponent<RectMask2D>(); viewport.offsetMax=new Vector2(0,-56);
            content = Rect("Settings", viewport); content.anchorMin = new Vector2(0,1); content.anchorMax = Vector2.one; content.pivot = new Vector2(0.5f,1); content.sizeDelta = new Vector2(0,422);
            var scroll = panel.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped;
            Label("@language",panel,new Vector2(20,-16),new Vector2(230,24),12,TextAnchor.MiddleLeft).color=accent;
            var close=Button("×",panel,new Vector2(-8,-6),new Vector2(44,44),()=>SetMenu(false));
            close.name="Close menu";close.anchorMin=close.anchorMax=close.pivot=Vector2.one;
            var closeText=close.GetComponentInChildren<Text>();closeText.resizeTextForBestFit=false;closeText.fontSize=26;
            string[] languages={"uz","en","ru"};string[] names={"O‘zbekcha","English","Русский"};
            for(int i=0;i<3;i++)
            {
                string code=languages[i];var button=Button(names[i],content,new Vector2(16+i*99,0),new Vector2(94,44),()=>Locale.SetLanguage(code));
                button.name="Language "+code;languageButtons.Add(button.GetComponent<UnityEngine.UI.Image>());
            }
            Label("@audio", content, new Vector2(20,-58), new Vector2(280,24), 11, TextAnchor.MiddleLeft).color = accent;
            musicMute = ButtonLabel("", content, new Vector2(16,-86), () => { Audio.ToggleMusic(); Refresh(); });
            Slider(content, -134, Audio.MusicVolume, Audio.SetMusic);
            sfxMute = ButtonLabel("", content, new Vector2(16,-182), () => { Audio.ToggleSfx(); Audio.Play(); Refresh(); });
            Slider(content, -230, Audio.SfxVolume, Audio.SetSfx);
            Label("@layers", content, new Vector2(20,-280), new Vector2(280,24), 11, TextAnchor.MiddleLeft).color = accent;
            borders = ButtonLabel("", content, new Vector2(16,-308), () => { Countries.SetBorders(!Countries.Borders); Refresh(); });
            labels = ButtonLabel("", content, new Vector2(16,-362), () => { Countries.SetLabels(!Countries.Labels); Refresh(); });
            shade.SetActive(false); panel.gameObject.SetActive(false);
            Countries.SelectionChanged += OnSelection;
            Locale.Changed += Refresh;
        }
        void Start() => Refresh();
        void Update() => ApplyLayout();
        public void ApplyLayout()
        {
            var area = GlobeSafeArea.Current;
            canvas.scaleFactor = Mathf.Max(0.5f, Mathf.Min(area.width / 430, area.height / 430));
            safe.anchorMin = new Vector2(area.xMin/Screen.width,area.yMin/Screen.height); safe.anchorMax = new Vector2(area.xMax/Screen.width,area.yMax/Screen.height);
            panel.sizeDelta = new Vector2(Mathf.Min(324,safe.rect.width-32),Mathf.Max(140,Mathf.Min(478,safe.rect.height-190)));
        }
        public void SetMenu(bool open)
        {
            MenuOpen = open; Input.InteractionBlocked = open; Controller.StopMotion();
            shade.SetActive(open); panel.gameObject.SetActive(open); selection.gameObject.SetActive(!open && !string.IsNullOrEmpty(selectedName.text));
            Audio.Play(open ? 1 : 2); if (!open) Audio.Save(); Refresh();
        }
        void OnSelection(string name) { RefreshCard(); if(name.Length>0)Audio.Play(3); }
        public void RefreshCard()
        {
            bool selected=Countries.Data!=null&&Countries.Selected>=0;
            selection.gameObject.SetActive(selected&&!MenuOpen);
            if(!selected){selectedName.text="";return;}
            var country=Countries.Data.countries[Countries.Selected];var info=CountryCatalog.Find(country.id);
            selectedName.text=CountryCatalog.Name(country);
            capitalHeading.text=Locale.Get(info?.capitals!=null&&info.capitals.Length>1?"capitals":"capital");
            capitalNames.text=CountryCatalog.Capitals(info);
            capitalNames.rectTransform.sizeDelta=new Vector2(328,Mathf.Max(26,capitalNames.preferredHeight+4));
            float next=103+capitalNames.rectTransform.sizeDelta.y;
            continentNames.text=Locale.Get("continent")+": "+CountryCatalog.Continents(info);
            continentNames.rectTransform.anchoredPosition=new Vector2(16,-next);
            continentNames.rectTransform.sizeDelta=new Vector2(328,Mathf.Max(26,continentNames.preferredHeight+4));
            next+=continentNames.rectTransform.sizeDelta.y;
            countryNote.text=Locale.Get(info?.noteKey);countryNote.rectTransform.anchoredPosition=new Vector2(16,-next);
            countryNote.rectTransform.sizeDelta=new Vector2(328,string.IsNullOrEmpty(countryNote.text)?0:countryNote.preferredHeight+6);
            selection.sizeDelta=new Vector2(360,next+countryNote.rectTransform.sizeDelta.y+14);
            Texture2D texture=null;
            if(!string.IsNullOrEmpty(info?.flag))
            {
                if(!flags.TryGetValue(info.flag,out texture)){texture=Resources.Load<Texture2D>(info.flag);flags[info.flag]=texture;}
            }
            flag.texture=texture;flag.gameObject.SetActive(texture);missingFlag.gameObject.SetActive(!texture);missingFlag.text=Locale.Get("flag.unavailable");
        }
        void Refresh()
        {
            foreach(var pair in localized)if(pair.Key)pair.Key.text=Locale.Get(pair.Value);
            musicMute.text = Locale.Get("music")+"  ·  "+Locale.Get(Audio.MusicMuted?"off":"on");
            sfxMute.text = Locale.Get("sfx")+"  ·  "+Locale.Get(Audio.SfxMuted?"off":"on");
            borders.text = Locale.Get("borders")+"  ·  "+Locale.Get(Countries.Borders?"on":"off");
            labels.text = Locale.Get("labels")+"  ·  "+Locale.Get(Countries.Labels?"on":"off");
            string[] codes={"uz","en","ru"};
            for(int i=0;i<languageButtons.Count;i++)languageButtons[i].color=Locale.Language==codes[i]?new Color(0.15f,0.42f,0.36f):new Color(0.11f,0.18f,0.23f);
            RefreshCard();
        }
        static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name,typeof(RectTransform)); var rt = go.GetComponent<RectTransform>(); rt.SetParent(parent,false); rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(0,1); return rt;
        }
        static void Stretch(RectTransform rt) { rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.pivot=new Vector2(0.5f,0.5f);rt.offsetMin=rt.offsetMax=Vector2.zero; }
        static Sprite rounded;
        static Image Image(RectTransform rt, Color color)
        {
            if (!rounded)
            {
                var texture=new Texture2D(32,32,TextureFormat.RGBA32,false); texture.name="Nations rounded panel"; texture.wrapMode=TextureWrapMode.Clamp;
                var pixels=new Color32[1024];
                for(int y=0;y<32;y++) for(int x=0;x<32;x++)
                {
                    float dx=Mathf.Max(8-x,x-23),dy=Mathf.Max(8-y,y-23);
                    float distance=new Vector2(Mathf.Max(0,dx),Mathf.Max(0,dy)).magnitude;
                    pixels[y*32+x]=new Color(1,1,1,Mathf.Clamp01(8-distance));
                }
                texture.SetPixels32(pixels);texture.Apply(false,true);
                rounded=Sprite.Create(texture,new Rect(0,0,32,32),new Vector2(0.5f,0.5f),100,0,SpriteMeshType.FullRect,new Vector4(10,10,10,10));
            }
            var image=rt.gameObject.AddComponent<Image>();image.sprite=rounded;image.type=UnityEngine.UI.Image.Type.Sliced;image.color=color;return image;
        }
        Text Label(string text, Transform parent, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
        {
            var rt=Rect(text,parent);rt.anchoredPosition=position;rt.sizeDelta=size;
            var label=rt.gameObject.AddComponent<Text>();label.font=Locale.Font;label.text=text.StartsWith("@")?Locale.Get(text.Substring(1)):text; if(text.StartsWith("@"))localized[label]=text.Substring(1);label.fontSize=fontSize;label.color=new Color(0.9f,0.95f,1);label.alignment=alignment;label.raycastTarget=false;return label;
        }
        RectTransform Button(string text,Transform parent,Vector2 position,Vector2 size,UnityEngine.Events.UnityAction action)
        {
            var rt=Rect(text,parent);rt.anchoredPosition=position;rt.sizeDelta=size;var image=Image(rt,new Color(0.11f,0.18f,0.23f,1));
            var button=rt.gameObject.AddComponent<Button>();button.targetGraphic=image;button.onClick.AddListener(()=>{Audio.Play();action();});
            var label=Label(text,rt,Vector2.zero,size,15,TextAnchor.MiddleCenter);Stretch(label.rectTransform);label.rectTransform.offsetMin=new Vector2(8,3);label.rectTransform.offsetMax=new Vector2(-8,-3);label.resizeTextForBestFit=true;label.resizeTextMinSize=11;label.resizeTextMaxSize=15;return rt;
        }
        Text ButtonLabel(string text,Transform parent,Vector2 position,UnityEngine.Events.UnityAction action) => Button(text,parent,position,new Vector2(292,44),action).GetComponentInChildren<Text>();
        void Slider(Transform parent,float y,float value,UnityEngine.Events.UnityAction<float> changed)
        {
            var rt=Rect("Volume",parent);rt.anchoredPosition=new Vector2(24,y);rt.sizeDelta=new Vector2(276,44);Image(rt,new Color(0,0,0,0.01f));
            var track=Rect("Track",rt);track.anchorMin=new Vector2(0,0.5f);track.anchorMax=new Vector2(1,0.5f);track.pivot=new Vector2(0.5f,0.5f);track.sizeDelta=new Vector2(-16,4);Image(track,new Color(0.2f,0.3f,0.35f));
            var fill=Rect("Fill",track);Stretch(fill);Image(fill,accent);
            var handleArea=Rect("Handle area",rt);handleArea.anchorMin=new Vector2(0,0.5f);handleArea.anchorMax=new Vector2(1,0.5f);handleArea.pivot=new Vector2(0.5f,0.5f);handleArea.sizeDelta=new Vector2(-24,28);
            var handle=Rect("Handle",handleArea);handle.sizeDelta=new Vector2(24,0);handle.pivot=new Vector2(0.5f,0.5f);var image=Image(handle,accent);
            var slider=rt.gameObject.AddComponent<Slider>();slider.fillRect=fill;slider.handleRect=handle;slider.targetGraphic=image;slider.minValue=0;slider.maxValue=1;slider.value=value;slider.onValueChanged.AddListener(changed);
        }
        void OnDestroy() { if (Input) Input.InteractionBlocked=false; if (Countries) Countries.SelectionChanged-=OnSelection; Locale.Changed-=Refresh; }
    }
}
