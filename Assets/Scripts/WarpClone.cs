using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * This is a behaviour script needed in every object that inherits from Warpable.
 * This will trigger the cloning functions.
 * 
 * It's always the MainObject that tells the clone what to do.
 * Only thing the clone does, is telling the MainObject what happened to it.
 * If they swap, roles are changed.
 */
public class WarpClone : MonoBehaviour
{
    [SerializeField]
    public bool IsClone = false; // Is this object the clone?
    [SerializeField]
    private int _cloneIndex = -1;

    public delegate void OnClonedDelegate();
    public OnClonedDelegate OnCloned;

    private void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Clone()
    {
        // Only clone if it's not a clone itself.
        if (!IsClone)
        {
            GameObject newClone = GameObject.Instantiate(gameObject);
            newClone.SetActive(false);
            newClone.GetComponent<WarpClone>().IsClone = true;
            _cloneIndex = GameManager.Instance.RegisterClone(gameObject, newClone);
            // Apply index to all the cloned objects.
            foreach (GameObject obj in GetClones())
            {
                WarpClone wc = obj.GetComponent<WarpClone>();
                wc.SetCloneIndex(_cloneIndex);
            }

            OnCloned();
        }
    }

    public void SetCloneIndex(int index)
    {
        _cloneIndex = index;
    }

    public List<GameObject> GetClones()
    {
        return GameManager.Instance.getCloneAtlas(_cloneIndex).Clones;
    }

    public GameObject GetMainObject()
    {
        return GameManager.Instance.getCloneAtlas(_cloneIndex).MainObject;
    }
}
