using System;
using System.Collections;
using CodeStage.AdvancedFPSCounter.CountersData;
using CodeStage.AdvancedFPSCounter.Labels;

namespace CodeStage.AdvancedFPSCounter
{
	using UnityEngine;

	/// <summary>
	/// Allows to see frames per second counter, memory usage counter and some simple hardware information right in running app on any device.<br/>
	/// Just call AFPSCounter.AddToScene() to use it.
	/// </summary>
	/// You also may add it to GameObject (without any child or parent objects, with zero rotation, zero position and 1,1,1 scale) as usual or through the<br/>
	/// "GameObject > Create Other > Code Stage > Advanced FPS Counter" menu.
	[DisallowMultipleComponent]
	public class AFPSCounter: MonoBehaviour
	{
		private const string CONTAINER_NAME = "Advanced FPS Counter";

		//internal static string NEW_LINE = Environment.NewLine;
		internal const char NEW_LINE = '\n';
		internal const char SPACE = ' ';

		/// <summary>
		/// Allows reaching public properties from code. Can be null.
		/// \sa AddToScene()
		/// </summary>
		public static AFPSCounter Instance { get; private set; }

		/// <summary>
		/// Frames Per Second counter.
		/// </summary>
		public FPSCounterData fpsCounter = new FPSCounterData();

		/// <summary>
		/// Mono or heap memory counter.
		/// </summary>
		public MemoryCounterData memoryCounter = new MemoryCounterData();

		/// <summary>
		/// Device hardware info.<br/>
		/// Shows CPU name, cores (threads) count, GPU name, total VRAM, total RAM, screen DPI and screen size.
		/// </summary>
		public DeviceInfoCounterData deviceInfoCounter = new DeviceInfoCounterData();

		/// <summary>
		/// Used to enable / disable plugin at runtime. Set to KeyCode.None to disable.
		/// </summary>
		public KeyCode hotKey = KeyCode.BackQuote;

		/// <summary>
		/// Allows to keep Advanced FPS Counter game object on new level (scene) load.
		/// </summary>
		public bool keepAlive = true;

		private bool obsoleteEnabled = true;

		[SerializeField]
		private OperationMode operationMode = OperationMode.Normal;

		[SerializeField]
		private bool forceFrameRate;

		[SerializeField]
		[Range(-1, 200)]
		private int forcedFrameRate = -1;

		[SerializeField]
		private Vector2 anchorsOffset = new Vector2(5,5);

		[SerializeField]
		private Font labelsFont;

		[SerializeField]
		[Range(0, 100)]
		private int fontSize;

		[SerializeField]
		[Range(0f, 10f)]
		private float lineSpacing = 1;

		[SerializeField]
		[Range(0, 10)]
		private int countersSpacing = 0;

		internal DrawableLabel[] labels;

		private int anchorsCount;
		private int cachedVSync = -1;
		private int cachedFrameRate = -1;
	    private bool inited;

#region editor stuff
#if UNITY_EDITOR
		[HideInInspector]
		[SerializeField]
		private bool fpsGroupToggle;
		
		[HideInInspector]
		[SerializeField]
		private bool memoryGroupToggle;

		[HideInInspector]
		[SerializeField]
		private bool deviceGroupToggle;

		[HideInInspector]
		[SerializeField]
		private bool lookAndFeelToggle;

		private const string MENU_PATH = "GameObject/Create Other/Code Stage/Advanced FPS Counter %&F";

