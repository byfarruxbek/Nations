using Nations.Audio;
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
        Text selectedName, musicMute, sfxMute, borders, labels;
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
            Label("N A T I O N S", safe, new Vector2(16,-16), new Vector2(200,36), 16, TextAnchor.MiddleLeft);
            var menu = Button("Menu", safe, new Vector2(-16,-12), new Vector2(52,48), () => SetMenu(!MenuOpen));
            menu.anchorMin = menu.anchorMax = menu.pivot = new Vector2(1,1);
            selection = Rect("Selected country", safe); selection.anchorMin = selection.anchorMax = new Vector2(0.5f,0); selection.pivot = new Vector2(0.5f,0);
            selection.anchoredPosition = new Vector2(0,110); selection.sizeDelta = new Vector2(290,64); Image(selection, ink);
            selectedName = Label("", selection, new Vector2(16,-8), new Vector2(258,30), 20, TextAnchor.MiddleCenter);
            Label("EXPLORE · NATURAL EARTH", selection, new Vector2(16,-36), new Vector2(258,20), 10, TextAnchor.MiddleCenter).color = accent;
            selection.gameObject.SetActive(false);
            var blocker = Rect("Menu backdrop", safe); Stretch(blocker); blocker.offsetMin = new Vector2(0,100);
            Image(blocker, new Color(0,0,0,0.3f)); blocker.gameObject.AddComponent<Button>().onClick.AddListener(() => SetMenu(false)); shade = blocker.gameObject;
            panel = Rect("Menu panel", safe); panel.anchorMin = panel.anchorMax = panel.pivot = Vector2.one; panel.anchoredPosition = new Vector2(-16,-72); Image(panel, ink);
            var viewport = Rect("Viewport", panel); Stretch(viewport); viewport.gameObject.AddComponent<RectMask2D>(); viewport.offsetMin=new Vector2(0,24);
            var hint=Label("Scroll to explore settings",panel,Vector2.zero,new Vector2(300,24),10,TextAnchor.MiddleCenter);hint.rectTransform.anchorMin=hint.rectTransform.anchorMax=new Vector2(0.5f,0);hint.rectTransform.pivot=new Vector2(0.5f,0);hint.color=new Color(0.55f,0.67f,0.72f);
            content = Rect("Settings", viewport); content.anchorMin = new Vector2(0,1); content.anchorMax = Vector2.one; content.pivot = new Vector2(0.5f,1); content.sizeDelta = new Vector2(0,478);
            var scroll = panel.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped;
            Label("Your orbit", content, new Vector2(20,-12), new Vector2(280,34), 24, TextAnchor.MiddleLeft);
            Button("Resume exploration", content, new Vector2(16,-54), new Vector2(292,44), () => SetMenu(false));
            Label("AUDIO", content, new Vector2(20,-108), new Vector2(280,24), 11, TextAnchor.MiddleLeft).color = accent;
            musicMute = ButtonLabel("", content, new Vector2(16,-136), () => { Audio.ToggleMusic(); Refresh(); });
            Slider(content, -184, Audio.MusicVolume, Audio.SetMusic);
            sfxMute = ButtonLabel("", content, new Vector2(16,-234), () => { Audio.ToggleSfx(); Audio.Play(); Refresh(); });
            Slider(content, -282, Audio.SfxVolume, Audio.SetSfx);
            Label("MAP LAYERS", content, new Vector2(20,-330), new Vector2(280,24), 11, TextAnchor.MiddleLeft).color = accent;
            borders = ButtonLabel("", content, new Vector2(16,-358), () => { Countries.SetBorders(!Countries.Borders); Refresh(); });
            labels = ButtonLabel("", content, new Vector2(16,-412), () => { Countries.SetLabels(!Countries.Labels); Refresh(); });
            shade.SetActive(false); panel.gameObject.SetActive(false);
            Countries.SelectionChanged += OnSelection;
        }
        void Start() => Refresh();
        void Update() => ApplyLayout();
        public void ApplyLayout()
        {
            var area = GlobeSafeArea.Current;
            canvas.scaleFactor = Mathf.Max(0.5f, Mathf.Min(area.width / 430, area.height / 430));
            safe.anchorMin = new Vector2(area.xMin/Screen.width,area.yMin/Screen.height); safe.anchorMax = new Vector2(area.xMax/Screen.width,area.yMax/Screen.height);
            panel.sizeDelta = new Vector2(Mathf.Min(324,safe.rect.width-32),Mathf.Max(140,Mathf.Min(478,safe.rect.height-180)));
        }
        public void SetMenu(bool open)
        {
            MenuOpen = open; Input.InteractionBlocked = open; Controller.StopMotion();
            shade.SetActive(open); panel.gameObject.SetActive(open); selection.gameObject.SetActive(!open && !string.IsNullOrEmpty(selectedName.text));
            Audio.Play(open ? 1 : 2); if (!open) Audio.Save(); Refresh();
        }
        void OnSelection(string name) { selectedName.text = name; selection.gameObject.SetActive(name.Length > 0 && !MenuOpen); if (name.Length > 0) Audio.Play(3); }
        void Refresh()
        {
            musicMute.text = "Music                         " + (Audio.MusicMuted ? "OFF" : "ON");
            sfxMute.text = "Sound effects                " + (Audio.SfxMuted ? "OFF" : "ON");
            borders.text = "Country outlines             " + (Countries.Borders ? "ON" : "OFF");
            labels.text = "Country names                " + (Countries.Labels ? "ON" : "OFF");
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
        static Text Label(string text, Transform parent, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
        {
            var rt=Rect(text,parent);rt.anchoredPosition=position;rt.sizeDelta=size;
            var label=rt.gameObject.AddComponent<Text>();label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");label.text=text;label.fontSize=fontSize;label.color=new Color(0.9f,0.95f,1);label.alignment=alignment;label.raycastTarget=false;return label;
        }
        RectTransform Button(string text,Transform parent,Vector2 position,Vector2 size,UnityEngine.Events.UnityAction action)
        {
            var rt=Rect(text,parent);rt.anchoredPosition=position;rt.sizeDelta=size;var image=Image(rt,new Color(0.11f,0.18f,0.23f,1));
            var button=rt.gameObject.AddComponent<Button>();button.targetGraphic=image;button.onClick.AddListener(()=>{Audio.Play();action();});
            var label=Label(text,rt,Vector2.zero,size,16,TextAnchor.MiddleCenter);Stretch(label.rectTransform);return rt;
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
        void OnDestroy() { if (Input) Input.InteractionBlocked=false; if (Countries) Countries.SelectionChanged-=OnSelection; }
    }
}
