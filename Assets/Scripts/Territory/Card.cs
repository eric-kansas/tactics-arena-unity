[System.Serializable]
public class Card
{
    public string suit;
    public string value;

    public Card(string suit, string value)
    {
        this.suit = suit;
        this.value = value;
    }
}
