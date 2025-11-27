using UnityEngine;
using TMPro;

public class MoneyManager :SingleTon<MoneyManager>
{
    [SerializeField] float defalutMoney = 100;
    [SerializeField] float curMoney;
    public float CurMoney => curMoney;
    [SerializeField] TextMeshProUGUI moneyTxt;

   
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