		[UnityEditor.MenuItem(MENU_PATH, false)]
		private static void AddToSceneInEditor()
		{
			AFPSCounter counter = FindObjectOfType<AFPSCounter>();
			if (counter != null)
			{
				if (counter.IsPlacedCorrectly())
				{
					if (UnityEditor.EditorUtility.DisplayDialog("Remove Advanced FPS Counter?", "Advanced FPS Counter already exists in scene and placed correctly. Dou you wish to remove it?", "Yes", "No"))
					{
						DestroyInEditorImmediate(counter);
					}
				}
				else
				{
					if (counter.MayBePlacedHere())
					{
						int dialogResult = UnityEditor.EditorUtility.DisplayDialogComplex("Fix existing Game Object to work with Adavnced FPS Counter?", "Advanced FPS Counter already exists in scene and placed onto  Game Object \"" + counter.name + "\".\nDo you wish to let plugin configure and use this Game Object further? Press Delete to remove plugin from scene at all.", "Fix", "Delete", "Cancel");

						switch (dialogResult)
						{
							case 0:
								counter.FixCurrentGameObject();
								break;
							case 1:
								DestroyInEditorImmediate(counter);
								break;
						}
					}
					else
					{
						int dialogResult = UnityEditor.EditorUtility.DisplayDialogComplex("Move existing Adavnced FPS Counter to own Game Object?", "Looks like Advanced FPS Counter plugin already exists in scene and placed incorrectly on Game Object \"" + counter.name + "\".\nDo you wish to let plugin move itself onto separate configured Game Object \"" + CONTAINER_NAME + "\"? Press Delete to remove plugin from scene at all.", "Move", "Delete", "Cancel");
						switch (dialogResult)
						{
							case 0:
								GameObject go = new GameObject(CONTAINER_NAME);
								UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create " + CONTAINER_NAME);
								UnityEditor.Selection.activeObject = go;
								AFPSCounter newCounter = go.AddComponent<AFPSCounter>();
								UnityEditor.EditorUtility.CopySerialized(counter, newCounter);
								DestroyInEditorImmediate(counter);
								break;
							case 1:
								DestroyInEditorImmediate(counter);
								break;
						}
					}
				}
			}
			else
			{
				GameObject container = GameObject.Find(CONTAINER_NAME);
				if (container == null)
				{
					container = new GameObject(CONTAINER_NAME);
					UnityEditor.Undo.RegisterCreatedObjectUndo(container, "Create " + CONTAINER_NAME);
					UnityEditor.Selection.activeObject = container;
				}
				container.AddComponent<AFPSCounter>();

				UnityEditor.EditorUtility.DisplayDialog("Adavnced FPS Counter added!", "Adavnced FPS Counter successfully added to the object \"" + CONTAINER_NAME + "\"", "OK");
			}

		}

		private bool MayBePlacedHere()
		{
			return (transform.childCount == 0 &&
					transform.parent == null);
		}

		private static void DestroyInEditorImmediate(AFPSCounter component)
		{
			if (component.transform.childCount == 0 && component.GetComponentsInChildren<Component>(true).Length <= 2)
			{
				DestroyImmediate(component.gameObject);
			}
			else
			{
				DestroyImmediate(component);
			}
		}
#endif
#endregion

		// добавить проверки в редакторе на положение и т.д.
		private void FixCurrentGameObject()
		{
			gameObject.name = CONTAINER_NAME;
			transform.position = Vector3.zero;
			transform.rotation = Quaternion.identity;
			transform.localScale = Vector3.one;
			tag = "Untagged";
			gameObject.layer = 0;
			gameObject.isStatic = false;
		}

		private static AFPSCounter GetOrCreateInstance
		{
			get
			{
				if (Instance == null)
				{
					AFPSCounter counter = FindObjectOfType<AFPSCounter>();
					if (counter != null)
					{
						Instance = counter;
					}
					else
					{
						GameObject go = new GameObject(CONTAINER_NAME);
						go.AddComponent<AFPSCounter>();
					}
				}
				return Instance;
			}
		}

		/// <summary>
		/// Creates and adds new %AFPSCounter instance to the scene if it doesn't exists.
		/// Use it to instantiate %AFPSCounter from code before using AFPSCounter.Instance.
		/// </summary>
		/// <returns>Existing or new %AFPSCounter instance.</returns>
		public static AFPSCounter AddToScene()
		{
			return GetOrCreateInstance;
		}

		/// <summary>
		/// Use it to completely dispose current %AFPSCounter instance.
		/// </summary>
		public static void Dispose()
		{
			if (Instance != null) Instance.DisposeInternal();
		}

		private void DisposeInternal()
		{
			Destroy(this);
			if (Instance == this) Instance = null;
		}

		/// <summary>
		/// Use it to change %AFPSCounter operation mode.
		/// </summary>
		/// Disabled: removes labels and stops all internal processes except Hot Key listener.<br/>
		/// Background: removes labels keeping counters alive. May be useful for hidden performance monitoring and benchmarking. Hot Key has no effect in this mode.<br/>
		/// Normal: shows labels and runs all internal processes as usual.
		public OperationMode OperationMode
		{
			get { return operationMode; }
			set
			{
				if (operationMode == value || !Application.isPlaying) return;
				operationMode = value;

				if (operationMode != OperationMode.Disabled)
				{
					if (operationMode == OperationMode.Background)
					{
						for (int i = 0; i < anchorsCount; i++)
						{
							labels[i].Clear();
						}
					}

					OnEnable();

					fpsCounter.UpdateValue();
					memoryCounter.UpdateValue();
					deviceInfoCounter.UpdateValue();

					UpdateTexts();
				}
				else
				{
					OnDisable();
				}
			}
		}

