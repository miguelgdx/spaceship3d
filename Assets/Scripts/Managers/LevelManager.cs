using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

// Handles everything related to the in-game level management
public class LevelManager : MonoBehaviour
{
    private int CurrentLevel = 1;
    public GameObject LevelPrompt;
    public GameObject[] AsteroidPrefabs;
    public GameObject GameOverPrompt;

    private Image PromptImage;
    private Text PromptText;
    public AudioClip GameOverSound;
    private int Difficulty;

    // Start is called before the first frame update
    void Start()
    {
        Difficulty = PlayerPrefs.GetInt("Difficulty", 0);
        PromptImage = LevelPrompt.GetComponent<Image>();
        PromptText = LevelPrompt.GetComponentInChildren<Text>();

        PromptImage.color = new Color(1, 1, 1, 0);
        PromptText.color = new Color(1, 1, 1, 0);

        // Make them invisible on start.
        PromptImage.enabled = false;
        PromptText.enabled = false;
        Invoke("ShowLevelPrompt", 1f);
        Invoke("MakeLevel", 2f);
        InvokeRepeating("CheckNoAsteroids", 10f, 2f);
        Invoke("PlayGameMusic", 1f);
    }

    private void PlayGameMusic()
    {
        Camera.main.GetComponent<AudioSource>().Play();
    }

    public void ShowLevelPrompt()
    {
        PromptImage.enabled = true;
        PromptText.enabled = true;
        FadeInPrompt(1f);
        Invoke("HideLevelPrompt", 2f);
    }

    public void NextLevelPrompt()
    {
        CurrentLevel += 1;
        PromptText.text = "LEVEL " + CurrentLevel;
        ShowLevelPrompt();
    }

    public void HideLevelPrompt()
    {
        FadeOutPrompt(0.5f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    // If no asteroids, go to next level.
    public void CheckNoAsteroids()
    {
        GameObject[] Asteroids = GameObject.FindGameObjectsWithTag("Asteroid");
        if (Asteroids.Length == 0)
        {
            NextLevelPrompt();
            Invoke("MakeLevel", 2f);
        }
    }

    public void FadeOutPrompt(float seconds)
    {
        PromptImage.DOColor(new Color(1, 1, 1, 0f), seconds);
        PromptText.DOColor(new Color(1, 1, 1, 0f), seconds);
    }

    public void FadeInPrompt(float seconds)
    {
        PromptImage.DOColor(new Color(1, 1, 1, 1f), seconds);
        PromptText.DOColor(new Color(1, 1, 1, 1f), seconds);
    }

    // Sets up the level with the required amount of asteroids.
    public void MakeLevel()
    {
        GameObject go = null;

        // Instantiate asteroids
        for (int i = 0; i < Difficulty + CurrentLevel; i++){
            go = Instantiate(AsteroidPrefabs[0]);
            go.transform.position = GetRandomPosition();
        }

    }

    private Vector3 GetRandomPosition()
    {
        Vector3 pos = new Vector3(Random.Range(-9f, 9f), 0.3f, Random.Range(4f, 2f));
        return pos;
    }

    public void GameOver()
    {
        GameOverPrompt.SetActive(true);
        Invoke("ShowGameOverPrompt", 1.5f);
        Invoke("ReturnToMainMenu", 8f);
        Camera.main.GetComponent<AudioSource>().Stop();
    }

    public void ShowGameOverPrompt()
    {
        GameManager.Instance.PlayLocalSound(GameOverSound, 0.5f, transform.position);
        GameOverPrompt.GetComponent<Image>().DOColor(new Color(1f, 1f, 1f, 1f), 2f);
    }

    private void ReturnToMainMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

}
