/// ProFlares - v1.08 - Copyright 2014-2015 All rights reserved - ProFlares.com


/// <summary>
/// ProFlare.cs
/// Holds the definition of a flare and all of its features.
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class SubElement{
	public Color color = Color.white;
	public Color colorFinal = Color.white;
	public float position = 0;
	public Vector3 offset = Vector2.zero;
	public float angle = 0;
	public float scale = 0;
	public float random = 0.5f;
	public float random2= 0.5f;
	public float RandomScaleSeed= 0.5f;
	public float RandomColorSeedR = 0.5f;
	public float RandomColorSeedG = 0.5f;
	public float RandomColorSeedB = 0.5f;
	public float RandomColorSeedA = 0.5f;
}

[System.Serializable]
public class ProFlareElement{
	
	public bool Editing;
	
	public bool Visible = true;
    
	//Element's texture index inside the texture atlas.
	public int elementTextureID;
	
	//Elements Sprite name from the texture atlas, this isn't checked at runtime. Its only used to help stop flares breaking when the atlas changes.
	public string SpriteName;
	
	public ProFlare flare;
	
	public ProFlareAtlas flareAtlas;
	
	public float Brightness = 1;
	public float Scale = 1;
	public float ScaleRandom = 0;
	public float ScaleFinal = 1;
	
	public Vector4 RandomColorAmount = Vector4.zero;
	
	//Element OffSet Properties
	public float position;
	
	public bool useRangeOffset = false;
	
	public float SubElementPositionRange_Min = -1f;
	public float SubElementPositionRange_Max = 1f;
	
	public float SubElementAngleRange_Min = -180f;
	public float SubElementAngleRange_Max = 180f;
	
	
	public Vector3 OffsetPosition;
	
	public Vector3 Anamorphic = Vector3.zero;
	
	public Vector3 OffsetPostion = Vector3.zero;
	
	
	//Element Rotation Properties
	public float angle = 0;
	public float FinalAngle = 0; //Final angle used by FlareBatch
	public bool useRandomAngle;
	public bool useStarRotation;
	public float AngleRandom_Min;
	public float AngleRandom_Max;
	public bool OrientToSource;
	public bool rotateToFlare;
	public float rotationSpeed;
	public float rotationOverTime;
	
	
	//Colour Properties,
	public bool useColorRange;
	
	public Color ElementFinalColor;
	
	public Color ElementTint = new Color(1,1,1,0.33f);
	
	
	public Color SubElementColor_Start = Color.white;
	public Color SubElementColor_End = Color.white;
	
	//Scale Curve
	public bool useScaleCurve;
	public AnimationCurve ScaleCurve = new AnimationCurve(new Keyframe(0, 0.1f), new Keyframe(0.5f, 1.0f), new Keyframe(1.0f, 0.1f));
	
    
	//Override Properties
	public bool OverrideDynamicEdgeBoost;
	public float DynamicEdgeBoostOverride = 1f;
	
	public bool OverrideDynamicCenterBoost;
	public float DynamicCenterBoostOverride = 1f;
	
	public bool OverrideDynamicEdgeBrightness;
	public float DynamicEdgeBrightnessOverride = 0.4f;
	
	public bool OverrideDynamicCenterBrightness;
	public float DynamicCenterBrightnessOverride = 0.4f;
	
	//public SubElement[] subElements;
	public List<SubElement> subElements = new List<SubElement>();
	
	
	
	public Vector2 size = Vector2.one;
	
	public enum Type{
		Single,
		Multi
	}
	
#if UNITY_EDITOR
	
	public bool EditDynamicTriggering;
	public bool EditOcclusion;
	
	public bool ElementSetting;
	public bool OffsetSetting;
	public bool ColorSetting;
	public bool ScaleSetting;
	public bool RotationSetting;
	public bool OverrideSetting;

	public Texture2D colorTexture;
	public bool colorTextureDirty;
#endif
	
	public Type type;
}

[ExecuteInEditMode]
public class ProFlare : MonoBehaviour {
	
	
	//Required External Objects
	public ProFlareAtlas _Atlas;

	public ProFlareBatch[] FlareBatches = new ProFlareBatch[0];
	
