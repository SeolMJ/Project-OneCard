using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SeolMJ;

public class PauseManager : MonoBehaviour
{

    public static PauseManager instance;

    [Header("References")]
    public GameObject contentObject;
    public PauseButton[] pauseButtons;

    [Header("Animations")]
    public float clickDelay;
    public float space;
    public float smooth;
    public Vector3 positionY;

    [Header("Hidden")]
    public PauseButton selectedButton;

    private Coroutine clickCoroutine;
    private Vector2[] velocities;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        velocities = new Vector2[pauseButtons.Length];
    }

    void Update()
    {
        UpdateInput();
        UpdateCards();
    }

    public void UpdateInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause(!contentObject.activeInHierarchy);
        }
    }

    public static void Pause(bool pause)
    {
        if (pause)
        {
            GameManager.timeScale = 0f;
            instance.contentObject.SetActive(true);
        }
        else
        {
            GameManager.timeScale = 1f;
            instance.contentObject.SetActive(false);
        }
    }

    public void UpdateCards()
    {
        int count = pauseButtons.Length;
        float halfCount = (count - 1) / 2f;
        for (int i = 0; i < count; i++)
        {
            if (selectedButton == pauseButtons[i]) continue;
            Vector2 position = new Vector2(count < 2 ? 0f : (i - halfCount) * space, positionY.x);
            Utils.SmoothDamp(pauseButtons[i].thisRect, position, ref velocities[i], smooth, Time.unscaledDeltaTime);
        }
    }

    public void Click(UnityEvent onClick)
    {
        if (clickCoroutine != null) StopCoroutine(clickCoroutine);
        foreach (PauseButton button in pauseButtons) button.enabled = false;
        clickCoroutine = StartCoroutine(OnClick(onClick));
    }

    IEnumerator OnClick(UnityEvent onClick)
    {
        bool invoked = false;
        float pastTime = 0;

        while (true)
        {
            // Animation
            selectedButton.thisRect.anchoredPosition = new Vector2(0, positionY.y + Mathf.Sqrt(pastTime) * positionY.z);

            // Invoking
            if (!invoked && pastTime >= clickDelay)
            {
                onClick.Invoke();
                invoked = true;
            }
            pastTime += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    /* Functions */

    public void Return()
    {
        SceneLoader.Load(0);
    }

}
