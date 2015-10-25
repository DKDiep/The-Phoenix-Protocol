using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public class ProFlareExporter {


	public static string animCurveExport(AnimationCurve curve){
		string curveString = "{";

		int keyCount = 0;

		foreach(Keyframe key in curve.keys){

			curveString = curveString+"\"key"+keyCount+"\": {\"time\":"+key.time+",\"value\":"+key.value+",\"in\":"+key.inTangent+",\"out\":"+key.outTangent+",\"tangentMode\":"+key.tangentMode+"}";
		
			keyCount++;
			if(keyCount != curve.keys.Length)
				curveString = curveString+",";

		}
		curveString = curveString+"}";

		return curveString;
	}

	// Use this for initialization
	public static void ExportFlare (ProFlare flare) {
		Debug.Log ("Export Flare");

		string fileName = "Assets/ProFlares/ExportedFlares/"+flare.gameObject.name+".txt";

		if (File.Exists(fileName))
		{
			Debug.Log(fileName+" already exists.");
			//return;
		}
		var sr = File.CreateText(fileName);
		sr.WriteLine ("{");

		sr.WriteLine ("\"meta\": {");
		//sr.WriteLine ("	\"frame\": {\"x\":768,\"y\":512,\"w\":256,\"h\":256},");

		sr.WriteLine ("	\"GlobalScale\": {0},",flare.GlobalScale);

		sr.WriteLine ("	\"MultiplyScaleByTransformScale\": {0},",boolToString(flare.MultiplyScaleByTransformScale));

		sr.WriteLine ("	\"GlobalBrightness\": {0},",flare.GlobalBrightness);

		sr.WriteLine ("	\"GlobalTintColor\": {\"r\":"+flare.GlobalTintColor.r+",\"g\":"+flare.GlobalTintColor.g+",\"b\":"+flare.GlobalTintColor.b+",\"a\":"+flare.GlobalTintColor.a+"},");
	
		sr.WriteLine ("	\"useMaxDistance\": {0},",boolToString(flare.useMaxDistance));

		sr.WriteLine ("	\"useDistanceScale\": {0},",boolToString(flare.useDistanceScale));

		sr.WriteLine ("	\"useDistanceFade\": {0},",boolToString(flare.useDistanceFade));

		sr.WriteLine ("	\"GlobalMaxDistance\": {0},",flare.GlobalMaxDistance);

//		//Angle Culling Properties
		sr.WriteLine ("	\"UseAngleLimit\": {0},",boolToString(flare.UseAngleLimit));

		sr.WriteLine ("	\"maxAngle\": {0},",flare.maxAngle);

		sr.WriteLine ("	\"UseAngleScale\": {0},",boolToString(flare.UseAngleScale));

		sr.WriteLine ("	\"UseAngleBrightness\": {0},",boolToString(flare.UseAngleBrightness));

		sr.WriteLine ("	\"UseAngleCurve\": {0},",boolToString(flare.UseAngleCurve));

		sr.WriteLine ("	\"AngleCurve\": {0},",ProFlareExporter.animCurveExport(flare.AngleCurve));

//		//Occlusion Properties
//		public LayerMask mask = 1;

/////////////////////////////		sr.WriteLine ("	\"mask\": {0},",(int)flare.mask);

		//		public bool RaycastPhysics;
		sr.WriteLine ("	\"RaycastPhysics\": {0},",boolToString(flare.RaycastPhysics));

		sr.WriteLine ("	\"OffScreenFadeDist\": {0},",flare.OffScreenFadeDist);

//		
//		//Dynamic Edge Properties

		sr.WriteLine ("	\"useDynamicEdgeBoost\": {0},",boolToString(flare.useDynamicEdgeBoost));

		sr.WriteLine ("	\"DynamicEdgeBoost\": {0},",flare.DynamicEdgeBoost);

		sr.WriteLine ("	\"DynamicEdgeBrightness\": {0},",flare.DynamicEdgeBrightness);

		sr.WriteLine ("	\"DynamicEdgeRange\": {0},",flare.DynamicEdgeRange);

		sr.WriteLine ("	\"DynamicEdgeBias\": {0},",flare.DynamicEdgeBias); 

		sr.WriteLine ("	\"DynamicEdgeCurve\": {0},",ProFlareExporter.animCurveExport(flare.DynamicEdgeCurve));

//		//Dynamic Center Properties
		sr.WriteLine ("	\"useDynamicCenterBoost\": {0},",boolToString(flare.useDynamicCenterBoost)); 

		sr.WriteLine ("	\"DynamicCenterBoost\": {0},",flare.DynamicCenterBoost); 

		sr.WriteLine ("	\"DynamicCenterBrightness\": {0},",flare.DynamicCenterBrightness); 

		sr.WriteLine ("	\"DynamicCenterRange\": {0},",flare.DynamicCenterRange); 

		sr.WriteLine ("	\"DynamicCenterBias\": {0},",flare.DynamicCenterBias); 

//		public bool neverCull;
		sr.WriteLine ("	\"neverCull\": {0},",boolToString(flare.neverCull)); 

		sr.WriteLine ("	\"Elements\": {");
		int count = 0;

		foreach (ProFlareElement element in flare.Elements) {
			sr.WriteLine ("		\"Element"+count+"\": {");

			sr.WriteLine ("			\"Editing\": {0},",boolToString(element.Editing));

			sr.WriteLine ("			\"Visible\": {0},",boolToString(element.Visible));

//			//Element's texture index inside the texture atlas.
//			public int elementTextureID;
			sr.WriteLine ("			\"elementTextureID\": {0},",element.elementTextureID);

//			
//			//Elements Sprite name from the texture atlas, this isn't checked at runtime. Its only used to help stop flares breaking when the atlas changes.
//			public string SpriteName;

			sr.WriteLine ("			\"Brightness\": {0},",element.Brightness);

			sr.WriteLine ("			\"Scale\": {0},",element.Scale);

			sr.WriteLine ("			\"ScaleRandom\": {0},",element.ScaleRandom);

			sr.WriteLine ("			\"ScaleFinal\": {0},",element.ScaleFinal);

			sr.WriteLine ("			\"RandomColorAmount\": {\"r\":"+element.RandomColorAmount.x+",\"g\":"+element.RandomColorAmount.y+",\"b\":"+element.RandomColorAmount.z+",\"a\":"+element.RandomColorAmount.w+"},");

//			//Element OffSet Properties
			sr.WriteLine ("			\"position\": {0},",element.position);

			sr.WriteLine ("			\"useRangeOffset\": {0},",boolToString(element.useRangeOffset));

			sr.WriteLine ("			\"SubElementPositionRange_Min\": {0},",element.SubElementPositionRange_Min);

			sr.WriteLine ("			\"SubElementPositionRange_Max\": {0},",element.SubElementPositionRange_Max);

			sr.WriteLine ("			\"SubElementAngleRange_Min\": {0},",element.SubElementAngleRange_Min);

			sr.WriteLine ("			\"SubElementAngleRange_Max\": {0},",element.SubElementAngleRange_Max);

			sr.WriteLine ("			\"OffsetPosition\": {\"r\":"+element.OffsetPosition.x+",\"g\":"+element.OffsetPosition.y+",\"b\":"+element.OffsetPosition.z+"},");

			sr.WriteLine ("			\"Anamorphic\": {\"r\":"+element.Anamorphic.x+",\"g\":"+element.Anamorphic.y+",\"b\":"+element.Anamorphic.z+"},");

			sr.WriteLine ("			\"OffsetPostion\": {\"r\":"+element.OffsetPostion.x+",\"g\":"+element.OffsetPostion.y+",\"b\":"+element.OffsetPostion.z+"},");

//			//Element Rotation Properties
			sr.WriteLine ("			\"angle\": {0},",element.angle);

			sr.WriteLine ("			\"useRandomAngle\": {0},",boolToString(element.useRandomAngle));

			sr.WriteLine ("			\"useStarRotation\": {0},",boolToString(element.useStarRotation));

			sr.WriteLine ("			\"AngleRandom_Min\": {0},",element.AngleRandom_Min);

			sr.WriteLine ("			\"AngleRandom_Max\": {0},",element.AngleRandom_Max);

			sr.WriteLine ("			\"OrientToSource\": {0},",boolToString(element.OrientToSource));

			sr.WriteLine ("			\"rotateToFlare\": {0},",boolToString(element.rotateToFlare));

			sr.WriteLine ("			\"rotationSpeed\": {0},",element.rotationSpeed);

			sr.WriteLine ("			\"rotationOverTime\": {0},",element.rotationOverTime);

//			//Colour Properties,
			sr.WriteLine ("			\"useColorRange\": {0},",boolToString(element.useColorRange));

			sr.WriteLine ("			\"ElementFinalColor\": {\"r\":"+element.ElementFinalColor.r+",\"g\":"+element.ElementFinalColor.g+",\"b\":"+element.ElementFinalColor.b+",\"a\":"+element.ElementFinalColor.a+"},");

			sr.WriteLine ("			\"ElementTint\": {\"r\":"+element.ElementTint.r+",\"g\":"+element.ElementTint.g+",\"b\":"+element.ElementTint.b+",\"a\":"+element.ElementTint.a+"},");

			sr.WriteLine ("			\"SubElementColor_Start\": {\"r\":"+element.SubElementColor_Start.r+",\"g\":"+element.SubElementColor_Start.g+",\"b\":"+element.SubElementColor_Start.b+",\"a\":"+element.SubElementColor_Start.a+"},");

			sr.WriteLine ("			\"SubElementColor_End\": {\"r\":"+element.SubElementColor_End.r+",\"g\":"+element.SubElementColor_End.g+",\"b\":"+element.SubElementColor_End.b+",\"a\":"+element.SubElementColor_End.a+"},");

			sr.WriteLine ("			\"useScaleCurve\": {0},",boolToString(element.useScaleCurve));

			sr.WriteLine ("			\"ScaleCurve\": {0},",ProFlareExporter.animCurveExport(element.ScaleCurve));

//			//Override Properties
			sr.WriteLine ("			\"OverrideDynamicEdgeBoost\": {0},",boolToString(element.OverrideDynamicEdgeBoost));

			sr.WriteLine ("			\"DynamicEdgeBoostOverride\": {0},",element.DynamicEdgeBoostOverride);

			sr.WriteLine ("			\"OverrideDynamicCenterBoost\": {0},",boolToString(element.OverrideDynamicCenterBoost));

			sr.WriteLine ("			\"DynamicCenterBoostOverride\": {0},",element.DynamicCenterBoostOverride);

			sr.WriteLine ("			\"OverrideDynamicEdgeBrightness\": {0},",boolToString(element.OverrideDynamicEdgeBrightness));

			sr.WriteLine ("			\"DynamicEdgeBrightnessOverride\": {0},",element.DynamicEdgeBrightnessOverride);

			sr.WriteLine ("			\"OverrideDynamicCenterBrightness\": {0},",boolToString(element.OverrideDynamicCenterBrightness));

			sr.WriteLine ("			\"DynamicCenterBrightnessOverride\": {0},",element.DynamicCenterBrightnessOverride);

			if(element.subElements.Count > 0){

				sr.WriteLine ("			\"subElements\": {");
				int count2 = 0;

				foreach (SubElement subElement in element.subElements) {
					sr.WriteLine ("				\"subElement"+count2+"\": {");
					
					sr.WriteLine ("					\"color\": {\"r\":"+subElement.color.r+",\"g\":"+subElement.color.g+",\"b\":"+subElement.color.b+",\"a\":"+subElement.color.a+"},");

					sr.WriteLine ("					\"position\": {0},",subElement.position);

					sr.WriteLine ("					\"offset\": {\"r\":"+subElement.color.r+",\"g\":"+subElement.color.g+",\"b\":"+subElement.color.b+"},");

					sr.WriteLine ("					\"angle\": {0},",subElement.angle);

					sr.WriteLine ("					\"scale\": {0},",subElement.scale);

					sr.WriteLine ("					\"random\": {0},",subElement.random);

					sr.WriteLine ("					\"random2\": {0},",subElement.random2);

					sr.WriteLine ("					\"RandomScaleSeed\": {0},",subElement.RandomScaleSeed);

					sr.WriteLine ("					\"RandomColorSeedR\": {0},",subElement.RandomColorSeedR);

					sr.WriteLine ("					\"RandomColorSeedG\": {0},",subElement.RandomColorSeedG);

					sr.WriteLine ("					\"RandomColorSeedB\": {0},",subElement.RandomColorSeedB);

					sr.WriteLine ("					\"RandomColorSeedA\": {0}",subElement.RandomColorSeedA);

					count2++;
					if(count2 == element.subElements.Count)
						sr.WriteLine ("				}");
					else
						sr.WriteLine ("				},");
				}

				sr.WriteLine ("			},");
			}

			sr.WriteLine ("			\"EditDynamicTriggering\": {0},",boolToString(element.EditDynamicTriggering));

			sr.WriteLine ("			\"EditOcclusion\": {0},",boolToString(element.EditOcclusion));

			sr.WriteLine ("			\"ElementSetting\": {0},",boolToString(element.ElementSetting));

			sr.WriteLine ("			\"OffsetSetting\": {0},",boolToString(element.OffsetSetting));

			sr.WriteLine ("			\"ColorSetting\": {0},",boolToString(element.ColorSetting));

			sr.WriteLine ("			\"ScaleSetting\": {0},",boolToString(element.ScaleSetting));

			sr.WriteLine ("			\"RotationSetting\": {0},",boolToString(element.RotationSetting));

			sr.WriteLine ("			\"OverrideSetting\": {0},",boolToString(element.OverrideSetting));

			sr.WriteLine ("			\"type\": \"{0}\"",(int)element.type);

			sr.WriteLine ("			\"size\": {\"x\":"+element.size.x+",\"y\":"+element.size.y+"},");

			sr.WriteLine ("			\"SpriteName\": \"{0}\"",element.SpriteName);

			count++;

			if(count == flare.Elements.Count)
				sr.WriteLine ("		}");
			else
				sr.WriteLine ("		},");
		}
		sr.WriteLine ("	}");

		sr.WriteLine ("}");

		sr.WriteLine ("}");

		sr.Close();

		EditorUtility.SetDirty (flare);

	}

	static string boolToString(bool _bool){

		if (_bool)
			return "1";
		else
			return "0";
	}

}
