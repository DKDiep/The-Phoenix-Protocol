using CodeStage.AdvancedFPSCounter.Labels;
using UnityEditor;
using UnityEngine;

namespace CodeStage.AdvancedFPSCounter
{
	[CustomEditor(typeof(AFPSCounter))]
	public class AFPSCounterEditor: Editor
	{
		private AFPSCounter self;

		private SerializedProperty operationMode;
		private SerializedProperty fpsGroupToggle;
		private SerializedProperty fpsCounter;
		private SerializedProperty fpsCounterEnabled;
		private SerializedProperty fpsCounterAnchor;
		private SerializedProperty fpsCounterUpdateInterval;
		private SerializedProperty fpsCounterShowMilliseconds;
		private SerializedProperty fpsCounterShowAverage;
		private SerializedProperty fpsCounterShowMinMax;
		private SerializedProperty fpsCounterMinMaxOnNewLine;
		private SerializedProperty fpsCounterResetMinMaxOnNewScene;
		private SerializedProperty fpsCounterMinMaxIntervalsToSkip;
		private SerializedProperty fpsCounterAverageFromSamples;
		private SerializedProperty fpsCounterResetAverageOnNewScene;
		private SerializedProperty fpsCounterWarningLevelValue;
		private SerializedProperty fpsCounterCriticalLevelValue;
		private SerializedProperty fpsCounterColor;
		private SerializedProperty fpsCounterColorWarning;
		private SerializedProperty fpsCounterColorCritical;

		private SerializedProperty memoryGroupToggle;
		private SerializedProperty memoryCounter;
		private SerializedProperty memoryCounterEnabled;
		private SerializedProperty memoryCounterAnchor;
		private SerializedProperty memoryCounterUpdateInterval;
		private SerializedProperty memoryCounterPreciseValues;
		private SerializedProperty memoryCounterColor;
		private SerializedProperty memoryCounterTotalReserved;
		private SerializedProperty memoryCounterAllocated;
		private SerializedProperty memoryCounterMonoUsage;

		private SerializedProperty deviceGroupToggle;
		private SerializedProperty deviceCounter;
		private SerializedProperty deviceCounterEnabled;
		private SerializedProperty deviceCounterAnchor;
		private SerializedProperty deviceCounterColor;
		private SerializedProperty deviceCounterCpuModel;
		private SerializedProperty deviceCounterGpuModel;
		private SerializedProperty deviceCounterRamSize;
		private SerializedProperty deviceCounterScreenData;

		private SerializedProperty lookAndFeelToggle;
		private SerializedProperty labelsFont;
		private SerializedProperty fontSize;
		private SerializedProperty lineSpacing;
		private SerializedProperty countersSpacing;
		private SerializedProperty anchorsOffset;

		private SerializedProperty hotKey;
		private SerializedProperty keepAlive;
		private SerializedProperty forceFrameRate;
		private SerializedProperty forcedFrameRate;

		private LabelAnchor groupAnchor = LabelAnchor.UpperLeft;

