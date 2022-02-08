using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasCameraAutoLink : MonoBehaviour
{

    public Canvas canvas;

    void Reset()
    {
        if (!canvas) canvas = GetComponent<Canvas>();
    }

    void Start()
    {
        canvas.worldCamera = GameManager.instance.uiCamera;
    }

}
