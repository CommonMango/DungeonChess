using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

//유닛을 생성할 이미지에 넣을 클래스 
public class UnitImage : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform originalParent;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private UnitData unitData;
    [SerializeField] private TextMeshProUGUI tmpro; 
    [SerializeField] private Coroutine coroutine;
    WaitForSeconds wfs = new WaitForSeconds(1f);

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
        if(MoneyManager.Instance.CurMoney < unitData.cost )
        {
            tmpro.text = "돈이 부족합니다..";
            if(coroutine == null)
            {
                coroutine = StartCoroutine(StartTxtDelay());
            }
        }
        else
        {
            MoneyManager.Instance.BuyUnit(unitData.cost);
            Vector2 worldPos = ConvertCanvasPosToWorldPos(eventData.position); //캔버스 좌표를 월드좌표로 변환
            SpawnUnit(worldPos); //유닛 스폰
            
        }

        transform.SetParent(originalParent); 

    }


    private Vector2 ConvertCanvasPosToWorldPos(Vector2 canvasPos)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(canvasPos);
        worldPos.z = 0f;
        Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);

        return worldPos2D;
    }

    //위치를 받아서 가까운 위치에 스폰하는 매서드  
    
    private void SpawnUnit(Vector2 pos)
    {
        var spawnNode= TileManager.Instance.ConvertPositionToCloseNode(pos);
        var tileNode = TileManager.Instance.GetTileNodeByIndex(TileManager.Instance.GetIndexByVector2(spawnNode));
        

        Instantiate(unitPrefab, tileNode.pos , Quaternion.identity);
        TileManager.Instance.SetPosCost(TileManager.Instance.GetIndexByVector2(tileNode.pos), TileManager.Instance.ObstacleCost);       
       
    }

   
    private IEnumerator StartTxtDelay()
    {
        yield return wfs;
        tmpro.text = "";
    }
}



   
