using UnityEngine;
using System.Collections;

public class ConsoleShipControl : MonoBehaviour {

	private const float rotationSpeed = 300;

	private Quaternion desiredRotation;
	private Quaternion currentRotation;
	private Quaternion rotation;
	private float xDeg, yDeg;
	private LineRenderer lineRenderer;
	private LineRenderer lineRenderer1;
	private LineRenderer lineRenderer2;
	private Transform leftEngine;
	private Transform hullFront;
	private Transform captainBridge;

	void Start () {

		Transform[] children = gameObject.GetComponentsInChildren<Transform>();
		foreach (Transform child in children) {
			if(child.name == "Engine") 
				leftEngine = child;
			if(child.name == "HullFront")
				hullFront = child;
			if(child.name == "CaptainBridge")
				captainBridge = child;
		}
			
		lineRenderer  = leftEngine.gameObject.AddComponent<LineRenderer>();
		lineRenderer1 = hullFront.gameObject.AddComponent<LineRenderer>();
		lineRenderer2 = captainBridge.gameObject.AddComponent<LineRenderer>();

		//Draw lines between upgrade buttons and parts of the ship
		DrawLines();
	}
		
	void Update () {
		
		if (Input.GetMouseButton(0))
		{
			// Get mouse x position and rotate the ship about the y axis
			xDeg += Input.GetAxis("Mouse X") * rotationSpeed * 0.02f;
			desiredRotation = Quaternion.Euler(0, xDeg, 0);
			currentRotation = transform.rotation;
			rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime);
			transform.rotation = rotation;

			DrawLines();
		}
	}

	private void DrawLines() 
	{
		lineRenderer.SetWidth(0.05f, 0.05f);
		lineRenderer1.SetWidth(0.05f, 0.05f);
		lineRenderer2.SetWidth(0.05f, 0.05f);

		lineRenderer.material = new Material (Shader.Find("Particles/Additive"));
		lineRenderer1.material = new Material (Shader.Find("Particles/Additive"));
		lineRenderer2.material = new Material (Shader.Find("Particles/Additive"));

		lineRenderer.SetColors(Color.gray, Color.grey);
		lineRenderer1.SetColors(Color.gray, Color.gray);
		lineRenderer2.SetColors(Color.gray, Color.gray);

		lineRenderer.SetVertexCount(2);
		lineRenderer1.SetVertexCount(2);
		lineRenderer2.SetVertexCount(2);

		lineRenderer.SetPosition(0, leftEngine.position);
		lineRenderer.SetPosition(1, new Vector3(-4.6f, 1.7f, 8));

		lineRenderer1.SetPosition(0, hullFront.position);
        lineRenderer1.SetPosition(1, new Vector3(-4.6f, 0.4f, 8));

		lineRenderer2.SetPosition(0, captainBridge.position);
        lineRenderer2.SetPosition(1, new Vector3(-4.6f, -0.9f, 8));
	}
}
