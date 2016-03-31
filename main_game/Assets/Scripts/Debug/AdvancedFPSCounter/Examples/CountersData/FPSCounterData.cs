using System;
using UnityEngine;

namespace CodeStage.AdvancedFPSCounter.CountersData
{
	/// <summary>
	/// Shows frames per second counter.
	/// </summary>
	[Serializable]
	public class FPSCounterData: BaseCounterData
	{
		private const string COROUTINE_NAME = "UpdateFPSCounter";

		private const string FPS_TEXT_START = "<color=#{0}><b>FPS: ";
		private const string FPS_TEXT_END = "</b></color>";

		private const string MS_TEXT_START = " <color=#{0}><b>[";
		private const string MS_TEXT_END = " MS]</b></color>";

		private const string MIN_TEXT_START = "<color=#{0}><b>MIN: ";
		private const string MIN_TEXT_END = "</b></color> ";

		private const string MAX_TEXT_START = "<color=#{0}><b>MAX: ";
		private const string MAX_TEXT_END = "</b></color>";

		private const string AVG_TEXT_START = " <color=#{0}><b>AVG: ";
		private const string AVG_TEXT_END = "</b></color>";

		/// <summary>
		/// If FPS will drop below this value, #ColorWarning will be used for counter text.
		/// </summary>
		public int warningLevelValue = 50;

		/// <summary>
		/// If FPS will be equal or less this value, #ColorCritical will be used for counter text.
		/// </summary>
		public int criticalLevelValue = 20;

		/// <summary>
		/// Average FPS counter accumulative data will be reset on new scene load if enabled.
		/// \sa AverageFromSamples, lastAverageValue
		/// </summary>
		public bool resetAverageOnNewScene = false;

		/// <summary>
		/// Minimum and maximum FPS readings will be reset on new scene load if enabled.
		/// \sa lastMinimumValue, lastMaximumValue
		/// </summary>
		public bool resetMinMaxOnNewScene = false;

		/// <summary>
		/// Amount of update intervals to skip before recording Minimum and maximum FPS. Use it to skip initialization performance spikes and drops.
		/// \sa lastMinimumValue, lastMaximumValue
		/// </summary>
		[Range(0, 10)]
		public int minMaxIntervalsToSkip = 3;

		/// <summary>
		/// Last calculated FPS value.
		/// </summary>
		public int lastValue = 0;

		/// <summary>
		/// Last calculated Milliseconds value.
		/// </summary>
		public float lastMillisecondsValue = 0;

		/// <summary>
		/// Last calculated Average FPS value.
		/// \sa AverageFromSamples, resetAverageOnNewScene
		/// </summary>
		public int lastAverageValue = 0;

		/// <summary>
		/// Last minimum FPS value.
		/// \sa resetMinMaxOnNewScene
		/// </summary>
		public int lastMinimumValue = -1;

		/// <summary>
		/// Last maximum FPS value.
		/// \sa resetMinMaxOnNewScene
		/// </summary>
		public int lastMaximumValue = -1;

		/// <summary>
		/// Current FPS level.
		/// \sa FPSLevel, onFPSLevelChange
		/// </summary>
		[NonSerialized]
		public FPSLevel currentFPSLevel;

		/// <summary>
		/// Callback to let you react on FPS level change.
		/// \sa FPSLevel, currentFPSLevel
		/// </summary>
		[NonSerialized]
		public Action<FPSLevel> onFPSLevelChange;

		[SerializeField]
		[Range(0.1f, 10f)]
		private float updateInterval = 0.5f;

		[SerializeField]
		private bool showMilliseconds = true;

		[SerializeField]
		private bool showAverage = true;

		[SerializeField]
		[Range(0, 100)]
		private int averageFromSamples = 50;

		[SerializeField]
		private bool showMinMax = false;

		[SerializeField]
		private bool minMaxOnNewLine = false;

		[SerializeField]
		private Color colorWarning = new Color32(236, 224, 88, 255);

		[SerializeField]
		private Color colorCritical = new Color32(249, 91, 91, 255);

		internal float newValue;

		private string colorCachedMs;
		private string colorCachedMin;
		private string colorCachedMax;
		private string colorCachedAvg;

		private string colorWarningCached;
		private string colorWarningCachedMs;
		private string colorWarningCachedMin;
		private string colorWarningCachedMax;
		private string colorWarningCachedAvg;

		private string colorCriticalCached;
		private string colorCriticalCachedMs;
		private string colorCriticalCachedMin;
		private string colorCriticalCachedMax;
		private string colorCriticalCachedAvg;

