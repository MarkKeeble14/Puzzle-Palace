using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManipulateRectTransformOnMouseInput : MonoBehaviour
{
    [SerializeField] private bool enablePositionChanges;
    [SerializeField] private bool enableZoomChanges;

    // References
    [SerializeField] private RectTransform rect;

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

    [SerializeField] private bool defaultLockPosition = true;
    [SerializeField] private bool lockPosition;
    private string lockPositionKey = "LockPosition";
    [SerializeField] private Image lockPositionActiveState;

    [SerializeField] private bool defaultLockZoom = true;
    [SerializeField] private bool lockZoom;
    private string lockZoomKey = "LockZoom";
    [SerializeField] private Image lockZoomActiveState;

    private bool scaleHasBeenSet;
    private float scaleSetTo;

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
        if (!PlayerPrefs.HasKey(lockZoomKey))
        {
            lockZoom = defaultLockZoom;
            PlayerPrefs.SetInt(lockZoomKey, lockZoom ? 1 : 0);
            PlayerPrefs.Save();
            lockZoomActiveState.gameObject.SetActive(lockZoom);
        }
        else
        {
            lockZoom = PlayerPrefs.GetInt(lockZoomKey) == 1;
            lockZoomActiveState.gameObject.SetActive(lockZoom);
        }

        if (!PlayerPrefs.HasKey(lockPositionKey))
        {
            lockPosition = defaultLockPosition;
            PlayerPrefs.SetInt(lockPositionKey, lockPosition ? 1 : 0);
            PlayerPrefs.Save();
            lockPositionActiveState.gameObject.SetActive(lockPosition);
        }
        else
        {
            lockPosition = PlayerPrefs.GetInt(lockPositionKey) == 1;
            lockPositionActiveState.gameObject.SetActive(lockPosition);
        }
    }

    public void SetScale(float scale)
    {
        // 
        defaultScale = 0;
        scaleHasBeenSet = true;
        scaleSetTo = scale;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 currentMousePosition = Input.mousePosition;
        deltaMousePosition = currentMousePosition - prevMousePosition;
        prevMousePosition = currentMousePosition;

        if (scaleHasBeenSet)
        {
            scaleSizeChange = Mathf.Lerp(scaleSizeChange, scaleSetTo, scaleSizeChangeRate * Time.deltaTime);
        }
        else
        {
            if (enableZoomChanges)
            {
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
            }
            currentScale = scaleSizeChange + defaultScale;
            rect.localScale = new Vector3(currentScale, currentScale, 1);
        }

        if (enablePositionChanges)
        {
            if (lockPosition)
            {
                rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, Vector2.zero, moveRTRate / 10 * Time.deltaTime);
            }
            else if (Input.GetMouseButton(0))
            {
                Vector2 prevRTPosition = rect.anchoredPosition;
                rect.anchoredPosition = new Vector2(
                    prevRTPosition.x + (deltaMousePosition.x * Time.deltaTime * moveRTRate),
                    prevRTPosition.y + (deltaMousePosition.y * Time.deltaTime * moveRTRate));
            }
        }
    }
}
