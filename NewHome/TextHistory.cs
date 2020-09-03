using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextHistory : MonoBehaviour {

     public GameObject[] historyDisplay;


     // Shift the history so that the first position (pos 0) receives the input text just
     // submitted by the player, and every history position after that receives the text
     // from the position becore it.
     public void IncrementHistory(string newInputText)
     {
          for (int i = transform.childCount - 1; i >= 0; i--)
          {
               if (i != 0)
                    transform.GetChild(i).GetComponent<Text>().text = transform.GetChild(i - 1).GetComponent<Text>().text;
               else
                    transform.GetChild(i).GetComponent<Text>().text = newInputText;
          }
     }


     // Have the history text become more transparent the more upwards it is on the screen.
     public void SetTextTransparency(bool isTransparent)
     {
          // if transparent, then have children's text become more 
          // transparent the higher on the y axis they are.
          if (isTransparent){
               float textAlpha = 1f;

               for (int i = 0; i < transform.childCount; i++)
               {
                    transform.GetChild(i).GetComponent<Text>().color = new Vector4(1, 1, 1, textAlpha);
                    textAlpha -= 0.2f;
               }
          }
          else
          {
               for (int i = 0; i < transform.childCount; i++)
               {
                    transform.GetChild(i).GetComponent<Text>().color = new Vector4(1, 1, 1, 1);
               }
          }
     }


}
