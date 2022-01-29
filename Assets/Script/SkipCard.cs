using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkipCard : Card
{

    [Header("Symbols")]
    public GameObject[] symbolObjects;
    public Image[] symbols;
    public float symbolDelay;

    public override void SelectSymbol(bool home)
    {
        if (activeJob != null) StopCoroutine(activeJob);
        activeJob = StartCoroutine(ActiveSymbols());
        if (home) GoHome();
    }

    IEnumerator ActiveSymbols()
    {
        WaitForSeconds wait = new WaitForSeconds(symbolDelay);
        symbolObjects[0].SetActive(false);
        symbolObjects[1].SetActive(false);
        symbolObjects[2].SetActive(false);

        symbolObjects[0].SetActive(true);
        symbols[0].sprite = GameManager.instance.GetSymbol(true, type, num);
        yield return wait;
        symbolObjects[1].SetActive(true);
        yield return wait;
        symbolObjects[2].SetActive(true);
        symbols[2].sprite = GameManager.instance.GetSymbol(true, type, num);
    }

}
