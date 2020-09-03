using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharController : MonoBehaviour {


     public enum CharState
     {
          Idle,
          Move
     };
     public CharState charState;


     // A list of tags this character can be referenced by.
     public List<string> charTags = new List<string>();

     public float currentSpeed = 0f;
     public float speedWalk = 0f;
     public float speedRun = 0f;

     //Left (-1) and Right(1)
     public int direction = 1;

     Rigidbody2D charBody;

     //Vector3 debugVector;



     private void Awake()
     {
         //debugVector = transform.position;
          charBody = GetComponent<Rigidbody2D>();
     }


     private void Update()
     {
          // Enum State Controller
          if (charState == CharState.Idle) { } // currently run no code -- will return to this later
          if (charState == CharState.Move) Move();

     }


     // Moves the character with different speeds and directions along x-axis. 
     public void Move()
     {

          charBody.AddForce(new Vector2(currentSpeed * direction, 0), ForceMode2D.Force);
     }



     // Stops all player movement. 
     public void Stop()
     {
          currentSpeed = 0f;
     }


     
     
     private void OnDrawGizmos()
     {
          // Draw the movement vector
          Gizmos.color = Color.yellow;
          //Gizmos.DrawLine(transform.position, debugVector);
          Gizmos.color = Color.white;
     }
}
