﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SeolMJ;
using static SeolMJ.CardUtils;

public class CardManager : MonoBehaviour, ITweenable
{

    public static CardManager instance;

    [Header("Settings")]
    public float cardSpeed;
    public float cardFadeSpeed;
    public float symbolDelay;

    [Header("Resources")]
    public CanvasGroup cardGroup;
    public CanvasGroup cardSystemGroup;
    public RectTransform cardParent;
    public RectTransform attackCardParent;

    [Header("My Turn")]
    public PopAnimator myTurnAnimator;
    public ParticleSystem myTurnParticle;

    [Header("OneCard")]
    public PopAnimator oneCardAnimator;
    public ParticleSystem oneCardParticle;

    [Header("Preview")]
    public Transform previewParent;
    public RectTransform previewPivot;

    [Header("Carders")]
    public GameObject carderPrefab;
    public Transform cardersParent;
    public List<RectTransform> carderRects;
    public float carderDistance;
    public float carderSpeed;

    [Header("Scene Resources")]
    public GameObject[] cardObjects;
    public GameObject[] idleObjects;

    [Header("Debug")]
    public bool playing;
    public CardInfo lastCard;
    public uint stack;
    public int turn;
    public Card previewCard;
    public bool opposite;
    public List<Carder> carders;
    public Stack<Card> inactiveCards = new(10);
    public Stack<Image> inactiveSymbols = new(32);

    private float angleOffset;

    #region Tween
    public Tween.Action TweenAction { get => action; set => action = value; }
    private Tween.Action action;
    private float progress;

    public void OnTween(float progress)
    {
        this.progress = 0f;
    }
    #endregion

    void Awake()
    {
        instance = this;
    }

    IEnumerator Start()
    {
        SaveData data = GameManager.instance.data;
        if (data.nowCarding)
        {
            Log("Game Resuming", 1);
            cardParent.gameObject.SetActive(true);
            cardSystemGroup.gameObject.SetActive(true);
            lastCard = data.nowCard;
            turn = data.nowTurn;
            stack = (uint)data.nowStack;
            Log($"{data.nowCards.Count} cards, turn {turn}, stack {stack}", 2);
            Log($"{data.nowEntities.Count} NPCs loading", 4);
            int iteration = 0;
            while (data.nowEntities.Count > GameManager.instance.activeNPCs.Count)
            {
                iteration++;
                if (iteration > 60) break;
                yield return null;
            }
            foreach (int index in data.nowEntities)
            {
                if (GameManager.instance.FindNPC(data.entities[index].name, out NPC npc))
                {
                    Log($"NPC: '{data.entities[index].name}'(={npc.name}) in game", 2);
                    npc.Accept(true);
                }
                else
                {
                    Error($"NPC: '{data.entities[index].name}' not loaded. {GameManager.instance.activeNPCs.Count} Active NPCs", 2);
                }
            }
            Player.instance.Accept(true);
            if (data.nowPreview)
            {
                Card card = PreviewCard(data.nowCard);
                NewCardStack(card);
                if (data.nowDamaged) OnDone(card);
                if (data.nowPicked) OnPick();
            }
            Log("Game Resumed", 3);
        }
    }

    void OnEnable()
    {
        
    }

    void OnDisable()
    {
        
    }

    void Update()
    {
        if (carders.Count == 0) return;
        for (int i = 0; i < carders.Count; i++) carderRects[i].localScale = Vector3.Lerp(carderRects[i].localScale, i == turn ? Vector3.one * 1.5f : Vector3.one, GameManager.deltaTime * carderSpeed);
    }

    // Core

    public void Resume()
    {
        action = null;

        cardParent.gameObject.SetActive(true);
        cardSystemGroup.gameObject.SetActive(true);
        cardGroup.alpha = 1;
        cardSystemGroup.alpha = 1;

        playing = true;

        foreach (Carder carder in carders) UpdateCarder(carder, carder.CardCount());
        GiveTurn();

        Log("Game Resumed");
    }

    public void Play()
    {
        stack = 0;
        turn = 0;
        opposite = false;
        lastCard = RandomCard(true);

        if (previewCard) ReturnCard(previewCard);
        previewCard = null;

        cardParent.gameObject.SetActive(true);
        cardSystemGroup.gameObject.SetActive(true);
        Tween.Run(this, DoPlay);

        playing = true;

        previewCard = PreviewCard(lastCard);

        foreach (Carder carder in carders) UpdateCarder(carder, carder.CardCount());
        GiveTurn();

        Log("Game Started");
    }

