using UnityEngine;
using System.Collections;

public class RandomPos : MonoBehaviour {
	
	Transform thisTransform;
	public float updateTime = 1;
	public float maxRandomTime = 1.5f;

	public float range = 2;
	Vector3 startPosition;
	// Use this for initialization
	void Start () {
	
		thisTransform = transform;
		startPosition = transform.position;
		StartCoroutine(update());
	}
	
	void OnEnable(){
		
		StartCoroutine(update());
		
	}
	
	IEnumerator update(){
		
		yield return new WaitForSeconds(updateTime+Random.Range(0f,maxRandomTime));
		
		thisTransform.position = startPosition+(Vector3.left*Random.Range(-range,range))+(Vector3.up*Random.Range(-range,range))+(Vector3.back*Random.Range(-range,range));
		StartCoroutine(update());
	}
	
}
