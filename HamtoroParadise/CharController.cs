using UnityEngine;

public class CharController : MonoBehaviour
{
     public float moveSpeed;
     public float rotateSpeed;
     public Transform playerCamera;

     public float gizmoLength = 3f;
     public bool toggleGizmos = true;


     // Start is called before the first frame update
     void Start()
     {
        
     }

     // Update is called once per frame
     void Update()
     {
          MoveCharacter();
          TogglePerspective();
     }



     private void MoveCharacterWASD()
     {
          float moveX = 0f;
          float moveZ = 0f;

          // X-Axis is left/right movement.
          if (Input.GetButton("Horizontal"))
          {
               moveX = Input.GetAxisRaw("Horizontal") * Time.deltaTime * moveSpeed;
          }

          // Z-Axis is forward/back movement.
          if (Input.GetButton("Vertical"))
          {
               moveZ = Input.GetAxisRaw("Vertical") * Time.deltaTime * moveSpeed;
          }

          // Move Character
          transform.localPosition += new Vector3(moveX, 0, moveZ);
     }


     private void MoveCharacter()
     {
          float moveForward = 0f;
          float rotate = 0f;

          // Rotate Character.
          if (Input.GetButton("Horizontal"))
          {
               rotate = Input.GetAxisRaw("Horizontal") * Time.deltaTime * rotateSpeed;
               transform.RotateAround(transform.position, transform.up, rotate);
          }

          // Move Forward.
          if (Input.GetButton("Vertical"))
          {
               moveForward = Input.GetAxisRaw("Vertical") * Time.deltaTime * moveSpeed;
               transform.Translate(new Vector3(0, 0, moveForward));
          }

          //transform.localPosition += new Vector3(0, 0, moveForward);
     }


     private void TogglePerspective()
     {
          if (Input.GetButtonDown("Jump"))
               playerCamera.GetComponent<PerspectiveTransition>().togglePerspective = true;
     }


     // Draw character orientation in world space.
     private void OnDrawGizmos()
     {

          if (toggleGizmos)
          {
               // Forward
               Gizmos.color = Color.blue;
               Gizmos.DrawLine(transform.position, transform.position + transform.forward * gizmoLength);

               // Back
               Gizmos.color = Color.red;
               Gizmos.DrawLine(transform.position, transform.position + transform.forward * -gizmoLength);

               // Left
               Gizmos.color = Color.yellow;
               Gizmos.DrawLine(transform.position, transform.position + transform.right * gizmoLength);

               // Right
               Gizmos.DrawLine(transform.position, transform.position + transform.right * -gizmoLength);
          }
     }


     
}
