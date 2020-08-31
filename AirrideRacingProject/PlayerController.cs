using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerController : MonoBehaviour
{
     public playerStateEnum playerState = playerStateEnum.Idle;
     public enum playerStateEnum
     {
          Idle,
          ChargeBoost,
          Airborne,
          Death
     }

     #region Player Stats
     [Header("Speed")]
     public float topSpeed = 40f;
     public float acceleration = 5f;
     public float deacceleration = 1f;

     [Header("Boost")]
     [Range(40f, 200f)]
     public float boostSpeed = 110;
     public float boostTimer = 1f;
     public float chargeRate = 1f;

     [Header("Flying and Rotation")]
     [Range(1f, 50f)]
     public float maxFlyTilt = 1f;
     [Range(10f, 100f)]
     public float kartHorizontalTilt = 1f;
     [Range(10f, 100f)]
     public float kartVirticalTilt = 1f;

     [Header("Steering")]
     public float steering = 40f;
     [Range(1f, 2.5f)]
     public float driftSteering = 1f;

     [Header("")]

     // Parameter Storage: used for power up collection
     private float topSpeedStorage = 0;
     private float accelerationStorage = 0;
     private float deaccelerationStorage = 0;
     private float boostSpeedStorage = 0;
     private float boostTimerStorage = 0;
     private float steeringStorage = 0;
     private float driftSteeringStorage = 0;
     private float maxFlyTiltStorage = 0;

     [Header("Miscellaneous")]
     // Hole Count
     public int holeCount = 0;

     // Speed, Drifting
     private Vector3 currentForwardVector;
     private float speed, currentSpeed;
     private int driftDirection;

     // Rotation Data
     private float rotate, currentRotate;
     private bool rotateToNormal = true;
     private RaycastHit surfaceNormalHit;
     private int airDir;
     private float airDirAmount; 

     // Flying
     private Vector3 tiltingForwardVector = new Vector3(0, 0, 0);
     public float currentFlyTilt = 1f;
     public float flyTiltCap;
     public float flyTiltLoss;
     public float tiltSmoothing = 0f;

     // Boosting
     private bool boost = false;
     public float boostCharge = 0f;
     public float kartLowerAmount = 1f;
     private bool startChargeReset = false;


     public LayerMask groundedMask;
     public LayerMask orientNormalMask;
     public LayerMask debugMask;
     private float rotationTimeScale = 5f;

     [Header("Necessary Components")]
     public Transform kartNormal;
     public Rigidbody sphere;
     #endregion

     #region Camera Properties
     [Header("Camera Properties")]
     Transform cameraTransform;
     Transform whiteOut;
     public float alphaRange;
     public float alphaSensitivity;
     #endregion


     // Vector Visualization
     Vector3 rotateVectorView;



     void Start()
     {
          cameraTransform = Camera.main.transform;
          whiteOut = GameObject.FindGameObjectWithTag("WhiteOut").transform;
          alphaRange = 0f;
     }


     #region Update and Fixed Update

     // Controller State Machine
     private void Update()
     {
          FollowCollider();
          SetCurrentSpeedAndRotation();   

          switch (playerState)
          {
               case playerStateEnum.Idle:
                    Accelerate();       // Automatically move forward.
                    SteerAmount();
                    CheckBoost();
                    CheckGround(false);      // If true -> Idle state.  Else if false -> Airborne state.
                    break;
               case playerStateEnum.ChargeBoost:
                    ChargingBoost();
                    break;
               case playerStateEnum.Airborne:
                    Accelerate();
                    AirborneSteering();
                    CheckGround(true);      // If true -> Idle state.  Else if false -> Airborne state.
                    break;
               case playerStateEnum.Death:
                    PlayerDeathEvent();
                    break;
          }
     }


     // Physics and Base Mechanics
     private void FixedUpdate()
     {
          // Set the Forward Vector -- safety for airborne steering.
          if (playerState != playerStateEnum.Airborne)
               currentForwardVector = transform.forward;


          // Forward Acceleration
          sphere.AddForce(currentForwardVector * currentSpeed, ForceMode.Acceleration);


          // Rotation to Surface Normals
          //if (rotateToNormal)
          //{
               GetComponent<GravityBody>().disableRotation = true;
               RotationOrientation(rotateToNormal);
          //}

          // Boost Acceleration
          if (boost)
          {
               sphere.AddForce(transform.forward * boostSpeed * boostCharge, ForceMode.Impulse);
               boost = false;
          }

          // Boost Animation
          // Sink player towards ground depending on strength of charge.
          if (startChargeReset)
               ResetBoost();

          Vector3 targetPosition = new Vector3(0f, kartLowerAmount, 0f);
          kartNormal.localPosition = Vector3.Lerp(Vector3.zero, targetPosition, Time.fixedDeltaTime * 80f * boostCharge);

           /*
          // Downward Boost Force: Grounding
          if (grounded = false && boost == true)
          {
               sphere.AddForce(-transform.up * boostSpeed, ForceMode.Impulse);
               boost = false;
          }
          */

          // Steering
          // Idle:
          float yAngle = (currentRotate / 50);
          float xAngle = Input.GetAxis("Vertical");
          transform.Rotate(0f, yAngle, 0f, Space.Self);
          KartBodySteeringTilt(xAngle, yAngle, -kartHorizontalTilt, kartVirticalTilt);

          // Camera Shifting
          cameraTransform.GetComponent<CameraBehavior>().ShiftCamera(yAngle, xAngle, currentSpeed, topSpeed, boostCharge);
     }

     #endregion


     #region Base Rotation and Movement

     // Has the Kart Controller's position follow the collider's position;
     private void FollowCollider()
     {
          transform.position = sphere.transform.position;
     }


     private void SetCurrentSpeedAndRotation()
     {
          currentSpeed = Mathf.SmoothStep(currentSpeed, speed, Time.deltaTime * 12f);
          currentRotate = Mathf.Lerp(currentRotate, rotate, Time.deltaTime * 4f);
          rotate = 0f; // This stops rotation when no button is pressed.
     }


     // Rotates the character's up-vector to the plane's normal currently below it, but just for surfaces with the orientNormal mask. 
     // Testing the case of: high speed and rotating with a curve that is not the planet's surface.
     private void RotationOrientation(bool rotateToNormal)
     {
          Vector3 targetDirection;

          // Rotate to Normal
          // Specify whether to rotate towards nearby surface normal or towards planet center.
          if (rotateToNormal)
          {
               targetDirection = surfaceNormalHit.normal;
               sphere.AddForce(-targetDirection * 50f, ForceMode.Force);
          }
          else
          {
               targetDirection = (transform.position - new Vector3(0, 0, 0)).normalized;
          }


          Quaternion targetRotation = Quaternion.FromToRotation(transform.up, targetDirection) * transform.rotation;
          transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationTimeScale * Time.fixedDeltaTime);
     }
     


     private void Accelerate()
     {
          // Automatically move forward.
          if (speed < topSpeed)
               speed += acceleration;

          // Speed can go over Top Speed via boosts. This decreases Speed back down over time. 
          if (speed > topSpeed)
               speed -= deacceleration;
     }
     #endregion


     #region Steering

     private void AirborneSteering()
     {
          // Player can fly for period of time.
          // Done via a tilting cap that subtracts over time. 
          if (flyTiltCap > 1)
               flyTiltCap -= flyTiltLoss;
          else
               flyTiltCap = 1;

          SteerAmount();
     }



     private void SteerAmount()
     {
          // Steering Left and Right:
          if (Input.GetAxisRaw("Horizontal") != 0)
          {
               int dir = Input.GetAxis("Horizontal") > 0 ? 1 : -1;
               float amount = Mathf.Abs(Input.GetAxis("Horizontal"));

               Steer(dir, amount);
          }

          // If Airborne, extend or shorten flight by tilting up or down:
          if (playerState == playerStateEnum.Airborne)
          {
               if (Input.GetAxisRaw("Vertical") != 0)
               {
                    airDir = Input.GetAxis("Vertical") > 0 ? 1 : -1;
                    airDirAmount = Mathf.Abs(Input.GetAxis("Vertical"));
                    if (airDir > 0)
                         currentFlyTilt = Mathf.Clamp(airDirAmount * maxFlyTilt, 1, flyTiltCap);
                    else
                         currentFlyTilt = maxFlyTilt;
               }
               else   // If no vertical input, then reset.
               {
                    airDir = 1;
                    airDirAmount = 0f;
               }

               // Tilt the Forward Vector of the applies the kart's physics movement
               currentForwardVector = Vector3.Lerp(transform.forward, transform.up * airDir, Time.fixedDeltaTime * airDirAmount * currentFlyTilt);
          }

          /*
          // If not airborne, then reset forward vector to the transform's.
          if (playerState != playerStateEnum.Airborne)
          {
               currentForwardVector = transform.forward;
          }
          */

     }


     private void Steer(int direction, float driftingAmount)
     {
          rotate = (steering * direction) * driftingAmount;
     }



     /// <summary>
     /// Tilt model to the left or right while turning.
     /// Tilt model up or down if in the air.
     /// Tilt amount depends on turn amount.
     /// </summary>
     /// <param name="steeringInput"></param>
     /// <param name="tiltModifier"></param>
     private void KartBodySteeringTilt(float xSteeringInput, float zSteeringInput, float hTiltModifier = 1f, float vTiltModifier = 1f)
     {

          // Horizontal Tilt
          float zSteeringAmount = zSteeringInput * hTiltModifier;


          // Smoothing transition of vertical tilt to non-tilt and vice versa.
          if (playerState != playerStateEnum.Airborne)
          {
               if (tiltSmoothing > 0)
                    tiltSmoothing -= 10f * Time.deltaTime;
               else
                    tiltSmoothing = 0f;
          }
          else
          {
               if (tiltSmoothing < 1)
                    tiltSmoothing += 10f * Time.deltaTime;
               else
                    tiltSmoothing = 1f;
          }

          float stateSwitch = Mathf.Lerp(0f, 1f, tiltSmoothing);                // the transition smoothing
          float verticalTilt = xSteeringInput * hTiltModifier * stateSwitch;    // the amount to vertically tilt w/ smoothing

          // Vertical Tilt
          float xSteeringAmount = Mathf.Lerp(0f, verticalTilt, Time.fixedDeltaTime * vTiltModifier);


          // The direction to rotate.
          Quaternion tiltDirection = Quaternion.Euler(xSteeringAmount, 0f, zSteeringAmount);

          // Rotate around local forward vector.
          kartNormal.rotation = transform.localRotation * tiltDirection;
     }
     #endregion


     #region Boost

     private void CheckBoost()
     {
          // If STARTING Charge Boost:
          if (Input.GetButtonDown("Jump") && boostCharge == 0) // && Input.GetAxis("Horizontal") != 0)
          {
               // Set the drift direction, then change the state. 
               driftDirection = Input.GetAxis("Horizontal") > 0 ? 1 : -1;
               startChargeReset = false;
               playerState = playerStateEnum.ChargeBoost;
          }
     }


     private void ChargingBoost()
     {
          // Stop forward movement via deacceleration:
          // The rate at which speed drops before player comes to a stop.
          if (speed > 0)
               speed -= deacceleration;

          // Increase steering ability.
          ChargeSteerAmount(driftSteering);

          // Determines if the player gets the full charge or not. 
          // Charges over time while button held.
          
          if (boostCharge < 1)
               boostCharge += Time.fixedDeltaTime * chargeRate;
          else
               boostCharge = 1;
          

          // If ENDING Charge Boost:
          if (Input.GetButtonUp("Jump"))
          {
               // Applies physics impulse: Gets that snappy beginning feel.
               boost = true;
               
               // Sustains the boost for a period of time.
               Boost(boostCharge);
               //playerState = playerStateEnum.Idle;
          }
     }



     public void Boost(float charge, float boostSpeedOverride = 1f)
     {
          startChargeReset = true;

          // If the Boost Override is used, i.e. Speed Gates:
          if (boostSpeedOverride != 1f)
          {
               currentSpeed = speed = boostSpeedOverride;
          }
          else // use the kart's boost stat
          {
               currentSpeed = speed = (boostSpeed * charge);
          }

          Invoke("EndBoost", boostTimer);
     }


     private void EndBoost()
     {
          playerState = playerStateEnum.Idle;
     }


     private void ResetBoost()
     {
          if (boostCharge > 0)
               boostCharge -= Time.fixedDeltaTime * 5f;
          else
               boostCharge = 0;
     }
     

     private void ChargeSteerAmount(float steerAmount)
     {
          if (Input.GetAxisRaw("Horizontal") != 0)
          {
               int dir = Input.GetAxis("Horizontal") > 0 ? 1 : -1;
               float amount = ExtensionMethods.Remap(Input.GetAxis("Horizontal"), -1, 1, steerAmount, steerAmount);

               Steer(dir, amount);
          }
     }
     #endregion


     #region Idle to Airborne State Control

     private void CheckGround(bool lookingForGround)
     {
          // If true, then player is currently grounded. Look for possibility of Air.
          // If false, then player is currently airborne. Look for possibility of Ground. 
          bool groundBelow;
          bool normalBelow;

          // Raycast down to check for ground below the player.
          Ray rayBelowKart = new Ray(transform.position, -transform.up);
          Ray rayTowardPlanet = new Ray(transform.position, Vector3.zero - transform.position);

          RaycastHit hit;

          // Looking for regular ground:
          if (Physics.Raycast(rayBelowKart, out hit, 1.5f, groundedMask))
          {
               groundBelow = true;
               normalBelow = false;
          }
          // Looking for orient normal ground:
          else if (Physics.Raycast(rayBelowKart, out hit, 1.5f, orientNormalMask))
          {
               groundBelow = false;
               normalBelow = true;
          }
          // If neither, then no ground is below. 
          else
          {
               groundBelow = false;
               normalBelow = false;
          }

          // If either ground detected, check the layer mask below kart.
          if (groundBelow == true || normalBelow == true)
          {
               CheckLayerMask();
          }

          // Airborne: If ground is below and if looking for Ground, then set Idle.
          if ((groundBelow == true || normalBelow == true) && lookingForGround == true)
          {
               playerState = playerStateEnum.Idle;
          }
               

          // Grounded: If ground is NOT below and looking for Air, then set Airborne.
          if (groundBelow == false && normalBelow == false && lookingForGround == false)
          {
               rotateToNormal = false;
               GetComponent<GravityBody>().disableGravity = false;
               flyTiltCap = maxFlyTilt;  // Reset the tilting cap.
               // small boost?
               playerState = playerStateEnum.Airborne;
          }
     }


     private void CheckLayerMask()
     {
          // Raycast down to check for ground below the player.
          Ray rayBelowKart = new Ray(transform.position, -transform.up);
          Ray rayTowardPlanet = new Ray(transform.position, Vector3.zero - transform.position);


          // If the layer is GROUND, then apply rotation to the planet.
          if (Physics.Raycast(rayBelowKart, out surfaceNormalHit, 1.5f, groundedMask))
          {
               rotateToNormal = false;
               GetComponent<GravityBody>().disableGravity = false;
               debugMask = groundedMask; // delete after debugging
          }

          // If the layer is ORIENT NORMAL, the apply rotation to the ground normal.
          if (Physics.Raycast(rayBelowKart, out surfaceNormalHit, 1.5f, orientNormalMask))
          {
               rotateToNormal = true;
               GetComponent<GravityBody>().disableGravity = true;
               debugMask = orientNormalMask; // delete after debugging
          }
     }
     #endregion


     #region Stat Storage and Manipulation

     public void StoreStats(int statType, float amount)
     {
          switch (statType)
          {
               case 1: // Top Speed
                    topSpeedStorage += amount;
                    break;
               case 2: // Acceleration
                    accelerationStorage += amount;
                    break;
               case 3: // Deacceleration
                    deaccelerationStorage += amount;
                    break;
               case 4: // Boost Speed
                    boostSpeedStorage += amount;
                    break;
               case 5: // Boost Timer
                    boostTimerStorage += amount;
                    break;
               case 6: // Steering Sensitivity
                    steeringStorage += amount;
                    break;
               case 7: // Drift Steering Sensitivity
                    driftSteeringStorage += amount;
                    break;
               case 8: // Fly Tilting
                    maxFlyTiltStorage += amount;
                    break;
          }
     }


     public void ApplyStats()
     {
          topSpeed += topSpeedStorage;
          topSpeedStorage = 0;

          acceleration += accelerationStorage;
          accelerationStorage = 0;

          deacceleration += accelerationStorage;
          accelerationStorage = 0;

          boostSpeed += boostSpeedStorage;
          boostSpeedStorage = 0;

          boostTimer += boostTimerStorage;
          boostTimerStorage = 0;

          steering += steeringStorage;
          steeringStorage = 0;

          driftSteering += driftSteeringStorage;
          driftSteeringStorage = 0;

          maxFlyTilt += maxFlyTiltStorage;
          maxFlyTiltStorage = 0;
     }

     #endregion


     #region Drawing
     /*
     private void OnDrawGizmos()
     {
          Gizmos.color = Color.blue;
          Gizmos.DrawLine(kartNormal.position, transform.position - transform.up * 1.5f);

          Gizmos.color = Color.yellow;
          Gizmos.DrawLine(transform.position, transform.position + currentForwardVector);
     }
     */
     #endregion



     /*
     private void Jump()
     {
          if (Input.GetButtonDown("Jump"))
          {
               if (grounded)
                    sphere.AddForce(transform.up * jumpForce, ForceMode.Impulse);
          }

          // Checking if anything is in front of player.
          grounded = false;
          Ray ray = new Ray(transform.position, -transform.up);
          RaycastHit hit;

          if (Physics.Raycast(ray, out hit, 1 + .1f, groundedMask))
               grounded = true;
     }
     */


     public void SetDeath()
     {
          playerState = playerStateEnum.Death;
     }


     public bool GetDeath()
     {
          if (playerState == playerStateEnum.Death)
               return true;
          else
               return false;
     }


     /// <summary>
     /// White Out screen becomes fully opaque, then game restarts. 
     /// </summary>
     private void PlayerDeathEvent()
     {
          alphaRange += Time.deltaTime * alphaSensitivity;
          whiteOut.GetComponent<MeshRenderer>().material.SetFloat("alpha", alphaRange);

          Invoke("ReloadScene", 3f);
     }


     private void ReloadScene()
     {
          SceneManager.LoadScene("SkaterScene_Og", LoadSceneMode.Single);
     }

     
}
