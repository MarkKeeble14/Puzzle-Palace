using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class UsesVirtualKeyboardMiniGameManager : MiniGameManager
{
    protected bool enterPressed;
    protected bool backPressed;
    protected bool keyPressed;
    protected string key;

    [SerializeField] protected VirtualKeyboard virtualKeyboard;

    protected Dictionary<string, Action> additionalFunctionsDict = new Dictionary<string, Action>();

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

    public void CallAdditionalFunction(string key)
    {
        additionalFunctionsDict[key]?.Invoke();
    }

    public void ResetVirtualKeyboardFuncs()
    {
        keyPressed = false;
        backPressed = false;
        enterPressed = false;
    }
}
