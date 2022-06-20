using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeatherManager : MonoBehaviour
{
    [SerializeField] private Transform playerTransform = null;

    [Header("Debug")]
    [SerializeField] private bool testChangeWind = false;
    [SerializeField] private Transform windArrow = null;
    [SerializeField] private Text windForceText = null;
    [SerializeField] private Text windForceBeaufortText = null;
    [SerializeField] private Gradient beaufortGradient;

    [Header("Wind Settings")]
    [SerializeField] private float maxWindForce = 130;
    [SerializeField] private float windChangeRotationSpeed = 100;
    [SerializeField] private float windChangeForceSpeed = 10;
    [SerializeField] private float delayToChangeWindInSeconds = 4;
    [SerializeField] private float maxWindGustForce = 10;
    [SerializeField] private float windGustOscillationSpeed = 10;

    [SerializeField] private ParticleSystem windParticles = null;
    [SerializeField] private float windParticlesMaxEmissionRate = 200;

    private Dictionary<int, Vector2> windForceSlices = new Dictionary<int, Vector2> 
    { 
        { 0, new Vector2(0, 1) },
        { 1, new Vector2(1, 5) },
        { 2, new Vector2(5, 11) },
        { 3, new Vector2(11, 19) },
        { 4, new Vector2(19, 28) },
        { 5, new Vector2(28, 38) },
        { 6, new Vector2(38, 49) },
        { 7, new Vector2(49, 61) },
        { 8, new Vector2(61, 74) },
        { 9, new Vector2(74, 88) },
        { 10, new Vector2(88, 102) },
        { 11, new Vector2(102, 117) },
        { 12, new Vector2(117, Mathf.Infinity) },
    };

    private static WeatherManager _instance;

    private KeyValuePair<int, Vector2> currendWindForceSlice;

    private Coroutine windChangeDirectionRoutine;
    private Coroutine windChangeForceRoutine;

    private Vector3 _windDirection;
    
    private float windAngle = 0;
    private float _windForce = 0;
    private float windForceWithoutGust = 0;
    private float targetedWindAngle;
    private float targetedwindForce;
    private float windTimeCounter = 0;

    private bool isWindChangingForce = false;

    private Action DoAction;

    private enum BeaufortLadder
    {
        Calm,
        Very_Light_Breeze,
        Light_Breeze,
        Gentle_Breeze,
        Moderate_Breeze,
        Fresh_Breeze,
        Strong_Breeze,
        High_Wind,
        Violent_Wind,
        Very_Violent_Wind,
        Storm,
        Violent_Storm,
        Hurricane
    }

    #region Accessors
    public static WeatherManager Instance => _instance;
    public Vector3 WindDirection => _windDirection;
    public float WindForce => _windForce;
    #endregion //Accesors
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(gameObject);

        _instance = this;
    }

    private void Start()
    {
        SetActionRandomWind();
    }

    private void Update()
    {
        windParticles.transform.position = new Vector3(playerTransform.position.x, windParticles.transform.position.y, playerTransform.position.z);

        DoAction?.Invoke();

        DrawDebugWind();
    }

    public void SetActionRandomWind()
    {
        DoAction = DoActionRandomWind;

        //Set a random wind at start
        windTimeCounter = delayToChangeWindInSeconds + 1;
    }

    public void SetActionAirCurrent(float force, Vector3 direction)
    {
        DoAction = DoActionVoid;
        
        Vector3 perp = Vector3.Cross(Vector3.forward, direction);
        float sign = Vector3.Dot(perp, Vector3.up) < 0 ? -1 : 1;

        SetWind(Vector3.Angle(Vector3.forward, direction) * sign, force * maxWindForce);
    }

    private void SetActionVoid()
    {
        DoAction = DoActionVoid;
    }

    private void DoActionRandomWind()
    {
        windTimeCounter += Time.deltaTime;

        if (windTimeCounter > delayToChangeWindInSeconds || testChangeWind)
        {
            testChangeWind = false;
            windTimeCounter = 0;

            SetWind(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, maxWindForce));
        }

        if (!isWindChangingForce)
            DoWindGust();
    }

    private void DoActionVoid()
    {

    }

    private void SetWind(float targetAngle, float targetForce)
    {
        ParticleSystem.EmissionModule emission = windParticles.emission;
        emission.rateOverTime = targetForce / maxWindForce * windParticlesMaxEmissionRate;

        windParticles.transform.rotation = Quaternion.Euler(new Vector3(0, targetAngle, 0));

        targetedWindAngle = targetAngle;
        targetedwindForce = targetForce;

        if (windChangeDirectionRoutine != null)
            StopCoroutine(windChangeDirectionRoutine);

        if (windChangeForceRoutine != null)
            StopCoroutine(windChangeForceRoutine);

        windChangeDirectionRoutine = StartCoroutine(ChangeWindRotationCoroutine());
        windChangeForceRoutine = StartCoroutine(ChangeWindForceCoroutine());
    }

    #region Wind Coroutine
    private IEnumerator ChangeWindRotationCoroutine()
    {
        while (windAngle != targetedWindAngle)
        {
            windAngle = Mathf.MoveTowardsAngle(windAngle, targetedWindAngle, windChangeRotationSpeed * Time.deltaTime);
            _windDirection = Quaternion.AngleAxis(windAngle, Vector3.up) * Vector3.forward;

            yield return null;
        }

        yield return null;
    }

    private IEnumerator ChangeWindForceCoroutine()
    {
        isWindChangingForce = true;

        while (_windForce != targetedwindForce)
        {
            windForceWithoutGust = Mathf.MoveTowards(_windForce, targetedwindForce, windChangeForceSpeed * Time.deltaTime);
            UpdateWindForce(windForceWithoutGust);

            yield return null;
        }

        isWindChangingForce = false;

        yield return null;
    }
    #endregion // Wind Coroutine

    private void UpdateWindForce(float windForce)
    {
        _windForce = windForce;

        if (windForceText != null)
            windForceText.text = Mathf.RoundToInt(_windForce) + " km/h";

        foreach (KeyValuePair<int, Vector2> windForceSlice in windForceSlices)
        {
            if (_windForce >= windForceSlice.Value.x && _windForce < windForceSlice.Value.y)
            {
                currendWindForceSlice = windForceSlice;

                string beaufortName = (BeaufortLadder)windForceSlice.Key + "";

                if (windForceBeaufortText != null)
                {
                    windForceBeaufortText.color = beaufortGradient.Evaluate(windForceSlice.Key / 12f);
                    windForceBeaufortText.text = beaufortName.Replace("_", " ") + " " + windForceSlice.Key;
                }

                break;
            }
        }
    }

    private void DoWindGust()
    {
        /*
        float windGustMaxForceRatio = Mathf.PingPong(Time.time * windGustOscillationSpeed, maxWindGustForce);

        //Etudier l'utilisation du perlin pour faire une rafale plus randome mais smooth
        //Mathf.PerlinNoise(0, 1);
        Debug.LogError("Do perlin noise on wind gust");

        Debug.Log("gust force ratio " + windGustMaxForceRatio);

        UpdateWindForce(windForceWithoutGust + windGustMaxForceRatio);*/
    }

    private void DrawDebugWind()
    {
        if (windArrow != null)
            windArrow.transform.localRotation = Quaternion.AngleAxis(windAngle - 90 - Camera.main.transform.eulerAngles.y, -transform.forward);
    }

    private void OnDestroy()
    {
        _instance = null;
    }
}
