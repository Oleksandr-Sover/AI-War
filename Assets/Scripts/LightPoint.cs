using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightPoint : MonoBehaviour
{
    public Light light_;

    [SerializeField] private float minlightTimeOff = 1f;
    [SerializeField] private float maxlightTimeOff = 1f;
    [SerializeField] private float minLightTimeOn = 3f;
    [SerializeField] private float maxLightTimeOn = 3f;
    private float lightTimerOn;
    private float lightTimeOn;
    private float lightTimerOff;
    private float lightTimeOff;
    private float minMinLightIntens = 0f;
    private float maxMinLightIntens = 0.5f;
    private float minMaxLightIntens = 1f;
    private float maxMaxLightIntens = 5f;
    private float minLightIntens;
    private float maxLightIntens;
    private float randState;

    private bool blick;

    void Awake()
    {
        light_ = GetComponent<Light>();
    }

    void Start()
    {
        randState = Random.value;

        if (randState > 0.8)
        {
            blick = true;
            lightTimeOn = Random.Range(minLightTimeOn, maxLightTimeOn);
            lightTimerOn = lightTimeOn;
            lightTimeOff = Random.Range(minlightTimeOff, maxlightTimeOff);
            lightTimerOff = lightTimeOff;
            minLightIntens = Random.Range(minMinLightIntens, maxMinLightIntens);
            maxLightIntens = Random.Range(minMaxLightIntens, maxMaxLightIntens);
        }
        else
        {
            blick = false;
            light_.intensity = Random.Range(minMaxLightIntens, maxMaxLightIntens);
            light_.spotAngle = Random.Range(80, 160);
        }


    }

    void Update()
    {
        if (blick)
        {
            if (lightTimerOn > 0)
                lightTimerOn -= Time.deltaTime;
            else
            {
                light_.intensity = minLightIntens;

                if (lightTimerOff > 0)
                    lightTimerOff -= Time.deltaTime;
                else
                {
                    lightTimerOn = lightTimeOn;
                    lightTimerOff = lightTimeOff;
                    light_.intensity = maxLightIntens;
                }
            }
        }
    }
}
