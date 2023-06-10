using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeginMiniGameButton : MonoBehaviour
{
    public void BeginGame()
    {
        MiniGameManager._Instance.BeginGame();
    }
}
