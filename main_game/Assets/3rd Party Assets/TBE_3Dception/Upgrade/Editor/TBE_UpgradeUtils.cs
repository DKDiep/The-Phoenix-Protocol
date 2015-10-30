/* 
 * Copyright (c) Two Big Ears Ltd., 2015
 * support@twobigears.com
 */

using UnityEngine;
using UnityEditor;
using System.Collections;

public class TBE_UpgradeUtils  
{

#if TBE_UPGRADE
	TBE_3DCore.TBE_Source[] AllSources_ = null;
	TBE_3DCore.TBE_Filter[] AllFilters_ = null;
	TBE_3DCore.TBE_RoomProperties[] AllRooms_ = null;
	TBE_3DCore.TBE_AmbiArray[] AllAmbiArray_ = null;
#endif

	public void enableUpgradeScripts()
	{
		BuildTargetGroup targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

		PlayerSettings.SetScriptingDefineSymbolsForGroup (targetGroup, "TBE_UPGRADE");
	}

	public void disableUpgradeScripts()
	{
		BuildTargetGroup targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
		
		PlayerSettings.SetScriptingDefineSymbolsForGroup (targetGroup, "NTBE_UPGRADE");
	}

	public void upgradeSceneSettings()
	{
#if TBE_UPGRADE
		Debug.Log("---- Updating Scene Settings ----");
		
		GameObject GlobalObj = GameObject.Find("3Dception Global");
		if (GlobalObj != null)
		{
			GameObject.DestroyImmediate(GlobalObj, true);
			Debug.Log ("Found and deleted 3Dception Global");
		}
		else
		{
			Debug.Log ("Did not find 3Dception Global");
		}
		
		GameObject EnvObj = GameObject.Find("3Dception Environment");
		if (EnvObj != null)
		{
			GameObject.DestroyImmediate(EnvObj, true);
			Debug.Log ("Found and deleted 3Dception Environment");
		}
		else
		{
			Debug.Log ("Did not find 3Dception Environment");
		}
		
		TBE.Utilities.TBE_SetupDiagnose.createGlobalObject();
		
		Debug.Log ("Created new 3Dception Global object");
		
		TBE_3DCore.TBE_GlobalListener[] GlobalListeners = GameObject.FindObjectsOfType<TBE_3DCore.TBE_GlobalListener>();
		
		foreach (TBE_3DCore.TBE_GlobalListener instance in GlobalListeners) 
		{
			Debug.Log ("Removing 3Dception Global Listener on " + instance.gameObject + " (no longer required)");
			GameObject.DestroyImmediate(instance, true);
		}
#endif
	}

	public string findSourceComponents(out int number)
	{
		number = 0;
#if TBE_UPGRADE

		string SourceObjectNames = string.Empty;
		AllSources_ = Resources.FindObjectsOfTypeAll(typeof(TBE_3DCore.TBE_Source)) as TBE_3DCore.TBE_Source[];

		number = AllSources_.Length;
		SourceObjectNames = AllSources_.Length + " instances of TBE_Source found on GameObjects: ";
		
		foreach (TBE_3DCore.TBE_Source instance in AllSources_) 
		{
			SourceObjectNames += instance.gameObject.name + ", ";
		}

		return SourceObjectNames;
#else
		return string.Empty;
#endif
	}

	public void updateSourceComponents()
	{
#if TBE_UPGRADE
		int counter = 0;
		foreach (TBE_3DCore.TBE_Source instance in AllSources_) 
		{	
			GameObject GameObj = instance.gameObject;
			EditorUtility.DisplayProgressBar("Updating instances of TBE_Source", GameObj.name,  counter / AllSources_.Length);
			
			Debug.Log("---- " + GameObj.name + " ----");
			
			TBE.Components.TBE_SourceControl SourceControl = GameObj.AddComponent<TBE.Components.TBE_SourceControl>();
			SourceControl.enabled = instance.enabled;
			SourceControl.attenuationMode = (TBE.TBEAttenMode) instance.attenuationMode;
			SourceControl.roomToggle = instance.roomToggle;
			SourceControl.autoDetectRoom = instance.detectRoomTrigger;
			GameObject tmpRoom = instance.erRoomObject;
			if(tmpRoom)
			{
				SourceControl.erRoomObject = tmpRoom;
			}
			SourceControl.dopplerIntensity = instance.dopplerIntensity;
			SourceControl.dopplerToggle = instance.dopplerToggle;
			SourceControl.maxDistanceMute = instance.maxDistanceMute;
			SourceControl.maximumDistance = instance.maximumDistance;
			SourceControl.minimumDistance = instance.minimumDistance;
			SourceControl.rollOffFactor = instance.rollOffFactor;
			SourceControl.roomPannerType = (TBE.TBEPannerType) instance.roomPannerType;
			SourceControl.selectChannel = (TBE.TBEChannel) instance.selectChannel;
			SourceControl.sourcePannerType = (TBE.TBEPannerType) instance.sourcePannerType;
			
			Debug.Log("Added new TBE_SourceControl and updated settings");
			
			AudioSource Source = GameObj.GetComponent<AudioSource>();
			Source.clip = instance.clip;
			Source.loop = instance.loop;
			Source.mute = instance.mute;
			Source.pitch = instance.pitch;
			Source.playOnAwake = instance.playOnAwake;
			Source.bypassReverbZones = !instance.reverbToggle;
			Source.volume = instance.volume;
			Source.spatialize = true;

			Debug.Log("Updated AudioSource settings");
			
			GameObject.DestroyImmediate(instance, true);
			
			Debug.Log("Removed TBE_Source");
			
			counter++;
		}
		AllSources_ = null;
		EditorUtility.ClearProgressBar();
#endif
	}

