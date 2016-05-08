using System;
using UnityEngine;

[Serializable]
public class MouseLook : MonoBehaviour
{
	private GameSettings settings;

	// Configuration parameters loaded through GameSettings
    private float xSensitivity;
    private float ySensitivity;
    private float xControllerSensitivity;
    private float yControllerSensitivity;
    private bool clampVerticalRotation;
    private float minimumX;
    private float maximumX;
    private bool smooth;
    private float smoothTime;
    private float yawSpeed;

	private Quaternion characterTargetRot;

    public void Init(Transform character, Transform camera, bool usingController)
    {
        if (settings == null)
            settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        LoadSettings();

        characterTargetRot = character.localRotation;

        if (usingController)
        {
            xSensitivity = xControllerSensitivity;
            ySensitivity = yControllerSensitivity;
        }
    }

    private void LoadSettings()
    {
        xSensitivity = settings.EngineerMouseLookXSensitivity;
        ySensitivity = settings.EngineerMouseLookYSensitivity;
        xControllerSensitivity = settings.EngineerControllerLookXSensitivity;
        yControllerSensitivity = settings.EngineerControllerLookYSensitivity;
        clampVerticalRotation = settings.EngineerMouseLookClampVerticalRotation;
        minimumX = settings.EngineerMouseLookMinimumX;
        maximumX = settings.EngineerMouseLookMaximumX;
        smooth = settings.EngineerMouseLookSmooth;
        smoothTime = settings.EngineerMouseLookSmoothTime;
        yawSpeed = settings.EngineerYawSpeed;
    }

    public void ResetTargetRotation(Transform character)
    {
        characterTargetRot = character.localRotation;
    }
		
    public bool LookRotation(Transform character, Transform camera)
    {
        float yRot = Input.GetAxis("Mouse X") * xSensitivity;
        float xRot = Input.GetAxis("Mouse Y") * ySensitivity;

        // Old Xbox yaw controls
        //float yawLeft = Input.GetButton("YawLeft") ? yawSpeed : 0;
        //float yawRight = Input.GetButton("YawRight") ? -yawSpeed : 0;
        //float zRot = yawLeft + yawRight;
        float zRot = Input.GetAxis("Yaw") * yawSpeed;

        // Check if the engineer has rotated
        bool rotated = yRot != 0 || xRot != 0 || zRot != 0;

        characterTargetRot *= Quaternion.Euler (-xRot, yRot, zRot);

        if(smooth)
            character.localRotation = Quaternion.Slerp (character.localRotation, characterTargetRot,
                smoothTime * Time.deltaTime);
        else
            character.localRotation = characterTargetRot;

        return rotated;
    }
}
