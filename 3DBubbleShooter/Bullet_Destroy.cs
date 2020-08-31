using UnityEngine;

public class Bullet_Destroy : MonoBehaviour
{
 
    //Damage is set via the weapon calling the bullet
    public int damage;

    //Target is set via the character calling the Fire() method
    public GameObject firedFrom;


    //If this bullet instance intersects with any collider...
    void OnTriggerEnter(Collider other)
    {
        //And if this collider is NOT another trigger...
        if ((other.isTrigger == false) && (firedFrom != other.gameObject))
        {
            //Then tell that object it was hit,
            //and destroy self.
            other.gameObject.SendMessage("BulletHit", damage, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
    }
}
