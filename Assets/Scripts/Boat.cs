using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boat : MonoBehaviour
{

    [System.Serializable]
    public struct BoatParts
    {
        [SerializeField] public Transform littleSail;
        [SerializeField] public Transform littleSailRotator;
        [SerializeField] public Transform bigSail;
        [SerializeField] public Transform bigSailRotator;
        [SerializeField] public Transform rudder;
        [SerializeField] public List<Transform> rightOars;
        [SerializeField] public List<Transform> leftOars;
    }

    [SerializeField] private BoatParts boatParts = new BoatParts();

    [Header("Serialized Object")]
    [SerializeField] private Text gearText = null;

    [Header ("Movement")]
    [SerializeField] private float sailAcceleration = 0.1f;
    [SerializeField] private float rowingBackTrackAcceleration = 5;
    [SerializeField] private float rowingAcceleration = 5;
    [SerializeField] private int minSpeedGear = -1;
    [SerializeField] private int maxSpeedGear = 2;
    //La vitesse maximale d'un bateau est gérer par sa friction et son acceleration par frame
    [SerializeField, Range(0,1)] private float friction = 0.98f;
    [SerializeField] private AnimationCurve driftingFrictionMultiplier = null;

    [Header ("Rotation")]
    [SerializeField] private float rotationSpeedWhenSailing = 10;
    [SerializeField] private float rotationSpeedWhenRowing = 10;
    [SerializeField] private float rudderRotationSpeed = 10;
    [SerializeField] private float sailMaxRotation = 60;
    [SerializeField] private float sailRotationSpeed = 0.5f;
    [SerializeField] private float maxRudderRotation = 60f;

    [Header ("Juice")]
    [SerializeField] float OarRandomMaxDelay = 0.05f;

    private Vector3 velocity;

    private Vector3 littleSailEndScale = new Vector3(0.4f, 3.9f, 2.87f);
    private Vector3 bigSailEndScale = new Vector3(0.25f, 5.14f, 4.19f);
    private Vector3 littleSailStartScale;
    private Vector3 bigSailStartScale;

    private Sequence deployOars;
    private Tween OpenLittleSail = null;
    private Tween CloseLittleSail = null;
    private Tween OpenBigSail = null;
    private Tween CloseBigSail = null;

    private Action DoAction;

    private bool hasDeployedAllOar = false;

    private int currentSpeedGear = 0;
    private int previousSpeedGear = 0;

    private float rotationInput;

    private void Awake()
    {
        DOTween.Init();
    }

    private void Start()
    {
        littleSailStartScale = boatParts.littleSail.localScale;
        bigSailStartScale = boatParts.bigSail.localScale;

        SetModeRow();
    }

    private void GetInput()
    {
        //Acceleration input
        if (Input.GetButtonDown("Vertical"))
            ChangeGear((int)Input.GetAxisRaw("Vertical"));

        //Rotation input
        rotationInput = Input.GetAxis("Horizontal");

        //Rotate sail
        if (Input.GetButton("RotateSail"))
            RotateSail(Input.GetAxis("RotateSail"));

    }

    private void Update()
    {
        GetInput();

        DoAction?.Invoke();
        Move();
    }

    private void Move()
    {
        //Move
        transform.position += velocity * Time.deltaTime;

        int zDirection = Vector3.Angle(velocity, transform.forward) > 90 ? -1 : 1;

        //Calculate drifting proportion
        float driftingRatio = Vector3.Angle(velocity, transform.forward * zDirection) / 180;

        //Calculate friction
        float friction = Mathf.Pow(this.friction, Time.deltaTime);

        //Apply friction relatively to how the boat drift
        velocity *= friction * driftingFrictionMultiplier.Evaluate(driftingRatio);
    }

    private void ChangeGear(int gearInput)
    {
        previousSpeedGear = currentSpeedGear;
        currentSpeedGear = Mathf.Clamp(currentSpeedGear + gearInput, minSpeedGear, maxSpeedGear);

        if (gearText == null) 
            Debug.Log("No Gear Text Serialized");
        else
            gearText.text = "Gear : " + currentSpeedGear;

        if (currentSpeedGear >= 0)
            DeploySail();

        if (currentSpeedGear == 1 && previousSpeedGear < 1)
            SetModeSail();
        else if (currentSpeedGear == 0 && previousSpeedGear > 0)
            SetModeRow();
    }

#region Sail mode
    private void SetModeSail()
    {
        DeployAllOar(false);
        DoAction = DoActionSail;
    }

    private void DoActionSail()
    {
        RotateBoatWhenSailing();
        MoveBoatWhenSailing();
    }

    private void MoveBoatWhenSailing()
    {
        float currentAccel = sailAcceleration * GetWindForce() * currentSpeedGear;

        //Calculate velocity
        velocity += currentAccel * transform.forward * Time.deltaTime;
    }

#region Rotation
    private void RotateBoatWhenSailing()
    {
        //Rudder rotation
        Quaternion targetedRotation = Quaternion.Euler(0, -rotationInput * maxRudderRotation + transform.eulerAngles.y, 0);
        boatParts.rudder.rotation = Quaternion.RotateTowards(boatParts.rudder.rotation, targetedRotation, rudderRotationSpeed);

        //Boat rotation
        float signe = rotationInput > 0 ? 1 : -1;
        float rudderTurnRatio = Quaternion.Angle(transform.rotation, targetedRotation) / maxRudderRotation * signe;

        transform.rotation *= Quaternion.AngleAxis(rudderTurnRatio * velocity.magnitude * rotationSpeedWhenSailing * Time.deltaTime, transform.up);

        Debug.Log(rudderTurnRatio * velocity.magnitude * rotationSpeedWhenSailing);
    }

    private void RotateSail(float inputValue)
    {
        Quaternion targetedRotation = Quaternion.Euler(0, inputValue * sailMaxRotation + transform.eulerAngles.y, 0);

        boatParts.littleSailRotator.rotation = Quaternion.RotateTowards(boatParts.littleSailRotator.rotation, targetedRotation, sailRotationSpeed);
        boatParts.bigSailRotator.rotation = Quaternion.RotateTowards(boatParts.bigSailRotator.rotation, targetedRotation, sailRotationSpeed);

        //To do = add other sail with weight value to create a global sail forward
        //sailForward = littleSailRotator.forward;
    }
#endregion //Rotation

    private float GetWindForce()
    {
        float WindToBoatAngle = Vector3.Angle(WeatherManager.Instance.WindDirection, boatParts.littleSailRotator.forward);
        return WindToBoatAngle < 90 ? WeatherManager.Instance.WindForce * (1 - WindToBoatAngle / 90) : 0;
    }

    private void DeploySail()
    {
        if (previousSpeedGear != currentSpeedGear)
        {
            switch (currentSpeedGear)
            {
                case 0:
                    if (previousSpeedGear > currentSpeedGear)
                    {
                        if (CloseLittleSail != null)
                            CloseLittleSail.Kill();

                        CloseLittleSail = boatParts.littleSail.DOScale(littleSailStartScale, 1);
                    }
                    break;

                case 1:
                    if (CloseLittleSail != null)
                        CloseLittleSail.Kill();

                    if (previousSpeedGear > currentSpeedGear)
                    {
                        if (OpenBigSail != null)
                            OpenBigSail.Kill();

                        if (CloseBigSail != null)
                            CloseBigSail.Kill();

                        CloseBigSail = boatParts.bigSail.DOScale(bigSailStartScale, 1);
                    }
                    else
                    {
                        if (OpenLittleSail != null)
                            OpenLittleSail.Kill();

                        OpenLittleSail = boatParts.littleSail.DOScale(littleSailEndScale, 1);
                        SoundManager.Instance.PlaySFX("OpenLittleSail");
                    }
                    break;

                case 2:
                    if (CloseBigSail != null)
                        CloseBigSail.Kill();

                    if (OpenBigSail != null)
                        OpenBigSail.Kill();

                    OpenBigSail = boatParts.bigSail.DOScale(bigSailEndScale, 1);
                    SoundManager.Instance.PlaySFX("OpenBigSail");
                    break;

                default:
                    break;
            }
        }
    }
    #endregion //Sail mode

#region Row mode

    private void SetModeRow()
    {
        DeployAllOar(true);

        DoAction = DoActionRow;
    }

    private void DoActionRow()
    {
        MoveBoatWhenRowing();
        RotateBoatWhenRowing();
    }

    private void RotateBoatWhenRowing()
    {
        //Boat rotation
        if (hasDeployedAllOar)
            transform.rotation *= Quaternion.AngleAxis(rotationInput * rotationSpeedWhenRowing * Time.deltaTime, transform.up);
    }

    private void MoveBoatWhenRowing()
    {
        float acceleration = currentSpeedGear < 0 ? rowingBackTrackAcceleration : rowingAcceleration;

        float currentAccel = currentSpeedGear * acceleration;

        //Calculate velocity
        velocity += currentAccel * transform.forward * Time.deltaTime;
    }

    private void DeployAllOar(bool willBeDeployed)
    {
        int targetedScale = willBeDeployed ? 1 : 0;

        if (deployOars != null)
            deployOars.Kill();

        deployOars = DOTween.Sequence();

        DeployAllOarFromASide(boatParts.rightOars, targetedScale, willBeDeployed);
        DeployAllOarFromASide(boatParts.leftOars, targetedScale, willBeDeployed);

        deployOars.Play()
            .OnComplete(() => hasDeployedAllOar = willBeDeployed);
    }

    private void DeployAllOarFromASide(List<Transform> oarsSide, int targetScaleX, bool willBeDeployed)
    {
        string clipName = willBeDeployed ? "OarDeploy" : "OarTidy";

        for (int i = 0; i < oarsSide.Count; i++)
        {
            deployOars.Insert(0.1f + UnityEngine.Random.value * OarRandomMaxDelay,
                oarsSide[i].GetChild(0).DOScaleX(targetScaleX, 0.2f)
                .Pause()
                .OnComplete(() => SoundManager.Instance.PlaySFX(clipName))
                );
        }
    }
#endregion //Row mode
}
