using UnityEngine;

[ExecuteInEditMode]
public class PerspectiveTransition : MonoBehaviour
{

     public enum PerspectiveMode
     {
          Outside,
          OverShoulder
     };
     public PerspectiveMode currentPerspective;

     public Vector3 OuterPosition;
     public int OuterRotation;
     private Quaternion startingRotation;

     public Transform OverShoulderTarget;
     private Vector3 OverShoulderPosition;
     public Vector3 OverShoulderOffset;

     public bool togglePerspective = false;

     private bool transitioning = false; // indicates when the camera is moving.

     [Range(0f,1f)]
     private float transitionSlider = 0;
     private float transitionSpeed = 1f;


     private void Awake()
     {
          currentPerspective = PerspectiveMode.Outside;
          togglePerspective = false;
          transitionSlider = 0f;
          transform.position = OuterPosition;
          startingRotation = transform.rotation;
     }


     void Update()
     {
          SetPosition();
          SetRotation();
          ChangePerspective();
     }

     private void FixedUpdate()
     {
          //SetPosition();
     }


     private void SetPosition()
     {
          OverShoulderPosition = OverShoulderTarget.position
                                        + OverShoulderTarget.right * OverShoulderOffset.x
                                        + OverShoulderTarget.up * OverShoulderOffset.y
                                        + OverShoulderTarget.forward * OverShoulderOffset.z;

          if (currentPerspective == PerspectiveMode.OverShoulder && !transitioning)
               transform.position = OverShoulderPosition;
     }

     private void SetRotation()
     {
          if (currentPerspective == PerspectiveMode.OverShoulder && !transitioning)
               transform.rotation = OverShoulderTarget.rotation;
     }



     private void ChangePerspective()
     {
          // If toggle perspective is FALSE, do not do anything.
          if (!togglePerspective)
               return;


          // If toggle perspective is TRUE and transitioning is FALSE, then begin transition.
          if (togglePerspective && !transitioning)
               transitioning = true;
          

          // Currently Outside: transition to over-shoulder.
          // slider currently = 0. ADD.
          if (currentPerspective == PerspectiveMode.Outside && transitioning)
          {
               // Move camera via slider
               transitionSlider += Time.deltaTime * transitionSpeed;

               // Once completely moved, change camera state and stop transitioning.
               if (transitionSlider >= 1)
               {
                    currentPerspective = PerspectiveMode.OverShoulder;
                    transitioning = false;
                    togglePerspective = false;
               }
          }

          // Currently OverShoulder: transition to outside.
          // slider currently = 1. SUBTRACT.
          if (currentPerspective == PerspectiveMode.OverShoulder && transitioning)
          {
               // Move camera via slider
               transitionSlider -= Time.deltaTime * transitionSpeed;

               // Once completely moved, change camera state and stop transitioning.
               if (transitionSlider <= 0)
               {
                    currentPerspective = PerspectiveMode.Outside;
                    transitioning = false;
                    togglePerspective = false;
               }
          }

          // Move the camera
          transform.position = Vector3.Lerp(OuterPosition, OverShoulderPosition, transitionSlider);

          // Rotate the camera
          transform.rotation = Quaternion.Lerp(startingRotation, OverShoulderTarget.rotation, transitionSlider);
     }
}
