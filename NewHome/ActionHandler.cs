using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionHandler : MonoBehaviour {

     [Header("Action Queue")]
     public List<string> queue = new List<string>();

     int wordIndex = 0;

     public bool insideAction = false;

     public CharController target_1;
     public CharController target_2;


     // Modified Variable Storage
     bool searchingForAdjectives = false;


     public void ProcessActionSequence()
     {
          //Debug.Log("Starting Action Sequence.");

          // Loop through the queue of words until all are used.
          for (wordIndex = 0; wordIndex < queue.Count; wordIndex++)
          {
               SendMessage(queue[wordIndex]);
          }

          // After all words are used, clear the queue.
          queue.Clear();

          //Debug.Log("Ending Action Sequence.");

     }


     // Functions associated with each word
     // This section will have all functions for every word in the dictionary. 
     // Because all of these functions are called via INVOKE, they cannot have parameters. 
     // Must make use of target characters' parameters within their 'CharController' script.

     #region Section Type: Action

          #region Sub-Section: Movement

     // Move by distance, time or until they hit a collider, 
     // and move at a set speed.
     void Movement(float speed)
     {
          // If we are already inside an Action, then this Action is not relevant. 
          // Abort.
          if (insideAction == true)
          {
               // This also indicates an end to all action words.
               // End adjective searching:
               searchingForAdjectives = false;

               return;
          }


          else // We can now enter this Action. 
          {
               /* -- Declare we have entered this Action -- */
               insideAction = true;

               // Set move speed
               target_1.currentSpeed = speed;

               // Attempt to use all move adjectives
               searchingForAdjectives = true;

               // Loop through the following words UNTIL not searching for adjectives.
               for (int i = wordIndex; i < queue.Count-1; i++)
               {
                    if (!searchingForAdjectives)
                         break;

                    SendMessage(queue[i + 1]);
               }

               // Allow the target to move
               target_1.charState = CharController.CharState.Move;

               /* -- Now that the Action is complete, exit the Action -- */
               insideAction = false;
          }    
     }

               #region Movement Synonyms

     void WALK()
     {
          Movement(target_1.speedWalk);
     }


     void GO()
     {
          Movement(target_1.speedWalk);
     }


     void RUN()
     {
          Movement(target_1.speedRun);
     }

     #endregion // Movement Synonyms Region: End


     void STOP()
     {
          // If we are already inside an Action, then this Action is not relevant. 
          // Abort.
          if (insideAction == true)
          {
               // This also indicates an end to all action words.
               // End adjective searching:
               searchingForAdjectives = false;

               return;
          }

          target_1.currentSpeed = 0f;
          target_1.charState = CharController.CharState.Idle;
     }

          #endregion // Movement Region: End

          #region Sub-Section: Speaking
     void SPEAK()
     {
          // If we are already inside an Action, then this Action is not relevant. 
          // Abort.
          if (insideAction == true)
          {
               // This also indicates an end to all action words.
               // End adjective searching:
               searchingForAdjectives = false;

               return;
          }


          Debug.Log("Performing 'Speak' Function.");

     }

     void INSPECT()
     {

          // If we are already inside an Action, then this Action is not relevant. 
          // Abort.
          if (insideAction == true)
          {
               // This also indicates an end to all action words.
               // End adjective searching:
               searchingForAdjectives = false;

               return;
          }

          Debug.Log("Performing 'Inspect' Function.");

     }

          #endregion // Speaking Region: End

     #endregion // Action Region: End


     // -- //


     #region Section Type: Adjective

          #region Direction

     void RIGHT()
     {
          ChangeDirection(1);
     }

     void LEFT()
     {
          ChangeDirection(-1);
     }

     void ChangeDirection(int directionToGo)
     {
          // If we are currently inside an action, and this is the 2nd word being called,
          if (insideAction == true)
          {
               // Change the target's direction.
               target_1.direction = directionToGo;

               // If used for an action, increment word index:
               wordIndex++;
          }

          // If we are NOT inside an action, and this is the 1st word being called,
          // then this is applying a description to objects within vicinity. 
          // Search tags?
          if (insideAction == false)
          {

          }
     }


     #endregion // Direction Region: End

          #region Speed

     void SpeedChange(float newSpeed)
     {
          // If we ARE in an action, directly change the speed.
          if (insideAction)
          {
               // simply half the speed;
               target_1.currentSpeed = target_1.currentSpeed * newSpeed;

               // and indicate that the word was used via word index
               wordIndex++;
          }

          // If not currently in an action, shift word's position down the Action Queue until applicable.
          if (!insideAction)
          {
               // If this is the last word in the list when trying to move, delete this word. 
               if (wordIndex + 2 > queue.Count)
               {
                    Debug.Log(queue[wordIndex] + " at end of list. Deleting.");
                    queue.RemoveAt(wordIndex);
               }
               else
               {
                    // Else, shift this word.
                    // Insert the word into the new position
                    queue.Insert(wordIndex + 2, queue[wordIndex]);

                    // Delete the original word
                    queue.RemoveAt(wordIndex);

                    // Shift word index back to compensate
                    wordIndex--;
               }
          } 
     }


     void SLOWLY()
     {
          SpeedChange(0.8f);
     }

     void QUICKLY()
     {
          SpeedChange(1.5f);
     }


          #endregion // Speed Region: End

     #region Distance
     // All distance is going to be preset in Metric (b/c Imperial SUCKS).
     // Conversions will still be made for Imperial (unfortunately).

     // Metric //

     private void METERS(int distance)
     {

     }
     

     // Imperial //

     // Convert unit from Meters to Feet
     private void FEET(int distance)
     {
          
     }

     #endregion


     #endregion




}
