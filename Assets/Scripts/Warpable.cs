using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base Class for objects that can travel around the map
public class Warpable : MonoBehaviour
{
    protected WarpClone _wcScript;
    protected Collider _collider;
    protected MeshRenderer _renderer;

    // Initializes the clones for the Warp trick.
    protected virtual void Initialize()
    {
        _collider = GetComponent<Collider>();
        _renderer = GetComponentInChildren<MeshRenderer>();
        _wcScript = GetComponent<WarpClone>();
        _wcScript.OnCloned = OnClonesLoaded;
        _wcScript.Clone();
        
    }

    // Requested to update the clones position.
    protected virtual void UpdateClones()
    {
        if (!_wcScript.IsClone)
            Repositionate();
    }

    // Calculates the clones position based on the location of the main object.
    private void Repositionate()
    {
        Camera cam = Camera.main;
        // DistanceZ here is the depth of the camera towards the plane (0), but the space ship could be slightly elevated.
        // We need the position.y (height) so we can correctly represent the world offsets for this SLICE of plane at this height.
        // Remember that perspective views are not square-ish.
        float distanceZ = -Camera.main.transform.position.y + transform.position.y;

        // Calculate the camera borders.
        float leftConstraint = -cam.ScreenToWorldPoint(new Vector3(0.0f, 0.0f, distanceZ)).x;
        float rightConstraint = -cam.ScreenToWorldPoint(new Vector3(Screen.width, 0.0f, distanceZ)).x;
        float bottomConstraint = -cam.ScreenToWorldPoint(new Vector3(0.0f, 0.0f, distanceZ)).z;
        float topConstraint = -cam.ScreenToWorldPoint(new Vector3(0.0f, Screen.height, distanceZ)).z;

        // If not in bounds, swap with a clone that is.
        Vector2 pos = new Vector2(transform.position.x, transform.position.z);
        //Debug.Log("Within?: " + withinBounds(leftConstraint, rightConstraint, topConstraint, bottomConstraint, pos));

        if (!withinBounds(leftConstraint, rightConstraint, topConstraint, bottomConstraint, pos))
        {
            for (int i = 0; i < _wcScript.GetClones().Count; i++)
            {
                GameObject cln = _wcScript.GetClones()[i];
                pos = new Vector2(cln.transform.position.x, cln.transform.position.z);
                if (withinBounds(leftConstraint, rightConstraint, topConstraint, bottomConstraint, pos))
                {
                    //Debug.Log("The clone: " + cln + " is inside!");
                    SwapRole(cln);
                }
            }
        }

        /*
        Debug.Log("Left constraint: " + leftConstraint);
        Debug.Log("Right constraint: " + rightConstraint);
        Debug.Log("Top constraint: " + topConstraint);
        Debug.Log("Bottom constraint: " + bottomConstraint);*/

        // Get camera height and width.
        float camHeight = Mathf.Abs(topConstraint) + Mathf.Abs(bottomConstraint);
        float camWidth = Mathf.Abs(leftConstraint) + Mathf.Abs(rightConstraint);

        // Use the renderer bounds so we know the Z-size of the ship at all moments (changes depending on the rotation).
        Bounds bounds = _renderer.bounds;
        float vBoundSize = bounds.extents.z;
        float hBoundSize = bounds.extents.x;

        // How much real vertical space it takes from the center to edge.
        float vOffset = vBoundSize;
        float hOffset = hBoundSize;

        // All clones will initially copy the X-Z coords.
        List<GameObject> myClones = _wcScript.GetClones();
        float xPosition = transform.position.x;
        float zPosition = transform.position.z;

        bool vLimitFlag = false;
        bool hLimitFlag = false;

        // Checking horizontal limits. Changes on X axis.
        if (transform.position.x - hOffset < leftConstraint)
        {
            hLimitFlag = true;
            xPosition = transform.position.x + camWidth;
        }
        if (transform.position.x + hOffset > rightConstraint)
        {
            hLimitFlag = true;
            xPosition = transform.position.x - camWidth;
        }
        // Checking vertical limits. Changes on Z axis.
        if (transform.position.z - vOffset < bottomConstraint)
        {
            vLimitFlag = true;
            zPosition = transform.position.z + camHeight;
        }
        if (transform.position.z + vOffset >= topConstraint)
        {
            vLimitFlag = true;
            zPosition = transform.position.z - camHeight;
        }

        // 1 Clone is needed to show on screen the effect.
        if ((hLimitFlag && !vLimitFlag) || (!hLimitFlag && vLimitFlag))
        {
            // Hide the rest of clones.
            for (int i = 1; i < myClones.Count; i++)
            {
                hideClone(myClones[i]);
            }
            myClones[0].transform.position = new Vector3(xPosition, transform.position.y, zPosition);
            myClones[0].transform.rotation = this.transform.rotation;
            myClones[0].SetActive(true);
        }
        // If more than 1 clone is needed (corners).
        else if (hLimitFlag && vLimitFlag)
        {
            for (int i = 0; i < myClones.Count; i++)
            {
                myClones[i].SetActive(true);
            }
            // One clone mirrors horizontally.
            myClones[0].transform.position = new Vector3(xPosition, transform.position.y, transform.position.z);
            // Vertically
            myClones[1].transform.position = new Vector3(transform.position.x, transform.position.y, zPosition);
            // Both.
            myClones[2].transform.position = new Vector3(xPosition, transform.position.y, zPosition);
            for (int i = 0; i < myClones.Count; i++)
                myClones[i].transform.rotation = transform.rotation;
        }
        else
        {
            for (int i = 0; i < myClones.Count; i++)
            {
                hideClone(myClones[i]);
            }
            // They should be out.

        }
    }

