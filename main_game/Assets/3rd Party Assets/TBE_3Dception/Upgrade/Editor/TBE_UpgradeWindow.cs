using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class TBE_UpgradeWindow : EditorWindow 
{	
	TBE_UpgradeUtils UpgradeUtil;
	string SourceComponentNames = string.Empty;

	int numOfFilterComponents = 0;
	int numOfRoomComponents = 0;
	int numOfSourceComponents = 0;
	int numOfAmbiComponents = 0;

	bool bUpgradable = false;

	static Vector2 windowSize = new Vector2 (850, 550);

	public static TBE_UpgradeWindow instance { get; private set; }

	[MenuItem ("3Dception/Upgrade to Unity 5.2")]
	static void Init () 
	{	
		InitWithoutScriptEnable ();

		if (instance.bUpgradable) 
		{	
			instance.UpgradeUtil = new TBE_UpgradeUtils();
			instance.UpgradeUtil.enableUpgradeScripts();
		}
	}

	static void InitWithoutScriptEnable () 
	{	
		instance = (TBE_UpgradeWindow) EditorWindow.GetWindow (typeof(TBE_UpgradeWindow));
		instance.Show ();
		instance.titleContent = new GUIContent("Upgrade");
		instance.minSize = windowSize;
		
		instance.bUpgradable = File.Exists ("Assets/TBE_3Dception/Core/TBE_3DCore.dll");
	}

	void OnGUI ()
	{	
		EditorGUILayout.Space ();

		GUILayout.Label ("3Dception — Upgrade", EditorStyles.boldLabel);

		if (instance == null) 
		{	
			InitWithoutScriptEnable();
			instance.UpgradeUtil = new TBE_UpgradeUtils();
			instance.Repaint();
		}

		if (EditorApplication.isCompiling) 
		{
			EditorGUILayout.LabelField ("Loading.. Waiting for Unity to finish compiling..", EditorStyles.boldLabel);
			instance.Repaint();
			return;
		}

		if (bUpgradable) {
			EditorGUILayout.LabelField ("This script will help you upgrade an existing project with 3Dception 1.1 and prior. Only game objects within your project will be upgraded. Any 3Dception APIs accessed via scripts will need to be manually updated. \n", EditorStyles.wordWrappedLabel);
			EditorGUILayout.LabelField ("CAUTION: MAKE SURE YOU HAVE A BACKUP OF YOUR PROJECT!", EditorStyles.boldLabel);
			
			EditorGUILayout.Space ();
			
			GUILayout.Label ("Step1 — Project Settings", EditorStyles.boldLabel);
			
			EditorGUILayout.Space ();
			
			EditorGUILayout.LabelField ("Go to Edit > Project Settings > Audio in the menu bar. \n\n" +
				"1) Under the 'Spatialiser Plugin' drop-down menu, choose '3Dception'\n" +
				"2) Change the 'DSP Buffer Size' drop-down menu to 'Good Latency' or 'Best Performance'.\n", EditorStyles.wordWrappedLabel);
			
			GUILayout.Label ("Step2 — Scene Settings", EditorStyles.boldLabel);
			
			EditorGUILayout.Space ();
			
			EditorGUILayout.LabelField ("Run steps 2 and 3 for EVERY scene in your project.\n\n" +
				"Click 'Update Scene Settings' to update global 3Dception settings for this scene.", EditorStyles.wordWrappedLabel);
			
			EditorGUILayout.Space ();
			
			if (GUILayout.Button ("Update Scene Settings")) {	
				instance.UpgradeUtil.upgradeSceneSettings ();
				EditorApplication.MarkSceneDirty();
			}
			
			EditorGUILayout.Space ();

			GUILayout.Label ("Step3 — TBE_Source/TBE_Filter/TBE_AmbiArray/TBE_Room", EditorStyles.boldLabel);
			
			EditorGUILayout.Space ();
			
			EditorGUILayout.LabelField ("Click 'Find Objects'. If there any instances of TBE_Source/Filter/AmbiArray/Room in your scene, click 'Upgrade Found Objects'.\n", EditorStyles.wordWrappedLabel);
			
			if (GUILayout.Button ("Find Objects")) {	
				instance.SourceComponentNames = instance.UpgradeUtil.findSourceComponents (out instance.numOfSourceComponents);
				instance.SourceComponentNames += "\n";
				instance.SourceComponentNames += instance.UpgradeUtil.findFilterComponents (out instance.numOfFilterComponents);
				instance.SourceComponentNames += "\n";
				instance.SourceComponentNames += instance.UpgradeUtil.findAmbiComponents (out instance.numOfAmbiComponents);
				instance.SourceComponentNames += "\n";
				instance.SourceComponentNames += instance.UpgradeUtil.findRoomComponents (out instance.numOfRoomComponents);
			}
			
			if (instance != null && !string.IsNullOrEmpty (instance.SourceComponentNames)) {
				EditorGUILayout.HelpBox (instance.SourceComponentNames, MessageType.None);

				if ((instance.numOfSourceComponents > 0 || instance.numOfFilterComponents > 0 
					|| instance.numOfAmbiComponents > 0 || instance.numOfRoomComponents > 0) 
					&& GUILayout.Button ("Upgrade Found Objects")) 
				{	
					instance.UpgradeUtil.updateRoomComponents ();
					instance.UpgradeUtil.updateSourceComponents ();
					instance.UpgradeUtil.updateFilterComponents ();
					instance.UpgradeUtil.updateAmbiComponents ();
					instance.SourceComponentNames = string.Empty;
					EditorApplication.MarkSceneDirty();
				}
			}

			EditorGUILayout.Space ();

			GUILayout.Label ("Step4 — Clean Project", EditorStyles.boldLabel);
			
			EditorGUILayout.Space ();
			
			EditorGUILayout.LabelField ("Once you have run steps 2 and 3 in all scenes in your project, click 'Clean Project' to remove old and unused files.\n", EditorStyles.wordWrappedLabel);
			
			if (GUILayout.Button ("Clean Project")) {	
				instance.UpgradeUtil.cleanProject ();
				AssetDatabase.Refresh ();
				EditorApplication.MarkSceneDirty();
			}
		}
		else 
		{
			EditorGUILayout.Space ();
			
			GUILayout.Label ("Nothing to upgrade in this project!", EditorStyles.boldLabel);
		}
	}

	void OnDestroy()
	{
		instance = null;
	}

}
