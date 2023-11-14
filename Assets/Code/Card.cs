using System.Text.RegularExpressions;
using UnityEngine;

public class Card : MonoBehaviour
{
    [SerializeField] private CardState currentState = CardState.OPEN;
    [SerializeField] private string cardName;
    [SerializeField] private int cardValue;

    [Header("Sprite")]
    [SerializeField] SpriteRenderer currentSprite;
    [SerializeField] private Sprite cardOpenSprite;
    [SerializeField] private Sprite cardCloseSprite;

    [Header("Events")]
    [SerializeField] private GameEvent OnDoneMovingCard;

    private bool doneMoving = false;
    private bool movingCard = false;
    private Vector3 endMove;
    private Vector3 startMove;
    private float progress;

    private void Update()
    {
        if (movingCard) MovingCard();
    }

    public void MoveCard(Vector3 target)
    {
        progress = 0;
        startMove = transform.position;
        endMove = target;
        movingCard = true;
    }

    private void MovingCard()
    {
        progress += Time.deltaTime;

        float duration = 0.25f;
        float percentageComplete = progress / duration;

        transform.position = Vector3.Lerp(startMove, endMove, percentageComplete);

        if (percentageComplete >= 1)
        {
            movingCard = false;
            doneMoving = true;

            if (currentState == CardState.OPEN) OnDoneMovingCard.Raise();

            switch (GameManager.Instance.GetTurnState())
            {
                case TurnState.PLAYER:
                    UIManager.Instance.ShowHitStandButton(true);
                    break;

                case TurnState.DEALER:
                    GameManager.Instance.GiveDealerCard();
                    break;
            }
        }
    }

    public void ChangeShade()
    {
        currentSprite.color = new Color(0.5f, 0.5f, 0.5f);
    }

    public void SetupCard(Sprite cardSprite, CardState state)
    {
        cardOpenSprite = cardSprite;
        currentState = state;

        InitializeCardValue();

        if (currentState == CardState.CLOSE) CloseCard();
        else OpenCard();
    }

    private string CleanCardName(string input)
    {
        string cleanedString = Regex.Replace(input, @"(Poker Card|\(Clone\))", "").Trim();
        return cleanedString;
    }

    private void InitializeCardValue()
    {
        cardName = CleanCardName(cardOpenSprite.name);
        Match match = Regex.Match(cardName, @"\b(?:[2-9]|10|Jack|Queen|King|Ace)\b", RegexOptions.IgnoreCase);

        switch (match.Value)
        {
            case "Ace":
                cardValue = 11;
                break;

            case "Jack":
            case "Queen":
            case "King":
                cardValue = 10;
                break;

            default:
                cardValue = int.Parse(match.Value);
                break;
        }
    }

    public void SetCardState(CardState cardState) => currentState = cardState;
    
    public int GetCardValue() => cardValue;
    public CardState GetCardState() => currentState;
    public void CloseCard() => currentSprite.sprite = cardCloseSprite;
    public void OpenCard()
    {
        currentSprite.sprite = cardOpenSprite;
        if (doneMoving) OnDoneMovingCard.Raise();
    }
}
