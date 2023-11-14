using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Text Mesh")]
    [SerializeField] private TextMeshProUGUI playerPointText;
    [SerializeField] private TextMeshProUGUI dealerPointText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI bidsText;
    [SerializeField] private TextMeshProUGUI biddingText;

    [Header("Slider")]
    [SerializeField] private Slider bidSlidder;
    [SerializeField] private CanvasGroup biddingGroup;


    [Header("Card Container")]
    [SerializeField] private Transform playerCardContainer;
    [SerializeField] private Transform dealerCardContainer;

    [Header("Buttons")]
    [SerializeField] private CanvasGroup hitStandGroup;

    [Header("Game Over")]
    [SerializeField] private CanvasGroup gameoverGroup;
    [SerializeField] private TextMeshProUGUI gameoverText;
    [SerializeField] private TextMeshProUGUI chipsText;

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
        playerPointText.text = "0";
        dealerPointText.text = "0";
        bidsText.text = "$0";
        biddingText.text = "$0";

        bidSlidder.onValueChanged.AddListener(delegate { SetBidding(); });

        ShowHitStandButton(false);
    }

    private void SetBidding()
    {
        int bid = (int)(GameManager.Instance.GetMoney() * bidSlidder.value);
        biddingText.text = "$" + bid;
    }

    public void UpdatePoints()
    {
        Card[] playerCard = playerCardContainer.GetComponentsInChildren<Card>();
        Card[] dealerCard = dealerCardContainer.GetComponentsInChildren<Card>();

        int playerPoint = GameManager.Instance.GetPoint(playerCard);
        int dealerPoint = GameManager.Instance.GetPoint(dealerCard);

        playerPointText.text = playerPoint.ToString();
        dealerPointText.text = dealerPoint.ToString();
    }

    public void ShowHitStandButton(bool status)
    {
        hitStandGroup.alpha = status ? 1 : 0;
        hitStandGroup.interactable = status;
        hitStandGroup.blocksRaycasts = status;
    }

    public void UpdateTextMoney(int money)
    {
        moneyText.text = "$" + money.ToString();
    }

    public void UpdateTextBid(int bid)
    {
        bidsText.text = "$" + bid.ToString();
    }

    public void BiddingButton()
    {
        int bid = (int)(GameManager.Instance.GetMoney() * bidSlidder.value);

        if (bid == 0) return;

        UpdateTextBid(bid);

        GameManager.Instance.GameStart();
        GameManager.Instance.SetBid(bid);

        biddingGroup.alpha = 0;
        biddingGroup.interactable = false;
        biddingGroup.blocksRaycasts = false;
    }

    public void GameOver(string gameover, GameState gameState, int bid)
    {
        Color positifColor = new Color(0.5205985f, 0.945098f, 0.4313725f);
        Color negativeColor = new Color(1f, 0.4663631f, 0.3647059f);
        Color textColor = new Color();

        gameoverGroup.alpha = 1;
        gameoverGroup.interactable = true;
        gameoverGroup.blocksRaycasts = true;

        int money = GameManager.Instance.GetMoney();

        switch (gameState)
        {
            case GameState.WIN:
                chipsText.text = "+$" + bid.ToString();
                money += bid;
                textColor = positifColor;
                break;

            case GameState.LOSE:
                chipsText.text = "-$" + bid.ToString();
                money -= bid;
                textColor = negativeColor;
                break;

            case GameState.DRAW:
                chipsText.color = new Color(0, 0, 0, 0);
                break;
        }

        UpdateTextMoney(money);
        PlayerPrefs.SetInt("Money", money);

        gameoverText.text = gameover;
        chipsText.color = textColor;
    }
}
