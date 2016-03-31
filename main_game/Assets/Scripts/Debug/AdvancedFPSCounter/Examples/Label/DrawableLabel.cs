using System.Text;
using UnityEngine;

namespace CodeStage.AdvancedFPSCounter.Labels
{
	internal class DrawableLabel
	{
		public LabelAnchor anchor = LabelAnchor.UpperLeft;
		public GUIText guiText = null;
		public StringBuilder newText;
		public bool dirty = false;

		private Vector2 pixelOffset;
		private Font font;
		private int fontSize;
		private float lineSpacing;

		public DrawableLabel(LabelAnchor anchor, Vector2 pixelOffset, Font font, int fontSize, float lineSpacing)
		{
			this.anchor = anchor;
			this.pixelOffset = pixelOffset;
			this.font = font;
			this.fontSize = fontSize;
			this.lineSpacing = lineSpacing;

			NormalizeOffset();

			newText = new StringBuilder(1000);
		}

		internal void CheckAndUpdate()
		{
			if (newText.Length > 0)
			{
				if (guiText == null)
				{

					GameObject anchorObject = new GameObject(anchor.ToString());
					guiText = anchorObject.AddComponent<GUIText>();

					if (anchor == LabelAnchor.UpperLeft)
					{
						anchorObject.transform.position = new Vector3(0f, 1f);
						guiText.anchor = TextAnchor.UpperLeft;
						guiText.alignment = TextAlignment.Left;
					}
					else if (anchor == LabelAnchor.UpperRight)
					{
						anchorObject.transform.position = new Vector3(1f, 1f);
						guiText.anchor = TextAnchor.UpperRight;
						guiText.alignment = TextAlignment.Right;
					}
					else if (anchor == LabelAnchor.LowerLeft)
					{
						anchorObject.transform.position = new Vector3(0f, 0f);
						guiText.anchor = TextAnchor.LowerLeft;
						guiText.alignment = TextAlignment.Left;
					}
					else if (anchor == LabelAnchor.LowerRight)
					{
						anchorObject.transform.position = new Vector3(1f, 0f);
						guiText.anchor = TextAnchor.LowerRight;
						guiText.alignment = TextAlignment.Right;
					}
					else if (anchor == LabelAnchor.UpperCenter)
					{
						anchorObject.transform.position = new Vector3(0.5f, 1f);
						guiText.anchor = TextAnchor.UpperCenter;
						guiText.alignment = TextAlignment.Center;
					}
					else if (anchor == LabelAnchor.LowerCenter)
					{
						anchorObject.transform.position = new Vector3(0.5f, 0f);
						guiText.anchor = TextAnchor.LowerCenter;
						guiText.alignment = TextAlignment.Center;
					}
					else
					{
						Debug.LogWarning("[AFPSCounter] Unknown label anchor for " + this);
						anchor = LabelAnchor.UpperLeft;
						anchorObject.transform.position = new Vector3(0f, 1f);
						guiText.anchor = TextAnchor.UpperLeft;
						guiText.alignment = TextAlignment.Left;
					}

					guiText.pixelOffset = pixelOffset;
					guiText.font = font;
					guiText.fontSize = fontSize;
					guiText.lineSpacing = lineSpacing;

					anchorObject.layer = AFPSCounter.Instance.gameObject.layer;
					anchorObject.tag = AFPSCounter.Instance.gameObject.tag;
					anchorObject.transform.parent = AFPSCounter.Instance.transform;
				}

				if (dirty)
				{
					guiText.text = newText.ToString();
					dirty = false;
				}
				newText.Length = 0;
			}
			else if (guiText != null)
			{
				Object.DestroyImmediate(guiText.gameObject);
			}
		}

		internal void Clear()
		{
			newText.Length = 0;
			if (guiText != null) Object.Destroy(guiText.gameObject);
		}

		internal void Dispose()
		{
			Clear();
			newText = null;
		}

		internal void ChangeFont(Font labelsFont)
		{
			font = labelsFont;
			if (guiText != null) guiText.font = font;
		}

		internal void ChangeFontSize(int newSize)
		{
			fontSize = newSize;
			if (guiText != null) guiText.fontSize = fontSize;
		}

		internal void ChangeOffset(Vector2 newPixelOffset)
		{
			pixelOffset = newPixelOffset;
			NormalizeOffset();

			if (guiText != null)
			{
				guiText.pixelOffset = pixelOffset;
			}
		}

		private void NormalizeOffset()
		{
			if (anchor == LabelAnchor.UpperLeft)
			{
				pixelOffset.y = -pixelOffset.y;
			}
			else if (anchor == LabelAnchor.UpperRight)
			{
				pixelOffset.x = -pixelOffset.x;
				pixelOffset.y = -pixelOffset.y;
			}
			else if (anchor == LabelAnchor.LowerLeft)
			{
				// it's fine
			}
			else if (anchor == LabelAnchor.LowerRight)
			{
				pixelOffset.x = -pixelOffset.x;
			}
			else if (anchor == LabelAnchor.UpperCenter)
			{
				pixelOffset.y = -pixelOffset.y;
				pixelOffset.x = 0;
			}
			else if (anchor == LabelAnchor.LowerCenter)
			{
				pixelOffset.x = 0;
			}
			else
			{
				pixelOffset.y = -pixelOffset.y;
			}
		}

		internal void ChangeLineSpacing(float lineSpacing)
		{
			this.lineSpacing = lineSpacing;

			if (guiText != null)
			{
				guiText.lineSpacing = lineSpacing;
			}
		}
	}
}