using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/**
 * Loads game scene
 */
public class LoadLevel : MonoBehaviour
{
    AsyncOperation LoadLevelOperation;
    float LoadProgress = 0;
    public Slider ProgressBar;
    public FadeUIPlane FadingPlane;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // ASYNC WAY --------------
        /*
        if (LoadLevelOperation != null)
        {
            LoadProgress = LoadLevelOperation.progress;
            ProgressBar.value = LoadProgress;
        }*/

    }

    public void PlayGame()
    {
        // Coroutine---------
        StartCoroutine(AsyncLevelLoad());


        // ASYNC WAY --------------
        //LoadLevelOperation = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

    }

    private IEnumerator AsyncLevelLoad()
    {
        yield return new WaitForSeconds(1);
        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);

        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            LoadProgress = operation.progress;
            float _progress = Mathf.Clamp01(operation.progress / 0.9f);

            Debug.Log("Loading... " + (int)(_progress * 100f) + "%");
            ProgressBar.value = LoadProgress;

            yield return null;
        }
        ProgressBar.value = 1f;
        FadingPlane.FadeIn(1);
        yield return new WaitForSeconds(2);
        operation.allowSceneActivation = true;
    }
}
