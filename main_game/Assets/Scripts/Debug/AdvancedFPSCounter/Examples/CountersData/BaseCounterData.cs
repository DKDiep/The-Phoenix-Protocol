using System;
using System.Text;
using CodeStage.AdvancedFPSCounter.Labels;
using UnityEngine;

namespace CodeStage.AdvancedFPSCounter.CountersData
{
	/// <summary>
	/// Base class for all counters.
	/// </summary>
	[Serializable]
	public abstract class BaseCounterData
	{
		[SerializeField]
		protected bool enabled = true;

		[SerializeField]
		protected LabelAnchor anchor = LabelAnchor.UpperLeft;

		[SerializeField]
		protected Color color;
		protected string colorCached;


		internal StringBuilder text;
		internal bool dirty = false;

		protected AFPSCounter main;

		/// <summary>
		/// Enables or disables counter with immediate label refresh.
		/// </summary>
		public bool Enabled
		{
			get { return enabled; }
			set
			{
				if (enabled == value || !Application.isPlaying) return;

				enabled = value;

				if (enabled)
				{
					Activate();
				}
				else
				{
					Deactivate();
				}
				main.UpdateTexts();
			}
		}

		/// <summary>
		/// Current anchoring label for the counter output. Refreshes both previous and specified label on switching anchor.
		/// </summary>
		public LabelAnchor Anchor
		{
			get
			{
				return anchor;
			}
			set
			{
				if (anchor == value || !Application.isPlaying) return;
				LabelAnchor prevAnchor = anchor;
				anchor = value;
				if (!enabled) return;

				dirty = true;
				main.MakeDrawableLabelDirty(prevAnchor);
				main.UpdateTexts();
			}
		}

		/// <summary>
		/// Regular color of the counter output.
		/// </summary>
		public Color Color
		{
			get { return color; }
			set
			{
				if (color == value || !Application.isPlaying) return;
				color = value;
				if (!enabled) return;

				CacheCurrentColor();

				Refresh();
			}
		}

		/// <summary>
		/// Updates counter's value and forces current label refresh.
		/// </summary>
		public void Refresh()
		{
			if (!enabled || !Application.isPlaying) return;
			UpdateValue(true);
			main.UpdateTexts();
		}

		// we have to cache color html tag to avoid extra alloactions
		protected abstract void CacheCurrentColor();

		internal virtual void UpdateValue()
		{
			UpdateValue(false);
		}

		internal virtual void UpdateValue(bool force){}

		internal void Init(AFPSCounter reference)
		{
			main = reference;
		}

		internal void Dispose()
		{
			main = null;

			if (text != null)
			{
				text.Remove(0, text.Length);
				text = null;
			}
		}

		internal virtual void Activate()
		{
			if (main.OperationMode == OperationMode.Normal)
			{
				if (text == null)
				{
					text = new StringBuilder(100);
				}
				else
				{
					text.Length = 0;
				}
			}
		}

		internal virtual void Deactivate()
		{
			if (text != null)
			{
				text.Remove(0, text.Length);
			}
			main.MakeDrawableLabelDirty(anchor);
		}
	}
}