	public string findFilterComponents(out int number)
	{
		number = 0;
		#if TBE_UPGRADE

		string FilterObjectNames = string.Empty;
		AllFilters_ = Resources.FindObjectsOfTypeAll(typeof(TBE_3DCore.TBE_Filter)) as TBE_3DCore.TBE_Filter[];

		number = AllFilters_.Length;
		FilterObjectNames = AllFilters_.Length + " instances of TBE_Filter found on GameObjects: ";
		
		foreach (TBE_3DCore.TBE_Filter instance in AllFilters_) 
		{
			FilterObjectNames += instance.gameObject.name + ", ";
		}
		
		return FilterObjectNames;
		#else
		return string.Empty;
		#endif
	}

	public void updateFilterComponents()
	{
		#if TBE_UPGRADE
		int counter = 0;
		foreach (TBE_3DCore.TBE_Filter instance in AllFilters_) 
		{	
			GameObject GameObj = instance.gameObject;
			EditorUtility.DisplayProgressBar("Updating instances of TBE_Filter", GameObj.name,  counter / AllFilters_.Length);
			
			Debug.Log("---- " + GameObj.name + " ----");
			
			TBE.Components.TBE_SourceControl SourceControl = GameObj.AddComponent<TBE.Components.TBE_SourceControl>();
			SourceControl.attenuationMode = (TBE.TBEAttenMode) instance.attenuationMode;
			SourceControl.autoDetectRoom = instance.detectRoomTrigger;
			SourceControl.dopplerIntensity = instance.dopplerIntensity;
			SourceControl.dopplerToggle = instance.dopplerToggle;
			SourceControl.enabled = instance.enabled;
			SourceControl.erRoomObject = instance.erRoomObject;
			SourceControl.maxDistanceMute = instance.maxDistanceMute;
			SourceControl.maximumDistance = instance.maximumDistance;
			SourceControl.minimumDistance = instance.minimumDistance;
			SourceControl.rollOffFactor = instance.rollOffFactor;
			SourceControl.roomPannerType = (TBE.TBEPannerType) instance.roomPannerType;
			SourceControl.roomToggle = instance.roomToggle;
			SourceControl.selectChannel = (TBE.TBEChannel) instance.selectChannel;
			SourceControl.sourcePannerType = (TBE.TBEPannerType) instance.sourcePannerType;
			
			Debug.Log("Added new TBE_SourceControl and updated settings");
			
			AudioSource Source = GameObj.GetComponent<AudioSource>();
			Source.spatialize = true;
			
			Debug.Log("Updated AudioSource settings");
			
			GameObject.DestroyImmediate(instance, true);
			
			Debug.Log("Removed TBE_Source");
			
			counter++;
		}
		AllFilters_ = null;
		EditorUtility.ClearProgressBar();
		#endif
	}

	public string findRoomComponents(out int number)
	{	
		number = 0;
		#if TBE_UPGRADE

		string RoomObjectNames = string.Empty;
		AllRooms_ = Resources.FindObjectsOfTypeAll(typeof(TBE_3DCore.TBE_RoomProperties)) as TBE_3DCore.TBE_RoomProperties[];

		number = AllRooms_.Length;
		RoomObjectNames = AllRooms_.Length + " instances of TBE_Room found on GameObjects: ";
		
		foreach (TBE_3DCore.TBE_RoomProperties instance in AllRooms_) 
		{
			RoomObjectNames += instance.gameObject.name + ", ";
		}
		
		return RoomObjectNames;
		#else
		return string.Empty;
		#endif
	}

