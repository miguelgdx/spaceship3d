using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Basic Control for GameObjects that will just pop particles for a moment
 */
public class GenericParticleSysController : MonoBehaviour
{
    ParticleSystem ps;
    // Start is called before the first frame update
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!ps.IsAlive())
            Destroy(gameObject);
    }
}
