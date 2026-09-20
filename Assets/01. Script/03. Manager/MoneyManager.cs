using UnityEngine;
using TMPro;

public class MoneyManager :MonoBehaviour
{
    [SerializeField] float defalutMoney = 100;
    [SerializeField] float curMoney;
    public float CurMoney => curMoney;
    [SerializeField] TextMeshProUGUI moneyTxt;

    private static MoneyManager instance;
    public static MoneyManager Instance{ get => instance; private set => instance = value;}

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
    }
    void OnEnable()
    {
        curMoney = defalutMoney;
        moneyTxt.text = $"돈: {curMoney}";
    } 

    public void BuyUnit(float cost)
    {
        curMoney -= cost;
        moneyTxt.text = $"돈: {curMoney}";
    }

    void OnDisable()
    {
        
    }
}
