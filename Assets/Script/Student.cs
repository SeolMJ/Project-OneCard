using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class Student : Agent
{
    public Agent agent;
    public CardInfo lastCard;
    public int count;
    public int turn;
    public int stack;
    public bool opposite;

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(GetInt(lastCard.type));
        sensor.AddObservation(GetInt(lastCard.num));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        CardInfo card = new CardInfo(GetType(actions.DiscreteActions[0]), GetNum(actions.DiscreteActions[1]));
        if (!Push(card))
        {
            if (stack > 0)
            {
                AddReward(-stack);
                stack = 0;
            }
            else
            {
                AddReward(-0.5f);
            }
        }
    }

    public static CardInfo GetCard(string card)
    {
        return new CardInfo(GetType(card[0]), GetNum(card[1]));
    }

    public bool Push(CardInfo card)
    {
        if (count < 2)
        {
            // End
            return false;
        }
        int last = (turn + (opposite ? 1 : -1) + count) % count;
        if (!Match(card.type, lastCard.type) && !Match(card.num, lastCard.num)) return false;
        if (stack != 0)
        {
            if (!Match(card.num, CardNum.AND))
            {
                AddReward(1f);

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
        if (card.num == CardNum.J) turn += opposite ? -1 : 1;
        else if (card.num == CardNum.Q) opposite = !opposite;
        else if (card.num == CardNum.K) turn += opposite ? 1 : -1;
        turn = (turn + count) % count;

        // End
        lastCard = card;
        turn = (turn + (opposite ? -1 : 1) + count) % count;
        return true;
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

    public static bool Match(CardType one, CardType two)
        => (one & two) != 0;

    public static bool Match(CardNum one, CardNum two)
        => (one & two) != 0;

    public void Stack(CardInfo card)
    {
        stack += card.num switch
        {
            CardNum.A => 3,
            CardNum.Two => 2,
            CardNum.BlackJoker => 5,
            CardNum.ColorJoker => 7,
            _ => 0
        };
        AddReward(stack);
    }

    public static int GetInt(CardType type)
    {
        return type switch
        {
            CardType.Diamond => 1,
            CardType.Spade   => 2,
            CardType.Heart   => 3,
            CardType.Clover  => 4,
            _ => 1
        };
    }

    public static int GetInt(CardNum num)
    {
        return num switch
        {
            CardNum.A          => 1,
            CardNum.Two        => 2,
            CardNum.Three      => 3,
            CardNum.Four       => 4,
            CardNum.Five       => 5,
            CardNum.Six        => 6,
            CardNum.Seven      => 7,
            CardNum.Eight      => 8,
            CardNum.Nine       => 9,
            CardNum.Ten        => 10,
            CardNum.J          => 11,
            CardNum.Q          => 12,
            CardNum.K          => 13,
            CardNum.BlackJoker => 14,
            CardNum.ColorJoker => 15,
            _ => '4'
        };
    }

    public static CardType GetType(int type)
    {
        return type switch
        {
            1 => CardType.Diamond,
            2 => CardType.Spade,
            3 => CardType.Heart,
            4 => CardType.Clover,
            _ => CardType.Diamond
        };
    }

    public static CardNum GetNum(int num)
    {
        return num switch
        {
            1  => CardNum.A,
            2  => CardNum.Two,
            3  => CardNum.Three,
            4  => CardNum.Four,
            5  => CardNum.Five,
            6  => CardNum.Six,
            7  => CardNum.Seven,
            8  => CardNum.Eight,
            9  => CardNum.Nine,
            10 => CardNum.Ten,
            11 => CardNum.J,
            12 => CardNum.Q,
            13 => CardNum.K,
            14 => CardNum.BlackJoker,
            15 => CardNum.ColorJoker,
            _ => CardNum.Four
        };
    }

}
