using UnityEngine;
using System.Collections;

public class ConsoleShipControl : MonoBehaviour {

	private const float rotationSpeed = 300;

	Quaternion desiredRotation;
	Quaternion currentRotation;
	Quaternion rotation;
	float xDeg, yDeg;
	LineRenderer lineRenderer;
	LineRenderer lineRenderer1;
	LineRenderer lineRenderer2;
	GameObject leftEngine;
	GameObject engine;
	GameObject hullFront;
	GameObject captainBridge;
	// Use this for initialization
	void Start () {
		leftEngine = GameObject.Find("Engine");
		hullFront = GameObject.Find("HullFront");
		captainBridge = GameObject.Find("CaptainBridge");
		engine = GameObject.Find("EnginePicture");
		lineRenderer = leftEngine.AddComponent<LineRenderer>();
		lineRenderer1 = hullFront.AddComponent<LineRenderer>();
		lineRenderer2 = captainBridge.AddComponent<LineRenderer>();
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton(0))
		{
			lineRenderer.SetWidth(0.1f, 0.1f);
			lineRenderer.material = new Material (Shader.Find("Particles/Additive"));
			lineRenderer.SetColors(Color.gray, Color.grey);
			lineRenderer1.SetWidth(0.1f, 0.1f);
			lineRenderer1.material = new Material (Shader.Find("Particles/Additive"));
			lineRenderer1.SetColors(Color.gray, Color.gray);
			lineRenderer2.SetWidth(0.1f, 0.1f);
			lineRenderer2.material = new Material (Shader.Find("Particles/Additive"));
			lineRenderer2.SetColors(Color.gray, Color.gray);

			lineRenderer.SetVertexCount(2);
			lineRenderer.SetPosition(0, leftEngine.transform.position);
			lineRenderer.SetPosition(1, new Vector3(0.1f, 5, 20));

			lineRenderer1.SetVertexCount(2);
			lineRenderer1.SetPosition(0, hullFront.transform.position);
			lineRenderer1.SetPosition(1, new Vector3(-1, 5, 40));

			lineRenderer2.SetVertexCount(2);
			lineRenderer2.SetPosition(0, captainBridge.transform.position);
			lineRenderer2.SetPosition(1, new Vector3(10, 5, 25));

			Debug.Log(leftEngine.transform.position);
		
			xDeg += Input.GetAxis("Mouse X") * rotationSpeed * 0.02f;

			desiredRotation = Quaternion.Euler(xDeg, 0, 270);
			currentRotation = transform.rotation;
			rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime);
			transform.rotation = rotation;
		}
	}
}