		private bool inited;

		private int currentAverageSamples;
		private float currentAverageRaw;
		private float[] accumulatedAverageSamples;

		private int minMaxIntervalsSkipped = 0;

		/*private bool onNormalFired = false;
		private bool onWarningFired = false;
		private bool onCriticalFired = false;*/

		internal FPSCounterData()
		{
			color = new Color32(85, 218, 102, 255);
		}

		#region properties
		/// <summary>
		/// Counter's value update interval.
		/// </summary>
		public float UpdateInterval
		{
			get { return updateInterval; }
			set
			{
				if (Math.Abs(updateInterval - value) < 0.001f || !Application.isPlaying) return;

				updateInterval = value;
				if (!enabled) return;

				RestartCoroutine();
			}
		}

		/// <summary>
		/// Shows Average FPS calculated from specified #AverageFromSamples amount or since game / scene start, depending on %AverageFromSamples value and #resetAverageOnNewScene toggle.
		/// \sa lastAverageValue
		/// </summary>
		public bool MinMaxOnNewLine
		{
			get { return minMaxOnNewLine; }
			set
			{
				if (minMaxOnNewLine == value || !Application.isPlaying) return;
				minMaxOnNewLine = value;
				if (!enabled) return;
				//if (!multiline) ResetAverage();

				Refresh();
			}
		}

		/// <summary>
		/// Shows average time in milliseconds spent to process 1 frame.
		/// \sa lastAverageValue
		/// </summary>
		public bool ShowMilliseconds
		{
			get { return showMilliseconds; }
			set
			{
				if (showMilliseconds == value || !Application.isPlaying) return;
				showMilliseconds = value;
				if (!showMilliseconds) lastMillisecondsValue = 0f;
				if (!enabled) return;

				Refresh();
			}
		}
		
		/// <summary>
		/// Shows Average FPS calculated from specified #AverageFromSamples amount or since game / scene start, depending on %AverageFromSamples value and #resetAverageOnNewScene toggle.
		/// \sa lastAverageValue
		/// </summary>
		public bool ShowAverage
		{
			get { return showAverage; }
			set
			{
				if (showAverage == value || !Application.isPlaying) return;
				showAverage = value;
				if (!showAverage) ResetAverage();
				if (!enabled) return;

				Refresh();
			}
		}

		/// <summary>
		/// Amount of last samples to get average from. Set 0 to get average from all samples since startup or level load. One Sample recorded per #UpdateInterval.
		/// \sa resetAverageOnNewScene
		/// </summary>
		public int AverageFromSamples
		{
			get { return averageFromSamples; }
			set
			{
				if (averageFromSamples == value || !Application.isPlaying) return;
				averageFromSamples = value;
				if (!enabled) return;

				if (averageFromSamples > 0)
				{
					if (accumulatedAverageSamples == null)
					{
						accumulatedAverageSamples = new float[averageFromSamples];
					}
					else if (accumulatedAverageSamples.Length != averageFromSamples)
					{
						Array.Resize(ref accumulatedAverageSamples, averageFromSamples);
					}
				}
				else
				{
					accumulatedAverageSamples = null;
				}
				ResetAverage();
				Refresh();
			}
		}

		/// <summary>
		/// Shows minimum and maximum FPS readings since game / scene start, depending on #resetMinMaxOnNewScene toggle.
		/// </summary>
		public bool ShowMinMax
		{
			get { return showMinMax; }
			set
			{
				if (showMinMax == value || !Application.isPlaying) return;
				showMinMax = value;
				if (!showMinMax) ResetMinMax();
				if (!enabled) return;

				Refresh();
			}
		}

		/// <summary>
		/// Color of the FPS counter while FPS is between #criticalLevelValue and #warningLevelValue.
		/// </summary>
		public Color ColorWarning
		{
			get { return colorWarning; }
			set
			{
				if (colorWarning == value || !Application.isPlaying) return;
				colorWarning = value;
				if (!enabled) return;

				CacheWarningColor();

				Refresh();
			}
		}

		/// <summary>
		/// Color of the FPS counter while FPS is below #criticalLevelValue.
		/// </summary>
		public Color ColorCritical
		{
			get { return colorCritical; }
			set
			{
				if (colorCritical == value || !Application.isPlaying) return;
				colorCritical = value;
				if (!enabled) return;

				CacheCriticalColor();

				Refresh();
			}
		}

		#endregion

