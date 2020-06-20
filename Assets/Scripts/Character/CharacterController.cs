using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Handles all the logic for the Character.
 */
public class CharacterController : Warpable
{
    [SerializeField]
    private GameObject Bullet;
    public GameObject[] BulletPool { get; private set; }
    private const int BULLET_POOL_LIMIT = 5;
    Vector3 BulletStartPos;
    
    Rigidbody _rb;
    float maxSpeed = 8f;
    float rotationSpeed = 4f;
    float moveAccel = 2f;
    float brakeSpeed = 8f;
    private bool turning = false;
    public GameObject DamageParticles;
    public GameObject ExplosionParticles;

    public AudioClip lowHitSound;
    public AudioClip mediumHitSound;
    public AudioClip hardHitSound;
    public AudioClip ExplosionSound;

    private AudioSource _audioSource;
    private CharacterStats _charStats;
    public bool CastingShield = false;
    float _lastHitTime = 0f;
    float TIME_BETWEEN_HITS = 1f;
    public GameObject ShieldObj;
    private void Awake()
    {
    }
    // Start is called before the first frame update
    void Start()
    {
        Initialize();
        
        _rb = GetComponent<Rigidbody>();
        _audioSource = GetComponent<AudioSource>();
        _charStats = GetComponent<CharacterStats>();
    }

    protected override void Initialize()
    {
        base.Initialize();
        // Prepare bullet pool
        BulletPool = new GameObject[BULLET_POOL_LIMIT];
        for (int i = 0; i < BULLET_POOL_LIMIT; i++)
        {
            BulletPool[i] = Instantiate(Bullet);
            BulletPool[i].GetComponent<BulletController>().IgnoreCollision(_collider);
            BulletPool[i].SetActive(false);
        }

        // Ignore collisions with shield.
        IgnoreCollisions();

        // Destroy Character components the clones won't need.
        if (_wcScript.IsClone)
        {
            Destroy(GetComponent<CharacterStats>());
        }
    }

    private void IgnoreCollisions()
    {
        Physics.IgnoreCollision(ShieldObj.GetComponent<Collider>(), _collider);
    }

    // Update is called once per frame
    void Update()
    {
        if (!_wcScript.IsClone)
        {
            // In order to check the forward speed, we need to project the velocity onto the transform.forward
            // Then we'll know how much of the speed comes from the forward/backwards thrust.

            Vector3 velocityProjection = Vector3.Project(_rb.velocity, transform.forward);
            float currentThrust = velocityProjection.magnitude;

            // Angle between the forward and speed vectors. If more than 90, then the thrust is negative.
            float angleFS = Vector3.Angle(transform.forward, _rb.velocity);
            //Debug.Log("Current angle FS: " + angleFS);
            if (angleFS > 90)
                currentThrust *= -1;

            turning = false;

            // To know how much the ship is being pushed sideways, we project the velocity to the right.
            Vector3 sideForce = Vector3.Project(_rb.velocity, transform.right);

            if (Input.GetKey(KeyCode.W))
            {
                Vector3 newForce = (maxSpeed - (currentThrust)) * transform.forward * moveAccel;
                _rb.AddForce(newForce - sideForce);
            }

            Debug.DrawRay(transform.position, _rb.velocity, Color.yellow);
            // Draw our deviation difference
            Debug.DrawRay(transform.position, sideForce, Color.cyan);

            if (Input.GetKey(KeyCode.D))
            {
                turning = true;
                _rb.AddTorque(transform.up * rotationSpeed);
            }

            if (Input.GetKey(KeyCode.A))
            {
                turning = true;
                _rb.AddTorque(-transform.up * rotationSpeed);
            }

            if (Input.GetKey(KeyCode.S))
            {
                // Braking
                if (currentThrust > 0f)
                {
                    _rb.AddForce((0 - (currentThrust * moveAccel)) * _rb.velocity);
                } 
            }

            // Fire bullets

            if (Input.GetKeyDown(KeyCode.Space))
            {
                FireBullet();
            }

            // Use shield
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                Shield(true);
            }
            if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                Shield(false);
            }



            // If we stopped turning, deaccelerate angular velocity.
            if (!turning)
            {
                _rb.angularVelocity = _rb.angularVelocity - _rb.angularVelocity / rotationSpeed;
            }

            UpdateClones();

