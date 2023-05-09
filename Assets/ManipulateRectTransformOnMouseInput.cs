using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ManipulateRectTransformOnMouseInput : MonoBehaviour
{
    [Header("Zooming")]
    [SerializeField] private float defaultScale;
    private float scaleSizeChange;
    private float currentScale;
    [SerializeField] private float scaleSizeChangeRate;
    [SerializeField] private Vector2 minMaxScaleSizeChange;

    [Header("Moving")]
    [SerializeField] private float moveRTRate;
    private Vector2 deltaMousePosition;
    private Vector2 prevMousePosition;

    // References
    private RectTransform rect;

    [SerializeField] private bool lockPosition;
    private string lockPositionKey = "LockPosition";
    [SerializeField] private Image lockPositionActiveState;

    [SerializeField] private bool lockZoom;
    private string lockZoomKey = "LockZoom";
    [SerializeField] private Image lockZoomActiveState;

    public void ToggleLockPosition()
    {
        lockPosition = !lockPosition;
        PlayerPrefs.SetInt(lockPositionKey, lockPosition ? 1 : 0);
        PlayerPrefs.Save();
        lockPositionActiveState.gameObject.SetActive(lockPosition);


    }

    public void ToggleLockZoom()
    {
        lockZoom = !lockZoom;
        PlayerPrefs.SetInt(lockZoomKey, lockZoom ? 1 : 0);
        PlayerPrefs.Save();
        lockZoomActiveState.gameObject.SetActive(lockZoom);
    }

    private void Awake()
    {
        rect = GetComponent<RectTransform>();

        if (!PlayerPrefs.HasKey(lockZoomKey))
        {
            PlayerPrefs.SetInt(lockZoomKey, 0);
            PlayerPrefs.Save();
        }
        else
        {
            lockZoom = PlayerPrefs.GetInt(lockZoomKey) == 1;
            lockZoomActiveState.gameObject.SetActive(lockZoom);
        }

        if (!PlayerPrefs.HasKey(lockPositionKey))
        {
            PlayerPrefs.SetInt(lockPositionKey, 0);
            PlayerPrefs.Save();
        }
        else
        {
            lockPosition = PlayerPrefs.GetInt(lockPositionKey) == 1;
            lockPositionActiveState.gameObject.SetActive(lockPosition);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 currentMousePosition = Input.mousePosition;
        deltaMousePosition = currentMousePosition - prevMousePosition;
        prevMousePosition = currentMousePosition;

        if (lockZoom)
        {
            scaleSizeChange = Mathf.Lerp(scaleSizeChange, 0, scaleSizeChangeRate * Time.deltaTime);
        }
        else
        {
            scaleSizeChange += Input.mouseScrollDelta.y * scaleSizeChangeRate * Time.deltaTime;
            if (scaleSizeChange > minMaxScaleSizeChange.y)
            {
                scaleSizeChange = minMaxScaleSizeChange.y;
            }
            if (scaleSizeChange < minMaxScaleSizeChange.x)
            {
                scaleSizeChange = minMaxScaleSizeChange.x;
            }
        }
        currentScale = scaleSizeChange + defaultScale;
        rect.localScale = new Vector3(currentScale, currentScale, 1);

        if (lockPosition)
        {
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, Vector2.zero, moveRTRate / 10 * Time.deltaTime);
        }
    }

    public void MouseDrag()
    {
        if (lockPosition) return;
        Vector2 prevRTPosition = rect.anchoredPosition;
        rect.anchoredPosition = new Vector2(
            prevRTPosition.x + (deltaMousePosition.x * Time.deltaTime * moveRTRate),
            prevRTPosition.y + (deltaMousePosition.y * Time.deltaTime * moveRTRate));

    }
}