		/// <summary>
		/// This is deprecated property. Use #OperationMode property instead.
		/// </summary>
		[Obsolete("Use AFPSCounter.Instance.OperationMode instead of AFPSCounter.Instance.enabled!")]
		public new bool enabled
		{
			get { return obsoleteEnabled; }
			set
			{
				if (obsoleteEnabled == value || !Application.isPlaying) return;
				obsoleteEnabled = value;

				if (obsoleteEnabled)
				{
					operationMode = OperationMode.Normal;
					OnEnable();
				}
				else
				{
					operationMode = OperationMode.Disabled;
					OnDisable();
				}
			}
		}

		/// <summary>
		/// Allows to see how your game performs on specified frame rate.<br/>
		/// <strong>\htmlonly<font color="7030A0">IMPORTANT:</font>\endhtmlonly this option disables VSync while enabled!</strong>
		/// </summary>
		/// Useful to check how physics performs on slow devices for example.
		public bool ForceFrameRate
		{
			get { return forceFrameRate; }
			set
			{
				if (forceFrameRate == value || !Application.isPlaying) return;
				forceFrameRate = value;
				if (operationMode == OperationMode.Disabled) return;

				RefreshForcedFrameRate();
			}
		}

		/// <summary>
		/// Desired frame rate for ForceFrameRate option, does not guarantee selected frame rate.
		/// Set to -1 to render as fast as possible in current conditions.
		/// </summary>
		public int ForcedFrameRate
		{
			get { return forcedFrameRate; }
			set
			{
				if (forcedFrameRate == value || !Application.isPlaying) return;
				forcedFrameRate = value;
				if (operationMode == OperationMode.Disabled) return;

				RefreshForcedFrameRate();
			}
		}


		/// <summary>
		/// Pixel offset for anchored labels. Automatically applied to all 4 corners.
		/// </summary>
		public Vector2 AnchorsOffset
		{
			get { return anchorsOffset; }
			set
			{
				if (anchorsOffset == value || !Application.isPlaying) return;
				anchorsOffset = value;
				if (operationMode == OperationMode.Disabled || labels == null) return;

				for (int i = 0; i < anchorsCount; i++)
				{
					labels[i].ChangeOffset(anchorsOffset);
				}
			}
		}

		/// <summary>
		/// Font to render labels with.
		/// </summary>
		public Font LabelsFont
		{
			get { return labelsFont; }
			set
			{
				if (labelsFont == value || !Application.isPlaying) return;
				labelsFont = value;
				if (operationMode == OperationMode.Disabled || labels == null) return;

				for (int i = 0; i < anchorsCount; i++)
				{
					labels[i].ChangeFont(labelsFont);
				}
			}
		}

		/// <summary>
		/// The font size to use (for dynamic fonts).
		/// </summary>
		/// If this is set to a non-zero value, the font size specified in the font importer is overridden with a custom size. This is only supported for fonts set to use dynamic font rendering. Other fonts will always use the default font size.
		public int FontSize
		{
			get { return fontSize; }
			set
			{
				if (fontSize == value || !Application.isPlaying) return;
				fontSize = value;
				if (operationMode == OperationMode.Disabled || labels == null) return;

				for (int i = 0; i < anchorsCount; i++)
				{
					labels[i].ChangeFontSize(fontSize);
				}
			}
		}

		/// <summary>
		/// Space between lines.
		/// </summary>
		public float LineSpacing
		{
			get { return lineSpacing; }
			set
			{
				if (Math.Abs(lineSpacing - value) < 0.001f || !Application.isPlaying) return;
				lineSpacing = value;
				if (operationMode == OperationMode.Disabled || labels == null) return;

				for (int i = 0; i < anchorsCount; i++)
				{
					labels[i].ChangeLineSpacing(lineSpacing);
				}
			}
		}
		
		/// <summary>
		/// Lines count between different counters in a single label.
		/// </summary>
		public int CountersSpacing
		{
			get { return countersSpacing; }
			set
			{
				if (Math.Abs(countersSpacing - value) < 0.001f || !Application.isPlaying) return;
				countersSpacing = value;
				if (operationMode == OperationMode.Disabled || labels == null) return;

				UpdateTexts();
				for (int i = 0; i < anchorsCount; i++)
				{
					labels[i].dirty = true;
				}
			}
		}

