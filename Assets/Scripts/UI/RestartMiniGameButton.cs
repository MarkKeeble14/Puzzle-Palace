using UnityEngine;

public class RestartMiniGameButton : MonoBehaviour
{
    public void RestartGame()
    {
        StartCoroutine(MiniGameManager._Instance.RestartGame());
    }
}
