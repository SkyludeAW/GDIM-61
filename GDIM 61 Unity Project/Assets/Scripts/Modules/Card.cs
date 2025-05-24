using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Card", menuName = "Scriptable Objects/Card")]
public class Card : ScriptableObject {
    public string Name;
    public string Description;
    public Sprite Art;

    public int Cost;
    public float HitPoint;
    public float Damage;
    public float AttackCooldown;
    public float Speed;
    public float KnockbackPower;
    public float KnockbackResistance;
    public float AttackRange;
    public float AggroRadius;

    public Sprite UnitDeploySprite;

    public Unit SummonedUnit;

    public static Queue<Card> ShuffleToQueue(List<Card> deck) {
        List<Card> shuffledDeck = new List<Card>(deck);
        ShuffleDeck(shuffledDeck);
        Queue<Card> shuffledQueue = new Queue<Card>();
        foreach (Card card in shuffledDeck)
            shuffledQueue.Enqueue(card);
        return shuffledQueue;
    }

    public static void ShuffleDeck(List<Card> deck) {
        // Shuffle the deck using Fisher–Yates algorithm
        for (int i = deck.Count - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            Card temp = deck[i];
            deck[i] = deck[j];
            deck[j] = temp;
        }
    }
}
