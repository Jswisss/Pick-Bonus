using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class Main_Game_Script : MonoBehaviour
{
    public Button playbutton;// play button to start game
    public Button IncreaseDN;// Increase Denomination
    public Button DecreaseDN;// Decrease Denomination

    public TextMeshProUGUI Current_Balance;// Dispolay Current balance
    public TextMeshProUGUI Current_Denomination;// Current bet on Play
    public TextMeshProUGUI Last_Game_Win_Amount;// Amount won on previous turn
    public TextMeshProUGUI GuideText;//Tells player what to do.
    private LinkedList<double> Treasure_Chest = new LinkedList<double>();// treasure chest amounts going to be used by cashout to process payouts

    private double Balance = 10.00; // set current and future Balance for player
    private double Starting_DM = 0.25; // set current denomination
    private double Denomination_Bet; // Set future denomination
    private double Payout; // Current Denomination x Multiplier
    private double Chest_Reward;//going to be used to save current value of chest and add it to linked list
    private double Current_Profits = 0;// going to be used to hold the current overall value of all the chest
    private double TempValue; // is a time value to hold the difference of chest_reward - Current_ Profits and is add to linked List
    private float Rate_Decay = .1f; // decay for my function

    public double Chest_result; // informs the chest script what amount to type out
    public bool Game_start; //informs round has started
    public bool EndofGame = false; // informs that the round is over
    public double current_Last_game = 0; // keeps track of amount rewarded
    public bool StopChestOpening;// Stops chest from opening
    public bool toggleOutline;

    public List<float> probability_List = new List<float> { 50, 30, 15, 5 };
    private int[] SmallMult = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }; // multiplier
    private int[] MedMult = { 12, 16, 24, 32, 48, 64 }; // multiplier
    private int[] LargeMult = { 100, 200, 300, 400, 500 }; // nultiplier
    private int[][] Multiplier_List = new int[4][]; // multiplier list
    private List<float> CumulativeProbability; // Cumulative probability for figuring out what multiplier to choose


    // Start is called before the first frame update
    void Start()
    {
        //initialize the list 
        Multiplier_List[0] = new int[1];
        Multiplier_List[1] = new int[10];
        Multiplier_List[2] = new int[6];
        Multiplier_List[3] = new int[5];

        Multiplier_List[0][0] = 0;
        Multiplier_List[1] = SmallMult;
        Multiplier_List[2] = MedMult;
        Multiplier_List[3] = LargeMult;

        // update Guide Text
        GuideText.text = "Increase or Decrease your bet.\n\nPress Start Game when you are ready.";

        //setups UI with starting values
        Current_Balance.text = " Balance: $" + Balance.ToString("F2");
        Current_Denomination.text = " Current Bet: $" + Starting_DM.ToString();
        Denomination_Bet = Starting_DM;
        Last_Game_Win_Amount.text = " Lastest Gains: $0.00";
        Game_start = false;
        StopChestOpening = true;
        toggleOutline = false;
        // creates button for mechanics
        Button play = playbutton.GetComponent<Button>();
        Button Increase = IncreaseDN.GetComponent<Button>();
        Button Decrease = DecreaseDN.GetComponent<Button>();

        play.onClick.AddListener(Start_Game);
        Increase.onClick.AddListener(Increase_Denomination);
        Decrease.onClick.AddListener(Decrease_Denomination);


    }

    // Update is called once per frame
    void Update()
    {
        // checking if game has end and reset UI
        if (EndofGame == true)
        {
            Round_Over();
        }

    }

    void Start_Game()// starts game and updates UI and turns off buttons and recreates chests
    {
        if (Denomination_Bet> Balance)
        {
            GuideText.text = "Welp Looks like you ran out of money.";
        }
        else
        {
            // turns off buttons, adjusts balance and resets last game profits
            current_Last_game = 0;
            Game_start = true;
            toggleOutline = true;
            Balance = Balance - Denomination_Bet;
            Current_Balance.text = " Balance: $" + Balance.ToString("F2");
            Last_Game_Win_Amount.text = " Lastest Gains: $0.00";
            playbutton.GetComponent<Image>().color = Color.gray;
            playbutton.GetComponent<Button>().interactable = false;
            IncreaseDN.GetComponent<Image>().color = Color.gray;
            IncreaseDN.GetComponent<Button>().interactable = false;
            DecreaseDN.GetComponent<Image>().color = Color.gray;
            DecreaseDN.GetComponent<Button>().interactable = false;

            calculate_Denomination();
            cashout();
            GuideText.text = "Click on any chest to collect or reward";
        }

    }

    void Increase_Denomination() // Increase Denomination and updates UI and code side bets
    {
        switch (Current_Denomination.text)
        {
            case " Current Bet: $0.25":
                Denomination_Bet = 0.50;

                if (Denomination_Bet > Balance)
                {
                    Denomination_Bet = .25;
                }
                else
                {
                    Current_Denomination.text = " Current Bet: $0.50";
                }
                break;
            case " Current Bet: $0.50":
                Denomination_Bet = 1.00;

                if (Denomination_Bet > Balance)
                {
                    Denomination_Bet = .50;
                }
                else
                {
                    Current_Denomination.text = " Current Bet: $1.00";
                }
                break;
            case " Current Bet: $1.00":
                Denomination_Bet = 5.00;

                if (Denomination_Bet > Balance)
                {
                    Denomination_Bet = 1.00;
                }
                else
                {
                    Current_Denomination.text = " Current Bet: $5.00";
                }
                break;
        }
    }

    void Decrease_Denomination() // decreases Denominations and update UI and code side bets
    {
        switch (Current_Denomination.text)
        {
            case " Current Bet: $0.50":
                Current_Denomination.text = " Current Bet: $0.25";
                Denomination_Bet = 0.25;
                break;
            case " Current Bet: $1.00":
                Current_Denomination.text = " Current Bet: $0.50";
                Denomination_Bet = 0.50;
                break;
            case " Current Bet: $5.00":
                Current_Denomination.text = " Current Bet: $1.00";
                Denomination_Bet = 1.00;
                break;
        }
    }

    int Multiplier() // returns a multiplier for our Payout.
    {
        int multiplierIndex = found_Probability(probability_List);

        int rnd = Random.Range(0, Multiplier_List[multiplierIndex].Length - 1);
        return Multiplier_List[multiplierIndex][rnd];

    }

    int found_Probability(List<float> probability)//Returns the probability Index for our multiplierIndex
    {
        float probabilitiesSum = 0;
        CumulativeProbability = new List<float>();

        for (int i = 0; i < probability.Count; i++)
        {
            probabilitiesSum += probability_List[i];
            CumulativeProbability.Add(probabilitiesSum);
        }

        float rnd = Random.Range(1, 101);
        for (int i = 0; i < probability.Count; i++)
        {
            if (rnd <= CumulativeProbability[i])
            {
                return i;
            }
        }
        return -1;


    }

    void calculate_Denomination()// calculates Payouts
    {
        Payout = Denomination_Bet * Multiplier();
        //Debug.Log(Payout);
    }

    void cashout()// puts profit amount into chunks and puts that into Linked List to be used later to display rewards.
    {
        if (Payout == 0)
        {

        }

        else
        {

            for (int ChestAmount_Position = 8; ChestAmount_Position > 0; ChestAmount_Position--)//For loop for adding amounts to LinkedList
            {
                if (ChestAmount_Position != 1)
                {
                    Chest_Reward = Payout * Mathf.Pow(1 - Rate_Decay, Mathf.Pow(ChestAmount_Position, 2)); //formula
                    Chest_Reward = (Mathf.Round((float)(Chest_Reward * 10) * 2) / 2) / 10;// rounds to 5 cents
                    Chest_Reward = double.Parse(Chest_Reward.ToString("F2")); // sets chest reward to two decimal point value
                    //Debug.Log("ChestReward:    " + Chest_Reward);
                    if (Chest_Reward >= .25 && Payout >= 1.00 && Treasure_Chest.Count == 0) // if greater than 25 cents and there is nothing in our chest list. Add to list
                    {
                        TempValue = Chest_Reward - Current_Profits; // temp value = the chest amount - your current profits. so you do not added chest amount from previous chest rewards
                        Current_Profits = Current_Profits + TempValue; // adds temp value to current profits
                        Treasure_Chest.AddLast(TempValue);// adds temp value to linked list
                    }
                    else if ((Chest_Reward - Current_Profits) >= 1.00 && Payout >= 2.50 && Treasure_Chest.Count == 1)// if greater than $1.00 and there is 1 item in our chest list. Add to list
                    {
                        TempValue = Chest_Reward - Current_Profits; // temp value = the chest amount - your current profits. so you do not added chest amount from previous chest rewards
                        Current_Profits = Current_Profits + TempValue; // adds temp value to current profits
                        Treasure_Chest.AddLast(TempValue);// adds temp value to linked list
                    }
                    else if ((Chest_Reward - Current_Profits) >= 5.00 && Treasure_Chest.Count >= 2)// if greater than $5.00 and there is 2> item in our chest list. Add to list
                    {
                        TempValue = Chest_Reward - Current_Profits; // temp value = the chest amount - your current profits. so you do not added chest amount from previous chest rewards
                        Current_Profits = Current_Profits + TempValue; // adds temp value to current profits
                        Treasure_Chest.AddLast(TempValue);// adds temp value to linked list
                    }
                }
                else
                {
                    TempValue = Payout - Current_Profits;// takes remaining profits and = to temp value
                    if (Payout < Current_Profits)// if somehow current profits> than profit
                    {
                        double tempvalue2 = Treasure_Chest.Last.Value; // set temp value = to last value enter to list
                        Treasure_Chest.RemoveLast(); // remove last value
                        TempValue = TempValue + tempvalue2;// add temp value 2 to temp value
                    }
                    //Debug.Log("TempValue Last:    " + TempValue);
                    Treasure_Chest.AddLast(TempValue);// add chest value to linked list
                }



            }
        }
        Treasure_Chest.AddLast(0);// add 0 to so that chest opener and other methods no where the list ends
    }


    public void Display_chest()
    {
        
        if (Treasure_Chest.First.Value != 0)// if not pooper
        {
            Chest_result = Treasure_Chest.First.Value; // set chest result to first value
            Treasure_Chest.RemoveFirst();// remove first chest amount
        }
        else
        {
            Chest_result = 0;// set pooper
            Treasure_Chest.RemoveFirst();// remove
        }

    }

    void Round_Over()// update balance, reset buttons, denominations.
    {
        
        Game_start = false;
        Last_Game_Win_Amount.text = " Lastest Gains: $" + Payout.ToString("F2");
        Current_Denomination.text = " Current Bet: $" + Starting_DM.ToString();
        GuideText.text = "Increase or Decrease your bet.\n\nPress Start Game when you are ready.";
        Balance = Balance + Payout;
        Current_Balance.text = " Balance: $" + Balance.ToString("F2");
        playbutton.GetComponent<Image>().color = Color.white;
        playbutton.GetComponent<Button>().interactable = true;
        IncreaseDN.GetComponent<Image>().color = Color.white;
        IncreaseDN.GetComponent<Button>().interactable = true;
        DecreaseDN.GetComponent<Image>().color = Color.white;
        DecreaseDN.GetComponent<Button>().interactable = true;
        Denomination_Bet = Starting_DM;
        Current_Profits = 0;
        EndofGame = false;
    }


}

