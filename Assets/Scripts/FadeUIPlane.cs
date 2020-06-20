using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/**
 * Use DoTween to fade UI planes by changing the color
 */
public class FadeUIPlane : MonoBehaviour
{
    float WAIT_START_TIME = 0.5f;
    float StartTime = 0;
    // Start is called before the first frame update
    void Start()
    {
        //StartTime = Time.time;
        FadeOut(1);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void FadeOut(float seconds)
    {
        StartCoroutine(AsyncFade(0.0f, seconds));
    }

    public void FadeIn(float seconds)
    {
        StartCoroutine(AsyncFade(1.0f, seconds));
    }

    public IEnumerator AsyncFade(float targetAlpha, float seconds)
    {
        GetComponent<Image>().DOColor(new Color(0, 0, 0, targetAlpha), seconds);
        while(GetComponent<Image>().color.a != targetAlpha)
        {
            yield return null;
        }
        yield return null;
    }
}
