using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeatherManager : MonoBehaviour
{
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
    private float targetedWindAngle;
    private float targetedwindForce;
    private float windTimeCounter = 0;

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
        SetRandomWind();
    }

    private void Update()
    {
        windTimeCounter += Time.deltaTime;

        if (windTimeCounter > delayToChangeWindInSeconds || testChangeWind)
        {
            testChangeWind = false;
            windTimeCounter = 0;
            SetRandomWind();
        }

        DrawDebugWind();
    }

    private void SetRandomWind()
    {
        targetedWindAngle = Random.Range(0, 360);
        targetedwindForce = Random.Range(0, maxWindForce);

        if (windChangeDirectionRoutine != null)
            StopCoroutine(windChangeDirectionRoutine);

        if (windChangeForceRoutine != null)
            StopCoroutine(windChangeForceRoutine);

        windChangeDirectionRoutine = StartCoroutine(ChangeWindRotationCoroutine());
        windChangeForceRoutine = StartCoroutine(ChangeWindForceCoroutine());
    }

    private IEnumerator ChangeWindRotationCoroutine()
    {
        while(windAngle != targetedWindAngle)
        {
            windAngle = Mathf.MoveTowardsAngle(windAngle, targetedWindAngle, windChangeRotationSpeed * Time.deltaTime);
            _windDirection = Quaternion.AngleAxis(windAngle, Vector3.up) * Vector3.forward;

            yield return null;
        }

        yield return null;
    }

    private IEnumerator ChangeWindForceCoroutine()
    {
        while (_windForce != targetedwindForce)
        {
            _windForce = Mathf.MoveTowards(_windForce, targetedwindForce, windChangeForceSpeed * Time.deltaTime);

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

            yield return null;
        }

        yield return null;
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