	public void updateRoomComponents()
	{
		#if TBE_UPGRADE
		int counter = 0;
		foreach (TBE_3DCore.TBE_RoomProperties instance in AllRooms_) 
		{	
			GameObject GameObj = instance.gameObject;
			EditorUtility.DisplayProgressBar("Updating instances of TBE_Room", GameObj.name,  counter / AllRooms_.Length);
			
			Debug.Log("---- " + GameObj.name + " ----");
			
			TBE.Components.TBE_RoomProperties NewRoomProps = GameObj.AddComponent<TBE.Components.TBE_RoomProperties>();
			NewRoomProps.diffuseZone = instance.diffuseZone;
			NewRoomProps.erLevel = instance.erLevel;
			NewRoomProps.hfReflections = instance.hfReflections;
			NewRoomProps.pivotPoint = instance.pivotPoint;
			NewRoomProps.reflectionBWall = instance.reflectionBWall;
			NewRoomProps.reflectionCeiling = instance.reflectionCeiling;
			NewRoomProps.reflectionFloor = instance.reflectionFloor;
			NewRoomProps.reflectionFWall = instance.reflectionFWall;
			NewRoomProps.reflectionLWall = instance.reflectionLWall;
			NewRoomProps.reflectionRWall = instance.reflectionRWall;
			NewRoomProps.roomPreset = (TBE.TBRoomPresets) instance.roomPreset;
			NewRoomProps.showGuides = instance.showGuides;
			
			Debug.Log("Added new TBE_RoomProperties and updated settings");

			GameObject.DestroyImmediate(instance, true);
			
			Debug.Log("Removed old TBE_RoomProperties");
			
			counter++;
		}
		AllRooms_ = null;
		EditorUtility.ClearProgressBar();
		#endif
	}

	public string findAmbiComponents(out int number)
	{	
		number = 0;
		#if TBE_UPGRADE

		string AmbiObjectNames = string.Empty;
		AllAmbiArray_ = Resources.FindObjectsOfTypeAll(typeof(TBE_3DCore.TBE_AmbiArray)) as TBE_3DCore.TBE_AmbiArray[];

		number = AllAmbiArray_.Length;
		AmbiObjectNames = AllAmbiArray_.Length + " instances of TBE_AmbiArray found on GameObjects: ";
		
		foreach (TBE_3DCore.TBE_AmbiArray instance in AllAmbiArray_) 
		{
			AmbiObjectNames += instance.gameObject.name + ", ";
		}
		
		return AmbiObjectNames;
		#else
		return string.Empty;
		#endif
	}

	public void updateAmbiComponents()
	{
		#if TBE_UPGRADE
		int counter = 0;
		foreach (TBE_3DCore.TBE_AmbiArray instance in AllAmbiArray_) 
		{	
			GameObject GameObj = instance.gameObject;
			EditorUtility.DisplayProgressBar("Updating instances of TBE_AmbiArray", GameObj.name,  counter / AllAmbiArray_.Length);
			
			Debug.Log("---- " + GameObj.name + " ----");


			TBE.Components.TBE_AmbiArray NewAmbiArray = GameObj.AddComponent<TBE.Components.TBE_AmbiArray>();
			NewAmbiArray.clipWX = instance.clipWX;
			NewAmbiArray.clipYZ = instance.clipYZ;
			NewAmbiArray.enabled = instance.enabled;
			NewAmbiArray.loop = instance.loop;
			NewAmbiArray.mute = instance.mute;
			NewAmbiArray.pitch = instance.pitch;
			NewAmbiArray.playOnAwake = instance.playOnAwake;
			NewAmbiArray.bypassListenerEffects = instance.bypassListenerEffects;
			NewAmbiArray.reverbToggle = instance.reverbToggle;
			NewAmbiArray.volume = instance.volume;
			
			Debug.Log("Added new TBE_AmbiArray and updated settings");
			
			GameObject.DestroyImmediate(instance, true);
			
			Debug.Log("Removed old TBE_AmbiArray");
			
			counter++;
		}
		AllAmbiArray_ = null;
		EditorUtility.ClearProgressBar();
		#endif
	}

	public void cleanProject()
	{
		disableUpgradeScripts ();

		tryDelete ("Assets/TBE_3Dception/Plugins/x86/tbe_3Dception.dll");
		tryDelete ("Assets/TBE_3Dception/Plugins/x86/libtbe_3Dception.so");
		tryDelete ("Assets/TBE_3Dception/Plugins/x86_64/tbe_3Dception.dll");
		tryDelete ("Assets/TBE_3Dception/Plugins/x86_64/libtbe_3Dception.so");
		tryDelete ("Assets/TBE_3Dception/Plugins/tbe_3Dception.bundle");
		tryDelete ("Assets/TBE_3Dception/Plugins/Android/armeabi-v7a/libtbe_3Dception.so");
		tryDelete ("Assets/TBE_3Dception/Plugins/iOS/libtbe_3Dception_ios.a");
		tryDelete ("Assets/TBE_3Dception/Core");
		tryDelete ("Assets/TBE_3Dception/Editor");
		tryDelete ("Assets/TBE_3Dception/Resources");
		tryDelete ("Assets/TBE_3Dception/TBE_Room.prefab");
		tryDelete ("Assets/TBE_3Dception/Utilities/Editor/TBE_Diagnose.cs");

		AssetDatabase.Refresh ();
	}

	void tryDelete(string path)
	{
		bool deleted = FileUtil.DeleteFileOrDirectory(path);
		
		if (deleted) 
		{
			Debug.Log ("3Dception Upgrade: Deleted " + path);
		} 
		else 
		{
			//TODO: check if plugin has been deleted successfully or not
		}
	}
}
