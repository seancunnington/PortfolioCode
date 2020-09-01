using UnityEngine;


// Use specified sprites with character's rotation in respect to camera position/rotation.


public class SpriteToCamera : MonoBehaviour
{

     public Transform followTransform;
     public float verticalOffset;

     public bool singleSprite = true;
     Animator spriteAnimator;
     Camera mainCamera;

     private float dotForward = 0f;
     private float dotRight = 0f;


     // Start is called before the first frame update
     void Awake()
     {
          mainCamera = Camera.main;
          spriteAnimator = GetComponent<Animator>();
     }


     void Update()
     {
          RotateTowardsCamera();
          SpritesForDirection();
     }


     // Rotate the GameObject around its Y-Axis to face the camera.
     private void RotateTowardsCamera()
     {
          transform.position = followTransform.position + new Vector3(0, verticalOffset, 0);
          Quaternion targetDirection = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);
          transform.rotation = targetDirection;
     }


     // If the GameObject has different sprites for front, back and sides, change sprites based on Dot to camera.
     private void SpritesForDirection()
     {
          // If there's only one sprite, don't do anything.
          if (singleSprite)
               return;

          // Get the Dot Products of this GameObject to the camera
          dotForward = Vector3.Dot(followTransform.forward, mainCamera.transform.forward);
          dotRight = Vector3.Dot(followTransform.right, mainCamera.transform.forward);

          // and set values in Animator
          spriteAnimator.SetFloat("DotForward", dotForward);
          spriteAnimator.SetFloat("DotRight", dotRight);

          // If pressing either move keys, set MoveBool in Animator
          if (Input.GetButton("Vertical"))
          {
               spriteAnimator.SetBool("MoveBool", true);
          }
          else
          {
               spriteAnimator.SetBool("MoveBool", false);
          }

          
     }
}
