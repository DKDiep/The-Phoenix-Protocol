/* Diagnose and setup 3Dception
 * Setup: creates 3Dception Global object, sets script execution order
 * Diagnose: Check if plugin paths are correct and if plugins exist. Also checks 
 * 			if the native plugin and the unity plugin are compatible.
 * 
 * Copyright (c) Two Big Ears Ltd., 2015
 * support@twobigears.com
 */

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace TBE
{
	namespace Utilities
	{
		public class TBE_SetupDiagnose : EditorWindow 
		{
			string windows32Status = string.Empty;
			string windows64Status = string.Empty;
			string linux32Status = string.Empty;
			string linux64Status = string.Empty;
			string osxStatus = string.Empty;
			string iosStatus = string.Empty;
			string androidStatus = string.Empty;
			string versionStatus = string.Empty;
			
			bool toggleGlobalObject = true;
			bool toggleScriptExecOrder = true;
			
			static Vector2 windowSize = new Vector2 (300, 600);
			
			public static TBE_SetupDiagnose instance { get; private set; }
			
			[MenuItem ("3Dception/Setup And Diagnostics")]
			static void Init () 
			{
				// Get existing open window or if none, make a new one:		
				instance = (TBE_SetupDiagnose)EditorWindow.GetWindow (typeof(TBE_SetupDiagnose));
				instance.ShowUtility ();
				instance.titleContent = new GUIContent("Setup/Diagnose");
				instance.minSize = windowSize;
				instance.maxSize = windowSize;
			}
			
			void OnGUI ()
			{	
				EditorGUILayout.Space ();
				
				GUILayout.Label ("3Dception — Project Setup", EditorStyles.boldLabel);
				
				EditorGUILayout.LabelField ("Chose the options below and click on 'Setup Scene' to automatically setup your scene.\n\n" +
				                            "The 3Dception Global object includes an initialisation and environment components, for controlling global environment properties .\n\n" +
				                            "'Set Execution Order' ensures the various components for 3Dception are executed in the correct order.\n", EditorStyles.wordWrappedLabel);
				
				toggleGlobalObject = GUILayout.Toggle (toggleGlobalObject, "Create 3Dception Global & Environment Object");
				
				toggleScriptExecOrder = GUILayout.Toggle (toggleScriptExecOrder, "Set Execution Order");
				
				if(GUILayout.Button("Setup Scene"))
				{
					if (toggleGlobalObject)
					{
						createGlobalObject();
					}

					if (toggleScriptExecOrder)
					{
						setScriptExecOrderForAll();
					}
				}
				
				EditorGUILayout.Space ();
				EditorGUILayout.Space ();
				
				GUILayout.Label ("3Dception — Diagnostics", EditorStyles.boldLabel);
				
				EditorGUILayout.LabelField ("Click on 'Run Diagnostics' if you have upgraded Unity or 3Dception to a new version to check if everything is okay.", EditorStyles.wordWrappedLabel);
				
				if(GUILayout.Button("Run Diagnostics"))
				{
					checkPluginStructure();
					checkVersionMatch();
				}
				
				EditorGUILayout.HelpBox (windows32Status + "\n" +
				                         windows64Status + "\n" +
				                         osxStatus + "\n" +
				                         iosStatus + "\n" +
				                         androidStatus + "\n" +
				                         linux32Status + "\n" +
				                         linux64Status + "\n" +
				                         versionStatus
				                         , MessageType.None);
			}
			
			void checkPluginStructure()
			{
				
				string unityVersion = Application.unityVersion;
				bool isVersion5 = unityVersion.StartsWith("5.2");
				
				if (isVersion5) 
				{	
					checkExists("Assets/TBE_3Dception/Plugins/x86/AudioPlugin3Dception.dll", "Windows 32", out windows32Status);
					checkExists("Assets/TBE_3Dception/Plugins/x86_64/AudioPlugin3Dception.dll", "Windows 64", out windows64Status);
					checkExists("Assets/TBE_3Dception/Plugins/x86/libAudioPlugin3Dception.so", "Linux 32", out linux32Status);
					checkExists("Assets/TBE_3Dception/Plugins/x86_64/libAudioPlugin3Dception.so", "Linux 64", out linux64Status);
					checkExists("Assets/TBE_3Dception/Plugins/AudioPlugin3Dception.bundle/Contents/MacOS/AudioPlugin3Dception", "OSX", out osxStatus);
					checkExists("Assets/TBE_3Dception/Plugins/iOS/libAudioPlugin3Dception_iOS.a", "iOS", out iosStatus);
					checkExists("Assets/TBE_3Dception/Plugins/Android/libAudioPlugin3Dception.so", "Android", out androidStatus);
				} 
				else 
				{
					// TODO warning of wrong unity version?
				}
			}
			
			void tryDelete(string path)
			{
				bool deleted = FileUtil.DeleteFileOrDirectory(path);
				
				if (deleted) {
					Debug.Log ("3Dception Diagnostics: Deleted " + path);
				} 
				else 
				{
					//TODO: check if plugin has been deleted successfully or not
				}
			}
			
			void checkExists(string path, string platform, out string uiMessage)
			{
				if (File.Exists (path))
				{
					uiMessage = platform + ": Plugin Available";
				} 
				else 
				{
					uiMessage = platform + ": Plugin Unavailable";
				}
			}
			
			void checkVersionMatch()
			{	
				string coreVersionString = TBE.Native.Engine.getVersionMajor () + "." + TBE.Native.Engine.getVersionMinor () + "." + TBE.Native.Engine.getVersionPatch ();
				if (coreVersionString.Equals (TBE.Constants.EXPECTED_CORE_VERSION)) 
				{
					versionStatus = "3Dception version is correct, all good!";
				}
				else
				{
					versionStatus = "3Dception version is incorrect, restart Unity and re-import the package";
				}
			}
			
			public static void createGlobalObject()
			{
				GameObject tbeGlobal = GameObject.Find ("3Dception Global");

				if (tbeGlobal != null) 
				{
					DestroyImmediate (tbeGlobal);
				}

				if (tbeGlobal == null) 
				{	
					// Create 3Dception Global object with components
					tbeGlobal = new GameObject("3Dception Global");
					tbeGlobal.AddComponent<TBE.Components.TBE_Initialise>();
					tbeGlobal.AddComponent<TBE.Components.TBE_InitialiseRoomModelling>();
					tbeGlobal.AddComponent<TBE.Components.TBE_Environment>();
				}
			}
			
			void setScriptExecOrderForAll()
			{
				setScriptExecutionOrder(typeof(TBE.Components.TBE_Initialise).Name, -400);
				setScriptExecutionOrder(typeof(TBE.Components.TBE_InitialiseRoomModelling).Name, -300);
				setScriptExecutionOrder(typeof(TBE.Components.TBE_Environment).Name, -250);
				setScriptExecutionOrder(typeof(TBE.Components.TBE_RoomProperties).Name, -200);
			}
			
			void setScriptExecutionOrder(string className, int order)
			{
				foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts())
				{
					if (monoScript.name == className)
					{	
						MonoImporter.SetExecutionOrder(monoScript, order);
						break;
					}
				}
			}
		}
	}
}


