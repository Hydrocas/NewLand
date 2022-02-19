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
    [SerializeField] private BeaufortLadder currentWindState = BeaufortLadder.Calm;

    [Header("Wind Settings")]
    [SerializeField] private float maxWindForce = 100;
    [SerializeField] private float windChangeRotationSpeed = 100;
    [SerializeField] private float windChangeForceSpeed = 10;
    [SerializeField] private float delayToChangeWindInSeconds = 4;

    private Dictionary<BeaufortLadder, Vector2> windForceSlices = new Dictionary<BeaufortLadder, Vector2> 
    { 
        { BeaufortLadder.Calm,              new Vector2(0, 1) },
        { BeaufortLadder.VeryLightBreeze,   new Vector2(1, 5) },
        { BeaufortLadder.LightBreeze,       new Vector2(5, 11) },
        { BeaufortLadder.GentleBreeze,      new Vector2(11, 19) },
        { BeaufortLadder.ModerateBreeze,    new Vector2(19, 28) },
        { BeaufortLadder.CoolBreeze,        new Vector2(28, 38) },
        { BeaufortLadder.StrongBreeze,      new Vector2(38, 49) },
        { BeaufortLadder.StrongGusts,       new Vector2(49, 61) },
        { BeaufortLadder.ViolentWind,       new Vector2(61, 74) },
        { BeaufortLadder.VeryStrongWind,    new Vector2(74, 88) },
        { BeaufortLadder.StrongStorm,       new Vector2(88, 102) },
        { BeaufortLadder.ViolentStorm,      new Vector2(102, 117) },
        { BeaufortLadder.Hurricane,         new Vector2(117, Mathf.Infinity) },
    };

    private static WeatherManager _instance;

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
        VeryLightBreeze,
        LightBreeze,
        GentleBreeze,
        ModerateBreeze,
        CoolBreeze,
        StrongBreeze,
        StrongGusts,
        ViolentWind,
        VeryStrongWind,
        StrongStorm,
        ViolentStorm,
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
