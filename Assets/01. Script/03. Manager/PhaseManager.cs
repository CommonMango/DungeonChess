using UnityEngine;
using System.Collections.Generic;

public enum PhaseState
{
    Placement, Battle, Reward
}

public class PhaseManager : SingleTon<PhaseManager>
{
    //▼ 현재 상태
    private PhaseState currentState;
    
    //▼ 페이즈 변경시 해야할 행동 목록 
    private List<IObserveStageChange> stageStateActions = new List<IObserveStageChange>();  
    
    //▼ 행동들 구독, 구독 취소 메서드
    public void AddSubScriber (IObserveStageChange subscriber) => stageStateActions.Add(subscriber);
    public void RmvSubScriber (IObserveStageChange subscriber) => stageStateActions.Remove(subscriber);

    void Awake()
    {
        SingletonInit();
        ChangeState(PhaseState.Placement);
    }
    
    void Update()
    {
        // //한쪽진영의 유닛이 전부 죽었을 때
        // ChangeState(PhaseState.Reward);
        // //Reward가 종료되면 
        // ChangeState(PhaseState.Placement);
    }

    public void ChangeState(PhaseState toChangePhase)
    {
        if(currentState != toChangePhase)
        {
            currentState = toChangePhase;
            notifyChangeState(currentState);
        }
    }

    private void notifyChangeState(PhaseState notifyState)
    {
        foreach(var subscriber in stageStateActions)
        {
            subscriber?.InChangeStageState(notifyState);
        }    
    }

    public void OnButtonClick()
    {
        ChangeState(PhaseState.Battle);
    }
}
