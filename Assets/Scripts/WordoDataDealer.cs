using UnityEngine;

public class WordoDataDealer : MonoBehaviour
{
    public static WordoDataDealer _Instance { get; private set; }

    private void Awake()
    {
        _Instance = this;
    }

    [SerializeField] private Color activeButtonColor;
    [SerializeField] private Color inactiveButtonColor;

    public Color GetActiveButtonColor()
    {
        return activeButtonColor;
    }

    public Color GetInactiveButtonColor()
    {
        return inactiveButtonColor;
    }
}
