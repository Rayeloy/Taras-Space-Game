using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Game Resources Data", menuName = "Managers/GameResourcesData")]
public class GameResourcesData : ScriptableObject
{
    //SHOP MENU
    public Sprite[] smallSquares;
    [SerializeField]
    public Sprite[] bigSquares;
    [SerializeField]
    public Sprite blueprintSquare;
}
