using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SeolMJ;

public abstract class Carder : MonoBehaviour
{

    protected static GameManager G => GameManager.instance;
    protected static CardManager C => CardManager.instance;

    public abstract void OnTurn();
    public abstract void AddCards(uint count);
    public abstract int CardCount();

    protected static LogPreset logPreset;

    public void Party(float distance)
    {
        if (C.playing) return;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, distance, GameManager.Resource.carderLayer);
        for (int i = 0; i < hits.Length; i++)
        {
            hits[i].GetComponent<Carder>()?.Accept();
        }
        if (string.IsNullOrEmpty(logPreset.prefix)) logPreset = new("Carder", GameManager.Resource.carderLogColor);
        Log4u.Log(logPreset, $"Party at pos({transform.position.x}, {transform.position.y}), dist {distance}, {hits.Length} targets");
    }

    public abstract void Accept(bool resume = false);

}
