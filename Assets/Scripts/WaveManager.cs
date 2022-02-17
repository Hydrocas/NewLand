using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance => _instance;
    private static WaveManager _instance;

    [SerializeField] private float amplitude = 1;
    [SerializeField] private float length = 2;
    [SerializeField] private float speed = 1;
    [SerializeField] private float offset = 0;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Debug.LogError("Instance already exist");
            Destroy(this);
        }
    }

    private void Update()
    {
        offset += speed * Time.deltaTime;
    }

    public float GetWaveHeight(float x)
    {
        return amplitude * Mathf.Sin(x / length + offset);
    }
}
