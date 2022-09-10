using System.Collections;
using UnityEngine;
using SeolMJ;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

public abstract class NPC : Carder
{

    public NPCInfo info;
    protected LogPreset NpcLog;

    void Start()
    {
        NpcLog = new("N-PC", GameManager.Resource.npcLogColor);
        info.position = transform.position;
        info.scene = GameManager.GetScene();
        int index = GameManager.Resource.npcs.IndexOf(info);
        if (G.data.entities[index].isValid)
        {
            transform.position = G.data.entities[index].position.Get();
        }
        else
        {
            ResetCards();
        }
        GameManager.RegisterNPC(this);
    }

    void OnDestroy()
    {
        Save();
        GameManager.DeRegisterNPC(this);
    }

    public void Save()
    {
        info.position = transform.position;
        info.scene = GameManager.GetScene();
    }

    public override void OnTurn()
    {
        StartCoroutine(OnOnTurn());
    }

    IEnumerator OnOnTurn()
    {
        if (C.carders.Count == 0) yield break;
        yield return new GameManager.WaitForScaledSeconds().Wait(Random.Range(0.25f, 0.5f));
        using var _ = new Busy(3);
    Retry:
        Task<int> pushTask = Task.Run(() => Think(in info.nowCards, false, CardInfo.Idle, info.sensitive, info.careful, info.quick));
        while (!pushTask.IsCompleted) yield return null;
        int push = pushTask.Result;

        if (push == -1)
        {
            if (C.stack > 0) AddCards(C.stack);
            else
            {
                Log($"Pick {info.nowCards.Count} + [1] = {info.nowCards.Count + 1}");
                info.nowCards.Add(C.Pick());
                C.Next();
                C.UpdateCarder(this, info.nowCards.Count);
                C.DamageCarder(this, 1);
            }
        }
        else
        {
            CardInfo last = C.lastCard;
            CardInfo card = info.nowCards[push];
            if (!C.Push(card, true, info.nowCards.Count < 2))
            {
                if (C.playing) goto Retry;
                else yield break;
            }
            CardModule(card, last, CardUtils.GetStack(card));
            C.NewCardStack(C.PreviewCard(card));
            info.nowCards.RemoveAt(push);
            C.UpdateCarder(this, info.nowCards.Count);
            C.PushCarder(this, 1);
            if (info.nowCards.Count == 0)
            {
                Log("½Â¸®!");
                C.Quit(this);
            }
            last = card;
            while (true)
            {
                pushTask = Task.Run(() => Think(in info.nowCards, true, last, info.sensitive, info.careful, info.quick));
                while (!pushTask.IsCompleted) yield return null;
                push = pushTask.Result;
                if (push == -1) break;
                card = info.nowCards[push];
                yield return new GameManager.WaitForScaledSeconds().Wait(Random.Range(0.25f, 0.5f));
                if (!C.Push(card, true, info.nowCards.Count < 2))
                {
                    if (C.playing) continue;
                    else yield break;
                }
                CardModule(card, last, CardUtils.GetStack(card));
                C.NewCardStack(C.PreviewCard(card));
                info.nowCards.RemoveAt(push);
                C.UpdateCarder(this, info.nowCards.Count);
                C.PushCarder(this, 1);
                if (info.nowCards.Count == 0)
                {
                    Log("½Â¸®!");
                    C.Quit(this);
                    break;
                }
                last = card;
            }
            C.Next();
        }
        
    }

    public override void AddCards(uint count)
    {
        for (int i = 0; i < count; i++)
        {
            CardInfo card = CardUtils.RandomCard(false);
            info.cards.Add(card);
            info.nowCards.Add(card);
        }

        Log($"Pick {info.nowCards.Count} + [{count}] = {count + info.nowCards.Count}");

        C.DamageCarder(this, (int)count);

        if (info.nowCards.Count > 19) Quit();
        else C.Damage();

        C.UpdateCarder(this, info.nowCards.Count);
    }

    public override int CardCount()
    {
        return info.nowCards.Count;
    }

    public override void Accept(bool resume)
    {
        if (!resume) info.nowCards = GetRandomCards();
        C.Join(this);
    }

    public List<CardInfo> GetRandomCards()
    {
        List<int> remain = new();
        List<CardInfo> result = new();
        for (int i = 0; i < info.cards.Count; i++) remain.Add(i);
        for (int i = 0; i < GameManager.Resource.defaultCardCount; i++)
        {
            if (remain.Count == 0)
            {
                Error($"Not Enough Card Remaining: Total {info.cards.Count}, Need {GameManager.Resource.defaultCardCount}");
                return result;
            }
            int random = Random.Range(0, remain.Count);
            result.Add(GetCard(remain[random]));
            remain.RemoveAt(random);
        }
        return result;
    }

    public CardInfo GetCard(int index)
    {
        if (index < 0 || info.cards.Count <= index) return new CardInfo();
        return info.cards[index];
    }

    public void ResetCards()
    {
        info.cards = new();
        for (int i = 0; i < GameManager.Resource.defaultCardCount; i++) info.cards.Add(CardUtils.RandomCard(false));
    }

