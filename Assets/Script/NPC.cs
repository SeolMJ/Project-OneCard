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
        NpcLog = new("NPC", GameManager.Resource.npcLogColor);
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
        Task<int> pushTask = Task.Run(() => Think());
        while (!pushTask.IsCompleted) yield return null;
        int push = pushTask.Result;

        if (push == -1)
        {
            if (C.stack > 0) AddCards(C.stack);
            else
            {
                Log("가능한 카드 없음. 새로 뽑음");
                info.nowCards.Add(C.Pick());
                C.Next();
                C.UpdateCarder(this, info.nowCards.Count);
                C.DamageCarder(this, 1);
            }
        }
        else
        {
            Log("카드 제출 완료");
            if (!C.Push(info.nowCards[push], true))
            {
                if (C.playing) goto Retry;
                else yield break;
            }
            C.NewCardStack(C.PreviewCard(info.nowCards[push]));
            info.nowCards.RemoveAt(push);
            C.UpdateCarder(this, info.nowCards.Count);
            C.PushCarder(this, 1);
            if (info.nowCards.Count == 0)
            {
                Log("승리!");
                C.Quit(this);
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

        C.DamageCarder(this, (int)count);

        Log($"막을 수 있는 카드 없음. 카드 {count}개 추가");

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
        Log("탈락");
        C.stack = 0;
        C.Quit(this);
        C.Next();
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

    public int Think()
    {
        int push = -1;
        List<int> available = new();

        for (int i = 0; i < info.nowCards.Count; i++) if (C.CheckCard(info.nowCards[i])) available.Add(i);

        if (available.Count == 0) return -1;

        if (Rand(info.sensitive))
        {
            List<int> attackable = new();

            for (int i = 0; i < available.Count; i++)
            {
                CardInfo card = info.nowCards[available[i]];
                if (CardUtils.Match(card.num, CardNum.Attack)) attackable.Add(available[i]);
            }
            
            if (attackable.Count > 0)
            {
                List<int> powerfuls = new();
                if (NextCount() < info.careful * 10f)
                {
                    int power = 0;
                    for (int i = 0; i < attackable.Count; i++)
                    {
                        int thisPower = Powerful(info.nowCards[available[i]]);
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
                        int thisPower = Powerful(info.nowCards[available[i]]);
                        if (thisPower < power)
                        {
                            power = thisPower;
                            powerfuls.Clear();
                            powerfuls.Add(available[i]);
                        }
                        else if (thisPower == power) powerfuls.Add(available[i]);
                    }
                }

                if (powerfuls.Count > 0) push = powerfuls[TrashRandom(powerfuls.Count)];
                else push = attackable[TrashRandom(attackable.Count)];
            }
            else
            {
                if (NextCount() < info.careful * 10f)
                {
                    bool queenFirst = Rand(0.5f);
                    
                    if (queenFirst)
                    {
                        int queen = Contain(CardNum.Q, available);
                        if (queen == -1) queen = Contain(CardNum.J, available);
                        if (queen != -1) push = queen;
                    }
                    else
                    {
                        int jack = Contain(CardNum.J, available);
                        if (jack == -1) jack = Contain(CardNum.Q, available);
                        if (jack != -1) push = jack;
                    }
                }
            }
        }
        

        if (push != -1)
        {
            int king = Contain(info.nowCards[push], available);
            if (king != -1) push = king;
        }

        if (push == -1)
        {
            if (Rand(info.quick))
            {
                int king = Contain(CardNum.K, available);
                if (king != -1) push = king;
            }
            if (push == -1) push = available[TrashRandom(available.Count)];
        }

        return push;
    }

    bool Rand(float percent)
    {
        return new System.Random().NextDouble() <= percent;
    }

    int TrashRandom(int max)
    {
        return new System.Random().Next(max);
    }

    int Powerful(CardInfo card)
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

    int NextCount()
    {
        if (C.carders.Count == 0) return -1;
        return C.carders[(C.turn + C.carders.Count + 1) % C.carders.Count].CardCount();
    }

    int Contain(CardNum num, List<int> available)
    {
        List<int> cont = new();
        for (int i = 0; i < available.Count; i++)
        {
            if (CardUtils.Match(info.nowCards[available[i]].num, num)) cont.Add(available[i]);
        }
        if (cont.Count == 0) return -1;
        return cont[TrashRandom(cont.Count)];
    }

    int Contain(CardInfo card, List<int> available)
    {
        List<int> cont = new();
        for (int i = 0; i < available.Count; i++)
        {
            if (CardUtils.Match(info.nowCards[available[i]].num, card.num) && CardUtils.Match(info.nowCards[available[i]].type, card.type)) cont.Add(available[i]);
        }
        if (cont.Count == 0) return -1;
        return cont[TrashRandom(cont.Count)];
    }

}
