using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerritoryDeck 
{

    static List<string> SUITS = new List<string> { "Hearts", "Diamonds", "Clubs", "Spades" };
    static List<string> VALUES = new List<string> { "2", "3", "4", "5", "6", "7", "8", "9", "10", "Jack", "Queen", "King", "Ace" };

    private Deck cardDeck;

    public TerritoryDeck() {
        cardDeck = new Deck(SUITS, VALUES);
        cardDeck.Shuffle();
    }

    public Card DrawCard()
    {
        return cardDeck.DrawCard();
    }
}
