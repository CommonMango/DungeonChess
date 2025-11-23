using UnityEditor.SceneManagement;

public interface IObserveStageChange
{
    public void ChangeStageState(PhaseState changeState);
}