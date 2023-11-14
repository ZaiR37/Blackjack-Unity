using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;

public class CardDeck : MonoBehaviour
{
    [SerializeField] private SpriteAtlas atlas;
    [SerializeField] private List<Sprite> cardDeck;

    public void ShuffleDeck()
    {
        Sprite[] cardArray = new Sprite[atlas.spriteCount];
        atlas.GetSprites(cardArray);

        for (int i = 0; i < cardArray.Length; i++)
        {
            int randomNumber = Random.Range(0, cardArray.Length);

            Sprite temp = cardArray[randomNumber];
            cardArray[randomNumber] = cardArray[i];
            cardArray[i] = temp;
        }

        cardDeck = cardArray.ToList();
    }

    public Sprite GetCard()
    {
        Sprite card = cardDeck[0];
        cardDeck.Remove(card);
        return card;
    }
}
