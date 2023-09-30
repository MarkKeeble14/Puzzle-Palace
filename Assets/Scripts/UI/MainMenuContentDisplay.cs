using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MainMenuScreen
{
    Single,
    Multi
}

public class MainMenuContentDisplay : MonoBehaviour
{
    [SerializeField] private MainMenuScreen activeScreen;
    [SerializeField] private SerializableDictionary<MainMenuScreen, Vector2> activeScreenPositioningData = new SerializableDictionary<MainMenuScreen, Vector2>();
    [SerializeField] private RectTransform screenContent;
    [SerializeField] private float speed;
    public void ChangeScreen()
    {
        switch (activeScreen)
        {
            case MainMenuScreen.Multi:
                activeScreen = MainMenuScreen.Single;
                break;
            case MainMenuScreen.Single:
                activeScreen = MainMenuScreen.Multi;
                break;
            default: throw new UnhandledSwitchCaseException();
        }
    }

    // Update is called once per frame
    void Update()
    {
        screenContent.offsetMin = Vector2.Lerp(screenContent.offsetMin, activeScreenPositioningData[activeScreen], Time.deltaTime * speed);
    }
}
