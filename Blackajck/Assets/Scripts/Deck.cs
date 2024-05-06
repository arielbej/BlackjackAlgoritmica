using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class Deck : MonoBehaviour
{
    public Sprite[] faces;
    public GameObject dealer;
    public GameObject player;
    public Button hitButton;
    public Button stickButton;
    public Button playAgainButton;
    public Text finalMessage;
    public Text probMessage;

    public Text TextProb1;
    public Text TextProb2;
    public Text TextProb3;

    public Text PuntosPlayer;
    public Text PuntosDealer;

    public Text creditos;
    public Dropdown betDropdown;

    public int[] values = new int[52];
    int cardIndex = 0;

    int bank = 1000;
    int currentBet = 0;

    private void Awake()
    {
        InitCardValues();
    }

    private void Start()
    {
        ShuffleCards();
        StartGame();

        betDropdown.onValueChanged.AddListener(delegate {
            PlaceBet();
        });

    }

    private void Update()
    {
        UpdateBank();

    }

    private void InitCardValues()
    {
        for (int i = 0; i < 52; i++)
        {
            values[i] = (i % 13) + 1;
            if (values[i] > 10)
            {
                values[i] = 10;
            }
        }

    }

    private void ShuffleCards()
    {
        for (int i = values.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            int temp = values[i];
            values[i] = values[j];
            values[j] = temp;

            Sprite tempFace = faces[i];
            faces[i] = faces[j];
            faces[j] = tempFace;
        }
    }

    void StartGame()
    {

        for (int i = 0; i < 2; i++)
        {
            PushPlayer();
            PushDealer();
        }

        CheckBlackjack();

        UpdatePlayerPoints();
        UpdateDealerPoints();
    }

    private void UpdatePlayerPoints()
    {
        int playerPoints = CalculateHandValue(player.GetComponent<CardHand>().cards);
        PuntosPlayer.text = "Puntos del Jugador: " + playerPoints;
    }

    private void UpdateDealerPoints()
    {
        int dealerPoints = CalculateHandValue(dealer.GetComponent<CardHand>().cards);
        PuntosDealer.text = "Puntos del Crupier: " + dealerPoints;
    }

    private void CalculateProbabilities()
    {
        int dealerScore = dealer.GetComponent<CardHand>().points;
        int playerScore = player.GetComponent<CardHand>().points;
        int higherScoreCount = 0;
        for (int i = cardIndex; i < values.Length; i++)
        {
            if (dealerScore + values[i] > playerScore)
                higherScoreCount++;
        }
        float probDealerHigher = (float)higherScoreCount / (values.Length - cardIndex);

        int inRangeCount = 0;
        for (int i = cardIndex; i < values.Length; i++)
        {
            if (playerScore + values[i] >= 17 && playerScore + values[i] <= 21)
                inRangeCount++;
        }
        float probPlayerInRange = (float)inRangeCount / (values.Length - cardIndex);

        int overCount = 0;
        for (int i = cardIndex; i < values.Length; i++)
        {
            if (playerScore + values[i] > 21)
                overCount++;
        }
        float probPlayerOver = (float)overCount / (values.Length - cardIndex);

        TextProb1.text = "Deal > Play: " + (1 - probDealerHigher).ToString("P2") + "\n";
        TextProb2.text = "17<=X<=21: " + probPlayerInRange.ToString("P2") + "\n";
        TextProb3.text = "X>21: " + probPlayerOver.ToString("P2");

    }

    void PushDealer()
    {
        int cardValue = values[cardIndex];
        if (cardValue == 11)
        {
            int dealerScore = dealer.GetComponent<CardHand>().points;
            if (dealerScore + 11 <= 21)
            {
                cardValue = 11;
            }
            else
            {
                cardValue = 1;
            }
        }


        dealer.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);
        cardIndex++;
    }

    void PushPlayer()
    {
        int cardValue = values[cardIndex];
        if (cardValue == 11)
        {
            int playerScore = player.GetComponent<CardHand>().points;
            if (playerScore + 11 <= 21)
            {
                cardValue = 11;
            }
            else
            {
                cardValue = 1;
            }
        }


        player.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);
        cardIndex++;
        CalculateProbabilities();
    }

    public void Hit()
    {
        if (cardIndex == 4)
        {
            dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);
        }

        PushPlayer();

        if (player.GetComponent<CardHand>().points > 21)
        {
            foreach (GameObject card in player.GetComponent<CardHand>().cards)
            {
                if (card.GetComponent<CardModel>().value == 11 && player.GetComponent<CardHand>().points > 21)
                {
                    player.GetComponent<CardHand>().points -= 10;
                }
            }

            if (player.GetComponent<CardHand>().points > 21)
            {
                finalMessage.text = "¡Has perdido! Tu puntuación es mayor a 21.";
                hitButton.interactable = false;
                stickButton.interactable = false;
            }
        }
        UpdatePlayerPoints();
    }

    public void Stand()
    {
        if (cardIndex == 4)
        {
            dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);
        }

        int playerScore = player.GetComponent<CardHand>().points;
        int dealerScore = dealer.GetComponent<CardHand>().points;

        while (dealerScore <= playerScore && dealerScore < 21)
        {
            PushDealer();
            dealerScore = dealer.GetComponent<CardHand>().points;
        }


        if (dealerScore > 21 || (playerScore <= 21 && playerScore > dealerScore))
        {
            finalMessage.text = "¡Has ganado!";
            bank += currentBet * 2;

        }
        else if (playerScore == dealerScore)
        {
            finalMessage.text = "¡Empate! Teneis los mismos puntos.";
            bank += currentBet;
        }
        else
        {
            finalMessage.text = "¡Has perdido!";
        }

        hitButton.interactable = false;
        stickButton.interactable = false;


        UpdateDealerPoints();
        UpdateBank();

    }

    public void PlaceBet()
    {
        int betAmount;

        if (int.TryParse(betDropdown.options[betDropdown.value].text, out betAmount))
        {
            currentBet = betAmount;
            bank -= betAmount;
            UpdateBank();
        }


    }


    private void UpdateBank()
    {
        creditos.text = "Créditos: " + bank;

    }

    public void PlayAgain()
    {
        hitButton.interactable = true;
        stickButton.interactable = true;
        finalMessage.text = "";
        player.GetComponent<CardHand>().Clear();
        dealer.GetComponent<CardHand>().Clear();
        cardIndex = 0;
        ShuffleCards();
        StartGame();

        UpdatePlayerPoints();
        UpdateDealerPoints();

        betDropdown.value = 0;

        UpdateBank();
    }

    private void CheckBlackjack()
    {
        List<GameObject> playerHand = player.GetComponent<CardHand>().cards;
        List<GameObject> dealerHand = dealer.GetComponent<CardHand>().cards;

        int playerPoints = CalculateHandValue(playerHand);
        int dealerPoints = CalculateHandValue(dealerHand);

        if (playerPoints == 21 && player.GetComponent<CardHand>().cards.Count == 2)
        {
            bool hasTen = false;
            bool hasAce = false;

            foreach (GameObject card in player.GetComponent<CardHand>().cards)
            {
                if (card.GetComponent<CardModel>().value == 10)
                {
                    hasTen = true;
                }
                else if (card.GetComponent<CardModel>().value == 1)
                {
                    hasAce = true;
                }
            }

            if (hasTen && hasAce)
            {
                finalMessage.text = "¡Tienes Blackjack! ¡Has ganado!";
                return;
            }
        }

        if (dealerPoints == 21 && dealer.GetComponent<CardHand>().cards.Count == 2)
        {
            bool hasTen = false;
            bool hasAce = false;

            foreach (GameObject card in dealer.GetComponent<CardHand>().cards)
            {
                if (card.GetComponent<CardModel>().value == 10)
                {
                    hasTen = true;
                }
                else if (card.GetComponent<CardModel>().value == 1)
                {
                    hasAce = true;
                }
            }

            if (hasTen && hasAce)
            {
                finalMessage.text = "¡El crupier tiene Blackjack! ¡Has perdido!";
                return;
            }
        }
    }

    private int CalculateHandValue(List<GameObject> hand)
    {
        int total = 0;
        int aceCount = 0;

        foreach (GameObject card in hand)
        {
            int cardValue = card.GetComponent<CardModel>().value;
            if (cardValue == 1)
            {
                aceCount += 1;
                cardValue = 11;
            }
            total += cardValue;
        }

        while (total > 21 && aceCount > 0)
        {
            total -= 10;
            aceCount--;
        }

        return total;
    }
}



