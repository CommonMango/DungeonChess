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
        // Battle managers belong to the battle scene and unload with it.
       
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
            PhaseState previousState = currentState;
            currentState = toChangePhase;
            if (currentState == PhaseState.Battle)
                RunManager.EnsureRun();
            // 전투에서 패배로 바뀌는 순간 한 번만 적용하고, 구독자의 정리 작업보다 먼저 계산합니다.
            if (previousState == PhaseState.Battle && currentState == PhaseState.Lose &&
                UnitManager.Instance != null)
                RunManager.EnsureRun().ReduceWill(UnitManager.Instance.CalculateSurvivingEnemyWillDamage());
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
