using UnityEngine;

public class MultiToolTipDisplayController : MonoBehaviour
{
    [SerializeField] private ToolTipDisplay singleToolTipDisplayPrefab;
    [SerializeField] private Transform spawnOn;

    public ToolTipDisplay Add(ToolTipDataContainer data)
    {
        ToolTipDisplay spawned = Instantiate(singleToolTipDisplayPrefab, spawnOn);
        spawned.SetText(data.text);
        spawned.AddOnConfirmAction(delegate
        {
            Destroy(gameObject);
            data.onConfirm?.Invoke();
        });

        spawned.AddOnCancelAction(delegate
        {
            Destroy(gameObject);
            data.onCancel?.Invoke();
        });
        return spawned;
    }
}