    private void hideClone(GameObject cln)
    {
        cln.transform.position = new Vector3(-9999, -9999, -9999);
        cln.SetActive(false);
    }

    protected void DeactivateClones()
    {
        List<GameObject> Clones = _wcScript.GetClones();
        foreach (GameObject obj in Clones)
        {
            obj.SetActive(false);
        }
    }

    private bool withinBounds(float left, float right, float top, float bottom, Vector2 pos)
    {
        Rect worldBounds = new Rect(left, bottom, right - left, top - bottom);
        return worldBounds.Contains(pos);
    }

    private Vector3 get2DPosition()
    {
        Vector3 pos = this.transform.position;
        pos.y = 0;
        return pos;
    }

    // Swaps position with the clone (it can't even be noticed!)
    private void SwapRole(GameObject clone)
    {
        // If object has a trail, swap it too.
        if (gameObject.GetComponentInChildren<TrailRenderer>() != null)
        {
            GameObject MainTrail = gameObject.GetComponentInChildren<TrailRenderer>().gameObject;
            Vector3 mainTrailPos = MainTrail.transform.position;
            GameObject CloneTrail = clone.GetComponentInChildren<TrailRenderer>().gameObject;
            Vector3 cloneTrailPos = CloneTrail.transform.position;

            MainTrail.transform.parent = clone.transform;
            CloneTrail.transform.parent = gameObject.transform;
            MainTrail.transform.position = cloneTrailPos;
            CloneTrail.transform.position = mainTrailPos;

        }

        Vector3 dest = clone.transform.position;
        clone.transform.position = transform.position;

        transform.position = dest;
    }

    private void OnClonesLoaded()
    {
        Debug.Log("On Clones Loaded trigger");
        IgnoreClonesCollision();
    }

    // Prevent Collisions with your own clones.
    private void IgnoreClonesCollision()
    {
        List<GameObject> Clones = _wcScript.GetClones();

        for (int i = 0; i < _wcScript.GetClones().Count; i++)
        {
            Collider c = Clones[i].GetComponent<Collider>();
            Physics.IgnoreCollision(_collider, c);
        }
        
    }

    // Destroys both the main and the clones.
    protected void DestroyAll()
    {
        // Destroy clones.
        List<GameObject> Clones = _wcScript.GetClones();
        foreach (GameObject obj in Clones)
        {
            Destroy(obj);
        }
        Clones.Clear();
        // Destroy self
        Destroy(gameObject);
    }

    // Rescaling clones: Intended for asteroids.
    protected void RescaleClones()
    {
        foreach (GameObject obj in _wcScript.GetClones())
        {
            obj.transform.localScale = gameObject.transform.localScale;
        }
    }
}
