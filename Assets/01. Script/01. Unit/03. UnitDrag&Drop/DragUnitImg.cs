using UnityEngine;
using UnityEngine.EventSystems;

//유닛을 생성할 이미지에 넣을 클래스 
public class UnitImage : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform originalParent;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject unitPrefab;
    bool isAllyAvailble = false;

    public void Start()
    {
        if(!isAllyAvailble)
        {
            SpawnUnit();
            isAllyAvailble = true;
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(originalParent.root);

        canvasGroup.alpha = 0.1f;
        canvasGroup.blocksRaycasts = false;
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position; 
        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f; //투명도 원상복귀
        canvasGroup.blocksRaycasts = true; //레이캐스트 원상복귀
        SpawnUnit(); //유닛 스폰
       
        transform.SetParent(originalParent); 

    }

    public int GetPossibleindex()
    {
        // 맨 아랫 줄에서 스폰할 수 있는 벡터의 인덱스를 찾는다. 
        // 패스파인더에게 맨 아랫줄의 인덱스를 하나씩 넣어보고 해당 타일노드의 cost가 몇인지를 확인하고 타일노드의 cost가 0이라면 아무것도 없는 곳이니깐 배치가 가능하다. 
        // 0이 아니라면 무언가 있는 곳이니깐 배치가 불가능하다. 
        // 맨 아랫줄의 인덱스는 (minX,minY)벡터의 인덱스부터 (maxX,minY)의 인덱스까지다. 

        int startIndex = PathFinder.Instance.GetIndexByVector2(new Vector2(PathFinder.Instance.MinX,PathFinder.Instance.MinY));
        int EndIndex = PathFinder.Instance.GetIndexByVector2(new Vector2(PathFinder.Instance.MaxX,PathFinder.Instance.MinY));
        int resultIndex = 0;
        
        for (int i = startIndex; i <= EndIndex; i++)
        {
            var target = PathFinder.Instance.GetTileNodeByIndex(i);

            if( target.cost == 0)
            {
                resultIndex = i;
                break;
            }  
        }
        if(resultIndex == 0)
            Debug.LogError("가능한 위치를 찾지 못함");

        return resultIndex; 
    }

    //스폰 가능한 위치를 찾아서 스폰하는 매서드  
    private void SpawnUnit()
    {
        int spawnIndex = GetPossibleindex();
        var spawnNode = PathFinder.Instance.GetTileNodeByIndex(spawnIndex);
        Instantiate(unitPrefab, spawnNode.pos , Quaternion.identity);
        PathFinder.Instance.SetPosCost(spawnIndex, PathFinder.Instance.ObstacleCost);        
    }
}

   