    void DoPlay()
    {
        if (progress < 1f)
        {
            progress += GameManager.deltaTime;
            cardGroup.alpha = Mathf.Lerp(0f, 1f, Tween.instance.sticky.Evaluate(progress));
            cardSystemGroup.alpha = cardGroup.alpha;
            return;
        }
        action = null;
    }

    public bool Push(CardInfo card, bool lazy = false, bool last = false)
    {
        if (carders.Count < 2)
        {
            Stop();
            return false;
        }
        if (!Match(card.type, lastCard.type) && !Match(card.num, lastCard.num)) return false;
        if (stack != 0)
        {
            if (!Match(card.num, CardNum.AND))
            {
                return false;
            }
            else if (Match(card.num, CardNum.Attack))
            {
                if (lastCard.num == CardNum.A)
                {
                    if (lastCard.type == CardType.Spade)
                    {
                        if (card.type == CardType.Spade && lastCard.num == CardNum.A) stack += 5;
                        else if (Match(card.num, CardNum.Joker)) Stack(card);
                        else return false;
                    }
                    else if (Match(card.num, CardNum.DefendA)) Stack(card);
                    else return false;
                }
                else if (Match(lastCard.num, CardNum.Joker))
                {
                    if (lastCard.num == CardNum.BlackJoker)
                    {
                        if (Match(card.num, CardNum.Joker)) Stack(card);
                        else if (card.num == CardNum.A && card.type == CardType.Spade) Stack(card);
                        else return false;
                    }
                    else
                    {
                        if (card.num == CardNum.ColorJoker) Stack(card);
                        else return false;
                    }
                }
                else if (lastCard.num == CardNum.Two && Match(card.num, CardNum.DefendTwo)) Stack(card);
                else return false;
            }
            else if (lastCard.num == CardNum.Two && card.num == CardNum.Three) stack = 0;
            else return false;
        }
        else if (Match(card.num, CardNum.Attack))
        {
            if (card.type == CardType.Spade && card.num == CardNum.A) stack += 5;
            else Stack(card);
        }

        // Specials
        if (card.num == CardNum.J)
        {
            if (lazy) turn += opposite ? -2 : 2;
            else turn += opposite ? -1 : 1;
        }
        else if (card.num == CardNum.Q) opposite = !opposite;
        else if (card.num == CardNum.K && !last) turn += opposite ? 1 : -1;
        turn = (turn + carders.Count) % carders.Count;

        // End
        lastCard = card;
        if (!lazy) Next();
        return true;
    }

    public void Next(bool plus = true)
    {
        if (carders.Count <= 1)
        {
            Stop();
            return;
        }
        if (plus) turn = (turn + (opposite ? -1 : 1) + carders.Count) % carders.Count;
        GiveTurn();
    }

    public void Damage(bool lazy = false)
    {
        stack = 0;
        if (previewCard) OnDone(previewCard);
        if (!lazy) Next();
    }

    public void Join(Carder carder)
    {
        carders.Add(carder);
        ReLayout();
    }

    public void Quit(Carder carder)
    {
        int index = carders.IndexOf(carder);
        carders.RemoveAt(index);
        Destroy(carderRects[index].gameObject);
        carderRects.RemoveAt(index);
        if (carders.Count <= 1)
        {
            Stop();
            return;
        }
        ReLayout();
    }

    public void Stop()
    {
        if (!playing) return;

        if (carders.Count > 0) for (int i = carders.Count - 1; i >= 0; i--) Destroy(carderRects[i].gameObject);
        carders.Clear();

        foreach (RectTransform rects in carderRects) Destroy(rects.gameObject);
        carderRects.Clear();

        if (previewCard) previewCard.Kill();
        previewCard = null;
        Player.instance.UpdateStatus(false);

        Tween.Run(this, DoStop);

        PartyField.instance.Close();

        playing = false;

        Log("Game Ended");
    }

    void DoStop()
    {
        if (progress < 1f)
        {
            progress += GameManager.deltaTime;//
            cardGroup.alpha = Mathf.Lerp(1f, 0f, Tween.instance.sticky.Evaluate(progress));
            cardSystemGroup.alpha = cardGroup.alpha;
            return;
        }
        cardParent.gameObject.SetActive(false);
        cardSystemGroup.gameObject.SetActive(false);
        action = null;
    }

