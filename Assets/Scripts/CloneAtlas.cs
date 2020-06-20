using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Defines a type where clones and main object are stored
 */
public class CloneAtlas
{
    public GameObject MainObject { get; set; }
    public List<GameObject> Clones { get; set; }
    public int NUM_CLONES = 3;

    public CloneAtlas(GameObject origin, GameObject clone)
    {
        Clones = new List<GameObject>();
        this.MainObject = origin;
        Clones.Add(clone);
        // Replicate the clone a few more times.
        for (int i = 1; i < NUM_CLONES; i++)
        {
            GameObject cln = GameObject.Instantiate(clone);
            Clones.Add(cln);
        }
    }
}
