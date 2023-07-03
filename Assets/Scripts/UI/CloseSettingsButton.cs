using UnityEngine;

public class CloseSettingsButton : MonoBehaviour
{
    public void OnPress()
    {
        UIManager._Instance.Unpause();
    }
}
