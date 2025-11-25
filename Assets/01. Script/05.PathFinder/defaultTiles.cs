using UnityEngine;
using System.Collections.Generic;

public class Tiles : SingleTon<Tiles>
{
    public List<Vector2> standAbleTiles = new List<Vector2>(); //갈 수 있는 위치정보 리스트 테스트용으로 public
    [SerializeField] int maxX = 11;
    [SerializeField] int maxY = 21;
    [SerializeField] int minX = -11;
    [SerializeField] int minY = -25;
    void Awake()
    {
        for (int x = minX; x <= maxX; x += 2)
        {
            for(int y = minY; y <= maxY; y += 2 )
            {
                standAbleTiles.Add(new Vector2(x,y));
            }
        }   
    }
}