		// preventing direct instantiation =P
		private AFPSCounter() { }

#region Unity callbacks
		private void Awake()
		{
			if (Instance != null && Instance.keepAlive)
			{
				Destroy(this);
				return;
			}

			if (!IsPlacedCorrectly())
			{
				Debug.LogWarning("Advanced FPS Counter is placed in scene incorrectly and will be auto-destroyed! Please, use \"GameObject->Create Other->Code Stage->Advanced FPS Counter\" menu to correct this!");
				Destroy(this);
				return;
			}

			fpsCounter.Init(this);
			memoryCounter.Init(this);
			deviceInfoCounter.Init(this);

			Instance = this;
			DontDestroyOnLoad(gameObject);

			anchorsCount = Enum.GetNames(typeof(LabelAnchor)).Length;
			labels = new DrawableLabel[anchorsCount];

			for (int i = 0; i < anchorsCount; i++)
			{
				labels[i] = new DrawableLabel((LabelAnchor)i, anchorsOffset, labelsFont, fontSize, lineSpacing);
			}

			inited = true;
		}

		#region EditorChecks
#if UNITY_EDITOR
		private void Start()
		{


			Camera[] cameras = Camera.allCameras;
			int len = Camera.allCamerasCount;
			bool willRender = false;

			for (int i = 0; i < len; i++)
			{
				Camera cam = cameras[i];
				GUILayer guiLayer = cam.GetComponent<GUILayer>();
				if (guiLayer != null && guiLayer.enabled)
				{
					// checking if AFPSCounter's layer in the camera's culling mask
					if ((cam.cullingMask & (1 << gameObject.layer)) != 0)
					{
						willRender = true;
						break;
					}
				}
			}

			if (!willRender)
			{
				Debug.LogWarning("Please check you have at least one camera with enabled GUILayer and layer \"" + LayerMask.LayerToName(gameObject.layer) + "\" in the culling mask!");
			}

			if (transform.position != Vector3.zero)
			{
				Debug.LogWarning("AFPSCounter should be placed on Game Object with zero position!", gameObject);
			}

			if (transform.rotation != Quaternion.identity)
			{
				Debug.LogWarning("AFPSCounter should be placed on Game Object with zero rotation!", gameObject);
			}

			if (transform.localScale != Vector3.one)
			{
				Debug.LogWarning("AFPSCounter should be placed on Game Object with 1,1,1 scale!", gameObject);
			}
		}
#endif
		#endregion

		private void Update()
		{
			if (!inited) return;

			if (hotKey != KeyCode.None)
			{
				if (Input.GetKeyDown(hotKey))
				{
					SwitchCounter();
				}
			}
		}

		private void OnLevelWasLoaded(int index)
		{
			if (!inited) return;

			if (!keepAlive)
			{
				DisposeInternal();
			}
			else
			{
				if (fpsCounter.Enabled)
				{
					if (fpsCounter.ShowMinMax && fpsCounter.resetMinMaxOnNewScene) fpsCounter.ResetMinMax();
					if (fpsCounter.ShowAverage && fpsCounter.resetAverageOnNewScene) fpsCounter.ResetAverage();
				}
			}
		}

		private void OnEnable()
		{
			if (operationMode == OperationMode.Disabled) return;
			ActivateCounters();
			Invoke("RefreshForcedFrameRate", 0.5f);
		}

		private void OnDisable()
		{
			if (!inited) return;

			DeactivateCounters();
			if (IsInvoking("RefreshForcedFrameRate")) CancelInvoke("RefreshForcedFrameRate");
			RefreshForcedFrameRate(true);

			for (int i = 0; i < anchorsCount; i++)
			{
				labels[i].Clear();
			}
		}

		private void OnDestroy()
		{
			if (inited)
			{
				fpsCounter.Dispose();
				memoryCounter.Dispose();
				deviceInfoCounter.Dispose();

				if (labels != null)
				{
					for (int i = 0; i < anchorsCount; i++)
					{
						labels[i].Dispose();
					}

					Array.Clear(labels, 0, anchorsCount);
					labels = null;
				}
				inited = false;
			}

			if (transform.childCount == 0 && GetComponentsInChildren<Component>().Length <= 2)
			{
				Destroy(gameObject);
			}
		}
#endregion

		private bool IsPlacedCorrectly()
		{
			return (transform.childCount == 0 &&
					transform.parent == null);
		}


