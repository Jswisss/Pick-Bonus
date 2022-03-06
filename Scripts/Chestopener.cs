using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Chestopener : MonoBehaviour
{
    private GameObject chest;// chest engame
    private GameObject Chest_cap;
    private bool openchest = false;// checks to see if chest has been open
    private GameObject Game_Manager;// grabs game manager to update variables in main Game script
    private TextMeshPro reward_amount;// text for displaying chest rewards
    
    // Start is called before the first frame update
    void Start() // sets private variables
    {
        Game_Manager = GameObject.Find("GameManager");
        chest = this.gameObject;
        Chest_cap = this.gameObject.transform.GetChild(0).gameObject;
        reward_amount = chest.GetComponentInChildren<TextMeshPro>();
        reward_amount.GetComponent<TextMeshPro>().enabled = false;
        chest.GetComponent<Outline>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
       if(Game_Manager.GetComponent<Main_Game_Script>().EndofGame == true)// reset chest animations and turns off and reset test
        {
            Chest_cap.GetComponent<Animator>().SetBool("Open", false);
            reward_amount.text = "";
            reward_amount.GetComponent<TextMeshPro>().enabled = false;
            openchest = false;
            Game_Manager.GetComponent<Main_Game_Script>().StopChestOpening = true;

            chest.GetComponent<Outline>().enabled = false;
        }
       if (Game_Manager.GetComponent<Main_Game_Script>().toggleOutline == true)
        {
            chest.GetComponent<Outline>().enabled = true;
        }
       
    }

    void OnMouseDown()
    {
        Game_Manager.GetComponent<Main_Game_Script>().toggleOutline = false;
        //if chest not open & game is start and the game has not ended do this.
        if (openchest == false && Game_Manager.GetComponent<Main_Game_Script>().Game_start == true && Game_Manager.GetComponent<Main_Game_Script>().EndofGame == false 
            && Game_Manager.GetComponent<Main_Game_Script>().StopChestOpening == true)
        {
            chest.GetComponent<Outline>().enabled = false;
            openchest = true;
            Chest_cap.GetComponent<Animator>().SetBool("Open",true); // activates animation and adjusted position
            Game_Manager.GetComponent<Main_Game_Script>().Display_chest();// activates Display chest

            if (Game_Manager.GetComponent<Main_Game_Script>().Chest_result == 0) // see if pooper
            {
                reward_amount.text = "Pooper";
                Game_Manager.GetComponent<Main_Game_Script>().StopChestOpening = false;
            }
            else
            {//updates current last game amounts, sets text to chest rewards, update UI last game win amounts
                Game_Manager.GetComponent<Main_Game_Script>().current_Last_game = Game_Manager.GetComponent<Main_Game_Script>().current_Last_game + 
                    Game_Manager.GetComponent<Main_Game_Script>().Chest_result;
                reward_amount.text = "$ " + Game_Manager.GetComponent<Main_Game_Script>().Chest_result.ToString("F2");
                //Game_Manager.GetComponent<Main_Game_Script>().Last_Game_Win_Amount.text = " Lastest Gains: $" + Game_Manager.GetComponent<Main_Game_Script>().current_Last_game.ToString("F2");
            }

            Game_Manager.GetComponent<Main_Game_Script>().Chest_result = 0;// reset chest results

            Invoke("turnOnText", 1.7f);

            if (reward_amount.text == "Pooper")
            {//game ends
                Invoke("Delay", 2.3f);
            }
        }
        
    }

    public void turnOnText()// Turns on text for the chest rewards and updates Lastest gains text
    {
        reward_amount.GetComponent<TextMeshPro>().enabled = true;
        Game_Manager.GetComponent<Main_Game_Script>().Last_Game_Win_Amount.text = " Lastest Gains: $" + Game_Manager.GetComponent<Main_Game_Script>().current_Last_game.ToString("F2");
    }

    public void Delay() // Ends the round of chest opens
    {
        Game_Manager.GetComponent<Main_Game_Script>().EndofGame = true;
    }
}
