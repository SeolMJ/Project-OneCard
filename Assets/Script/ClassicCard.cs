using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SeolMJ;

public class ClassicCard : Card
{

    [Header("Symbols")]
    public Transform symbolParent;

    public override void SelectSymbol(bool home)
    {
        if (!gameObject.activeInHierarchy)
        {
            Error("Init Canceled: GameObject not Active");
            return;
        }
        if (activeJob != null) StopCoroutine(activeJob);
        activeJob = num switch
        {
            CardNum.A => StartCoroutine(ActiveSymbols(16)),
            CardNum.Two => StartCoroutine(ActiveSymbols(5, 10)),
            CardNum.Three => StartCoroutine(ActiveSymbols(5, 8, 10)),
            CardNum.Four => StartCoroutine(ActiveSymbols(0, 4, 11, 15)),
            CardNum.Five => StartCoroutine(ActiveSymbols(0, 4, 8, 11, 15)),
            CardNum.Six => StartCoroutine(ActiveSymbols(0, 2, 4, 11, 13, 15)),
            CardNum.Seven => StartCoroutine(ActiveSymbols(0, 2, 4, 7, 11, 13, 15)),
            CardNum.Eight => StartCoroutine(ActiveSymbols(0, 1, 3, 4, 11, 12, 14, 15)),
            CardNum.Nine => StartCoroutine(ActiveSymbols(0, 1, 3, 4, 8, 11, 12, 14, 15)),
            CardNum.Ten => StartCoroutine(ActiveSymbols(0, 1, 3, 4, 6, 9, 11, 12, 14, 15)),
            CardNum.BlackJoker => StartCoroutine(ActiveSymbols(16)),
            CardNum.ColorJoker => StartCoroutine(ActiveSymbols(16)),
            _ => StartCoroutine(ActiveSymbols(0, 16, 15))
        };
        if (home) GoHome();
    }

    IEnumerator ActiveSymbols(params int[] indexs)
    {
        WaitForSeconds wait = new WaitForSeconds(CardManager.instance.symbolDelay / indexs.Length);
        for (int i = 0; i < indexs.Length; i++)
        {
            Instantiate(GameManager.Resource.symbolPrefabs[indexs[i]], symbolParent).GetComponent<Image>().sprite = GameManager.instance.GetSymbol(indexs[i] == 16, type, num);
            if (i != indexs.Length) yield return wait;
        }
    }

    public override void Done(bool check)
    {
        base.Done(check);
        if (check) Instantiate(GameManager.Resource.donePrefab, symbolParent);
    }

    static LogPreset logPreset => new ("Card Item", GameManager.Resource.cardItemLogColor);

    public void Log(string message)
    {
        Log4u.Log(logPreset, message);
    }

    public void Error(string message)
    {
        Log4u.Error(logPreset, message);
    }

}