		internal void MakeDrawableLabelDirty(LabelAnchor anchor)
		{
			if (operationMode == OperationMode.Normal)
			{
				labels[(int)anchor].dirty = true;
			}
		}

		internal void UpdateTexts()
		{
			if (operationMode != OperationMode.Normal) return;

			bool anyContentPresent = false;

			if (fpsCounter.Enabled)
			{
				DrawableLabel label = labels[(int)fpsCounter.Anchor];
				if (label.newText.Length > 0)
				{
					label.newText.Append(new String(NEW_LINE, countersSpacing + 1));
				}
				label.newText.Append(fpsCounter.text);
				label.dirty |= fpsCounter.dirty;
				fpsCounter.dirty = false;

				anyContentPresent = true;
			}

			if (memoryCounter.Enabled)
			{
				DrawableLabel label = labels[(int)memoryCounter.Anchor];
				if (label.newText.Length > 0)
				{
					label.newText.Append(new String(NEW_LINE, countersSpacing + 1));
				}
				label.newText.Append(memoryCounter.text);
				label.dirty |= memoryCounter.dirty;
				memoryCounter.dirty = false;

				anyContentPresent = true;
			}

			if (deviceInfoCounter.Enabled)
			{
				DrawableLabel label = labels[(int)deviceInfoCounter.Anchor];
				if (label.newText.Length > 0)
				{
					label.newText.Append(new String(NEW_LINE, countersSpacing + 1));
				}
				label.newText.Append(deviceInfoCounter.text);
				label.dirty |= deviceInfoCounter.dirty;
				deviceInfoCounter.dirty = false;

				anyContentPresent = true;
			}

			if (anyContentPresent)
			{
				for (int i = 0; i < anchorsCount; i++)
				{
					labels[i].CheckAndUpdate();
				}
			}
			else
			{
				for (int i = 0; i < anchorsCount; i++)
				{
					labels[i].Clear();
				}
			}
		}

		private IEnumerator UpdateFPSCounter()
		{
			while (true)
			{
				float previousUpdateTime = Time.unscaledTime;
				int previousUpdateFrames = Time.frameCount;
				yield return new WaitForSeconds(fpsCounter.UpdateInterval);
				float timeElapsed = Time.unscaledTime - previousUpdateTime;
				int framesChanged = Time.frameCount - previousUpdateFrames;

				fpsCounter.newValue = framesChanged / timeElapsed;
				fpsCounter.UpdateValue(false);
				UpdateTexts();
			}
		}
		
		private IEnumerator UpdateMemoryCounter()
		{
			while (true)
			{
				memoryCounter.UpdateValue();
				UpdateTexts();
				yield return new WaitForSeconds(memoryCounter.UpdateInterval);
			}
		}

		private void SwitchCounter()
		{
			if (operationMode == OperationMode.Disabled)
			{
				OperationMode = OperationMode.Normal;
			}
			else if (operationMode == OperationMode.Normal)
			{
				OperationMode = OperationMode.Disabled;
			}
		}

		private void ActivateCounters()
		{
			fpsCounter.Activate();
			memoryCounter.Activate();
			deviceInfoCounter.Activate();

			if (fpsCounter.Enabled || memoryCounter.Enabled || deviceInfoCounter.Enabled)
			{
				UpdateTexts();
			}
		}

		private void DeactivateCounters()
		{
			if (Instance == null) return;

			fpsCounter.Deactivate();
			memoryCounter.Deactivate();
			deviceInfoCounter.Deactivate();
		}

		private void RefreshForcedFrameRate()
		{
			RefreshForcedFrameRate(false);
		}

		private void RefreshForcedFrameRate(bool disabling)
		{
			if (forceFrameRate && !disabling)
			{
				if (cachedVSync == -1)
				{
					cachedVSync = QualitySettings.vSyncCount;
					cachedFrameRate = Application.targetFrameRate;
					QualitySettings.vSyncCount = 0;
				}
				
				Application.targetFrameRate = forcedFrameRate;
			}
			else
			{
				if (cachedVSync != -1)
				{
					QualitySettings.vSyncCount = cachedVSync;
					Application.targetFrameRate = cachedFrameRate;
					cachedVSync = -1;
				}
			}
		}

		internal static string Color32ToHex(Color32 color)
		{
			return color.r.ToString("x2") + color.g.ToString("x2") + color.b.ToString("x2") + color.a.ToString("x2");
		}
	}
}