		public void OnEnable()
		{
			self = (target as AFPSCounter);

			operationMode = serializedObject.FindProperty("operationMode");

			fpsGroupToggle = serializedObject.FindProperty("fpsGroupToggle");

			fpsCounter = serializedObject.FindProperty("fpsCounter");
			fpsCounterEnabled = fpsCounter.FindPropertyRelative("enabled");
			fpsCounterUpdateInterval = fpsCounter.FindPropertyRelative("updateInterval");
			fpsCounterAnchor = fpsCounter.FindPropertyRelative("anchor");
			fpsCounterShowMilliseconds = fpsCounter.FindPropertyRelative("showMilliseconds");
			fpsCounterShowAverage = fpsCounter.FindPropertyRelative("showAverage");
			fpsCounterShowMinMax = fpsCounter.FindPropertyRelative("showMinMax");
			fpsCounterMinMaxOnNewLine = fpsCounter.FindPropertyRelative("minMaxOnNewLine");
			fpsCounterResetMinMaxOnNewScene = fpsCounter.FindPropertyRelative("resetMinMaxOnNewScene");
			fpsCounterMinMaxIntervalsToSkip = fpsCounter.FindPropertyRelative("minMaxIntervalsToSkip");
			fpsCounterAverageFromSamples = fpsCounter.FindPropertyRelative("averageFromSamples");
			fpsCounterResetAverageOnNewScene = fpsCounter.FindPropertyRelative("resetAverageOnNewScene");
			fpsCounterWarningLevelValue = fpsCounter.FindPropertyRelative("warningLevelValue");
			fpsCounterCriticalLevelValue = fpsCounter.FindPropertyRelative("criticalLevelValue");
			fpsCounterColor = fpsCounter.FindPropertyRelative("color");
			fpsCounterColorWarning = fpsCounter.FindPropertyRelative("colorWarning");
			fpsCounterColorCritical = fpsCounter.FindPropertyRelative("colorCritical");

			memoryGroupToggle = serializedObject.FindProperty("memoryGroupToggle");

			memoryCounter = serializedObject.FindProperty("memoryCounter");
			memoryCounterEnabled = memoryCounter.FindPropertyRelative("enabled");
			memoryCounterUpdateInterval = memoryCounter.FindPropertyRelative("updateInterval");
			memoryCounterAnchor = memoryCounter.FindPropertyRelative("anchor");
			memoryCounterPreciseValues = memoryCounter.FindPropertyRelative("preciseValues");
			memoryCounterColor = memoryCounter.FindPropertyRelative("color");
			memoryCounterTotalReserved = memoryCounter.FindPropertyRelative("totalReserved");
			memoryCounterAllocated = memoryCounter.FindPropertyRelative("allocated");
			memoryCounterMonoUsage = memoryCounter.FindPropertyRelative("monoUsage");

			deviceGroupToggle = serializedObject.FindProperty("deviceGroupToggle");

			deviceCounter = serializedObject.FindProperty("deviceInfoCounter");
			deviceCounterEnabled = deviceCounter.FindPropertyRelative("enabled");
			deviceCounterAnchor = deviceCounter.FindPropertyRelative("anchor");
			deviceCounterColor = deviceCounter.FindPropertyRelative("color");
			deviceCounterCpuModel = deviceCounter.FindPropertyRelative("cpuModel");
			deviceCounterGpuModel = deviceCounter.FindPropertyRelative("gpuModel");
			deviceCounterRamSize = deviceCounter.FindPropertyRelative("ramSize");
			deviceCounterScreenData = deviceCounter.FindPropertyRelative("screenData");

			lookAndFeelToggle = serializedObject.FindProperty("lookAndFeelToggle");
			labelsFont = serializedObject.FindProperty("labelsFont");
			fontSize = serializedObject.FindProperty("fontSize");
			lineSpacing = serializedObject.FindProperty("lineSpacing");
			countersSpacing = serializedObject.FindProperty("countersSpacing");
			anchorsOffset = serializedObject.FindProperty("anchorsOffset");

			hotKey = serializedObject.FindProperty("hotKey");
			keepAlive = serializedObject.FindProperty("keepAlive");
			forceFrameRate = serializedObject.FindProperty("forceFrameRate");
			forcedFrameRate = serializedObject.FindProperty("forcedFrameRate");
		}

