using UnityEditor.SceneManagement;

public interface IObserveStageChange
{
    public void InChangeStageState(PhaseState changeState);
}