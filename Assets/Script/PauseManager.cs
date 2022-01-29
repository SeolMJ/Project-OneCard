using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PauseManager : MonoBehaviour
{

    [Header("References")]
    public PauseButton[] pauseButtons;

    [Header("Animations")]
    public float clickDelay;

    private Coroutine clickCoroutine;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Click(UnityEvent onClick)
    {
        if (clickCoroutine != null) StopCoroutine(clickCoroutine);
        clickCoroutine = StartCoroutine(OnClick(onClick));
    }

    IEnumerator OnClick(UnityEvent onClick)
    {
        yield return new WaitForSeconds(clickDelay);
        onClick.Invoke();
    }

}
