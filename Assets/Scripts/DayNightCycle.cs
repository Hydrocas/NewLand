using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField] private float speed = 0.1f;

    private void Update()
    {
        transform.rotation *= Quaternion.AngleAxis(speed * Time.deltaTime, Vector3.right);
    }
}
