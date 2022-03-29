using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using SeolMJ;

using static SeolMJ.Log4u;
using System.Collections;
using UnityEditor;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;

    [Header("Datas")]
    public SaveData data;

    [Header("Stats")]
    public int health;
    public int damage;
    public int luck;
    public int money;

    [Header("References")]
    public ResourceManager resource;
    public EventSystem eventSystem;

    [Header("Loading")]
    public CanvasGroup busyGroup;
    public LoadingCard[] busyCards;
    public float busySpeed;

    public static int busyWorks;

    public static ResourceManager Resource => instance.resource;

    public static ResourceManager EditorResource
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType<GameManager>();
            }
            return instance.resource;
        }
    }

    // Save Load
    [HideInInspector] public bool loaded;
    private string dataPath;

    // Loading
    private float busyAlphaVel;

    // NPCs
    [HideInInspector] public List<NPC> activeNPCs;

    // Statics
    public static float timeScale = 1f;
    public static float deltaTime => Time.deltaTime * timeScale;
    public static double totalTime;

    void Awake()
    {
        if (instance == null || instance == this)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        totalTime = 0;
    }

    void Start()
    {
        Init();
    }

    void Update()
    {
        totalTime += deltaTime;

        // Save
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S)) Save();

        // Loading
        bool isBusy = busyWorks > 0;
        if (isBusy || busyGroup.alpha != 0)
        {
            busyGroup.alpha = Mathf.SmoothDamp(busyGroup.alpha, isBusy ? 1 : 0, ref busyAlphaVel, isBusy ? busySpeed * 0.5f : busySpeed);
            foreach (LoadingCard card in busyCards) card.Animate();
        }
    }

    LogPreset SystemLog;
    LogPreset SaveLog;
    LogPreset LoadLog;

    void Init()
    {
        SceneLoader.SetScene(GetScene());

        // Setup Logs
        SystemLog = new("System", Resource.systemLogColor);
        SaveLog = new("Save", Resource.saveLogColor);
        LoadLog = new("Load", Resource.loadLogColor);

        // Load
        Log(SystemLog, "Initializing");
        dataPath = Application.persistentDataPath;
        Load();
    }

    void Setup()
    {
        Log(SystemLog, "Setting up");
        data = new();
        data.name = "Unknown";
        data.cards = new();
        data.scene = 0;
        data.nowCards = new();
        data.nowEntities = new();
        data.nowTurn = 0;
        data.nowStack = 0;
        data.position = new(0, 0);
        data.entities = new();
        for (int i = 0; i < Resource.defaultCardCount; i++) data.cards.Add(CardUtils.RandomCard());
        for (int i = 0; i < Resource.npcs.Count; i++) data.entities.Add(new());
        loaded = true;
    }

    // Save & Load

    public async void Load()
    {
        using var _ = new Busy(7);

        Log(LoadLog, "Loading", 1);
        if (!File.Exists(dataPath + "/data.seolmj"))
        {
            Error(LoadLog, "Not Found", 3);
            Setup();
            return;
        }
        //using (FileStream file = File.Open(dataPath + "/data.seolmj", FileMode.Open))
        using (StreamReader reader = new(new FileStream(dataPath + "/data.seolmj", FileMode.Open, FileAccess.Read, FileShare.Read)))
        {
            // File Load
            try { data = JsonUtility.FromJson<SaveData>(await reader.ReadToEndAsync()); }
            catch (Exception e)
            {
                Error(LoadLog, "Load failed", e, 3);
                reader.Dispose();
                return;
            }

            #region Binary Method
            // Binary Method
            /*
            BinaryFormatter formatter = new();
            try { data = (SaveData)formatter.Deserialize(file); }
            catch (Exception e) { Error(LoadLog, "Load failed", e, 3); return; }
            Log(LoadLog, $"Success : {data.name}", 2); */
            #endregion

            // Global
            Log(LoadLog, $"{data.cards.Count} cards loaded", 2);

            // Stats
            health = data.health;
            damage = data.damage;
            luck = data.luck;
            money = data.money;
            Log(SaveLog, $"Health: {health}, Damage: {damage}, Luck: {luck}, Money: {money}", 2);

            // NPCs
            List<NPCInfo> npcs = Resource.npcs;
            if (data.entities == null)
            {
                data.entities = new();
                Error(LoadLog, $"No NPC saved", 2);
            }
            else if (data.entities.Count != 0)
            {
                Log(LoadLog, $"total {data.entities.Count} NPCs loading", 2);
            }

            for (int i = 0; i < data.entities.Count; i++) // Apply exsisting npcs
            {
                if (npcs.Count <= i) continue;
                if (data.entities[i].cards.Count == 0) // Random Card equip
                {
                    for (int j = 0; j < Resource.defaultCardCount; j++) data.entities[i].cards.Add(CardUtils.RandomCard());
                }
                npcs[i].cards = new(data.entities[i].cards);
                npcs[i].nowCards = IndexToNowCards(i);
                npcs[i].position = data.entities[i].position.Get();
            }

            if (data.entities.Count < npcs.Count) // Fill empty npcs
            {
                for (int i = data.entities.Count; i < npcs.Count; i++)
                {
                    data.entities.Add(new());
                    Error(LoadLog, "Missing NPC", 2);
                }
            }
        }
        Log(LoadLog, "Success", 3);
        loaded = true;
    }

    public async void Save()
    {
        using var _ = new Busy(7);

        Log(SaveLog, "Saving", 1);
        //using (FileStream stream = File.Create(dataPath + "/data.seolmj"))
        using (StreamWriter writer = new(new FileStream(dataPath + "/data.seolmj", FileMode.Create, FileAccess.Write, FileShare.Write)))
        {
            // Data Apply
            if (Player.instance)
            {
                // Cards
                Log(SaveLog, $"{data.cards.Count} cards saved", 2);

                // Scene
                data.scene = SceneManager.GetActiveScene().buildIndex;
                Log(SaveLog, $"{data.scene}th scene saved", 2);

                // Stats
                data.health = health;
                data.damage = damage;
                data.luck = luck;
                data.money = money;
                Log(SaveLog, $"Health: {data.health}, Damage: {data.damage}, Luck: {data.luck}, Money: {data.money}", 2);

                // Card Game
                if (Player.instance.carding)
                {
                    data.nowCarding = true;
                    if (CardManager.instance)
                    {
                        if (CardManager.instance.previewCard)
                        {
                            data.nowPreview = true;
                            data.nowCard = CardManager.instance.previewCard.GetInfo();
                            data.nowPicked = CardManager.instance.previewCard.picked;
                            data.nowDamaged = CardManager.instance.previewCard.done;
                        }
                        else data.nowPreview = false;
                        data.nowTurn = CardManager.instance.turn;
                        data.nowStack = (int)CardManager.instance.stack;
                        data.nowCards.Clear();
                        foreach (Card card in Player.instance.cards) data.nowCards.Add(data.cards.IndexOf(card.GetInfo()));
                        Log(SaveLog, $"OneCard : {data.cards.Count} cards, turn {data.nowTurn}, stack {data.nowStack}", 2);
                        foreach (NPC npc in activeNPCs) npc.Save();
                        Log(SaveLog, $"OneCard : {data.nowEntities.Count} NPCs playing", 2);
                    }
                    else Error(SaveLog, $"CardManager not exsisting", 2);
                }
                else
                {
                    data.nowCarding = false;
                    data.nowCards.Clear();
                    Log(SaveLog, $"OneCard saved (none)", 2);
                }

                // Player
                if (Player.instance)
                {
                    data.position = new SaveVector(Player.instance.transform.position);
                    data.velocity = new SaveVector(Player.instance.rigidbody.velocity);
                    Log(SaveLog, $"Player Transform saved: position ({data.position.x}, {data.position.y}), velocity ({data.velocity.x}, {data.velocity.y})", 2);
                }
                else Error(SaveLog, $"Player not exsisting", 2);

                // NPCs
                List<NPCInfo> npcs = Resource.npcs;
                if (npcs.Count > 0)
                {
                    for (int i = 0; i < npcs.Count; i++)
                    {
                        List<int> nowCards = NowCardsToIndex(i);
                        data.entities[i] = new(npcs[i].position, npcs[i].name, new(npcs[i].cards), npcs[i].nowCards == null ? new() : (nowCards ?? new()), true);
                        if (nowCards != null) if (!data.nowEntities.Contains(i)) data.nowEntities.Add(i);
                        Log(SaveLog, $"NPC '{data.entities[i].name}'(={npcs[i].name}) saved (pos<{data.entities[i].position.x}, {data.entities[i].position.y}>, {data.entities[i].cards.Count} cards, {data.entities[i].nowCards.Count} ingame cards)", 2);
                    }
                    Log(SaveLog, $"{data.entities.Count} NPCs saved", 2);
                }
                else Error(SaveLog, $"NPC not exsisting", 2);
            }
            else Error(SaveLog, $"Player not exsisting", 2);

            #region Binary Method
            // Binary Method
            /*
            BinaryFormatter formatter = new();
            try { formatter.Serialize(stream, data); }
            catch (Exception e) { Error(SaveLog, "Failed", e, 3); return; }
            */
            #endregion

            try { await writer.WriteAsync(JsonUtility.ToJson(data)); }
            catch (Exception e)
            {
                Error(SaveLog, "Save Failed", e, 3);
                writer.Dispose();
                return;
            }

            Log(SaveLog, "Success", 3);
        }
    }

    // Util Methods

    public static CardInfo GetCard(int index)
    {
        if (index < 0 || instance.data.cards.Count <= index) return new CardInfo();
        return instance.data.cards[index];
    }

    public static void AddCard(CardInfo card)
    {
        instance.data.cards.Add(card);
    }

    public static List<CardInfo> GetRandomCards()
    {
        List<int> remain = new();
        List<CardInfo> result = new();
        for (int i = 0; i < instance.data.cards.Count; i++) remain.Add(i);
        for (int i = 0; i < Resource.defaultCardCount; i++)
        {
            int random = UnityEngine.Random.Range(0, remain.Count);
            result.Add(GetCard(remain[random]));
            remain.RemoveAt(random);
        }
        return result;
    }

    public static GameObject Spawn(GameObject target)
    {
        GameObject newObject = Instantiate(target);
        if (newObject.name.Contains("(Clone)")) newObject.name = newObject.name[..^7];
        return newObject;
    }

    public Sprite GetSymbol(bool special, CardType type, CardNum num)
    {
        if (CardUtils.Match(num, CardNum.Specials) && special)
        {
            return num switch
            {
                CardNum.J => CardUtils.Match(type, CardType.Color) ? Resource.specialSymbols[0] : Resource.specialSymbols[1],
                CardNum.Q => CardUtils.Match(type, CardType.Color) ? Resource.specialSymbols[2] : Resource.specialSymbols[3],
                CardNum.K => CardUtils.Match(type, CardType.Color) ? Resource.specialSymbols[4] : Resource.specialSymbols[5],
                _ => null
            };
        }
        else
        {
            return type switch
            {
                CardType.All => Resource.symbols[5],
                CardType.Black => Resource.symbols[4],
                CardType.Color => Resource.symbols[5],
                CardType.Diamond => Resource.symbols[0],
                CardType.Spade => Resource.symbols[1],
                CardType.Heart => Resource.symbols[2],
                CardType.Clover => Resource.symbols[3],
                _ => null
            };
        }
    }

    /*public static async Task<int> FindNPC(string name)
    {
        return await Task.Run(() =>
        {
            try
            {
                for (int i = 0; i < instance.npcs.Count; i++)
                {
                    if (instance.npcs[i].name == name) return i;
                }
            }
            catch { return -1; }
            return -1;
        });
    }*/

    public bool FindNPC(string name, out NPC npc)
    {
        for (int i = 0; i < activeNPCs.Count; i++)
        {
            if (activeNPCs[i].info.name == name)
            {
                npc = activeNPCs[i];
                return true;
            }
        }
        npc = null;
        return false;
    }

    public List<int> NowCardsToIndex(int npc)
    {
        List<NPCInfo> npcs = Resource.npcs;
        if (npcs[npc].nowCards == null) return null;
        if (npcs[npc].nowCards.Count == 0) return null;
        List<int> indexs = new();
        for (int i = 0; i < npcs[npc].nowCards.Count; i++)
        {
            indexs.Add(npcs[npc].cards.IndexOf(npcs[npc].nowCards[i]));
        }
        return indexs;
    }

    public List<CardInfo> IndexToNowCards(int npc)
    {
        List<NPCInfo> npcs = Resource.npcs;
        if (data.entities[npc].nowCards == null) return null;
        if (data.entities[npc].nowCards.Count == 0) return null;
        List<CardInfo> indexs = new();
        for (int i = 0; i < data.entities[npc].nowCards.Count; i++)
        {
            indexs.Add(npcs[npc].cards[data.entities[npc].nowCards[i]]);
        }
        return indexs;
    }

    public static int GetScene()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }

    public static void RegisterNPC(NPC npc)
    {
        if (!instance.activeNPCs.Contains(npc)) instance.activeNPCs.Add(npc);
    }

    public static void DeRegisterNPC(NPC npc)
    {
        if (instance.activeNPCs.Contains(npc)) instance.activeNPCs.Remove(npc);
    }

    public static string ToHtmlStringRGBA(Color32 color)
    {
        return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", color.r, color.g, color.b, color.a);
    }

    public class WaitForScaledSeconds : CustomYieldInstruction
    {

        public WaitForScaledSeconds Wait(float seconds)
        {
            time = totalTime + seconds;
            return this;
        }

        public override bool keepWaiting
        {
            get
            {
                return totalTime < time;
            }
        }

        public double time;

    }

}