    // Cards

    public CardInfo Pick()
    {
        CardInfo card = RandomCard();
        OnPick();
        return card;
    }

    public Card GetCard(CardInfo info, Transform parent, CardInitMode mode = CardInitMode.Normal)
    {
        if (inactiveCards.Count == 0)
        {
            GameObject obj = Instantiate(GameManager.Resource.cardPrefab, parent);
            Card card = obj.GetComponent<Card>();
            card.Init(info, mode);
            return card;
        }
        else
        {
            Card card = inactiveCards.Pop();
            if (!card) return GetCard(info, parent, mode);
            card.transform.SetParent(parent, false);
            card.transform.SetAsLastSibling();
            card.transform.localRotation = Quaternion.identity;
            card.gameObject.SetActive(true);
            card.Init(info, mode);
            return card;
        }
    }

    public void ReturnCard(Card card)
    {
        card.gameObject.SetActive(false);
        inactiveCards.Push(card);
    }

    public void ReLayout()
    {
        angleOffset = 360f / carders.Count;
        for (int i = 0; i < carders.Count; i++)
        {
            if (carderRects.Count <= i) carderRects.Add(Instantiate(carderPrefab, cardersParent).GetComponent<RectTransform>());
            carderRects[i].anchoredPosition = new Vector3(Mathf.Cos(i * angleOffset * Mathf.Deg2Rad), Mathf.Sin(i * angleOffset * Mathf.Deg2Rad), 0) * carderDistance;
            carderRects[i].GetComponent<Image>().color = carders[i] is Player ? Color.red : Color.white;
            carderRects[i].GetComponentInChildren<TMP_Text>().text = carders[i].CardCount().ToString();
        }
    }

    public void UpdateCarder(Carder carder, int cardCount)
    {
        // Update Count
        int index = carders.IndexOf(carder);
        if (index == -1) return;
        carderRects[index].GetComponentInChildren<TMP_Text>().text = cardCount.ToString();
    }

    public void DamageCarder(Carder carder, int count)
    {
        int index = carders.IndexOf(carder);
        if (index == -1) return;
        Vector2 position = carderRects[index].position;
        for (int i = 0; i < count; i++)
        {
            MiniCardRenderer.Add(previewPivot.position, position, i * -0.1f);
        }
    }

    public void PushCarder(Carder carder)
    {
        int index = carders.IndexOf(carder);
        if (index == -1) return;
        Vector3 position = carderRects[index].position;
        MiniCardRenderer.Add(position, previewPivot.position, 0f);
    }

    public void PushCarder(Carder carder, int count)
    {
        int index = carders.IndexOf(carder);
        if (index == -1) return;
        Vector3 position = carderRects[index].position;
        for (int i = 0; i < count; i++)
        {
            MiniCardRenderer.Add(position, previewPivot.position, i * -0.1f);
        }
    }

    public void NewCardStack(Card card)
    {
        if (previewCard) previewCard.ReturnAndKill();
        previewCard = card;
    }

    public Card PreviewCard(CardInfo card)
    {
        Card newCard = GetCard(card, previewParent, CardInitMode.Return);
        newCard.MoveTo(Vector2.up * (cardParent.sizeDelta.y / 2f + 440f));
        return newCard;
    }
    
    public bool CheckCard(CardInfo card)
    {
        if (!Match(card.type, lastCard.type) && !Match(card.num, lastCard.num)) return false;
        if (stack != 0)
        {
            if (!Match(card.num, CardNum.AND)) return false;
            else if (Match(card.num, CardNum.Attack))
            {
                if (lastCard.num == CardNum.A)
                {
                    if (lastCard.type == CardType.Spade)
                    {
                        if (card.type == CardType.Spade && lastCard.num == CardNum.A) return true;
                        else if (card.num == CardNum.Joker) return true;
                        else return false;
                    }
                    else if (Match(card.num, CardNum.DefendA)) return true;
                    else return false;
                }
                else if (Match(lastCard.num, CardNum.Joker))
                {
                    if (lastCard.num == CardNum.BlackJoker)
                    {
                        if (Match(card.num, CardNum.Joker)) return true;
                        else if (card.num == CardNum.A && card.type == CardType.Spade) return true;
                        else return false;
                    }
                    else
                    {
                        if (card.num == CardNum.ColorJoker) return true;
                        else return false;
                    }
                }
                else if (lastCard.num == CardNum.Two && Match(card.num, CardNum.DefendTwo)) return true;
                else return false;
            }
            else if (!(lastCard.num == CardNum.Two && card.num == CardNum.Three)) return false;
        }
        return true;
    }

