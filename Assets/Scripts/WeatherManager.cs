using UnityEngine;
using UnityEngine.UI;

public class WeatherManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool testChangeWind = false;

    [Header ("Serialized Object")]
    [SerializeField] private Transform windArrow = null;
    [SerializeField] private Text windForceText = null;

    [SerializeField] private float maxWindForce = 100;

    private Vector3 windDirection = Vector3.forward;

    private float windAngle = 0;
    private float windForce = 0;

    public Vector3 WindDirection => windDirection;
    public float WindForce => windForce;

    public static WeatherManager Instance => instance;
    private static WeatherManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);

        instance = this;
    }

    private void Start()
    {
        UpdateWind();
    }

    private void Update()
    {
        if (testChangeWind)
        {
            testChangeWind = false;
            UpdateWind();
        }

        BlowWind(windAngle, windForce);
    }

    private void UpdateWind()
    {
        windAngle = Random.Range(0, 360);
        windForce = Mathf.Round(Random.Range(0, maxWindForce));
    }

    private void BlowWind(float angle, float force)
    {
        windDirection = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;

        windArrow.transform.localRotation = Quaternion.AngleAxis(angle -90 - Camera.main.transform.eulerAngles.y, -transform.forward);
        windForceText.text = force + " km/h";
    }

    private void OnDestroy()
    {
        instance = null;
    }
}
