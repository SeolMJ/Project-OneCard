using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SeolMJ
{
    public static class CardUtils
    {

        public static CardManager cardManager => CardManager.instance;
        public static GameManager gameManager => GameManager.instance;

        static readonly int[] safetyCards = { 2, 3, 4, 5, 6, 7, 8, 9 };
        static readonly int[] pickableCards = { 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7,
                                            8, 8, 8, 8, 9, 9, 9, 9, 10, 10, 10, 10, 11, 11, 11, 11, 12, 12, 12, 12, 13, 14 };

        public static async void GiveTurn()
        {
            while (cardManager.carders.Count <= cardManager.turn) await System.Threading.Tasks.Task.Delay(1);
            if (cardManager.carders[cardManager.turn] is Player)
            {
                cardManager.myTurnAnimator.Animate();
                cardManager.myTurnParticle.Emit(50);
            }
            cardManager.carders[cardManager.turn].OnTurn();
        }

        public static CardInfo RandomCard(bool safe = false)
        {
            CardNum num = (CardNum)(1 << (safe ? safetyCards[Random.Range(0, 8)] : pickableCards[Random.Range(0, 50)]));
            CardType type = num == CardNum.BlackJoker ? CardType.Black : (num == CardNum.ColorJoker ? CardType.All : (CardType)(1 << Random.Range(0, 4)));
            return new CardInfo(type, num);
        }

        public static bool Match(CardType one, CardType two)
            => (one & two) != 0;

        public static bool Match(CardNum one, CardNum two)
            => (one & two) != 0;

        public static void Stack(CardInfo card)
        {
            cardManager.stack += card.num switch
            {
                CardNum.A => 3,
                CardNum.Two => 2,
                CardNum.BlackJoker => 5,
                CardNum.ColorJoker => 7,
                _ => 0
            };
        }

        public static void ChangeSet(bool isCard)
        {
            if (cardManager.cardObjects.Length > 0)
                foreach (GameObject cardObject in cardManager.cardObjects) cardObject.SetActive(isCard);
            if (cardManager.idleObjects.Length > 0)
                foreach (GameObject idleObject in cardManager.idleObjects) idleObject.SetActive(!isCard);
        }

        public static int GetTurn()
        {
            cardManager.turn = (cardManager.turn + cardManager.carders.Count) % cardManager.carders.Count;
            return cardManager.turn;
        }

    }
}