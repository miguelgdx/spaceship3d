using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * Handles all the logic related to character stats and the UI
 */
public class CharacterStats : MonoBehaviour
{
    public float MAX_HP = 1f;
    public float MAX_MP = 1f;
    public float CurrentHP = 1f;
    public float CurrentMP = 1f;
    public Slider HPSlider;
    public Slider MPSlider;
    private float LastMPUsage = 0;
    private float TIME_BEFORE_MP_RECOV = 2f;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("RecoverMP", 1, 2f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Damage(float dmg)
    {
        CurrentHP -= dmg;
        if (CurrentHP < 0.2)
        {
            CurrentHP = 0;
            GetComponent<CharacterController>().PlayerDies();
        }
            

        OnHPChange();
    }

    public void ConsumeMP(float amount)
    {
        CurrentMP -= amount;
        if (CurrentMP < 0)
        {
            CurrentMP = 0;
        }
        LastMPUsage = Time.time;
        OnMPChange();
    }

    public void RecoverMP()
    {
        if (Time.time - LastMPUsage > TIME_BEFORE_MP_RECOV)
        {
            if (CurrentMP < 1f)
            {
                CurrentMP += 0.1f;
            } 
            else
            {
                CurrentMP = 1f;
            }
            OnMPChange();
        }
        
    }

    private void OnHPChange()
    {
        HPSlider.value = CurrentHP;
    }
    private void OnMPChange()
    {
        MPSlider.value = CurrentMP;
    }
}
