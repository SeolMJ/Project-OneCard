using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SeolMJ;
using static SeolMJ.Utils;
using static SeolMJ.CardUtils;

public class Player : Carder
{

    public static Player instance;

    [Header("Camera")]
    public new Camera camera;
    public Transform cameraTransform;
    public Vector3 cameraOffset;
    public float cameraSpeed;

    [Header("Cards")]
    public Transform cardHilight;
    public float cardMinDist;
    public AnimationCurve cardHeightCurve;
    public RectTransform multiSelectRect;

    [Header("Movement")]
    public new Rigidbody2D rigidbody;
    public float moveSpeed;
    public float moveSmooth;

    [Header("Mode")]
    public RectTransform[] modeRects;
    public string[] modeContents;
    public TMP_Text modeText;
    public RectTransform modeTextRect;
    public CanvasGroup modeTextGroup;
    public Vector3 modeSpeed;

    [Header("Debug")]
    public List<Card> cards;

    // Global
    [HideInInspector] public bool carding;
    private Vector2 mousePos;

    // Camera
    private Vector3 cameraVel;

    // Cards
    private int cardMode; // 0 = Cards, 1 = Move, 2 = Idle, 
    private bool selected;
    private Card selectedCard;
    private bool lazySelected;
    private int lazyturn;
    private Coroutine initCardRoutine;

    // Attack
    private bool attackCardOpened;

    // Movement
    [HideInInspector] public Vector2 velocity, velocityVel;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (!G)
        {
            SceneLoader.Return();
            return;
        }

        // Load
        transform.position = G.data.position.Get();
        rigidbody.velocity = G.data.velocity.Get();

        // Reset
        if (!camera) camera = G.GetComponent<Camera>();
        cards = new List<Card>();
        cardMode = 0;

