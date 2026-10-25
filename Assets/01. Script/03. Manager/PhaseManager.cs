using System.Collections.Generic;
using UnityEngine;
public enum PhaseState
{
    Placement, Battle, Reward, Lose, Reforming, DefeatChoice
}

public class PhaseManager : MonoBehaviour
{
    //▼ 현재 상태
    private PhaseState currentState;
    public PhaseState CurrentState => currentState;
    public int BattleAttempt { get; private set; }
    public bool IsReformation => BattleAttempt == 1 && currentState == PhaseState.Placement;
    private BattleReformation reformation;
    private int firstDefeatDamage;
    private bool resultResolved;
    
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
        reformation = gameObject.AddComponent<BattleReformation>();
        gameObject.AddComponent<BattleRetreatUI>();
       
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
        // 최종 결과가 확정된 전투는 다시 시작할 수 없습니다.
        if (toChangePhase == PhaseState.Battle)
        {
            if (resultResolved || currentState != PhaseState.Placement || BattleAttempt >= 2) return;
            if (BattleAttempt == 0) reformation.Capture();
            BattleAttempt++;
        }
        // 첫 패배 당시의 생존 적 피해를 보관하고 선택을 기다립니다.
        if (currentState == PhaseState.Battle && toChangePhase == PhaseState.Lose && BattleAttempt == 1)
        {
            firstDefeatDamage = SumEnemyWillDamage();
            currentState = PhaseState.DefeatChoice;
            notifyChangeState(currentState);

            return;
        }
        if (resultResolved && toChangePhase != PhaseState.Placement) return;
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
            if (currentState == PhaseState.Reward || currentState == PhaseState.Lose) resultResolved = true;
            notifyChangeState(currentState);
            if (currentState == PhaseState.Battle)
                UnitManager.Instance.CheckBattleResult();
        }
    }

    // 전투 전에는 전체 적, 첫 패배 후에는 그 시점에 보관한 생존 적 피해를 사용합니다.
    public int RetreatDamage => currentState == PhaseState.DefeatChoice ? firstDefeatDamage :
        (int)(((long)SumEnemyWillDamage() * 7 + 9) / 10);

    private int SumEnemyWillDamage()
    {
        long total = 0;
        foreach (var unit in FindObjectsByType<Unit>(FindObjectsSortMode.None))
            if (unit.gameObject.scene == gameObject.scene && unit.isActiveAndEnabled &&
                unit.HP > 0 && !unit.unitData.isAlly)
                total += Mathf.Max(0, unit.unitData.willDamage);
        return (int)System.Math.Min(int.MaxValue, total);
    }

    public void Retreat()
    {
        if (resultResolved || !(currentState == PhaseState.DefeatChoice ||
            (currentState == PhaseState.Placement && BattleAttempt == 0))) return;
        int damage = RetreatDamage;
        // 먼저 종료를 잠가 연속 클릭과 중복 결과 통지를 차단합니다.
        resultResolved = true;
        currentState = PhaseState.Lose;
        RunManager.EnsureRun().ReduceWill(damage);
        notifyChangeState(currentState);
    }

    public void ChooseReformation()
    {
        if (resultResolved || currentState != PhaseState.DefeatChoice || BattleAttempt != 1) return;
        currentState = PhaseState.Reforming;
        notifyChangeState(currentState);
        StartCoroutine(reformation.Restore(this));
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
