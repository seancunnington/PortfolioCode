using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InputController : MonoBehaviour
{
     string inputText;
     public InputField inputField;
     public GameObject inputFieldText;

     public GameObject HistoryHandler;
     int historyCounter = -1;

     public Dictionary dictionary;
     public ActionHandler actionHandler;


     private void Start()
     {
          inputField.ActivateInputField();
     }
  

     public void Update()
     {
          InputManager();

          // If the input field ever loses focus (the player using the mouse to click
          // away from the input text region), then auto re-focus back to the text field. 
          if (!inputField.isFocused)
               inputField.Select();

     }


     private void InputManager()
     {
          // Enter
          if (Input.GetKeyDown(KeyCode.Return))
          {
               MatchText(); // Filter for words and send them to Action Handler.
               StoreText(); // Store input text in History Handler for later use.
          }

          // Up/Down Arrows
          if (Input.GetButtonDown("Vertical"))
               NavigateHistory(Input.GetAxis("Vertical")); // Move through History Handler queue for repeated text uses.


          // Shift


          // Control
          if (Input.GetKey(KeyCode.LeftControl))
               HistoryHandler.GetComponent<TextHistory>().SetTextTransparency(false);
          else
               HistoryHandler.GetComponent<TextHistory>().SetTextTransparency(true);
     }


     // Filter through the entire input string and find each word via space separations.
     // Send recogized words via Dictionary to the Action Handler. 
     private void MatchText()
     {
          int currentIndex = 0;
          string textIn = inputField.text + " ";  //adding the final space to help with isolating the last word

          // Setting all text to uppercase so there's no issue with calling a word's 
          // associated function inside Action Handler.
          textIn = textIn.ToUpper();

          // Filter the string for each word
          while (currentIndex < textIn.Length)
               currentIndex = FilterOneWord(currentIndex, textIn);

          // Once the input string is filtered and the Action Queue is filled, 
          // then begin Action Sequence.
          actionHandler.ProcessActionSequence();
     }



     // Filter one word out of the string by searching for spaces
     private int FilterOneWord(int index, string textIn)
     {
          string tempWord = "";

          // loop through the string until a 'space' OR punctuation is found.
          for (int i = index; i < textIn.Length; i++)
          {
               char c = textIn[i];

               if (c == ' ' || c == ',' || c == '.' || c == '!' || c == '?')
               {
                    index = i;
                    break;
               }
               tempWord = tempWord + c;
          }

          // set the index after the found space
          index++;

          // send the found word to the Action Handler to store
          AddToActionHandler(tempWord);

          // Save the index for further use within the same string
          return index;
     }


     // Add words to Action Handler queue
     private void AddToActionHandler(string word)
     {
          if (CompareToDictionary(word))
               actionHandler.queue.Add(word);
     }


     // Check to see if the attempted word exists in the dictionary. Return bool results.
     private bool CompareToDictionary(string word)
     {
          if (dictionary.dictionary.Contains(word))
               return true;
          else
          {
               Debug.Log("'" + word + "' not found.");
               return false;
          }
     }



     private void NavigateHistory(float inputDirection)
     {
          // Set up the vertical direction
          int direction;

          if (inputDirection > 0)       direction = 1;
          else if (inputDirection < 0)  direction = -1;
          else                          direction = 0;

          historyCounter += direction;
          direction = 0;

          //If history counter is -1, then delete input field text and exit function
          if (historyCounter <= -1)
          {
               historyCounter = -1;
               inputField.text = "";
               return;
          }


          //Clamp the counter within history child range
          if (historyCounter < 0)
               historyCounter = 0;
          if (historyCounter >= HistoryHandler.transform.childCount)
               historyCounter = HistoryHandler.transform.childCount-1;

          //Check if this history position has any user input yet
          if (HistoryHandler.transform.GetChild(historyCounter).GetComponent<Text>().text == "")
          {
               // If not, keep it at the oldest user input
               if (historyCounter > 0)
                    historyCounter--;  
               else
                    historyCounter = 0;
          }
          
          //Assign the input field with the associated history text
          inputField.text = HistoryHandler.transform.GetChild(historyCounter).GetComponent<Text>().text;
     }



     public void StoreText()
     {
          // If there's nothing in the input field, don't store it.
          if (inputField.text == "")
               return;

          // Store the text in the History and increment History
          inputText = inputFieldText.GetComponent<Text>().text;
          HistoryHandler.GetComponent<TextHistory>().IncrementHistory(inputText);

          //Reset input text to nothing
          inputField.text = "";

          //Reset History Counter
          historyCounter = -1;

          // Reactivate input text
          inputField.ActivateInputField();
     }


}
