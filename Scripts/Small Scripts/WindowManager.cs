using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowManager : MonoBehaviour
{
    GameObject window;
    RectTransform windowRect;


    public float headerHeight;
    float windowHeight;
    public bool isFolded;
    // Start is called before the first frame update

    Vector2 winToMouseDelta;
    void Start()
    {
        window = this.transform.parent.gameObject;
        windowRect = window.GetComponent<RectTransform>();
        RectTransform header = this.GetComponent<RectTransform>();
        header.sizeDelta = new Vector2(windowRect.sizeDelta.x, header.sizeDelta.y);
    }

    public void Fold()
    {
        if (isFolded)
        {
            windowRect.sizeDelta = new Vector2(windowRect.sizeDelta.x, windowHeight);
            for (int i = 1; i < window.transform.childCount; i++)
                window.transform.GetChild(i).gameObject.SetActive(true);
            isFolded = false;
        }
        else
        {
            windowHeight = windowRect.sizeDelta.y;
            windowRect.sizeDelta = new Vector2(windowRect.sizeDelta.x, headerHeight);
            for (int i = 1; i < window.transform.childCount; i++)
                window.transform.GetChild(i).gameObject.SetActive(false);
            isFolded = true;
        }
    }

    public void DragBegin()
    {
        Vector2 mousePos = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
        Vector2 windowPos = windowRect.anchoredPosition;
        winToMouseDelta = mousePos - windowPos;
    }

    public void Drag()
    {
        Vector2 mousePos = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
        Vector2 newPos = mousePos - winToMouseDelta;
        float width = windowRect.sizeDelta.x/2;
        float height = windowRect.sizeDelta.y/2;
        // X
        if ((newPos.x - width > 0) && (newPos.x + width < Screen.width))
            windowRect.anchoredPosition = new Vector2(newPos.x, windowRect.anchoredPosition.y);
        else if (newPos.x - width < 0)
            windowRect.anchoredPosition = new Vector2(width, windowRect.anchoredPosition.y);
        else if (newPos.x + width > Screen.width)
            windowRect.anchoredPosition = new Vector2(Screen.width - width, windowRect.anchoredPosition.y);
        // Y
        if ((newPos.y - height > 0) && (newPos.y + height < Screen.height))
            windowRect.anchoredPosition = new Vector2(windowRect.anchoredPosition.x, newPos.y);
        else if (newPos.y - height < 0)
            windowRect.anchoredPosition = new Vector2(windowRect.anchoredPosition.x, height);
        else if (newPos.y + height > Screen.height)
            windowRect.anchoredPosition = new Vector2(windowRect.anchoredPosition.x, Screen.height - height);
    }
}
