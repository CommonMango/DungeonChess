using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NodeMapView : MonoBehaviour
{
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private Camera mapCamera;
    [SerializeField] private EventSystem mapEventSystem;
    private NodeRunController run;
    private Canvas canvas;
    private GameObject mapPanel, resultPanel;
    private TMP_Text progress, status, resultTitle;
    private readonly Dictionary<string, Button> buttons = new Dictionary<string, Button>();
    private readonly List<(string from, string to, Image image)> edges = new List<(string, string, Image)>();
    private readonly Color muted = new Color(.19f,.25f,.31f);
    private readonly Color visited = new Color(.2f,.65f,.57f);

    public void Initialize(NodeRunController controller)
    {
        run = controller;
        var go = new GameObject("NodeMapCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.transform.SetParent(transform, false);
        canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 100;
        var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600,900); scaler.matchWidthOrHeight = .5f;
        mapPanel = Panel("Map", go.transform, new Color(.045f,.065f,.09f));
        Text("DUNGEON CHESS", mapPanel.transform, new Vector2(-540,350), new Vector2(360,55), 32);
        Text("진행 경로", mapPanel.transform, new Vector2(-540,285), new Vector2(360,60), 42);
        Text("적을 살피고 배치하세요.\n전투에서 승리하면 연결된\n다음 노드로 진행합니다.", mapPanel.transform, new Vector2(-535,150), new Vector2(370,140), 23);
        progress = Text("", mapPanel.transform,new Vector2(-535,-40),new Vector2(370,90),27);
        status = Text("",mapPanel.transform,new Vector2(-535,-160),new Vector2(370,135),22);
        MakeButton("새 도전",mapPanel.transform,new Vector2(-540,-330),new Vector2(240,60),run.ResetRun);
        Text("금색: 선택 가능   ·   초록: 완료   ·   회색: 잠김",mapPanel.transform,new Vector2(180,360),new Vector2(900,60),22);
        var area = new GameObject("Routes",typeof(RectTransform));area.transform.SetParent(mapPanel.transform,false);
        var rt=area.GetComponent<RectTransform>();rt.anchoredPosition=new Vector2(180,-20);rt.sizeDelta=new Vector2(960,700);
        foreach(var node in run.Map.nodes)foreach(string id in node.next)
        {
            Vector2 a=node.position,b=run.Map.Find(id).position,delta=b-a;
            var line=new GameObject("Link "+node.id+" to "+id,typeof(RectTransform),typeof(Image));line.transform.SetParent(area.transform,false);
            var rect=line.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(delta.magnitude,4);rect.anchoredPosition=(a+b)*.5f;
            rect.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);
            var img=line.GetComponent<Image>();img.raycastTarget=false;edges.Add((node.id,id,img));
        }
        int index=0;
        foreach(var node in run.Map.nodes)
        {
            string id=node.id;index++;
            var button=MakeButton(index.ToString("00")+"  "+node.type.displayName,area.transform,node.position,new Vector2(136,64),()=>run.Select(id));
            buttons.Add(id,button);
        }
        resultPanel=Panel("BattleResult",go.transform,new Color(.035f,.05f,.07f,.94f));
        resultTitle=Text("",resultPanel.transform,new Vector2(0,65),new Vector2(700,90),46);resultTitle.alignment=TextAlignmentOptions.Center;
        MakeButton("경로로 돌아가기",resultPanel.transform,new Vector2(0,-80),new Vector2(300,70),run.RequestReturn);
        resultPanel.SetActive(false);
    }
    public void Refresh()
    {
        foreach(var node in run.Map.nodes)
        {
            var button=buttons[node.id];bool available=run.CanSelect(node.id);
            button.interactable=available;
            button.GetComponent<Image>().color=run.IsVisited(node.id)?visited:available?node.type.color:muted;
            button.GetComponentInChildren<TMP_Text>().color=available?new Color(.08f,.09f,.1f):Color.white;
        }
        foreach(var edge in edges)edge.image.color=run.IsVisited(edge.from)&&run.IsVisited(edge.to)?visited:muted;
        progress.text="완료한 전투  "+run.CompletedCount;
        status.text=run.IsComplete?"모든 전투를 완료했습니다!":run.IsFailed?"이번 도전이 끝났습니다.\n새 도전으로 다시 시작하세요.":run.IsBusy?"전투에 진입하는 중…":"금색 전투 노드를 선택하세요.";
    }
    public void SetStatus(string message) => status.text=message;
    public void SetMapVisible(bool visible)
    {
        canvas.gameObject.SetActive(visible);mapPanel.SetActive(visible);resultPanel.SetActive(false);
        mapCamera.gameObject.SetActive(visible);mapEventSystem.gameObject.SetActive(visible);
    }
    public void ShowResult(bool success)
    {
        canvas.gameObject.SetActive(true);mapPanel.SetActive(false);resultPanel.SetActive(true);
        resultTitle.text=success?"전투 승리":"전투 패배";
    }
    private GameObject Panel(string name,Transform parent,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
        go.GetComponent<Image>().color=color;return go;
    }
    private TMP_Text Text(string value,Transform parent,Vector2 pos,Vector2 size,float fontSize)
    {
        var go=new GameObject("Label",typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>();rect.anchoredPosition=pos;rect.sizeDelta=size;
        var text=go.GetComponent<TextMeshProUGUI>();text.font=font;text.text=value;text.fontSize=fontSize;text.color=new Color(.9f,.93f,.94f);text.raycastTarget=false;
        return text;
    }
    private Button MakeButton(string title,Transform parent,Vector2 pos,Vector2 size,UnityEngine.Events.UnityAction click)
    {
        var go=new GameObject(title,typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>();rect.anchoredPosition=pos;rect.sizeDelta=size;
        go.GetComponent<Image>().color=new Color(.22f,.32f,.4f);
        var button=go.GetComponent<Button>();button.transition=Selectable.Transition.None;button.onClick.AddListener(click);
        Text(title,go.transform,Vector2.zero,size-new Vector2(12,4),22).alignment=TextAlignmentOptions.Center;
        return button;
    }
}
