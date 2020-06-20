using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Handles all the logic for the character bullets.
 */
public class BulletController : Warpable
{
    Rigidbody _rb;
    Collider _col;
    const float TIME_ALIVE = 0.8f;
    private float _startTime = 0f;
    private Collider PlayerCollider;
    public GameObject DestructionParticles;
    public AudioClip BulletSoundClip;
    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateClones();
        if (!_wcScript.IsClone && Time.time - _startTime >= TIME_ALIVE)
        {
            _rb.velocity = Vector3.zero;
            gameObject.SetActive(false);
            // Deactivate clones too.
            DeactivateClones();
        }
        else
        {
            //Debug.Log("Time alive: " + (Time.time - _startTime));
        }
            
    }

    public void Fire()
    {
        GameManager.Instance.PlayLocalSound(BulletSoundClip, 0.5f, transform.position);
        GameObject player = PlayerCollider.gameObject;
        _startTime = Time.time;
        _rb.AddForce(player.transform.forward * 10, ForceMode.Impulse);
    }

    public void IgnoreCollision(Collider c)
    {
        PlayerCollider = c;
        // Ignore collision with player
        Physics.IgnoreCollision(PlayerCollider, _col);
    }

    // If a bullet receives a hit signal, it will set inactive with their clones.
    public void Hit()
    {
        // Instantiate destruction particles on the bullet that received the call
        GameObject go = Instantiate(DestructionParticles);
        go.transform.position = transform.position;

        // Make the original object reset and go back to the pool.
        GameObject main = _wcScript.GetMainObject();
        Rigidbody mainRb = main.GetComponent<Rigidbody>();
        mainRb.velocity = Vector3.zero;
        main.SetActive(false);

        // Deactivate clones.
        DeactivateClones();

    }
}