    // Efects

    public void OnDone(Card card, bool set = true)
    {
        if (!card) return;
        if (set) card.image.sprite = GameManager.Resource.doneSprite;
        Card doneCard = GetCard(CardInfo.Idle, card.transform, CardInitMode.Direct);
        doneCard.image.sprite = GameManager.Resource.doneCardSprite;
        doneCard.transform.localPosition = Vector3.zero;
    }

    public void OnPick()
    {
        Card card = GetCard(CardInfo.Idle, previewParent, CardInitMode.Popup);
        card.MoveTo(Vector2.up * (cardParent.sizeDelta.y / 2f + 440f));
        card.image.sprite = GameManager.Resource.pickCardSprite;
    }

    #region Logging

    public static LogPreset? logPreset;

    public static void Log(string content, byte state = 0)
    {
        logPreset ??= new("Card", GameManager.Resource.cardLogColor);
        Log4u.Log(logPreset.Value, content, state);
    }

    public static void Error(string content, byte state = 0)
    {
        logPreset ??= new("Card", GameManager.Resource.cardLogColor);
        Log4u.Error(logPreset.Value, content, state);
    }

    #endregion

}

[Serializable]
public struct CardInfo
{
    public CardType type;
    public CardNum num;

    public CardInfo(CardType type, CardNum num)
    {
        this.type = type;
        this.num = num;
    }

    public static CardInfo Idle => new(CardType.None, CardNum.None);

    public static bool operator==(CardInfo left, CardInfo right) => left.type == right.type && left.num == right.num;
    public static bool operator!=(CardInfo left, CardInfo right) => left.type != right.type || left.num != right.num;

    public override bool Equals(object obj) => base.Equals(obj);
    public override int GetHashCode() => base.GetHashCode();
    public override string ToString() => $"[{TypeToString()}|{NumToString()}]";

    public char TypeToString() =>
        type switch
        {
            CardType.None => '_',
            CardType.Diamond => '◆',
            CardType.Spade => '♠',
            CardType.Heart => '♥',
            CardType.Clover => '♣',
            CardType.Color => '□',
            CardType.Black => '■',
            _ => '_'
        };

    public char NumToString() =>
        num switch
        {
            CardNum.None => '_',
            CardNum.A => 'A',
            CardNum.Two => '2',
            CardNum.Three => '3',
            CardNum.Four => '4',
            CardNum.Five => '5',
            CardNum.Six => '6',
            CardNum.Seven => '7',
            CardNum.Eight => '8',
            CardNum.Nine => '9',
            CardNum.Ten => 'X',
            CardNum.J => 'J',
            CardNum.Q => 'Q',
            CardNum.K => 'K',
            CardNum.ColorJoker => 'C',
            CardNum.BlackJoker => 'B',
            _ => '_',
        };

}

[Flags]
public enum CardType
{
    None    = 0,
    Diamond = 1 << 0,
    Spade   = 1 << 1,
    Heart   = 1 << 2,
    Clover  = 1 << 3,
    Black   = Spade   | Clover,
    Color   = Diamond | Heart,
    All     = Diamond | Spade | Heart | Clover
}

[Flags]
public enum CardNum
{
    None       = 0,
    A          = 1 << 0,
    Two        = 1 << 1,
    Three      = 1 << 2,
    Four       = 1 << 3,
    Five       = 1 << 4,
    Six        = 1 << 5,
    Seven      = 1 << 6,
    Eight      = 1 << 7,
    Nine       = 1 << 8,
    Ten        = 1 << 9,
    J          = 1 << 10,
    Q          = 1 << 11,
    K          = 1 << 12,
    BlackJoker = 1 << 13,
    ColorJoker = 1 << 14,
    Specials   = J          | Q          | K,
    Attack     = A          | Two        | ColorJoker | BlackJoker,
    AND        = A          | Two        | Three      | BlackJoker | ColorJoker,
    DefendTwo  = A          | Two        | BlackJoker | ColorJoker,
    DefendA    = A          | BlackJoker | ColorJoker,
    Joker      = BlackJoker | ColorJoker
}