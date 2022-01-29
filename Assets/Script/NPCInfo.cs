using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPC Info", menuName = "New NPC Info")]
public class NPCInfo : ScriptableObject
{

    [Header("Base")]
    public new string name;

    [Header("Cards")]
    public List<CardInfo> cards;
    public List<CardInfo> nowCards;

    [Header("Behavior")]
    public float sensitive;
    public float careful;
    public float quick;

    [Header("Transform")]
    public int scene;
    public Vector2 position;


}