		public override void OnInspectorGUI()
		{
			if (self == null) return;

			serializedObject.Update();
			
			int indent = EditorGUI.indentLevel;

			if (PropertyFieldChanged(operationMode, new GUIContent("Operation Mode", "Disabled: removes labels and stops all internal processes except Hot Key listener\n\nBackground: removes labels keeping counters alive; use for hidden performance monitoring\n\nNormal: shows labels and runs all internal processes as usual")))
			{
				self.OperationMode = (OperationMode)operationMode.enumValueIndex;
			}

			EditorGUILayout.PropertyField(hotKey, new GUIContent("Hot Key", "Used to enable / disable plugin. Set to None to disable"));
			EditorGUILayout.PropertyField(keepAlive, new GUIContent("Keep Alive", "Prevent current Game Object from destroying on level (scene) load"));

			EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			if (PropertyFieldChanged(forceFrameRate, new GUIContent("Force FPS", "Allows to see how your game performs on specified frame rate.\nDoes not guarantee selected frame rate. Set -1 to render as fast as possible in current conditions.\nIMPORTANT: this option disables VSync while enabled!")))
			{
				self.ForceFrameRate = forceFrameRate.boolValue;
			}

			//EditorGUI.indentLevel = 1;
			if (PropertyFieldChanged(forcedFrameRate, new GUIContent("")))
			{
				self.ForcedFrameRate = forcedFrameRate.intValue;
			}
			//EditorGUI.indentLevel = indent;
			EditorGUILayout.EndHorizontal();

			
			if (Foldout(lookAndFeelToggle, "Look and feel"))
			{
				EditorGUI.indentLevel = 1;

				EditorGUIUtility.fieldWidth = 70;

				if (PropertyFieldChanged(labelsFont, new GUIContent("Labels font", "Leave blank to use default font")))
				{
					self.LabelsFont = (Font)labelsFont.objectReferenceValue;
				}

				if (PropertyFieldChanged(fontSize, new GUIContent("Font size", "Set to 0 to use font size specified in the font importer")))
				{
					self.FontSize = fontSize.intValue;
				}

				if (PropertyFieldChanged(lineSpacing, new GUIContent("Line spacing", "Space between lines in labels")))
				{
					self.LineSpacing = lineSpacing.floatValue;
				}

				if (PropertyFieldChanged(countersSpacing, new GUIContent("Counters spacing", "Lines count between different counters in a single label")))
				{
					self.CountersSpacing = countersSpacing.intValue;
				}

				EditorGUIUtility.wideMode = true;
				if (PropertyFieldChanged(anchorsOffset, new GUIContent("Pixel offset", "Offset in pixels, will be applied to all 4 corners automatically")))
				{
					self.AnchorsOffset = anchorsOffset.vector2Value;
				}
				EditorGUIUtility.wideMode = false;

				GUILayout.BeginHorizontal();
				groupAnchor = (LabelAnchor)EditorGUILayout.EnumPopup(new GUIContent("Move all to", "Use to explicitly move all counters to the specified anchor label.\nSelect anchor and press Apply"), groupAnchor);

				if (GUILayout.Button(new GUIContent("Apply", "Press to move all counters to the selected anchor label"), GUILayout.Width(45)))
				{
					self.fpsCounter.Anchor = groupAnchor;
					fpsCounterAnchor.enumValueIndex = (int)groupAnchor;

					self.memoryCounter.Anchor = groupAnchor;
					memoryCounterAnchor.enumValueIndex = (int)groupAnchor;

					self.deviceInfoCounter.Anchor = groupAnchor;
					deviceCounterAnchor.enumValueIndex = (int)groupAnchor;
				}
				GUILayout.EndHorizontal();
				 

				EditorGUIUtility.fieldWidth = 0;
				EditorGUI.indentLevel = indent;

				EditorGUILayout.Space();
			}

			if (ToggleFoldout(fpsGroupToggle, "FPS Counter", fpsCounterEnabled))
			{
				self.fpsCounter.Enabled = fpsCounterEnabled.boolValue;
			}

			if (fpsGroupToggle.boolValue)
			{
				EditorGUI.indentLevel = 2;

				if (PropertyFieldChanged(fpsCounterUpdateInterval, new GUIContent("Interval", "Update interval in seconds")))
				{
					self.fpsCounter.UpdateInterval = fpsCounterUpdateInterval.floatValue;
				}

				if (PropertyFieldChanged(fpsCounterAnchor))
				{
					self.fpsCounter.Anchor = (LabelAnchor)fpsCounterAnchor.enumValueIndex;
				}

				float minVal = fpsCounterCriticalLevelValue.intValue;
				float maxVal = fpsCounterWarningLevelValue.intValue;

				EditorGUILayout.MinMaxSlider(new GUIContent("Colors range", "This range will be used to apply colors below on specific FPS:\nCritical: 0 - min\nWarning: min+1 - max-1\nNormal: max+"), ref minVal, ref maxVal, 1, 60);

				fpsCounterCriticalLevelValue.intValue = (int)minVal;
				fpsCounterWarningLevelValue.intValue = (int)maxVal;

				GUILayout.BeginHorizontal();
				if (PropertyFieldChanged(fpsCounterColor, new GUIContent("Normal")))
				{
					self.fpsCounter.Color = fpsCounterColor.colorValue;
				}

				GUILayout.Label(maxVal + "+ FPS", GUILayout.Width(75));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();

				if (PropertyFieldChanged(fpsCounterColorWarning, new GUIContent("Warning")))
				{
					self.fpsCounter.ColorWarning = fpsCounterColorWarning.colorValue;
				}
				GUILayout.Label((minVal + 1) + " - " + (maxVal - 1) + " FPS", GUILayout.Width(75));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				if (PropertyFieldChanged(fpsCounterColorCritical, new GUIContent("Critical")))
				{
					self.fpsCounter.ColorCritical = fpsCounterColorCritical.colorValue;
				}
				GUILayout.Label("0 - " + minVal + " FPS", GUILayout.Width(75));
				GUILayout.EndHorizontal();

				EditorGUILayout.Space();

				if (PropertyFieldChanged(fpsCounterShowMilliseconds, new GUIContent("Milliseconds", "Shows average time in milliseconds spent to process 1 frame")))
				{
					self.fpsCounter.ShowMilliseconds = fpsCounterShowMilliseconds.boolValue;
				}

				if (PropertyFieldChanged(fpsCounterShowAverage, new GUIContent("Average FPS", "Shows Average FPS calculated from specified Samples amount or since game or scene start, depending on Samples value and 'Reset On Load' toggle")))
				{
					self.fpsCounter.ShowAverage = fpsCounterShowAverage.boolValue;
				}

				if (fpsCounterShowAverage.boolValue)
				{
					EditorGUI.indentLevel = 3;

					if (PropertyFieldChanged(fpsCounterAverageFromSamples, new GUIContent("Samples", "Amount of last samples to get average from. Set 0 to get average from all samples since startup or level load. One Sample recodred per Interval")))
					{
						self.fpsCounter.AverageFromSamples = fpsCounterAverageFromSamples.intValue;
					}

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PropertyField(fpsCounterResetAverageOnNewScene, new GUIContent("Auto reset", "Average FPS counter accumulative data will be reset on new scene load if enabled"));
					if (GUILayout.Button("Reset now"))
					{
						self.fpsCounter.ResetAverage();
					}
					EditorGUILayout.EndHorizontal();

					EditorGUI.indentLevel = 2;
				}

				if (PropertyFieldChanged(fpsCounterShowMinMax, new GUIContent("MinMax FPS", "Shows minimum and maximum FPS readouts since game or scene start, depending on 'Reset On Load' toggle")))
				{
					self.fpsCounter.ShowMinMax = fpsCounterShowMinMax.boolValue;
				}

				if (fpsCounterShowMinMax.boolValue)
				{
					EditorGUI.indentLevel = 3;

					EditorGUILayout.PropertyField(fpsCounterMinMaxIntervalsToSkip, new GUIContent("Delay", "Amount of update intervals to skip before recording minimum and maximum FPS, use it to skip initialization performance spikes and drops"));

					if (PropertyFieldChanged(fpsCounterMinMaxOnNewLine, new GUIContent("On new line", "Controls placing Min Max on the new line")))
					{
						self.fpsCounter.MinMaxOnNewLine = fpsCounterMinMaxOnNewLine.boolValue;
					}

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PropertyField(fpsCounterResetMinMaxOnNewScene, new GUIContent("Auto reset", "Minimum and maximum FPS readouts will be reset on new scene load if enabled"));
					if (GUILayout.Button("Reset now"))
					{
						self.fpsCounter.ResetMinMax();
					}
					EditorGUILayout.EndHorizontal();

					EditorGUI.indentLevel = 2;
				}

				EditorGUILayout.Space();

				EditorGUI.indentLevel = indent;
			}

			if (ToggleFoldout(memoryGroupToggle, "Memory Counter", memoryCounterEnabled))
			{
				self.memoryCounter.Enabled = memoryCounterEnabled.boolValue;
			}

			if (memoryGroupToggle.boolValue)
			{
				EditorGUI.indentLevel = 2;

				if (PropertyFieldChanged(memoryCounterUpdateInterval, new GUIContent("Interval", "Update interval in seconds")))
				{
					self.memoryCounter.UpdateInterval = memoryCounterUpdateInterval.floatValue;
				}

				if (PropertyFieldChanged(memoryCounterAnchor))
				{
					self.memoryCounter.Anchor = (LabelAnchor)memoryCounterAnchor.enumValueIndex;
				}

				if (PropertyFieldChanged(memoryCounterColor, new GUIContent("Color")))
				{
					self.memoryCounter.Color = memoryCounterColor.colorValue;
				}

				EditorGUILayout.Space();

				if (PropertyFieldChanged(memoryCounterPreciseValues, new GUIContent("Precise", "Maked memory usage output more precise thus using more system resources (not recommended)")))
				{
					self.memoryCounter.PreciseValues = memoryCounterPreciseValues.boolValue;
				}

				if (PropertyFieldChanged(memoryCounterTotalReserved, new GUIContent("Total", "Total reserved memory size")))
				{
					self.memoryCounter.TotalReserved = memoryCounterTotalReserved.boolValue;
				}

				if (PropertyFieldChanged(memoryCounterAllocated, new GUIContent("Allocated", "Amount of allocated memory")))
				{
					self.memoryCounter.Allocated = memoryCounterAllocated.boolValue;
				}

				if (PropertyFieldChanged(memoryCounterMonoUsage, new GUIContent("Mono", "Amount of memory used by managed Mono objects")))
				{
					self.memoryCounter.MonoUsage = memoryCounterMonoUsage.boolValue;
				}

				EditorGUILayout.Space();

				EditorGUI.indentLevel = indent;
			}


			if (ToggleFoldout(deviceGroupToggle, "Device Information", deviceCounterEnabled))
			{
				self.deviceInfoCounter.Enabled = deviceCounterEnabled.boolValue;
			}

			if (deviceGroupToggle.boolValue)
			{
				EditorGUI.indentLevel = 2;

				if (PropertyFieldChanged(deviceCounterAnchor))
				{
					self.deviceInfoCounter.Anchor = (LabelAnchor)deviceCounterAnchor.intValue;
				}

				if (PropertyFieldChanged(deviceCounterColor, new GUIContent("Color")))
				{
					self.deviceInfoCounter.Color = deviceCounterColor.colorValue;
				}

				EditorGUILayout.Space();

				if (PropertyFieldChanged(deviceCounterCpuModel, new GUIContent("CPU", "CPU model and cores (including virtual cores from Intel's Hyper Threading) count")))
				{
					self.deviceInfoCounter.CpuModel = deviceCounterCpuModel.boolValue;
				}

				if (PropertyFieldChanged(deviceCounterGpuModel, new GUIContent("GPU", "GPU model, graphics API version, supported shader model (if possible), approximate pixel fill-rate in megapixels per second (if possible) and total Video RAM size (if possible)")))
				{
					self.deviceInfoCounter.GpuModel = deviceCounterGpuModel.boolValue;
				}

				if (PropertyFieldChanged(deviceCounterRamSize, new GUIContent("RAM", "Total RAM size")))
				{
					self.deviceInfoCounter.RamSize = deviceCounterRamSize.boolValue;
				}

				if (PropertyFieldChanged(deviceCounterScreenData, new GUIContent("Screen", "Screen resolution, size and DPI (if possible)")))
				{
					self.deviceInfoCounter.ScreenData = deviceCounterScreenData.boolValue;
				}

				EditorGUI.indentLevel = indent;
			}
			
			EditorGUILayout.Space();
			serializedObject.ApplyModifiedProperties();
		}

