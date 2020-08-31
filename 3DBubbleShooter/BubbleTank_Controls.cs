using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BubbleTank_Controls : Base_Character
{

    #region Collectables Variables

    int hxpTotal;

    #endregion

    #region Body Card Variables
        public BubbleBody bodyCard;
        private GameObject bubbleBrain;

    #endregion

    #region Movement & Rotation Variables

        PlayerInputActions controls_PlayerInput;

        // Using two control states to fix rotation conflicts
        // If any input read from one control scheme, then switch to that control scheme.
        // Isolates rotation so it doesn't read from multiple sources.
        enum Control_State { mouseKeyboard, gamepad };
        Control_State current_state;

        #region Rotation

        //reading in from the controller axis
        Vector2 moveHor;
        float moveVer;
        Vector2 gamepadRotate;
        Vector2 mouseRotate;
        Quaternion beforeRotation;
        Quaternion currentRotation;
        public float rotationSpeed;

        // Set temporary storage for rotation of either control scheme.
        // This is how change is detected.
        Vector2 temp_mouseRotate;
        Vector2 temp_gamepadRotate;

        //angle of control stick as a single number
        float angle;
        Vector2 angleVector;

        //center of the control stick
        Vector2 center;

        //offsetting the y value of the center - for mouse rotation
        public float centerOffset;

        //disable rotation controls
        public bool rotateDisable;
        #endregion

        #region Movement

        Rigidbody rb;

        //speed multiplier
        public float horSpeed;
        public float verSpeed;

        //dash variable
        private float dash;
        public float dash_rate;
        public float dash_speed;
        private float dash_next;


        #endregion

    #endregion

    #region Fire and Action Variables
        
        //Firing Weapon
        private float fireWeapon;

    #endregion

    public override void Awake()
    {
        base.Awake();

        hxpTotal = 0;

        #region Body Card Attributes
        bubbleBrain = transform.Find("Brain").gameObject;
        WearBodyCard();
        #endregion

        #region Rotation Variables
        angle = 0.0f;
        angleVector = new Vector2(0,0);
        center = new Vector2(0,1);
        temp_mouseRotate = mouseRotate;
        temp_gamepadRotate = gamepadRotate;

        beforeRotation = new Quaternion(0,0,0,0);
        currentRotation = new Quaternion(0,0,0,0);

        #endregion

        #region Movement Variables
        rb = GetComponent<Rigidbody>();
        #endregion

        #region Input Controls

        // Initialize the controls for the player.
        controls_PlayerInput = new PlayerInputActions();

        // Declare the control scheme enum 
        current_state = Control_State.gamepad;

        // Y axis Up and Down movement
        controls_PlayerInput.PlayerController.MoveYUp.performed += ctx => moveVer = ctx.ReadValue<float>();
        controls_PlayerInput.PlayerController.MoveYUp.canceled += ctx => moveVer = 0f;
        controls_PlayerInput.PlayerController.MoveYDown.performed += ctx => moveVer = -1 * ctx.ReadValue<float>();
        controls_PlayerInput.PlayerController.MoveYDown.canceled += ctx => moveVer = 0f;

        // X and Z axis movement
        controls_PlayerInput.PlayerController.MoveXZ.performed += ctx => moveHor = ctx.ReadValue<Vector2>();
        controls_PlayerInput.PlayerController.MoveXZ.canceled += ctx => moveHor = Vector2.zero;

        // Gamepad Aiming
        controls_PlayerInput.PlayerController.StickAim.performed += ctx => gamepadRotate = ctx.ReadValue<Vector2>();
        controls_PlayerInput.PlayerController.StickAim.canceled += ctx => gamepadRotate = Vector2.zero;

        // Mouse Aiming
        controls_PlayerInput.PlayerController.MouseAim.performed += ctx => mouseRotate = ctx.ReadValue<Vector2>();
        controls_PlayerInput.PlayerController.MouseAim.canceled += ctx => mouseRotate = Vector2.zero;

        // Fire Bubbles
        controls_PlayerInput.PlayerController.Fire.performed += ctx => fireWeapon = ctx.ReadValue<float>();
        controls_PlayerInput.PlayerController.Fire.canceled += ctx => fireWeapon = 0f;

        // Dash Movement
        controls_PlayerInput.PlayerController.Dash.performed += ctx => dash = ctx.ReadValue<float>();
        controls_PlayerInput.PlayerController.Dash.canceled += ctx => dash = 0f;

        #endregion

    }


    void FixedUpdate()
    {
        MoveAround();
        FireWeapon(fireWeapon);

        // Will need to add player states:
        // If hit by an explosive force, need to disable all rotation and movement forces 
        // for a period of time. 
        FaceDirection(rotateDisable);
        

    }


    //Overriding BulletHit from Base_Character class
    //reason: need to restart scene on death instead of destroy
    public override void BulletHit(int damage)
    {
        base.health -= damage;
        Debug.Log("Player Hit, Health: " + health);
  
        if(health <= 0)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    //Changes Player's form and stats according to the new Body Card
    void WearBodyCard()
    {
        //Brain Mesh & Material setup
        bubbleBrain.GetComponent<MeshFilter>().mesh = bodyCard.bubbleBrainMesh;
        bubbleBrain.GetComponent<MeshRenderer>().material = bodyCard.bubbleBrainMaterial;

        //Shell Mesh & Material setup
        GetComponent<MeshFilter>().mesh = bodyCard.bubbleShellMesh;
        GetComponent<MeshRenderer>().material = bodyCard.bubbleShellMaterial;

        //Change Weapon
        //if (bodyCard.weapon.vfx.Count != 0){
            bulletVFX = bodyCard.weapon.vfx[0];
        //}

        //Change Stats
        horSpeed = bodyCard.moveSpeed;
    }


    void HXPCollect(int hxpAmount)
    {
        hxpTotal += hxpAmount;
    }


    // Container to call Fire() from base class.
    // Separated so each character can have different ways of firing.
    void FireWeapon(float fireWeapon){

        //if weapon has not been fired, then exit function
        //reading as a float b/c of Unity's input controller
        if (fireWeapon == 0) return;

        //Fire() located in Base_Character class.
        Fire(   weapon.bulletType,
                bulletVFX,
                weapon.damage, 
                weapon.scaleSize, 
                weapon.fireRate, 
                weapon.speed);
    }


    // Moves character via forces.
    void MoveAround()
    {
        // General Movement
        rb.AddForce( new Vector3(   moveHor.x*horSpeed, 
                                    moveVer*verSpeed, 
                                    moveHor.y*horSpeed), 
                                    ForceMode.Acceleration);
        Dash();
    }

    void Dash()
    {
        // Upward/Downward dashes
        // If dash button was pressed, send Impulse force to quickly move player.
        // Cannot dash if timer has not been reset.
        if (dash != 0)
        {
            // Dash timer not finished -- cannot dash
            if (dash_next > 0){
                return;
            }
            else // able to dash!
            {
                // Dash Up/Down
                if (moveVer != 0)
                    rb.AddForce( new Vector3(0,moveVer*dash_speed,0), ForceMode.Impulse);

                // Dash in direction of movement
                if (moveVer == 0)
                    rb.AddForce( new Vector3(moveHor.x*dash_speed*2,0,moveHor.y*dash_speed*2), ForceMode.Impulse);

                //Reset timer
                dash_next = dash_rate;
            }
        }
        //Finish timer
        if (dash_next > 0)
            dash_next -= Time.deltaTime;
    }


    // Switching between character aiming control schemes based on player rotation input.
    // Isolates rotation code so it doesn't read from multiple sources.
    void FaceDirection(bool disable)
    {
        // If 'disable' boolean is true, then do not apply any rotation forces.
        // Reason: when player is hit by an explosive force, they should roll and flop around
        // for a period of time. 
        if (disable) return;

        // Read rotation only from Gamepad. 
        if ( current_state == Control_State.gamepad)
        {
            FaceGamepad();
            // If any input rotation input read from Mouse + Keyboard, 
            // then switch to that state.
            if (temp_mouseRotate != mouseRotate)
            {
                current_state = Control_State.mouseKeyboard;
                temp_mouseRotate = mouseRotate;
            }
        }

        // Read rotation only from Mouse + Keyboard.
        if ( current_state == Control_State.mouseKeyboard)
        {
            FaceMouse();
            // If any input rotation input read from the Gamepad, 
            // then switch to that state.
            if (temp_gamepadRotate != gamepadRotate)
            {
                current_state = Control_State.gamepad;
                temp_gamepadRotate = gamepadRotate;
            }

        }
    }


    //Character Aiming via GAMEPAD
    void FaceGamepad()
    {
        // If no input for rotation, exit function
        if (gamepadRotate == Vector2.zero)
            return;

        // Else, update angle variable
        angle = Vector2.SignedAngle(center, gamepadRotate);

        // And transform player rotation to new angle  -  OLD
        transform.eulerAngles = new Vector3(0,-angle,0);

        // And transform player rotation to new angle
        //transform.eulerAngles = Vector3.Lerp( transform.eulerAngles, new Vector3(0,-angle,0), 0.5f);

        //currentRotation.SetLookRotation(new Vector3(gamepadRotate.x, 0, gamepadRotate.y), Vector3.up);
        //currentRotation = currentRotation.
        //rb.MoveRotation(Quaternion.Slerp(beforeRotation, currentRotation, rotationSpeed));

        //beforeRotation = currentRotation;
    }


    //Character Aiming via MOUSE
    void FaceMouse()
    {
        // If there is no change in mouse movement, then disable rotation
        // Allows players to roll around on walls
        if (temp_mouseRotate == mouseRotate) 
            return;

        // Else:
        // Remap mouse X and Y positions to match controller input (-1, 1)
        var mX = mouseRotate.x - (Screen.width/2);
        mX = Remap(mX,-Screen.width/2, Screen.width/2, -1, 1);

        var mY = mouseRotate.y - (Screen.height/2);
        mY = Remap(mY,-Screen.height/2, Screen.height/2, -1, 1);

        // Store in new Vec2
        Vector2 newMouseRotate = new Vector2(mX,mY + centerOffset);

        // Update angle variable with a Y-Axis offset to the center of the screen,
        // to compensate for player character not being centered
        angle = Vector2.SignedAngle(new Vector2(center.x, center.y + centerOffset), newMouseRotate);

        // And transform player rotation to new angle
        transform.eulerAngles = new Vector3(0,-angle,0);

        // Update the check for any mouse rotation
        temp_mouseRotate = mouseRotate;
    }


    // Instead of trying to use forces to aim the bubble ship:
    // Let's just disable the Facing functions when hit by explosive forces, returning control after a timer.
    // Easy solution for a too time-consuming problem.

/*
    // Player's Bubble Ship should automatically reallign it's rotational values to
    // try to always be upright.
    void AutoReallign()
    {
        // Check to see if rotation forces are needed //
        float xRot = transform.rotation.eulerAngles.x;
        float zRot = transform.rotation.eulerAngles.z;
        bool applyForce = false;

        if (xRot != 0) applyForce = true;
        if (zRot != 0) applyForce = true;
        if (xRot == 0 & zRot == 0) applyForce = false;    // Using single & to always checking both operands.
        if (applyForce == false) return; // If no rotation forces needed, end here.

        // Apply rotational forces //
        Vector3 AngleGoal = new Vector3(0, transform.eulerAngles.y, 0);
        Quaternion AngleQuat = Quaternion.LookRotation(AngleGoal, Vector3.up);
        Quaternion AngleDifference = Quaternion.FromToRotation(rb.transform.rotation.eulerAngles, AngleGoal);

        float AngleToCorrect = Quaternion.Angle(transform.rotation, AngleQuat);
        Vector3 Perpendicular = Vector3.Cross(transform.up, transform.forward);
        if (Vector3.Dot(rb.transform.forward, Perpendicular) < 0) AngleToCorrect *= -1;
        Quaternion Correction = Quaternion.AngleAxis(AngleToCorrect, transform.up);

        Vector3 MainRotation = RectifyAngleDifferenceXZ((AngleDifference).eulerAngles);
        Vector3 CorrectiveRotation = RectifyAngleDifferenceXZ((Correction).eulerAngles);

        rb.AddTorque((MainRotation - CorrectiveRotation/2) - rb.angularVelocity, ForceMode.VelocityChange);

    }

    private Vector3 RectifyAngleDifferenceXZ(Vector3 angleDiff)
    {
        if (angleDiff.x > 180) angleDiff.x -= 360;
        if (angleDiff.z > 180) angleDiff.z -= 360;

        return angleDiff;
    }
*/

    float Remap(float from, float fromMin, float fromMax, float toMin, float toMax)
    {
        var fromAbs = from - fromMin;
        var fromMaxAbs = fromMax - fromMin;

        var normal = fromAbs / fromMaxAbs;

        var toMaxAbs = toMax - toMin;
        var toAbs = toMaxAbs * normal;

        var to = toAbs + toMin;
        
        return to;
    }

    void OnEnable()
    {
        controls_PlayerInput.PlayerController.Enable();
    }

    void OnDisable()
    {
        controls_PlayerInput.PlayerController.Disable();
    }
}
