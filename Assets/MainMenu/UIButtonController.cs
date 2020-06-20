using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * Handles Generic UI Button.
 * Needed for buttons that require to keep the "selected" state
 */
public class UIButtonController : MonoBehaviour
{
    public Sprite SelectedImage;
    public Sprite NonSelectedImage;
    private Image ButtonImage;
    private Button UIButton;
    // Start is called before the first frame update
    void Awake()
    {
        ButtonImage = GetComponent<Image>();
        UIButton = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Select()
    {
        UIButton.interactable = false;
        ButtonImage.sprite = SelectedImage;

    }
    public void Deselect()
    {
        ButtonImage.sprite = NonSelectedImage;
        UIButton.interactable = true;
    }
}
