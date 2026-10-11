using System.Collections.Generic;
using UnityEngine;
public enum PhaseState
{
    Placement, Battle, Reward, Lose
}

public class PhaseManager : MonoBehaviour
{
    //▼ 현재 상태
    private PhaseState currentState;
    public PhaseState CurrentState => currentState;
    
    //▼ 페이즈 변경시 해야할 행동 목록 
    private List<IObserveStageChange> stageStateActions = new List<IObserveStageChange>();  
    
    //▼ 행동들 구독, 구독 취소 메서드
    public void AddSubScriber (IObserveStageChange subscriber)
    {
        if (!stageStateActions.Contains(subscriber))
            stageStateActions.Add(subscriber);
        subscriber.InChangeStageState(currentState);
    }
    public void RmvSubScriber (IObserveStageChange subscriber) => stageStateActions.Remove(subscriber);

    private static PhaseManager instance;
    public static PhaseManager Instance{ get => instance; private set => instance = value;}



    void Awake()
    { 
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
       
        ChangeState(PhaseState.Placement);
    }
    
    void Update()
    {
        // 적군진영의 유닛이 전부 죽었을 때
        // ChangeState(PhaseState.Reward);
        //아군 진영의 유닛이 전부 죽었을 떄 
        //.ChangeState(PhaseState.Lose);
        // //Reward가 종료되면 
        // ChangeState(PhaseState.Placement);
    }

    public void ChangeState(PhaseState toChangePhase)
    {
        if(currentState != toChangePhase)
        {
            currentState = toChangePhase;
            notifyChangeState(currentState);
            if (currentState == PhaseState.Battle)
                UnitManager.Instance.CheckBattleResult();
        }
    }

    private void notifyChangeState(PhaseState notifyState)
    {
        foreach(var subscriber in stageStateActions.ToArray())
        {
            subscriber?.InChangeStageState(notifyState);
        }    
    }

    public void OnButtonClick()
    {
        if (currentState == PhaseState.Placement)
            ChangeState(PhaseState.Battle);
    }
}