	public bool EditingAtlas;
	
	public bool isVisible = true;
	//
	public List<ProFlareElement> Elements = new List<ProFlareElement>();
	
	//Cached Components
	public Transform thisTransform;
	
	//Screen space Flare Position
	public Vector3 LensPosition;
	
	//Global Settings
	public bool EditGlobals;
	public float GlobalScale = 100;
	public bool MultiplyScaleByTransformScale = false;

	public float GlobalBrightness = 1;
	public Color GlobalTintColor = Color.white;
	
	
	//Distance Fall off
	public bool useMaxDistance = true;
	public bool useDistanceScale = true;
	public bool useDistanceFade = true;
	public float GlobalMaxDistance = 150f;
    
	//Angle Culling Properties
	public bool UseAngleLimit;
	public float maxAngle = 90f;
	public bool UseAngleScale;
	public bool UseAngleBrightness = true;
	public bool UseAngleCurve;
	public AnimationCurve AngleCurve = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(1, 1.0f));
	
	//Occlusion Properties
	public LayerMask mask = 1;
	public bool RaycastPhysics;
	public bool Occluded = false;
	public float OccludeScale = 1;
	
#if UNITY_EDITOR
	public GameObject OccludingObject;
#endif
    
	//Editor Inspector Toggles.
#if UNITY_EDITOR
	public bool EditDynamicTriggering;
	public bool EditOcclusion;
	public bool IoSetting;

#endif
    
	
	public float OffScreenFadeDist = 0.2f;
	
	//Dynamic Edge Properties
	public bool useDynamicEdgeBoost = false;
	public float DynamicEdgeBoost = 1f;
	public float DynamicEdgeBrightness = 0.1f;
	public float DynamicEdgeRange = 0.3f;
	public float DynamicEdgeBias = -0.1f;
	
	public AnimationCurve DynamicEdgeCurve = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(0.5f, 1), new Keyframe(1, 0f));
	
	//Dynamic Center Properties
	public bool useDynamicCenterBoost = false;
	public float DynamicCenterBoost = 1f;
	public float DynamicCenterBrightness = 0.2f;
	public float DynamicCenterRange = 0.3f;
	public float DynamicCenterBias = 0.0f;
    
	public bool neverCull;
	
	
	void Awake(){
		DisabledPlayMode = false;
		Initialised = false;
		thisTransform = transform;
	}
	
	void Start(){
		//Cache Transform.
		thisTransform = transform;
		
		if((_Atlas != null)&&(FlareBatches.Length == 0)){
             PopulateFlareBatches();
 		}
		
		if(!Initialised){
               Init();
       } 
		
		//Make Sure All elements reference the correct atlas.
		for(int i = 0; i < Elements.Count; i++){
			Elements[i].flareAtlas = _Atlas;
			Elements[i].flare = this;
		}
	}
	
	
	void PopulateFlareBatches(){
		
        //Atlas is set but Flare is not try and find a FlareBatch with the same atlas;
        ProFlareBatch[] flareBatchs = GameObject.FindObjectsOfType(typeof(ProFlareBatch)) as ProFlareBatch[];
        int matchCount = 0;
		
		foreach (ProFlareBatch flareBatch in flareBatchs){
               if (flareBatch._atlas == _Atlas){
               		 matchCount++;
                }
		}
		
		FlareBatches = new ProFlareBatch[matchCount];

		int count = 0;
		
		foreach (ProFlareBatch flareBatch in flareBatchs){
			if (flareBatch._atlas == _Atlas){
				FlareBatches[count] = flareBatch;
				count++;
            }
		}
	}
	
	bool Initialised;
 	
	void Init(){
	 	
		if(thisTransform == null)
			thisTransform = transform;
		
		if(_Atlas == null)
			return;
		 
		PopulateFlareBatches();
		 
		
		//Make Sure All elements reference the correct atlas.
		for(int i = 0; i < Elements.Count; i++){
			Elements[i].flareAtlas = _Atlas;
		}
		
		//All Conditions met, add flare to flare batch.
		for(int i = 0; i < FlareBatches.Length; i++){
			if(FlareBatches[i] != null)
				if(FlareBatches[i]._atlas == _Atlas)
					FlareBatches[i].AddFlare(this);
		}
		
		Initialised = true;
	}
	
	public void ReInitialise(){
		Initialised = false;
		Init();
	}
	
	/// <summary>
	/// Rebuilding the FlareBatch geometry is more expensive than just updating it, and can cause a small hickup in the frame
	/// While in play mode, we avoid doing a full rebuild by using a 'soft' enable/disable. If Flare isn't in the FlareBatches list it will always be added.
	/// </summary>
	
	public bool DisabledPlayMode;
	void OnEnable(){
		if(Application.isPlaying && DisabledPlayMode){
		 	DisabledPlayMode = false;
		}else{
			if(_Atlas)
			for(int i = 0; i < FlareBatches.Length; i++){
				if(FlareBatches[i] != null){
					FlareBatches[i].dirty = true;
				}else{
					Initialised = false;	
				}
			}
			Init();
		}
	}
	
	void OnDisable(){
		if(Application.isPlaying){
			DisabledPlayMode = true;
		}else{
 
			for(int i = 0; i < FlareBatches.Length; i++){
				
				if(FlareBatches[i] != null){
					FlareBatches[i].RemoveFlare(this);// .Flares.Remove(this);
					FlareBatches[i].dirty = true;
				}else{
					
					Initialised = false;	
				}
			}
		}
	}
	
	/// <summary>
	/// If Flare is destroyed, it will be removed from the FlaresBatch List.
	/// </summary>
	void OnDestroy(){		
		for(int i = 0; i < FlareBatches.Length; i++){
			if(FlareBatches[i] != null){
//				FlareBatches[i].Flares.Remove(this);
				FlareBatches[i].RemoveFlare(this);// .Flares.Remove(this);
				FlareBatches[i].dirty = true;
			}else{
				Initialised = false;	
			}
		}
	}
    
	/// <summary>
	/// Helper piece of code that only runs while in the editor.
	/// It checks the state of the flare against its atlas.
	/// </summary>
