using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageColliderEdit : MonoBehaviour
{

    public Image image;
    [Space]
    public float threshold;

    void Reset()
    {
        image = GetComponent<Image>();
    }

    void Start()
    {
        image.alphaHitTestMinimumThreshold = threshold;
    }

}
