using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// 
/// </summary>
/// 
public abstract class Base_Character : MonoBehaviour
{
    /////Health Variables
    [SerializeField]
    private int maxHealth = 0;
    public int health;

    /////Weapon Firing Variables
    public Weapon weapon;
    
    private GameObject bulletEmitter;
    protected GameObject bulletVFX;
    //public float Bullet_Forward_Force;
    float nextFire;



    //Initialize Variables
    public virtual void Awake()
    {
        health = maxHealth;
        bulletEmitter = transform.Find("Bullet_Emitter").gameObject;
        Debug.Log("Weapon Name: " + weapon.name);

        //If there is a weapon set to this character, then apply its VFX
        if(weapon != null)
        {
            if (weapon.vfx.Count != 0){
                bulletVFX = weapon.vfx[0];
            }
        }
    }

    //When hit by any bullet...
    public virtual void BulletHit(int damage)
    {
        //Subtract health...
        health -= damage;
        
        //If health is less than zero...
        if(health <= 0){
            //AND if this object has a parent,

            if(transform.parent != null){
                //Then destroy the parent, destroying everything else associated with this game object. 
                Destroy(transform.parent.gameObject);
            } else {
                //if THIS is the parent object, then just destroy self.
                Destroy(gameObject);
            }
        }
    }

    public Quaternion GetRotation()
    {
        return transform.rotation;
    }


    //Process of Shooting
    public void Fire(BulletType bulletType, GameObject vfx, int damage, float scaleSize, float fireRate, float speed)
    {
        //This sets the firing rate
        if (nextFire > 0)
        {
            //cannot fire
            nextFire -= Time.deltaTime;
            return;
        } 

        //create an instance of a bullet
        GameObject Temporary_Bullet_Handler;
        Temporary_Bullet_Handler = Instantiate( vfx, 
                                        bulletEmitter.transform.position, 
                                        bulletEmitter.transform.rotation) as GameObject;

        //set the gameObject firing this bullet
        Temporary_Bullet_Handler.GetComponent<Bullet_Destroy>().firedFrom = this.gameObject;
        
        //Damage
        Temporary_Bullet_Handler.GetComponent<Bullet_Destroy>().damage = damage;

        //Bullet Scale
        Temporary_Bullet_Handler.transform.localScale = new Vector3(1*scaleSize, 1*scaleSize, 1*scaleSize);

        //Rotating bullet's "forward" vector
        Temporary_Bullet_Handler.transform.localRotation = GetRotation();

        // Use the 'Fire' that is specified via bulletType
        if (bulletType == BulletType.SimpleVector)     FireSimpleVector(Temporary_Bullet_Handler, speed);
        if (bulletType == BulletType.Physics)          FirePhysics(Temporary_Bullet_Handler, speed);
        if (bulletType == BulletType.StationaryMine)   FireStationaryMine();

        //if bullet doesn't hit any other object for 10 seconds, destroy bullet
        Destroy(Temporary_Bullet_Handler, 10.0f);


        //weapon was fired, resetting fire rate
        nextFire = fireRate;   
    }


    // Fires a bullet following a Vector3
    // Use Cases: Ignores outside physics and should allow for more bullets on screen.
    private void FireSimpleVector(GameObject handler, float speed)
    {
        // Move bullet via the vector's Magnitude
        //handler.transform.position += Time.deltaTime * speed;

        // Testing repository commits //

    }

    
    // Fires a bullet with a RigidBody
    // Use Cases: Allows for bounces and interaction with outside physics (whirlpools, etc).
    private void FirePhysics(GameObject handler, float speed)
    {
        //using the bullet's rigidbody, apply force
        Rigidbody Temporary_RigidBody;
        Temporary_RigidBody = handler.GetComponent<Rigidbody>();
        Temporary_RigidBody.AddForce(transform.forward * speed);
    }

    private void FireStationaryMine()
    {
        Debug.Log("Stationary Mine Firing: WIP");
    }
}