        // Else
        Canvas.GetDefaultCanvasMaterial().enableInstancing = true;
    }

    void Update()
    {
        if (GameManager.timeScale == 0) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(carding ? cardManager.cardParent : cardManager.attackCardParent, Input.mousePosition, camera, out mousePos);
        mousePos += (Vector2)transform.position;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!carding) Party(4);
        }

        if (carding) // Cards
        {
            UpdateMode();
            switch (cardMode)
            {
                case 0:
                    UpdateSelection();
                    UpdateSkip();
                    break;
                case 2:
                    UpdateSelection();
                    break;
            }
            UpdateCard();
            UpdateModeVisual();
        }
        else // Movement
        {
            UpdateMovement();
            // UpdateAttackCard();
        }
    }

    void LateUpdate()
    {
        Physics2D.Simulate(GameManager.deltaTime);

        UpdateCamera();
    }

    public void AddCard(CardInfo card)
    {
        Card newCard = cardManager.GetCard(card, cardManager.cardParent);
        newCard.Init(card);
        newCard.MoveTo(Vector2.down * (cardManager.cardParent.sizeDelta.y / 2f + 440f));
        cards.Add(newCard);
        cardHilight.SetAsLastSibling();
    }

    public void PickCard(CardInfo card)
    {
        GameManager.AddCard(card);
        AddCard(card);
    }

    public void ClearCard()
    {
        if (cards.Count == 0) return;
        foreach (Card card in cards) card.Kill();
        cards.Clear();
    }

    IEnumerator InitCard()
    {
        List<CardInfo> newCards = GameManager.GetRandomCards();
        for (int i = 0; i < GameManager.Resource.defaultCardCount; i++)
        {
            AddCard(newCards[i]);
            yield return new GameManager.WaitForScaledSeconds().Wait(0.05f);
        }
        cardManager.UpdateCarder(this, cards.Count);
    }

    public void InitCardForce()
    {
        for (int i = 0; i < G.data.nowCards.Count; i++)
        {
            AddCard(GameManager.GetCard(G.data.nowCards[i]));
        }
        cardManager.UpdateCarder(this, cards.Count);
    }

    public void UpdateStatus(bool status, bool fromData = false)
    {
        carding = status;
        ChangeSet(carding);
        if (carding)
        {
            cardManager.Join(this);

            if (fromData) cardManager.Resume();
            else cardManager.Play();

            if (initCardRoutine != null) StopCoroutine(initCardRoutine);

            if (fromData) InitCardForce();
            else initCardRoutine = StartCoroutine(InitCard());
        }
        else
        {
            if (initCardRoutine != null) StopCoroutine(initCardRoutine);
            ClearCard();
        }
    }

    public void UpdateMovement()
    {
        Vector2 keyInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        velocity = keyInput == Vector2.zero
            ? Vector2.zero
            : Vector2.SmoothDamp(velocity, keyInput.normalized, ref velocityVel, moveSmooth);
        if (keyInput != Vector2.zero) rigidbody.velocity += moveSpeed * GameManager.deltaTime * velocity.normalized;
        else velocityVel = Vector2.zero;
    }

    /*
    public void UpdateAttackCard()
    {
        if (attackCards.Count == 0) return;
        attackCardOpened = mousePos.y <= -320f;
        float bottom = -cardManager.attackCardParent.sizeDelta.y / 2f + 32f;
        float deltaTime = GameManager.deltaTime * cardManager.cardSpeed;
        for (int i = 0; i < attackCards.Count; i++)
        {
            float index = attackCards.Count == 1 ? 0f : ((float)i / (attackCards.Count - 1) - 0.5f);
            attackCards[i].thisRect.anchoredPosition = Vector2.Lerp(attackCards[i].thisRect.anchoredPosition
                , attackCardOpened
                ? new Vector2(960f * index, bottom + index * index * -280f)
                : new Vector2(0, bottom - 240f)
                , deltaTime * Mathf.Sqrt(Mathf.Sqrt(i + 1)));
            attackCards[i].transform.localRotation = Quaternion.Lerp(attackCards[i].transform.localRotation
                , Quaternion.AngleAxis(attackCardOpened
                ? index * -30f
                : 0
                , Vector3.forward), deltaTime);
        }
    }
    */

    public void UpdateCamera()
    {
        cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, transform.position + cameraOffset, ref cameraVel, cameraSpeed);
    }

    public void UpdateMode()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            cardMode -= Mathf.RoundToInt(Input.mouseScrollDelta.y);
            cardMode = (cardMode + 3) % 3;
            switch (cardMode)
            {
                case 0:
                    selected = false;
                    cardHilight.gameObject.SetActive(true);
                    break;
                case 1:
                    break;
                case 2:
                    selected = false;
                    cardHilight.gameObject.SetActive(true);
                    break;
            }
            modeText.text = modeContents[cardMode];
            modeTextGroup.alpha = 1f;
            modeText.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();
        }
    }

    public void UpdateSelection()
    {
        if (Input.GetMouseButton(0) && selectedCard)
        {
            selected = true;
            selectedCard.MoveTo(Vector2.Lerp(selectedCard.thisRect.anchoredPosition, mousePos, GameManager.deltaTime * cardManager.cardSpeed));
            selectedCard.RotateTo(Quaternion.Lerp(selectedCard.transform.rotation, Quaternion.identity, GameManager.deltaTime * cardManager.cardSpeed));
            if (cardManager.previewCard && cardManager.carders.Contains(this) && Vector2.Distance(cardManager.previewCard.thisRect.anchoredPosition, selectedCard.thisRect.anchoredPosition) <= cardMinDist && (cardManager.carders[GetTurn()] == this || lazySelected) && cardMode == 0)
            {
                if (!cardHilight.gameObject.activeInHierarchy) cardHilight.gameObject.SetActive(true);
                cardHilight.position = cardManager.previewCard.transform.position;
                cardHilight.rotation = cardManager.previewCard.transform.rotation;
            }
            else if(cardHilight.gameObject.activeInHierarchy) cardHilight.gameObject.SetActive(false);
        }
        if (Input.GetMouseButtonUp(0) && selected && selectedCard)
        {
            cardHilight.gameObject.SetActive(true);
            Vector2 cardPos = selectedCard.thisRect.anchoredPosition;
            if (cardManager.previewCard && Vector2.Distance(cardManager.previewCard.thisRect.anchoredPosition, cardPos) <= cardMinDist && cardMode == 0)
            {
                if (cardManager.carders.Contains(this) && (cardManager.carders[GetTurn()] == this || lazySelected))
                {
                    bool pushed;
                    if (Input.GetKey(KeyCode.Space))
                    {
                        if (Match(selectedCard.num, cardManager.lastCard.num) || !lazySelected)
                        {
                            pushed = cardManager.Push(new CardInfo(selectedCard.type, selectedCard.num), true);
                            if (!lazySelected) lazyturn = GetTurn();
                            if (pushed)
                            {
                                lazySelected = true;
                                cardManager.UpdateCarder(this, cards.Count);
                                cardManager.PushCarder(this);
                            }
                        }
                        else pushed = false;
                    }
                    else pushed = cardManager.Push(new CardInfo(selectedCard.type, selectedCard.num));
                    if (pushed)
                    {
                        cards.Remove(selectedCard);
                        selectedCard.transform.SetParent(cardManager.previewParent);
                        selectedCard.Return();
                        cardManager.NewCardStack(selectedCard);
                        selectedCard = null;
                        cardManager.UpdateCarder(this, cards.Count);
                        cardManager.PushCarder(this);
                        if (cards.Count == 0) cardManager.Quit(this);
                        else if (cards.Count == 1)
                        {
                            cardManager.oneCardAnimator.Animate();
                            cardManager.oneCardParticle.Emit(50);
                        }
                    }
                    else
                    {
                        cardManager.OnDone(selectedCard, false);
                        Log("That card can't be used");
                    }
                }
            }
            else
            {
                if (cards.Count > 1)
                {
                    cards.Remove(selectedCard);
                    float min = Mathf.Infinity;
                    Card closest = null;
                    for (int i = 0; i < cards.Count; i++)
                    {
                        float offset = Mathf.Abs(cards[i].thisRect.anchoredPosition.x - cardPos.x);
                        if (offset < min)
                        {
                            min = offset;
                            closest = cards[i];
                        }
                    }
                    if (closest) cards.Insert(cards.IndexOf(closest) + (cardPos.x - closest.thisRect.anchoredPosition.x > 0 ? 1 : 0), selectedCard);
                    else cards.Insert(cards.Count - 1, selectedCard);
                }
            }
            selected = false;
        }
        if (!Input.GetKey(KeyCode.Space) && lazySelected)
        {
            cardManager.Next(lazyturn == GetTurn());
            lazySelected = false;
        }
        multiSelectRect.anchoredPosition = Vector2.Lerp(multiSelectRect.anchoredPosition, new Vector2(0, Input.GetKey(KeyCode.Space) && cardMode == 0 ? 0 : -128f), GameManager.deltaTime * 10f);
    }

    public void UpdateSkip()
    {
        if (Input.GetMouseButtonDown(1) && cardManager.carders[GetTurn()] == this)
        {
            if (cardManager.stack > 0) AddCards(cardManager.stack);
            else
            {
                if (cards.Count < 20) PickCard(cardManager.Pick());
                cardManager.UpdateCarder(this, cards.Count);
                cardManager.Next();
                cardManager.DamageCarder(this, 1);
            }
        }
    }

    public void UpdateCard()
    {
        if (cards.Count == 0)
        {
            cardHilight.gameObject.SetActive(false);
            selectedCard = null;
            return;
        }
        float canvasWidth = cardManager.cardParent.sizeDelta.x;
        float width = cards.Count * 400f + (cards.Count - 1) * 25f;
        float halfWidth = width / 2f;
        float bottom = -cardManager.cardParent.sizeDelta.y / 2f;
        float deltaTime = GameManager.deltaTime * cardManager.cardSpeed;
        float mouseHeight = cardMode switch
        {
            0 => (Input.mousePosition.y / Screen.height * 4f - 0.25f) * (selected ? 0.1f : 1f),
            1 => 0,
            2 => Input.mousePosition.y / Screen.height * 4f - 0.25f,
            _ => 0
        };
        int count = cards.Count;
        float mul = 1f;
        float mousePosHalf = mousePos.y + 540f;
        if (count < 4 && count > 1) mul /= count switch
        {
            2 => 5 - count,
            3 => 4.5f - count,
            _ => 1f
        };
        float minDist = Mathf.Infinity;
        int closest = 0;
        for (int i = 0; i < count; i++)
        {
            if (selected && cards[i] == selectedCard) continue;
            float index = count == 1 ? 0f : ((float)i / (count - 1) - 0.5f) * mul;
            float pos = 200f + i * 425f - mousePos.x / canvasWidth * width;
            float offset = (pos - halfWidth - mousePos.x) / canvasWidth;
            float posX = Mathf.Abs(cards[i].thisRect.anchoredPosition.x - mousePos.x);
            float perlinPlusIndex = index * 10f + Time.time * 0.2f;
            float perlinMinusIndex = index * 10f - Time.time * 0.2f;
            if (posX < minDist)
            {
                minDist = posX;
                closest = i;
            }
            cards[i].thisRect.anchoredPosition = Vector2.Lerp(cards[i].thisRect.anchoredPosition
                , Vector2.Lerp(new Vector2(960f * index, bottom + index * index * -240f)
                , new Vector2(pos - halfWidth
                , mousePosHalf * cardHeightCurve.Evaluate(mousePosHalf / 1080f) + (1 - Mathf.Abs(offset)) * 320f - 700f)
                , mouseHeight) + new Vector2(Mathf.PerlinNoise(perlinPlusIndex, perlinMinusIndex) * 32f
                , Mathf.PerlinNoise(perlinMinusIndex, perlinPlusIndex) * 32f)
                , deltaTime);
            cards[i].transform.localRotation = Quaternion.Lerp(cards[i].transform.localRotation
                , Quaternion.AngleAxis(Mathf.Lerp(index * -30f, offset * -10f, mouseHeight), Vector3.forward), deltaTime);
        }
        if (cardMode != 0 && cardMode != 2)
        {
            selectedCard = null;
            if (cardHilight.gameObject.activeInHierarchy) cardHilight.gameObject.SetActive(false);
            return;
        }
        if (selected) return;
        selectedCard = cards[closest];
        cardHilight.position = selectedCard.transform.position;
        cardHilight.rotation = selectedCard.transform.rotation;
        if (selectedCard.transform.GetSiblingIndex() != cardManager.cardParent.childCount - 2) selectedCard.transform.SetSiblingIndex(cardManager.cardParent.childCount - 2);
    }

    public void UpdateModeVisual()
    {
        for (int i = 0; i < modeRects.Length; i++)
        {
            modeRects[i].localScale = Vector3.Lerp(modeRects[i].localScale, cardMode == i ? Vector3.one : new Vector3(0.75f, 0.75f, 1f), modeSpeed.z * GameManager.deltaTime);
        }
        if (modeTextGroup.alpha == 0) return;
        Vector2 pos = modeRects[cardMode].anchoredPosition + Vector2.right * 96f;
        modeTextGroup.alpha = Mathf.MoveTowards(modeTextGroup.alpha, 0, GameManager.deltaTime * modeSpeed.x);
        modeTextRect.anchoredPosition = Vector2.Lerp(modeTextRect.anchoredPosition, pos, GameManager.deltaTime * modeSpeed.y);
    }

    // Inherit Methods

    public override void OnTurn()
    {
        Log("My Turn!");
    }

    public override void AddCards(uint count)
    {
        for (int i = 0; i < count; i++)
        {
            PickCard(RandomCard());
            if (cards.Count > 19)
            {
                Log("Lose");
                cardManager.Quit(this);
                break;
            }
        }
        cardManager.DamageCarder(this, (int)count);
        cardManager.UpdateCarder(this, cards.Count);
        cardManager.Damage();
    }

    public override int CardCount()
    {
        return cards.Count;
    }

    public override void Accept(bool resume)
    {
        cardManager.Ready(() => UpdateStatus(true, resume));
    }

    #region Logging

    public static new LogPreset? logPreset;

    public static new void Log(string content, byte state = 0)
    {
        logPreset ??= new("Player", GameManager.Resource.playerLogColor);
        Log4u.Log(logPreset.Value, content, state);
    }

    #endregion

}