		/// <summary>
		/// Resets Average FPS counter accumulative data.
		/// </summary>
		public void ResetAverage()
		{
			if (!Application.isPlaying) return;

			lastAverageValue = 0;
			currentAverageSamples = 0;
			currentAverageRaw = 0;

			if (averageFromSamples > 0 && accumulatedAverageSamples != null)
			{
				Array.Clear(accumulatedAverageSamples, 0, accumulatedAverageSamples.Length);
			}
		}

		/// <summary>
		/// Resets minimum and maximum FPS readings.
		/// </summary>
		public void ResetMinMax()
		{
			if (!Application.isPlaying) return;
			lastMinimumValue = -1;
			lastMaximumValue = -1;
			minMaxIntervalsSkipped = 0;
			
			UpdateValue(true);
			dirty = true;
		}

		internal override void Activate()
		{
			if (!enabled) return;
			base.Activate();

			lastValue = 0;
			lastMinimumValue = -1;

			if (main.OperationMode == OperationMode.Normal)
			{
				if (colorCached == null)
				{
					CacheCurrentColor();
				}

				if (colorWarningCached == null)
				{
					CacheWarningColor();
				}

				if (colorCriticalCached == null)
				{
					CacheCriticalColor();
				}

				text.Append(colorCriticalCached).Append("0").Append(FPS_TEXT_END);
				dirty = true;
			}

			if (!inited)
			{
				main.StartCoroutine(COROUTINE_NAME);
				inited = true;
			}
		}

		internal override void Deactivate()
		{
			if (!inited) return;
			base.Deactivate();

			main.StopCoroutine(COROUTINE_NAME);
			ResetMinMax();
			ResetAverage();
			lastValue = 0;
			currentFPSLevel = FPSLevel.Normal;

			inited = false;
		}

		internal override void UpdateValue(bool force)
		{
			if (!enabled) return;

			int roundedValue = (int)newValue;
			if (lastValue != roundedValue || force)
			{
				lastValue = roundedValue;
				dirty = true;
			}

			if (lastValue <= criticalLevelValue)
			{
				if (lastValue != 0 && currentFPSLevel != FPSLevel.Critical)
				{
					currentFPSLevel = FPSLevel.Critical;
					if (onFPSLevelChange != null) onFPSLevelChange(currentFPSLevel);
				}
			}
			else if (lastValue < warningLevelValue)
			{
				if (lastValue != 0 && currentFPSLevel != FPSLevel.Warning)
				{
					currentFPSLevel = FPSLevel.Warning;
					if (onFPSLevelChange != null) onFPSLevelChange(currentFPSLevel);
				}
			}
			else
			{
				if (lastValue != 0 && currentFPSLevel != FPSLevel.Normal)
				{
					currentFPSLevel = FPSLevel.Normal;
					if (onFPSLevelChange != null) onFPSLevelChange(currentFPSLevel);
				}
			}

			// since ms calculates from fps we can calc it when fps changed
			if (dirty && showMilliseconds)
			{
				lastMillisecondsValue = 1000f / newValue;
			}

			int currentAverageRounded = 0;
			if (showAverage)
			{
				if (averageFromSamples == 0)
				{
					currentAverageSamples++;
					currentAverageRaw += (lastValue - currentAverageRaw) / currentAverageSamples;
				}
				else
				{
					if (accumulatedAverageSamples == null)
					{
						accumulatedAverageSamples = new float[averageFromSamples];
						ResetAverage();
					}

					accumulatedAverageSamples[currentAverageSamples % averageFromSamples] = lastValue;
					currentAverageSamples++;

					currentAverageRaw = GetAverageFromAccumulatedSamples();
				}

				currentAverageRounded = Mathf.RoundToInt(currentAverageRaw);

				if (lastAverageValue != currentAverageRounded || force)
				{
					lastAverageValue = currentAverageRounded;
					dirty = true;
				}
			}

			if (showMinMax)
			{
				if (minMaxIntervalsSkipped < minMaxIntervalsToSkip)
				{
					if (!force) minMaxIntervalsSkipped ++;
				}
				else if (dirty)
				{
					if (lastMinimumValue == -1)
						lastMinimumValue = lastValue;
					else if (lastValue < lastMinimumValue)
					{
						lastMinimumValue = lastValue;
						dirty = true;
					}

					if (lastMaximumValue == -1)
						lastMaximumValue = lastValue;
					else if (lastValue > lastMaximumValue)
					{
						lastMaximumValue = lastValue;
						dirty = true;
					}
				}
			}

			if (dirty && main.OperationMode == OperationMode.Normal)
			{
				string coloredStartText;

				if (lastValue >= warningLevelValue)
					coloredStartText = colorCached;
				else if (lastValue <= criticalLevelValue)
					coloredStartText = colorCriticalCached;
				else
					coloredStartText = colorWarningCached;

				text.Length = 0;
				text.Append(coloredStartText).Append(lastValue).Append(FPS_TEXT_END);

				if (showMilliseconds)
				{
					if (lastValue >= warningLevelValue)
						coloredStartText = colorCachedMs;
					else if (lastValue <= criticalLevelValue)
						coloredStartText = colorCriticalCachedMs;
					else
						coloredStartText = colorWarningCachedMs;

					text.Append(coloredStartText).Append(lastMillisecondsValue.ToString("F")).Append(MS_TEXT_END);
				}

				if (showAverage)
				{
					if (currentAverageRounded >= warningLevelValue)
						coloredStartText = colorCachedAvg;
					else if (currentAverageRounded <= criticalLevelValue)
						coloredStartText = colorCriticalCachedAvg;
					else
						coloredStartText = colorWarningCachedAvg;


					text.Append(coloredStartText).Append(currentAverageRounded).Append(AVG_TEXT_END);
				}

				if (showMinMax)
				{
					if (minMaxOnNewLine)
					{
						text.Append(AFPSCounter.NEW_LINE);
					}
					else
					{
						text.Append(AFPSCounter.SPACE);
					}

					if (lastMinimumValue >= warningLevelValue)
						coloredStartText = colorCachedMin;
					else if (lastMinimumValue <= criticalLevelValue)
						coloredStartText = colorCriticalCachedMin;
					else
						coloredStartText = colorWarningCachedMin;

					text.Append(coloredStartText).Append(lastMinimumValue).Append(MIN_TEXT_END);
					if (lastMaximumValue >= warningLevelValue)
						coloredStartText = colorCachedMax;
					else if (lastMaximumValue <= criticalLevelValue)
						coloredStartText = colorCriticalCachedMax;
					else
						coloredStartText = colorWarningCachedMax;

					text.Append(coloredStartText).Append(lastMaximumValue).Append(MAX_TEXT_END);
				}
			}
		}