    public void Quit()
    {
        Log("Å»¶ô");
        C.stack = 0;
        C.Quit(this);
        C.Next();
    }

    public void CardModule(CardInfo card, CardInfo lastCard, int stack)
    {
        Log4u.Log(NpcLog, $"{card} ¡ç {lastCard}, [{stack}] : {info.name}");
    }

    public void Log(string content)
    {
        Log4u.Log(NpcLog, $"{info.name} : {content}");
    }

    public void Error(string content)
    {
        Log4u.Error(NpcLog, $"{info.name} : {content}");
    }

    // AI

    public static int Think(in List<CardInfo> cards, bool lazy, CardInfo last, float sensitive, float careful, float quick)
    {
        int push = -1;
        List<int> available = new();

        if (lazy)
        {
            for (int i = 0; i < cards.Count; i++) if (CardUtils.Match(last.num, cards[i].num) && C.CheckCard(cards[i])) available.Add(i);
        }
        else
        {
            for (int i = 0; i < cards.Count; i++) if (C.CheckCard(cards[i])) available.Add(i);
        }

        if (available.Count == 0) return -1;

        if (Rand(sensitive))
        {
            List<int> attackable = new();

            for (int i = 0; i < available.Count; i++)
            {
                CardInfo card = cards[available[i]];
                if (CardUtils.Match(card.num, CardNum.Attack)) attackable.Add(available[i]);
            }
            
            if (attackable.Count > 0)
            {
                List<int> powerfuls = new();
                if (NextCount() < careful * 10f)
                {
                    int power = 0;
                    for (int i = 0; i < attackable.Count; i++)
                    {
                        int thisPower = Powerful(cards[available[i]]);
                        if (thisPower > power)
                        {
                            power = thisPower;
                            powerfuls.Clear();
                            powerfuls.Add(available[i]);
                        }
                        if (thisPower == power) powerfuls.Add(available[i]);
                    }
                }
                else
                {
                    int power = 6;
                    for (int i = 0; i < attackable.Count; i++)
                    {
                        int thisPower = Powerful(cards[available[i]]);
                        if (thisPower < power)
                        {
                            power = thisPower;
                            powerfuls.Clear();
                            powerfuls.Add(available[i]);
                        }
                        else if (thisPower == power) powerfuls.Add(available[i]);
                    }
                }

                if (powerfuls.Count > 0) push = powerfuls[Rand(powerfuls.Count)];
                else push = attackable[Rand(attackable.Count)];
            }
            else
            {
                if (NextCount() < careful * 10f)
                {
                    bool queenFirst = Rand(0.5f);
                    
                    if (queenFirst)
                    {
                        int queen = Contain(CardNum.Q, in cards, available);
                        if (queen == -1) queen = Contain(CardNum.J, in cards, available);
                        if (queen != -1) push = queen;
                    }
                    else
                    {
                        int jack = Contain(CardNum.J, in cards, available);
                        if (jack == -1) jack = Contain(CardNum.Q, in cards, available);
                        if (jack != -1) push = jack;
                    }
                }
            }
        }
        

        if (push != -1)
        {
            int king = Contain(cards[push], in cards, available);
            if (king != -1) push = king;
        }

        if (push == -1)
        {
            if (Rand(quick))
            {
                int king = Contain(CardNum.K, in cards, available);
                if (king != -1) push = king;
            }
            if (push == -1) push = available[Rand(available.Count)];
        }

        return push;
    }

    static System.Random rand = new();

    static bool Rand(float percent)
    {
        return rand.NextDouble() <= percent;
    }

    static int Rand(int max)
    {
        return rand.Next(max);
    }

    static int Powerful(CardInfo card)
    {
        if (CardUtils.Match(card.num, CardNum.A) && CardUtils.Match(card.type, CardType.Spade)) return 4;
        return card.num switch
        {
            CardNum.Two => 1,
            CardNum.A => 2,
            CardNum.BlackJoker => 3,
            CardNum.ColorJoker => 5,
            _ => 0
        };
    }

    static int NextCount()
    {
        if (C.carders.Count == 0) return -1;
        return C.carders[(C.turn + C.carders.Count + 1) % C.carders.Count].CardCount();
    }

    static int Contain(CardNum num, in List<CardInfo> cards, List<int> available)
    {
        List<int> cont = new();
        for (int i = 0; i < available.Count; i++)
        {
            if (CardUtils.Match(cards[available[i]].num, num)) cont.Add(available[i]);
        }
        if (cont.Count == 0) return -1;
        return cont[Rand(cont.Count)];
    }

    static int Contain(CardInfo card, in List<CardInfo> cards, List<int> available)
    {
        List<int> cont = new();
        for (int i = 0; i < available.Count; i++)
        {
            if (CardUtils.Match(cards[available[i]].num, card.num) && CardUtils.Match(cards[available[i]].type, card.type)) cont.Add(available[i]);
        }
        if (cont.Count == 0) return -1;
        return cont[Rand(cont.Count)];
    }

    //int[] Multiple()

}
