using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Deck
{
    public List<Card> cards;

    public Deck(List<string> suits, List<string> values)
    {
        cards = new List<Card>();
        foreach (string suit in suits)
        {
            foreach (string value in values)
            {
                cards.Add(new Card(suit, value));
            }
        }
    }

    public void Shuffle()
    {
        int deckSize = cards.Count;
        System.Random rng = new System.Random();

        for (int i = 0; i < deckSize - 1; i++)
        {
            // Generate a random index within the remaining unshuffled portion of the deck
            int randomIndex = rng.Next(i, deckSize);

            // Swap the current card with the randomly selected card
            Card temp = cards[i];
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
    }

    public Card DrawCard()
    {
        if (cards.Count == 0)
        {
            Debug.LogWarning("No cards left in the deck.");
            return null;
        }

        Card drawnCard = cards[0];
        cards.RemoveAt(0);
        return drawnCard;
    }
}
