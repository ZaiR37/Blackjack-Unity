using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Card")]
    [SerializeField] private CardDeck cardDeck;
    [SerializeField] private Transform cardPrefabs;

    [Header("Player")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform dealerTransform;

    private TurnState currentTurn = TurnState.NONE;

    [SerializeField] private int bid;
    [SerializeField] private int money;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        money = PlayerPrefs.GetInt("Money");

        if (money <= 0)
        {
            money = 1000;
            PlayerPrefs.SetInt("Money", money);
        }

        UIManager.Instance.UpdateTextMoney(money);
        UIManager.Instance.UpdateTextBid(bid);
    }

    public void GameStart()
    {
        cardDeck.ShuffleDeck();
        StartCoroutine(GivingCards());
    }

    private IEnumerator GivingCards()
    {
        float time = 0.3f;

        GiveCardTo(playerTransform, cardDeck.GetCard(), CardState.OPEN);
        yield return new WaitForSeconds(time);
        GiveCardTo(playerTransform, cardDeck.GetCard(), CardState.OPEN);
        yield return new WaitForSeconds(time);
        GiveCardTo(dealerTransform, cardDeck.GetCard(), CardState.OPEN);
        yield return new WaitForSeconds(time);
        GiveCardTo(dealerTransform, cardDeck.GetCard(), CardState.CLOSE);
        yield return new WaitForSeconds(time);

        currentTurn = TurnState.PLAYER;
        UIManager.Instance.ShowHitStandButton(true);
        StopAllCoroutines();
    }

    private void GiveCardTo(Transform player, Sprite cardSprite, CardState cardState)
    {
        Transform newCardTransform = Instantiate(cardPrefabs, cardDeck.transform.position, Quaternion.identity);
        newCardTransform.parent = player;

        Vector3 target = player.position;
        target.x = target.x + (player.childCount * 1f);
        target.z = -player.childCount;

        Card card = newCardTransform.GetComponent<Card>();
        card.SetupCard(cardSprite, cardState);
        card.MoveCard(target);
    }

    public void HitButton()
    {
        GiveCardTo(playerTransform, cardDeck.GetCard(), CardState.OPEN);
        UIManager.Instance.ShowHitStandButton(false);
    }

    public void StandButton()
    {
        Card[] dealerCard = dealerTransform.GetComponentsInChildren<Card>();
        currentTurn = TurnState.DEALER;

        dealerCard.ToList().ForEach(card =>
        {
            card.SetCardState(CardState.OPEN);
            card.OpenCard();
        });

        UIManager.Instance.ShowHitStandButton(false);

        if (GetPoint(dealerCard) >= 17) CheckPoint();
        else GiveDealerCard();
    }

    public void RestartButton()
    {
        SceneManager.LoadScene(0);
    }

    public void CheckPoint()
    {
        Card[] playerCard = playerTransform.GetComponentsInChildren<Card>();
        Card[] dealerCard = dealerTransform.GetComponentsInChildren<Card>();

        int playerPoint = GetPoint(playerCard);
        int dealerPoint = GetPoint(dealerCard);

        if (playerPoint > 21)
        {
            currentTurn = TurnState.NONE;
            playerCard.ToList().ForEach(card => card.ChangeShade());

            UIManager.Instance.GameOver("Player Bust", GameState.LOSE, bid);

            Debug.Log("Player Bust");
            return;
        }
        else if (dealerPoint > 21)
        {
            currentTurn = TurnState.NONE;
            dealerCard.ToList().ForEach(card => card.ChangeShade());

            UIManager.Instance.GameOver("Dealer Bust", GameState.WIN, bid);

            Debug.Log("Dealer Bust");
            return;
        }

        if (dealerPoint >= 17)
        {
            currentTurn = TurnState.NONE;

            if (dealerPoint == playerPoint)
            {
                UIManager.Instance.GameOver("Draw", GameState.DRAW, bid);
            }
            else if (dealerPoint < playerPoint)
            {
                UIManager.Instance.GameOver("Player Won", GameState.WIN, bid);
            }
            else
            {
                UIManager.Instance.GameOver("Dealer Won", GameState.LOSE, bid);
            }
        }
    }

    public int GetPoint(Card[] playerCard)
    {
        int playerPoint = 0;
        int ace = 0;

        foreach (var card in playerCard)
        {
            if (card.GetCardState() == CardState.CLOSE) continue;
            if (card.GetCardValue() == 11) ace += 1;
            playerPoint += card.GetCardValue();
        }

        if (playerPoint > 21)
        {
            for (int i = 0; i < ace; i++)
            {
                playerPoint -= 10;
                if (playerPoint <= 21) break;
            }
        }

        return playerPoint;
    }

    public void SetBid(int money) => bid = money;

    public int GetMoney() => money;
    public TurnState GetTurnState() => currentTurn;
    public void GiveDealerCard()
    {
        GiveCardTo(dealerTransform, cardDeck.GetCard(), CardState.OPEN);
    }
}
