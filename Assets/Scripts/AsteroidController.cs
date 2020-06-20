using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Handles all the logic for asteroids in game
 */
public class AsteroidController : Warpable
{
    Rigidbody _rb;
    private const float BULLET_IMPACT_FORCE = 20f;
    [SerializeField]
    public int ShatterLevel = 0;
    public AudioClip[] ShatterSounds;

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
        _rb = GetComponent<Rigidbody>();
        // Make it move to random direction.
        Vector3 randDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        _rb.AddForce(randDir.normalized * 12, ForceMode.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateClones();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Always send the signal to the main object.
        _wcScript.GetMainObject().GetComponent<AsteroidController>().OnHit(collision, gameObject);
    }

    public void OnHit(Collision collision, GameObject originObject)
    {
        if (collision.collider.gameObject.tag == "Bullet")
        {
            GameObject Bullet = collision.collider.gameObject;
            // If a clone asteroid was hit, apply force depending on the clone.
            if (originObject.GetComponent<WarpClone>().IsClone)
            {
                ApplyImpactVelocityFromClone(collision.gameObject, originObject);
            }
            else
            {
                // If main object was hit
                ApplyImpactVelocity(collision.collider.gameObject);
            }
            
            // Shatter the object. Children and clones will inherit properties.
            Shatter();
            Bullet.GetComponent<BulletController>().Hit();
        }   
    }

    // ORIGINAL ASTEROID HIT
    private void ApplyImpactVelocity(GameObject obj)
    {
        // Get impact direction
        Vector3 direction = (transform.position - obj.transform.position).normalized;
        Vector3 force = direction * BULLET_IMPACT_FORCE;
        _rb.AddForce(force, ForceMode.Impulse);
    }

    // CLONE HIT
    private void ApplyImpactVelocityFromClone(GameObject obj, GameObject clone)
    {
        // Get impact direction
        Vector3 direction = (clone.transform.position - obj.transform.position).normalized;
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        Vector3 force = direction * rb.mass * rb.velocity.magnitude;
        Debug.Log("Asteroid collision force: " + force);
        _rb.AddForce(force, ForceMode.Impulse);
    }

    // Asteroid shattering or destruction.
    private void Shatter()
    {
        switch (ShatterLevel)
        {
            case 0:
                GameManager.Instance.PlayLocalSound(ShatterSounds[0], 0.5f, transform.position);
                break;
            case 1:
                GameManager.Instance.PlayLocalSound(ShatterSounds[1], 0.5f, transform.position);
                break;
            case 2:
                GameManager.Instance.PlayLocalSound(ShatterSounds[2], 0.5f, transform.position);
                break;
        }
        // Instantiate another asteroid.
        ShatterLevel += 1;
        if (ShatterLevel > 2)
        {
            DestroyAll();
            return;
        }
        transform.localScale = transform.localScale / 1.5f;
        // Clones must be scaled too.
        RescaleClones();
        GameObject half = Instantiate(gameObject);
        Rigidbody hrb = half.GetComponent<Rigidbody>();
        hrb.velocity = _rb.velocity;
        hrb.mass = hrb.mass - 1;

    }

    private void NoCollisionTimer()
    {

    }
}
