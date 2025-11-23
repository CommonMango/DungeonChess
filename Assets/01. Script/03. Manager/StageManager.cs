using UnityEngine;
using System.Collections.Generic;
public enum PhaseState
{
    Placement, Battle, Reward
}

public class StagekManager : MonoBehaviour
{
    private PhaseState currentState = PhaseState.Placement;
    //
    private List<IObserveStageChange> stageStateActions = new List<IObserveStageChange>();  
    
    public void AddSubScriber (IObserveStageChange subscriber) => stageStateActions.Add(subscriber);
    public void DelSubScriber (IObserveStageChange subscriber) => stageStateActions.Remove(subscriber);
   
    void Update()
    {
        
    }

    private ? ChangeState()
    {
        
    }


}
