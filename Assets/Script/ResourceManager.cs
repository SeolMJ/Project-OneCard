using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ResourceManager", menuName = "Resource Container")]
public class ResourceManager : ScriptableObject
{

    [Header("Cards")]
    public int defaultCardCount;

    [Header("Carders & NPCs")]
    public LayerMask carderLayer;
    [Space]
    public List<NPCInfo> npcs;

    [Header("UI")]
    public Sprite selectableDefaultSprite;
    public ColorBlock selectableColor;
    public ColorBlock selectableTextColor;
    public Vector4 selectableScale;

    [Header("Logging")]
    public Color32 systemLogColor;
    public Color32 loadLogColor;
    public Color32 saveLogColor;
    public Color32 playerLogColor;
    public Color32 cardLogColor;
    public Color32 npcLogColor;
    public Color32 carderLogColor;
    public Color32 cardItemLogColor;
    public Color32 sceneLogColor;

    [Header("Resources")]
    public Sprite[] symbols;
    public Sprite[] specialSymbols;
    public Sprite[] numbers;
    [Space]
    public GameObject cardPrefab;
    public GameObject[] symbolPrefabs;
    [Space]
    public Sprite doneSprite;
    public Sprite doneCardSprite;
    public Sprite pickCardSprite;
    [Space]
    public GameObject speachBobble;

    [Header("Scenes")]
    public int[] gameScenes;

}
