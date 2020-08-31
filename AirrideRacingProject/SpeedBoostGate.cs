using UnityEngine;

public class SpeedBoostGate : MonoBehaviour
{

     private float gateBoostSpeed = 200f;

     // When a player enters the gate trigger, override their boost.
     void OnTriggerEnter(Collider collider)
     {
          Transform player = collider.transform.parent.GetChild(0);
          player.GetComponent<PlayerController>().Boost(1f, gateBoostSpeed);
     }


}
