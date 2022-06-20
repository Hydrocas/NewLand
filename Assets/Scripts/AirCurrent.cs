
using UnityEngine;

public class AirCurrent : MonoBehaviour
{
    [SerializeField] private ParticleSystem windParticles = null;
    [SerializeField] private float maxWindEmissionRate = 20;
    [SerializeField, Range(0.5f, 1)] private float currentForce = 0.5f;

    private void Start()
    {
        ParticleSystem.EmissionModule emission = windParticles.emission;
        ParticleSystem.ShapeModule shape = windParticles.shape;

        float volume = shape.scale.x * shape.scale.y * shape.scale.z;
        float newVolume = transform.localScale.x * shape.scale.y * transform.localScale.z;

        emission.rateOverTime = (newVolume * maxWindEmissionRate) / volume * currentForce;
        shape.scale = new Vector3(transform.localScale.x, shape.scale.y, transform.localScale.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        WeatherManager.Instance.SetActionAirCurrent(currentForce, transform.forward);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        WeatherManager.Instance.SetActionRandomWind();
    }
}
