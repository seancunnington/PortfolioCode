using UnityEngine;

public class CameraBehavior : MonoBehaviour
{

     public float shiftAmount = 15f;
     public float shiftSpeed = 2f;

     public float tiltAmountX = 1f;
     public float tiltAmountY = 1f;
     public float tiltAmountZ = 1f;

     public float speedModifier = 1;

     private Vector3 basePosition;


     private void Start()
     {
          basePosition = transform.localPosition;
     }

     
     public void ShiftCamera(float shiftAmountX, float shiftAmountY, float speedInput, float topSpeed, float boostCharge)
     {

          //speedModifier = ExtensionMethods.Remap(speedInput, 0, topSpeed, 0, 1);
          speedModifier = boostCharge;

          // Shift the camera's position left/right based on the input for turning.
          //Vector3 targetPosition = new Vector3(shiftAmount * shiftAmountX * boostCharge, 0, 0) + basePosition;
          //transform.localPosition = Vector3.Lerp(basePosition, targetPosition, Time.fixedDeltaTime * shiftSpeed);

          // Tilt the camera in the direction turning AND if there is any up/down input.
          Quaternion tiltDirection = Quaternion.Euler(     shiftAmountY * -tiltAmountY,                 // Rotates around right vector, up and down.
                                                            shiftAmountX * tiltAmountZ * boostCharge,    // Rotates around up vector, left and right.
                                                            shiftAmountX * tiltAmountX);                 // Rotates around forward vector, tilting left and right. 

          // tiltDirection is last, so that the calculated rotation is LOCAL.
          transform.rotation = transform.parent.transform.rotation * tiltDirection;

     }

     public void AirborneCamera(float fallingRate)
     {

     }

}
