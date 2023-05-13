using UnityEngine;

public abstract class UsesVirtualKeyboardMiniGameManager : MiniGameManager
{
    protected bool enterPressed;
    protected bool backPressed;
    protected bool keyPressed;
    protected string key;

    [SerializeField] protected VirtualKeyboard virtualKeyboard;

    public void SimulateEnter()
    {
        // Debug.Log("Enter");
        enterPressed = true;
    }

    public void SimulateBack()
    {
        // Debug.Log("Back");
        backPressed = true;
    }

    public void KeyPressed(string s)
    {
        // Debug.Log(s);
        key = s;
        keyPressed = true;
    }

    public void ResetVirtualKeyboardFuncs()
    {
        keyPressed = false;
        backPressed = false;
        enterPressed = false;
    }
}