		protected override void CacheCurrentColor()
		{
			string colorString = AFPSCounter.Color32ToHex(color);
			colorCached = String.Format(FPS_TEXT_START, colorString);
			colorCachedMs = String.Format(MS_TEXT_START, colorString);
			colorCachedMin = String.Format(MIN_TEXT_START, colorString);
			colorCachedMax = String.Format(MAX_TEXT_START, colorString);
			colorCachedAvg = String.Format(AVG_TEXT_START, colorString);
		}

		protected void CacheWarningColor()
		{
			string colorString = AFPSCounter.Color32ToHex(colorWarning);
			colorWarningCached = String.Format(FPS_TEXT_START, colorString);
			colorWarningCachedMs = String.Format(MS_TEXT_START, colorString);
			colorWarningCachedMin = String.Format(MIN_TEXT_START, colorString);
			colorWarningCachedMax = String.Format(MAX_TEXT_START, colorString);
			colorWarningCachedAvg = String.Format(AVG_TEXT_START, colorString);
		}

		protected void CacheCriticalColor()
		{
			string colorString = AFPSCounter.Color32ToHex(colorCritical);
			colorCriticalCached = String.Format(FPS_TEXT_START, colorString);
			colorCriticalCachedMs = String.Format(MS_TEXT_START, colorString);
			colorCriticalCachedMin = String.Format(MIN_TEXT_START, colorString);
			colorCriticalCachedMax = String.Format(MAX_TEXT_START, colorString);
			colorCriticalCachedAvg = String.Format(AVG_TEXT_START, colorString);
		}

		private void RestartCoroutine()
		{
			main.StopCoroutine(COROUTINE_NAME);
			main.StartCoroutine(COROUTINE_NAME);
		}

		private float GetAverageFromAccumulatedSamples()
		{
			float averageFps;
			float totalFps = 0;

			for (int i = 0; i < averageFromSamples; i++)
			{
				totalFps += accumulatedAverageSamples[i];
			}

			if (currentAverageSamples < averageFromSamples)
			{
				averageFps = totalFps / currentAverageSamples;
			}
			else
			{
				averageFps = totalFps / averageFromSamples;
			}

			return averageFps;
		}
	}
}