		private bool PropertyFieldChanged(SerializedProperty property)
		{
			return PropertyFieldChanged(property, null);
		}

		private bool PropertyFieldChanged(SerializedProperty property, GUIContent content, params GUILayoutOption[] options)
		{
			bool result = false;
			
			EditorGUI.BeginChangeCheck();

			if (content == null)
			{
				EditorGUILayout.PropertyField(property, options);
			}
			else
			{
				EditorGUILayout.PropertyField(property, content, options);
			}

			if (EditorGUI.EndChangeCheck())
			{
				result = true;
			}

			return result;
		}

		private bool ToggleFoldout(SerializedProperty foldout, string caption, SerializedProperty toggle)
		{
			bool toggleStateChanged = false;

			Rect foldoutRect = EditorGUILayout.BeginHorizontal();
			Rect toggleRect = new Rect(foldoutRect);

			toggleRect.width = 15;

			EditorGUI.BeginChangeCheck();
			EditorGUI.PropertyField(toggleRect, toggle, new GUIContent(""));
			if (EditorGUI.EndChangeCheck())
			{
				toggleStateChanged = true;
			}

			foldoutRect.xMin = toggleRect.xMax + 15;

			foldout.boolValue = EditorGUI.Foldout(foldoutRect, foldout.boolValue, caption, true);
			EditorGUILayout.LabelField("");
			EditorGUILayout.EndHorizontal();

			return toggleStateChanged;
		}

		private bool Foldout(SerializedProperty foldout, string caption)
		{
			Rect foldoutRect = EditorGUILayout.BeginHorizontal();
			foldoutRect.xMin += 11;
			foldout.boolValue = EditorGUI.Foldout(foldoutRect, foldout.boolValue, caption, true);
			EditorGUILayout.LabelField("");
			EditorGUILayout.EndHorizontal();

			return foldout.boolValue;
		}
	}
}