using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 첫 전투 직전의 구성과 위치를 보관하며 전투 씬과 함께 정리됩니다.
public sealed class BattleReformation : MonoBehaviour
{
    private GameObject storage;
    private readonly List<GameObject> originals = new List<GameObject>();

    public void Capture()
    {
        storage = new GameObject("전투 초기 상태");
        SceneManager.MoveGameObjectToScene(storage, gameObject.scene);
        storage.SetActive(false);
        foreach (var unit in FindObjectsByType<Unit>(FindObjectsSortMode.None))
        {
            if (unit.gameObject.scene != gameObject.scene) continue;
            // 비활성 부모 아래 복제해 등록·행동트리 실행 없이 초기 상태를 보관합니다.
            var copy = Instantiate(unit.gameObject, unit.transform.position, unit.transform.rotation, storage.transform);
            originals.Add(copy);
        }
    }

    public IEnumerator Restore(PhaseManager phase)
    {
        foreach (var arrow in FindObjectsByType<ArrowProjectile>(FindObjectsSortMode.None))
            if (arrow.gameObject.scene == gameObject.scene) Destroy(arrow.gameObject);
        foreach (var unit in FindObjectsByType<Unit>(FindObjectsSortMode.None))
        {
            if (unit.gameObject.scene != gameObject.scene) continue;
            unit.gameObject.SetActive(false);
            Destroy(unit.gameObject);
        }
        // 사망 및 등록 해제가 끝난 다음 점유 타일을 초기화합니다.
        yield return null;
        TileManager.Instance.InitTiles();
        foreach (var original in originals)
        {
            var restored = Instantiate(original, original.transform.position, original.transform.rotation);
            // OnEnable에서 전투 씬의 아군·적 부모 아래로 자동 등록됩니다.
            var unit = restored.GetComponent<Unit>();
            if (restored.TryGetComponent<Rigidbody2D>(out var rigid))
            {
                rigid.position = restored.transform.position;
                rigid.linearVelocity = Vector2.zero;
                rigid.angularVelocity = 0f;
            }
            UnitManager.Instance.updateIndex(unit);
        }
        // 복구된 유닛의 Start와 초기 능력치 설정을 마친 뒤 배치를 허용합니다.
        yield return null;
        phase.ChangeState(PhaseState.Placement);
        foreach (var button in FindObjectsByType<Button>(FindObjectsSortMode.None))
        {
            if (button.gameObject.scene != gameObject.scene) continue;
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            {
                if (button.onClick.GetPersistentTarget(i) != phase || button.onClick.GetPersistentMethodName(i) != nameof(PhaseManager.OnButtonClick)) continue;
                var label = button.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = "재정비 · 다시 전투 (2/2)";
            }
        }
    }
}