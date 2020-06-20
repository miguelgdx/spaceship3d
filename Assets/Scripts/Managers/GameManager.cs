using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Handles global game logic and keeps clones regs.
 */
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField]
    private List<CloneAtlas> CloneAtlas;
    public GameObject SFXPrefab; // Prefab to instantiate global sound effects.

    private void Awake()
    {
        if (Instance != null) { 
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CloneAtlas = new List<CloneAtlas>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int RegisterClone(GameObject origin, GameObject clone)
    {
        CloneAtlas c = new CloneAtlas(origin, clone);
        CloneAtlas.Add(c);
        return (CloneAtlas.Count - 1);
    }
    public CloneAtlas getCloneAtlas(int index)
    {
        return CloneAtlas[index];
    }

    public void PlayLocalSound(AudioClip clip, float volume, Vector3 position)
    {
        GameObject sfx = Instantiate(SFXPrefab);
        sfx.transform.position = position;
        sfx.GetComponent<GlobalSoundFX>().playGlobalSound(clip, volume);
    }
}
