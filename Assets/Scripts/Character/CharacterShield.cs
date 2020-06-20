using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controls all the logic for the Character Shield.
public class CharacterShield : MonoBehaviour
{
    Collider _collider;
    GameObject _parentCharacter;
    public GameObject CollisionParticles;
    public AudioClip[] CollisionSounds;
    public AudioClip ShieldAppearSound;
    float _lastHitTime = 0f;
    float TIME_BETWEEN_HITS = 1f;

    // Start is called before the first frame update
    void Start()
    {
        _parentCharacter = transform.parent.gameObject;
        _collider = GetComponent<Collider>();
        IgnoreCollisions();
        InvokeRepeating("ConsumePassiveMP", 0, 0.1f);
    }

    // Play a sound each time it's active.
    void OnEnable()
    {
        GameManager.Instance.PlayLocalSound(ShieldAppearSound, 0.5f, transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ConsumePassiveMP()
    {
        ConsumeMP(0.01f);
    }

    void ConsumeMP(float amount)
    {
        if (!_parentCharacter.GetComponent<WarpClone>().IsClone)
        {
            if (!gameObject.activeSelf)
                return;
            Debug.Log("Consuming MP");
            _parentCharacter.GetComponent<CharacterStats>().ConsumeMP(amount);
        }
            
    }

    void IgnoreCollisions()
    {
        // Ignore collisions with all bullets.
        GameObject[] bullets = _parentCharacter.GetComponent<CharacterController>().BulletPool;

        foreach (GameObject b in bullets)
        {
            Physics.IgnoreCollision(_collider, b.GetComponent<Collider>());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.tag == "Asteroid")
        {
            // Make parent object (main) be pushed.
            // Main obj
            GameObject mainChar = _parentCharacter.GetComponent<WarpClone>().GetMainObject();
            // Now get the shield script.
            CharacterShield cs = mainChar.GetComponentInChildren<CharacterShield>();
            // Apply asteroid collision on main ship object.
            cs.OnAsteroidCollision(collision, gameObject);
        }
    }

    public void OnAsteroidCollision(Collision collision, GameObject originObject)
    {
        GameObject asteroid = collision.gameObject;
        Rigidbody _rb = _parentCharacter.GetComponent<Rigidbody>();
        // If collision origin was a CLONE, apply artificial force.
        Vector3 replicationForce = Vector3.zero;
        if (originObject.transform.parent.GetComponent<WarpClone>().IsClone)
        {
            replicationForce = ApplyImpactVelocityFromClone(collision.gameObject, originObject);
        }
        else
        {
            ApplyImpactVelocity(collision.gameObject);
        }

        // If collision speed is fast enough, then it's a good hit.

            GameObject go = Instantiate(CollisionParticles);
            go.transform.position = collision.contacts[0].point;
        go.transform.LookAt(asteroid.transform);

        

            // Depending on the magnitude of the hit, play one sound or another.
            switch (asteroid.GetComponent<AsteroidController>().ShatterLevel)
            {
                
                case 0:
                // Huge hit.
                GameManager.Instance.PlayLocalSound(CollisionSounds[0], 0.5f, transform.position);
                if (!IsInvincible())
                    ConsumeMP(0.4f);
                    break;
                case 1:
                GameManager.Instance.PlayLocalSound(CollisionSounds[1], 0.5f, transform.position);
                if (!IsInvincible())
                    ConsumeMP(0.3f);
                break;
                case 2:
                GameManager.Instance.PlayLocalSound(CollisionSounds[2], 0.5f, transform.position);
                if (!IsInvincible())
                    ConsumeMP(0.2f);
                break;
                    
            }

        // Remove comment to make the shield have a short span where it doesn't consume MP on collision.
        /*
        if (!IsInvincible())
            _lastHitTime = Time.time;
            */

    }

    private bool IsInvincible()
    {
        if (Time.time - _lastHitTime < TIME_BETWEEN_HITS)
        {
            return true;
        }
        return false;
    }

    // OK: Checked.
    // Applies direct impact velocity on main object to make it repel each other.
    private void ApplyImpactVelocity(GameObject obj)
    {
        // Get impact direction
        Vector3 direction = (transform.position - obj.transform.position).normalized;
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        Vector3 force = direction * rb.mass * (rb.velocity.magnitude / rb.mass);
        _parentCharacter.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);

    }

    // OK: Checked. TODO: Seems there are 2 collisions when right in the intersection.
    // Applies impact on main object when it comes from a clone.
    private Vector3 ApplyImpactVelocityFromClone(GameObject obj, GameObject clone)
    {
        // Get impact direction
        Vector3 direction = (clone.transform.position - obj.transform.position).normalized;
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        Rigidbody cloneRb = clone.GetComponent<Rigidbody>();

        //Vector3 velocitySum = rb.velocity + 

        Vector3 force = direction * rb.mass * (rb.velocity.magnitude / rb.mass);
        _parentCharacter.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
        // Make the other object, bounce back.
        obj.GetComponent<Rigidbody>().AddForce(-force, ForceMode.Impulse);
        return force;
    }
}
