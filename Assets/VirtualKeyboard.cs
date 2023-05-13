using System.Collections.Generic;
using UnityEngine;

public class VirtualKeyboard : MonoBehaviour
{
    [SerializeField] private List<VirtualKeyboardContent> Content = new List<VirtualKeyboardContent>();
    private Dictionary<string, KeyVirtualKeyboardButton> spawnedKeysDict = new Dictionary<string, KeyVirtualKeyboardButton>();
    [SerializeField] private Transform parentTo;
    [SerializeField] private KeyVirtualKeyboardButton keyVirtualKeyboardButtonPrefab;
    [SerializeField] private EnterVirtualKeyboardButton enterVirtualKeyboardButtonPrefab;
    [SerializeField] private BackVirtualKeyboardButton backVirtualKeyboardButtonPrefab;
    [SerializeField] private Transform virtualKeyboardRowPrefab;

    private bool generated;

    public void Generate()
    {
        if (generated)
        {
            foreach (KeyValuePair<string, KeyVirtualKeyboardButton> kvp in spawnedKeysDict)
            {
                kvp.Value.Blackout(false);
            }
            return;
        }
        generated = true;

        for (int i = 0; i < Content.Count; i++)
        {
            Transform t = Instantiate(virtualKeyboardRowPrefab, parentTo);
            List<VirtualKeyboardContentData> content = Content[i].Data;
            for (int p = 0; p < content.Count; p++)
            {
                VirtualKeyboardContentData current = content[p];
                VirtualKeyboardButton spawned;
                switch (current.Type)
                {
                    case VirtualKeyboardContentType.KEY:
                        spawnedKeysDict.Add(current.Shown, Instantiate(keyVirtualKeyboardButtonPrefab, t));
                        spawned = spawnedKeysDict[current.Shown];
                        break;
                    case VirtualKeyboardContentType.ENTER_FUNCTION:
                        spawned = Instantiate(enterVirtualKeyboardButtonPrefab, t);
                        break;
                    case VirtualKeyboardContentType.BACK_FUNCTION:
                        spawned = Instantiate(backVirtualKeyboardButtonPrefab, t);
                        break;
                    default:
                        throw new UnhandledSwitchCaseException();
                }
                spawned.SetShown(current.Shown);
            }
        }
    }

    public void BlackoutKey(string s, bool v)
    {
        spawnedKeysDict[s].Blackout(v);
    }
}
