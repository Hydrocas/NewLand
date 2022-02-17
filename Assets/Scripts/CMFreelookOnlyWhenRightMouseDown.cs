using UnityEngine;
using Cinemachine;

public class CMFreelookOnlyWhenRightMouseDown : MonoBehaviour
{
    void Start()
    {
        CinemachineCore.GetInputAxis = GetAxisCustom;
    }

    public float GetAxisCustom(string axisName)
    {
        if (axisName == "Mouse X")
        {
            if (Input.GetButton("FreeCam"))
            {
                return Input.GetAxis("Mouse X");
            }
            else
            {
                return 0;
            }
        }
        else if (axisName == "Mouse Y")
        {
            if (Input.GetButton("FreeCam"))
            {
                return Input.GetAxis("Mouse Y");
            }
            else
            {
                return 0;
            }
        }
        return Input.GetAxis(axisName);
    }
}