            // Stop casting shield if we're short of MP.
            if (_charStats.CurrentMP < 0.2f)
            {
                CastingShield = false;
            }

        }
        else // If it's a clone..
        {
            
        }

        // Whatever happens here, will be applied on clones too.

        if (CastingShield)
        {
            ShieldObj.SetActive(true);
        }
        else
            ShieldObj.SetActive(false);



    }

    private void Shield(bool activate)
    {
        if (activate && _charStats.CurrentMP > 0.2f)
        {
            CastingShield = true;
        }
        else
        {
            CastingShield = false;
        }

        if (!activate)
            CastingShield = false;
    }

    protected override void UpdateClones()
    {
        base.UpdateClones();

        if (!_wcScript.IsClone)
        {
            // Tell clones if we are using shield
            List<GameObject> clones = _wcScript.GetClones();
            foreach (GameObject c in clones)
            {
                c.GetComponent<CharacterController>().CastingShield = CastingShield;
            }
        }
        
    }
    private void FireBullet()
    {
        GameObject bullet = getBulletFromPool();
        if (bullet != null)
        {
            //bullet.transform.position = BulletStartPos;
            bullet.transform.position = transform.Find("BulletStart").position;
            // Give the bullet the ship velocity.
            bullet.GetComponent<Rigidbody>().velocity = _rb.velocity;
            bullet.SetActive(true);
            bullet.GetComponent<BulletController>().Fire();
        }
    }

    // If a bullet is available in the pool, retrieve it
    private GameObject getBulletFromPool()
    {
        foreach (GameObject go in BulletPool)
            if (go.activeInHierarchy == false)
                return go;

        return null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collided with: " + collision.gameObject);
        // If there's a collision with an asteroid
        if (collision.collider.gameObject.tag == "Asteroid" && !CastingShield)
        {
            // Always send the signal to the main object.
            _wcScript.GetMainObject().GetComponent<CharacterController>().OnAsteroidCollision(collision, gameObject);
        }
    }

    public void OnAsteroidCollision(Collision collision, GameObject originObject)
    {
        GameObject asteroid = collision.gameObject;

        // If collision origin was a CLONE, apply artificial force.
        Vector3 replicationForce = Vector3.zero;
        if (originObject.GetComponent<WarpClone>().IsClone)
        {
            replicationForce = ApplyImpactVelocityFromClone(collision.gameObject, originObject);
            Debug.Log("Collision from clone, new velocity: " + _rb.velocity.magnitude);
        }
        else
        {
            ApplyImpactVelocity(collision.gameObject);
        }

        // If collision speed is fast enough, then it's a good hit.
        if ((_rb.velocity.magnitude + asteroid.GetComponent<Rigidbody>().velocity.magnitude) > 2 || replicationForce.magnitude + asteroid.GetComponent<Rigidbody>().velocity.magnitude > 2)
        {
            GameObject go = Instantiate(DamageParticles);
            go.transform.position = collision.contacts[0].point;

            // Depending on the magnitude of the hit, play one sound or another.
            switch (asteroid.GetComponent<AsteroidController>().ShatterLevel)
            {
                case 0:
                    // Huge hit.
                    _audioSource.PlayOneShot(hardHitSound, 0.5f);
                    if (!IsInvincible())
                        _charStats.Damage(0.3f);
                    break;
                case 1:
                    _audioSource.PlayOneShot(mediumHitSound, 0.5f);
                    if (!IsInvincible())
                        _charStats.Damage(0.2f);
                    break;
                case 2:
                    _audioSource.PlayOneShot(lowHitSound, 0.5f);
                    if (!IsInvincible())
                        _charStats.Damage(0.1f);
                    break;
            }
        }

        // Make invencivility if proceeds.
        if (!IsInvincible())
            _lastHitTime = Time.time;

    }

    // If player dies, all of the clones are destroyed too.
    public void PlayerDies()
    {
        GameManager.Instance.PlayLocalSound(ExplosionSound, 0.8f, transform.position);
        GameObject go = Instantiate(ExplosionParticles);
        go.transform.position = transform.position;
        GameManager.Instance.GetComponent<LevelManager>().GameOver();
        DestroyAll();
    }

    // OK: Checked.
    // Applies direct impact velocity on main object to make it repel each other.
    private void ApplyImpactVelocity(GameObject obj)
    {
        // mass * dist / time.
        // Get impact direction
        Vector3 direction = (transform.position - obj.transform.position).normalized;
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        Vector3 force = direction * rb.mass * (rb.velocity.magnitude / rb.mass);
        _rb.AddForce(force, ForceMode.Impulse);
        
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
        Debug.Log("Asteroid collision force: " + force);
        _rb.AddForce(force, ForceMode.Impulse);
        // Make the other object, bounce back.
        obj.GetComponent<Rigidbody>().AddForce(-force, ForceMode.Impulse);
        return force;
    }

    private bool IsInvincible()
    {
        if (Time.time - _lastHitTime < TIME_BETWEEN_HITS)
        {
            return true;
        }
        return false;
    }

   
}
