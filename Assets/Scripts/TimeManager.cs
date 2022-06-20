using System;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private Text hourText = null;
    [SerializeField] private Text dateText = null;
    [SerializeField] private bool toggleTime = true;
    [SerializeField] private bool toggleConstellation = false;

    [Header("Time Settings")]
    [SerializeField] private Vector3 startDate = Vector3.zero;
    [SerializeField] private Vector3 startTime = Vector3.zero;
    [SerializeField] private float timeSpeed = 1;

    [Header("Light Settings")]
    [SerializeField] private LightingPreset preset = null;
    [SerializeField] private Color dayAmbientLight;
    [SerializeField] private Color nightAmbientLight;
    [SerializeField] private AnimationCurve lightChangeCurve;
    [SerializeField] private Material skyboxMat = null;
    [SerializeField] private AnimationCurve starShowCurve = null;
    [SerializeField] private float maxConstellationIntensity = 0.2f;

    [Header("Sun Settings")]
    [SerializeField] private Light sunLight = null;
    [SerializeField] private float sunMaxIntensity;
    [SerializeField] private float sunriseHour;
    [SerializeField] private float sunsetHour;

    [Header("Moon Settings")]
    [SerializeField] private Light moonLight = null;
    [SerializeField] private Transform moonPlane = null;
    [SerializeField] private float moonMaxIntensity;
    [SerializeField] private int lunaisonDuration = 29;

    private TimeSpan sunriseTime;
    private TimeSpan sunsetTime;
    private DateTime startDateTime;

    private DateTime currentTime;
    private DateTime moonSubstractTime;

    private double elapsedMoonDays;

    private bool isContellationShow = false;

    private void Start()
    {
        startDateTime = new DateTime((int)startDate.x, (int)startDate.y, (int)startDate.z, (int)startTime.x, (int)startTime.y, (int)startTime.z);
        moonSubstractTime = startDateTime;
        currentTime = startDateTime;
        sunriseTime = TimeSpan.FromHours(sunriseHour);
        sunsetTime = TimeSpan.FromHours(sunsetHour);


        moonPlane.localPosition = new Vector3(moonPlane.localPosition.x, moonPlane.localPosition.y, -1000f);
        moonPlane.localScale = new Vector3(15, 1, 15);
    }

    private void Update()
    {
        if (toggleTime)
        {
            currentTime = currentTime.AddSeconds(Time.deltaTime * timeSpeed);

            if (hourText != null)
                hourText.text = currentTime.ToString("HH:mm");

            if (dateText != null)
                dateText.text = currentTime.ToString("yyyy-MM-dd");

            if (elapsedMoonDays > lunaisonDuration)
            {
                moonSubstractTime = currentTime;
                elapsedMoonDays = 0;
            }

            elapsedMoonDays = currentTime.Subtract(moonSubstractTime).TotalDays;
        }

        if (toggleConstellation)
        {
            toggleConstellation = false;
            isContellationShow = !isContellationShow;
            int targetValue = isContellationShow ? 1 : 0;

            skyboxMat.SetFloat("ConstellationIntensity", maxConstellationIntensity * targetValue);
        }

        float sunAngleRatio = Vector3.Dot(Vector3.down, sunLight.transform.forward);
        float dayRatio = Mathf.InverseLerp(-1, 1, sunAngleRatio);
        float sunsetSunriseRatio = Mathf.InverseLerp(0.75f, 1, Mathf.Abs(Vector3.Dot(Vector3.forward, sunLight.transform.forward)));
        skyboxMat.SetFloat("DayRatio", dayRatio);
        skyboxMat.SetFloat("SunsetRatio", sunsetSunriseRatio);
        skyboxMat.SetVector("SunDirection", sunLight.transform.forward);
        skyboxMat.SetFloat("StarShowCurve", starShowCurve.Evaluate(dayRatio));

        RotateSun();
        RotateMoon();

        UpdateLighting(currentTime.Hour + (currentTime.Minute / 60) / 24);
    }

    #region Sun
    private void RotateSun()
    {
        float sunLightRotation;

        if (currentTime.TimeOfDay > sunriseTime && currentTime.TimeOfDay < sunsetTime)
        {
            TimeSpan sunriseToSunsetDuration = GetTimeDifferenceInHours(sunriseTime, sunsetTime);
            TimeSpan timeSinceSunrise = GetTimeDifferenceInHours(sunriseTime, currentTime.TimeOfDay);

            double percentage = timeSinceSunrise.TotalMinutes / sunriseToSunsetDuration.TotalMinutes;

            sunLightRotation = Mathf.Lerp(0, 180, (float)percentage);
        }
        else
        {
            TimeSpan sunsetToSunriseDuration = GetTimeDifferenceInHours(sunsetTime, sunriseTime);
            TimeSpan timeSinceSunset = GetTimeDifferenceInHours(sunsetTime, currentTime.TimeOfDay);

            double percentage = timeSinceSunset.TotalMinutes / sunsetToSunriseDuration.TotalMinutes;

            sunLightRotation = Mathf.Lerp(180, 360, (float)percentage);
        }

        sunLight.transform.rotation = Quaternion.AngleAxis(sunLightRotation, Vector3.right);
    }
    #endregion //Sun

    #region Moon
    private void RotateMoon()
    {
        //Ratio cycle lunaire
        //elapsedMoonDays / lunaisonDuration;

        //moonLight.transform.rotation = Quaternion.AngleAxis(sunLightRotation, Vector3.right);
    }
    #endregion //Moon

    private TimeSpan GetTimeDifferenceInHours(TimeSpan fromTime, TimeSpan toTime)
    {
        TimeSpan difference = toTime - fromTime;

        if (difference.TotalSeconds < 0)
            difference += TimeSpan.FromHours(24);

        return difference;
    }

    private void UpdateLighting(float timeRatio)
    {
        /*
        sunLight.transform.localRotation = Quaternion.Euler(new Vector3((timeRatio * 360) - 90, 170, 0));
        moonLight.transform.localRotation = Quaternion.Euler(new Vector3((timeRatio * 360) - 270, 170, 0));
        */
        float sunAngleRatio = Vector3.Dot(sunLight.transform.forward, Vector3.down);

        sunLight.intensity = Mathf.Lerp(0, sunMaxIntensity, lightChangeCurve.Evaluate(sunAngleRatio));
        moonLight.intensity = Mathf.Lerp(moonMaxIntensity, 0, lightChangeCurve.Evaluate(sunAngleRatio));

        RenderSettings.ambientLight = Color.Lerp(nightAmbientLight, dayAmbientLight, lightChangeCurve.Evaluate(sunAngleRatio));
    }
}