#if UNITY_EDITOR
	void LateUpdate(){
		for(int i = 0; i < Elements.Count; i++){
			if(Elements[i].SpriteName == ""){
                Elements[i].SpriteName = _Atlas.elementsList[Elements[i].elementTextureID].name;
			}
		}
 
		for(int i = 0; i < Elements.Count; i++){
			
			bool error = false;
			
			if(!_Atlas)
				continue;
			
			if(Elements[i].elementTextureID >= _Atlas.elementsList.Count){
				Debug.LogWarning("ProFlares - Elements ID No longer exists");
				error = true;
			}
			
			if(!error)
				if(Elements[i].SpriteName != _Atlas.elementsList[Elements[i].elementTextureID].name){
					Debug.LogWarning("Pro Flares - Where did my elements go? - Lets try and find it. -" + Elements[i].SpriteName,gameObject);
					error = true;
				}
			
			if(error){
				bool Found = false;
				for(int i2 = 0; i2 < _Atlas.elementsList.Count; i2++){
					if(Elements[i].SpriteName == _Atlas.elementsList[i2].name){
						Debug.LogWarning("Pro Flares - Ah its ok i Found it = "+Elements[i].SpriteName);
						Found = true;
						Elements[i].elementTextureID = i2;
						for(int f = 0; f < FlareBatches.Length; f++){ 
 								FlareBatches[f].dirty = true;
						}
					}
				}
				if(!Found){
					Debug.LogError("Pro Flares - NOOOO.... It must of been deleted, resetting to first in list. :( -" +Elements[i].SpriteName,gameObject);
					Elements[i].elementTextureID = 0;
					Elements[i].SpriteName = _Atlas.elementsList[0].name;
					for(int f = 0; f < FlareBatches.Length; f++){ 
							FlareBatches[f].dirty = true;
					}
				}
			}
		}
	}
#endif
	
	
	
	void Update () {
		if(!Initialised){
			Init();
		}
	}
	
	void OnDrawGizmos() {
		Gizmos.color = GlobalTintColor;
		Gizmos.DrawSphere(transform.position,0.1f);
		//Gizmos.DrawIcon(transform.position,"ProFlaresGizmo.tga",true);
	}
}
