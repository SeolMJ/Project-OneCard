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

    public void Party(float range)
    {
        if (C.playing) return;
        Vector3 position = new Vector3(transform.position.x, transform.position.y + 0.603f, transform.position.z);
        Collider[] hits = Physics.OverlapSphere(position, range, GameManager.Resource.carderLayer);
        if (hits.Length < 2) return;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].TryGetComponent(out Carder carder))
            {
                carder.Accept();
            }
        }
        Player.instance.Accept(false);
        Log($"Party at Position ({position.x}, {position.y}, {position.z}), Range {range}, {hits.Length} Targets");
    }

    public abstract void Accept(bool resume = false);

    #region Logging

    public static LogPreset? logPreset;

    public static void Log(string content, byte state = 0)
    {
        logPreset ??= new("Carder", GameManager.Resource.carderLogColor);
        Log4u.Log(logPreset.Value, content, state);
    }

    #endregion

}