[Serializable]
public class SaveData
{
    // Global
    public string name;
    public List<CardInfo> cards;
    public int scene;

    // OneCard
    public bool nowCarding;
    public List<int> nowCards;
    public List<int> nowEntities;
    public bool nowPreview;
    public CardInfo nowCard;
    public bool nowPicked;
    public bool nowDamaged;
    public int nowTurn;
    public int nowStack;

    // Player
    public SaveVector position;
    public SaveVector velocity;
    public int health;
    public int damage;
    public int luck;
    public int money;

    // World
    public List<EntityData> entities;

}

[Serializable]
public struct EntityData
{
    public bool isValid;
    public string name;
    public SaveVector position;
    public List<CardInfo> cards;
    public List<int> nowCards;

    public EntityData(Vector2 position, string name, List<CardInfo> cards, List<int> nowCards, bool isValid = false)
    {
        this.isValid = isValid;
        this.name = name;
        this.position = new SaveVector(position);
        this.cards = cards;
        this.nowCards = nowCards;
    }
}

[Serializable]
public struct SaveVector
{
    public float x;
    public float y;

    public SaveVector(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public SaveVector(Vector2 vector)
    {
        x = vector.x;
        y = vector.y;
    }

    public Vector2 Get()
    {
        return new Vector2(x, y);
    }
}

public class Busy : IDisposable
{
    readonly int amount;
    
    public Busy(int amount)
    {
        this.amount = amount;
        GameManager.busyWorks += amount;
    }

    public void Dispose()
    {
        GameManager.busyWorks -= amount;
    }
}

