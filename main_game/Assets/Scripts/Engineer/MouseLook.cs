using System;
using UnityEngine;

[Serializable]
public class MouseLook : MonoBehaviour
{
    public float xSensitivity = 2f;
    public float ySensitivity = 2f;
    public bool clampVerticalRotation = true;
    public float minimumX = -90F;
    public float maximumX = 90F;
    public bool smooth;
    public float smoothTime = 5f;

	private Quaternion characterTargetRot;

    public void Init(Transform character, Transform camera)
    {
        characterTargetRot = character.localRotation;
    }
		
    public void LookRotation(Transform character, Transform camera)
    {
        float yRot = Input.GetAxis("Mouse X") * xSensitivity;
        float xRot = Input.GetAxis("Mouse Y") * ySensitivity;

        characterTargetRot *= Quaternion.Euler (-xRot, yRot, 0f);

        if(smooth)
            character.localRotation = Quaternion.Slerp (character.localRotation, characterTargetRot,
                smoothTime * Time.deltaTime);
        else
            character.localRotation = characterTargetRot;
    }
		
    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w  = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);
        angleX       = Mathf.Clamp (angleX, minimumX, maximumX);
        q.x          = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }
}
