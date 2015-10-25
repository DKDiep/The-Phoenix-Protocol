// Amplify Motion - Full-scene Motion Blur for Unity Pro
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.Callbacks;
using UnityEditorInternal;

public class AmplifyMotionObjectEditorBase : Editor
{
	void OnEnable()
	{
		GetValues( target as AmplifyMotionObjectBase );
	}

	[PostProcessBuild]
	static void OnPostprocessBuild( BuildTarget target, string pathToBuiltProject )
	{
		AmplifyMotionObjectBase[] objs = Resources.FindObjectsOfTypeAll( typeof( AmplifyMotionObjectBase ) ) as AmplifyMotionObjectBase[];
		foreach ( AmplifyMotionObjectBase obj in objs )
		{
			if ( obj.Type == AmplifyMotion.ObjectType.Particle )
				GetValues( obj );
		}
	}

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		GetValues( target as AmplifyMotionObjectBase );
	}

	static void GetValues( AmplifyMotionObjectBase obj )
	{
		// if the object is a Particle System (Shuriken)
		ParticleSystem particleSystem = obj.GetComponent<ParticleSystem>();
		if ( particleSystem != null )
		{
			SerializedObject so = null;
			try
			{
				so = new SerializedObject( particleSystem );
			}
			catch
			{
				Debug.LogWarning( "[AmplifyMotion] Can't serialize particle system object " + particleSystem.name + ". Aborting." );
				return;
			}

			obj.ParticleSystemDesc.sizeOverLifeTimeActive = so.FindProperty( "SizeModule.enabled" ).boolValue;
			obj.ParticleSystemDesc.sizeBySpeedActive = so.FindProperty( "SizeBySpeedModule.enabled" ).boolValue;

			if ( obj.ParticleSystemDesc.sizeOverLifeTimeActive )
			{
				// size by lifetime
				obj.ParticleSystemDesc.curveSizeOverLifeTime = so.FindProperty( "SizeModule.curve.maxCurve" ).animationCurveValue;
			}
			if ( obj.ParticleSystemDesc.sizeBySpeedActive )
			{
				// size by speed
				obj.ParticleSystemDesc.curveBySpeed = so.FindProperty( "SizeBySpeedModule.curve.maxCurve" ).animationCurveValue;
				obj.ParticleSystemDesc.speedRangeMin = so.FindProperty( "SizeBySpeedModule.range.x" ).floatValue;
				obj.ParticleSystemDesc.speedRangeMax = so.FindProperty( "SizeBySpeedModule.range.y" ).floatValue;
			}
		}
	}
}
