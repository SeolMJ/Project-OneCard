using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class ChatElement : MonoBehaviour
{

    public RectTransform thisRect;

    [Header("RectTransforms")]
    public RectTransform textRect;
    public RectTransform boxRect;

    [Header("Dynamic UIs")]
    public Image iconImage;
    public TMP_Text text;
    public ContentSizeFitter fitter;

    private float targetHeight;

    async void OnEnable()
    {
        while (!ChatManager.instance) await Task.Delay(1);
        ChatManager.AddElement(this);
    }

    void OnDisable()
    {
        ChatManager.RemoveElement(this);
    }

    public void Init(string content, Sprite image)
    {
        text.text = content;
        ChatManager.Layout();
        iconImage.sprite = image;
    }

    public void Apply()
    {
        float width = (transform.parent as RectTransform).rect.width;
        thisRect.sizeDelta = new Vector2(width, 32);
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        textRect.sizeDelta = new Vector2(width - 112, 32);
        text.ForceMeshUpdate();
        if (text.textInfo.lineCount < 2)
        {
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.SetLayoutHorizontal();
        }
        fitter.SetLayoutVertical();
        boxRect.sizeDelta = new Vector2(Mathf.Min(width - 80, textRect.sizeDelta.x + 32), Mathf.Max(32, textRect.sizeDelta.y) + 32);
        boxRect.anchoredPosition = new Vector2(80, 0);
        thisRect.sizeDelta = new Vector2(width, boxRect.sizeDelta.y);
    }

    public void Move(float height)
    {

    }

}
