// Amplify Motion - Full-scene Motion Blur for Unity Pro
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4  || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9
#define UNITY_4
#endif
#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4  || UNITY_5_5 || UNITY_5_6 || UNITY_5_7 || UNITY_5_8 || UNITY_5_9
#define UNITY_5
#endif

using System;
using System.Collections.Generic;
using UnityEngine;
#if !UNITY_4
using UnityEngine.Rendering;
#endif

namespace AmplifyMotion
{
internal class ParticleState : AmplifyMotion.MotionState
{
	public class Particle
	{
		public int refCount;
		public Matrix4x4 prevLocalToWorld;
		public Matrix4x4 currLocalToWorld;
	}

	public ParticleSystem m_particleSystem;
	public ParticleSystemRenderer m_renderer;

	private Mesh m_mesh;

	private ParticleSystem.Particle[] m_particles;
	private Dictionary<uint, Particle> m_particleDict;
	private List<uint> m_listToRemove;
	private Stack<Particle> m_particleStack;
	private int m_capacity;

	private Matrix4x4 m_localToWorld;

	private AnimationCurve m_curveLifeTimeSize;
	private AnimationCurve m_curveSpeedSize;

	private float m_rangeMinSpeedSize;
	private float m_rangeMaxSpeedSize;

	private MaterialDesc[] m_sharedMaterials;

	public bool m_moved = false;
	private bool m_wasVisible;

	public ParticleState( AmplifyMotionCamera owner, AmplifyMotionObjectBase obj )
		: base( owner, obj )
	{
		m_particleSystem = m_obj.GetComponent<ParticleSystem>();
		m_renderer = m_particleSystem.GetComponent<Renderer>().GetComponent<ParticleSystemRenderer>();
	}

	private Mesh CreateQuadMesh()
	{
		Mesh mesh = new Mesh();

		Vector3[] vertices = new Vector3[ 4 ];
		vertices[ 0 ] = new Vector3( -0.5f, -0.5f, 0 );
		vertices[ 1 ] = new Vector3( 0.5f, -0.5f, 0 );
		vertices[ 2 ] = new Vector3( 0.5f, 0.5f, 0 );
		vertices[ 3 ] = new Vector3( -0.5f, 0.5f, 0 );

		int[] tris = new int[ 6 ];

		tris[ 0 ] = 0;
		tris[ 1 ] = 1;
		tris[ 2 ] = 2;
		tris[ 3 ] = 2;
		tris[ 4 ] = 3;
		tris[ 5 ] = 0;

		Vector2[] uv = new Vector2[ 4 ];

		uv[ 0 ] = new Vector2( 0, 0 );
		uv[ 1 ] = new Vector2( 1, 0 );
		uv[ 2 ] = new Vector2( 0, 1 );
		uv[ 3 ] = new Vector2( 1, 1 );

		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = tris;

		return mesh;
	}

	internal override void Initialize()
	{
		if ( m_renderer == null )
		{
			Debug.LogError( "[AmplifyMotion] Invalid Particle Mesh in object " + m_obj.name );
			m_error = true;
			return;
		}

		base.Initialize();

		if ( m_renderer.renderMode == ParticleSystemRenderMode.Mesh )
			m_mesh = m_renderer.mesh;
		else
			m_mesh = CreateQuadMesh();

		m_sharedMaterials = ProcessSharedMaterials( m_renderer.sharedMaterials );

		m_capacity = m_particleSystem.maxParticles;

		m_particleDict = new Dictionary<uint, Particle>( m_capacity );
		m_particles = new ParticleSystem.Particle[ m_capacity ];
		m_listToRemove = new List<uint>( m_capacity );
		m_particleStack = new Stack<Particle>( m_capacity );

		for ( int k = 0; k < m_capacity; k++ )
			m_particleStack.Push( new Particle() );

		//Initialize Size Curves
		m_curveLifeTimeSize = null;
		m_curveSpeedSize = null;
		m_rangeMinSpeedSize = -1f;
		m_rangeMaxSpeedSize = -1f;

		//size by lifetime
		if ( m_obj.ParticleSystemDesc.sizeOverLifeTimeActive )
		{
			m_curveLifeTimeSize = m_obj.ParticleSystemDesc.curveSizeOverLifeTime;
		}
		//size by speed
		if ( m_obj.ParticleSystemDesc.sizeBySpeedActive )
		{
			m_curveSpeedSize = m_obj.ParticleSystemDesc.curveBySpeed;
			m_rangeMinSpeedSize = m_obj.ParticleSystemDesc.speedRangeMin;
			m_rangeMaxSpeedSize = m_obj.ParticleSystemDesc.speedRangeMax;
		}

		m_wasVisible = false;
	}

	Matrix4x4 buildMatrix4x4( ParticleSystem.Particle particle, float sizeParticle )
	{
		Matrix4x4 matrix =
			Matrix4x4.TRS( particle.position, Quaternion.AngleAxis( particle.rotation, particle.axisOfRotation ),
			new Vector3( sizeParticle, sizeParticle, sizeParticle ) );
		return matrix;
	}

	Matrix4x4 buildMatrixBillBoard( ParticleSystem.Particle particle, float sizeParticle )
	{
		Quaternion rotPartBillboard = Quaternion.AngleAxis( particle.rotation, Vector3.back );
		Quaternion rotPartHorizontalBillboard = Quaternion.AngleAxis( particle.rotation, Vector3.forward );

		Vector3 vect = new Vector3( 180f, m_transform.rotation.y, 0f );
		Quaternion rotPartVerticalBillboard = Quaternion.AngleAxis( particle.rotation, vect );

		//BillBoard
		if ( m_renderer.renderMode == ParticleSystemRenderMode.Billboard || m_renderer.renderMode == ParticleSystemRenderMode.Stretch )
		{
			Matrix4x4 matrix =
				Matrix4x4.TRS( particle.position, m_owner.transform.rotation * rotPartBillboard,
				new Vector3( sizeParticle, sizeParticle, sizeParticle ) );
			return matrix;
		}
		//Horizontal BillBoard
		else if ( m_renderer.renderMode == ParticleSystemRenderMode.HorizontalBillboard )
		{
			const float rcpScale = 1.0f / 1.4f;
			float size = sizeParticle * rcpScale;
			Matrix4x4 matrix =
				Matrix4x4.TRS( particle.position, m_transform.rotation * rotPartHorizontalBillboard,
				new Vector3( size, size, size ) ); // HACK: because Unity...
			return matrix;
		}
		//Vertical BillBoard
		else if ( m_renderer.renderMode == ParticleSystemRenderMode.VerticalBillboard )
		{
			const float rcpScale = 1.0f / 1.4f;
			float size = sizeParticle * rcpScale;
			Matrix4x4 matrix =
				Matrix4x4.TRS( particle.position, m_owner.transform.rotation * rotPartVerticalBillboard,
				new Vector3( size, size, size ) ); // HACK: because Unity...
			return matrix;
		}
		//Streched BillBoard
		else
		{
			Matrix4x4 matrix =
				Matrix4x4.TRS( particle.position, m_owner.transform.rotation * rotPartBillboard,
				new Vector3( sizeParticle, sizeParticle, sizeParticle ) );
			return matrix;
		}
	}

	void RemoveDeadParticles()
	{
		m_listToRemove.Clear();

		var enumerator = m_particleDict.GetEnumerator();
		while ( enumerator.MoveNext() )
		{
			KeyValuePair<uint, Particle> pair = enumerator.Current;

			if ( pair.Value.refCount <= 0 )
			{
				m_particleStack.Push( pair.Value );
				if(!m_listToRemove.Contains(pair.Key))
					m_listToRemove.Add( pair.Key );
			}
			else
				pair.Value.refCount = 0;
		}

		for ( int i = 0; i < m_listToRemove.Count; i++ )
			m_particleDict.Remove( m_listToRemove[ i ] );
	}

#if UNITY_4
	internal override void UpdateTransform( bool starting )
#else
	internal override void UpdateTransform( CommandBuffer updateCB, bool starting )
#endif
	{
		if ( !m_initialized || m_capacity != m_particleSystem.maxParticles )
		{
			Initialize();
			return;
		}

		Profiler.BeginSample( "Particle.Update" );

		if ( !starting && m_wasVisible )
		{
			var enumerator = m_particleDict.GetEnumerator();
			while ( enumerator.MoveNext() )
			{
				Particle particle = enumerator.Current.Value;
				particle.prevLocalToWorld = particle.currLocalToWorld;
			}
		}

		m_moved = true;

		int numAlive = m_particleSystem.GetParticles( m_particles );
		float rcpStartLifetime = 1.0f / m_particleSystem.startLifetime;

		for ( int i = 0; i < numAlive; i++ )
		{
			uint randomSeed = m_particles[ i ].randomSeed;
			float sizeParticle = m_particles[ i ].size;

			//Size Life Time change module
			if ( m_curveLifeTimeSize != null )
				sizeParticle = sizeParticle * m_curveLifeTimeSize.Evaluate( 1 - ( m_particles[ i ].lifetime * rcpStartLifetime ) );

			//Size By Speed change module
			if ( m_curveSpeedSize != null && m_rangeMinSpeedSize > 0f && m_rangeMaxSpeedSize > 0f )
			{
				if ( ( m_particles[ i ].velocity.magnitude > m_rangeMinSpeedSize ) && ( m_particles[ i ].velocity.magnitude < m_rangeMaxSpeedSize ) )
					sizeParticle = sizeParticle * m_curveSpeedSize.Evaluate( m_particles[ i ].velocity.magnitude );
				else
					sizeParticle = sizeParticle * m_particles[ i ].size;
			}

			//Mesh Particle
			if ( m_renderer.renderMode == ParticleSystemRenderMode.Mesh )
			{
				Matrix4x4 particleMatrix = buildMatrix4x4( m_particles[ i ], sizeParticle );
				if ( m_particleSystem.simulationSpace == ParticleSystemSimulationSpace.World )
					m_localToWorld = particleMatrix;
				else
					m_localToWorld = m_transform.localToWorldMatrix * particleMatrix;
			}
			//BillBoard Particle
			else
			{
				if ( m_particleSystem.simulationSpace == ParticleSystemSimulationSpace.Local )
					m_particles[ i ].position = m_transform.TransformPoint( m_particles[ i ].position );

				Matrix4x4 particleMatrix = buildMatrixBillBoard( m_particles[ i ], sizeParticle );
				m_localToWorld = particleMatrix;
			}

			Particle particle;
			if ( !m_particleDict.TryGetValue( randomSeed, out particle ) && m_particleStack.Count > 0 )
				m_particleDict[ randomSeed ] = particle = m_particleStack.Pop();

			if ( particle != null )
			{
				particle.refCount = 1;
				particle.currLocalToWorld = m_localToWorld;
			}
		}

		if ( starting || !m_wasVisible )
		{
			var enumerator = m_particleDict.GetEnumerator();
			while ( enumerator.MoveNext() )
			{
				Particle particle = enumerator.Current.Value;
				particle.prevLocalToWorld = particle.currLocalToWorld;
			}
		}

		RemoveDeadParticles();

		m_wasVisible = m_renderer.isVisible;

		Profiler.EndSample();
	}

#if UNITY_4
	internal override void RenderVectors( Camera camera, float scale, AmplifyMotion.Quality quality )
	{
		Profiler.BeginSample( "Particle.Render" );

		if ( m_initialized && !m_error && m_renderer.isVisible )
		{
			bool mask = ( m_owner.Instance.CullingMask & ( 1 << m_obj.gameObject.layer ) ) != 0;
			if ( !mask || ( mask && m_moved ) )
			{
				const float rcp255 = 1 / 255.0f;
				int objectId = mask ? m_owner.Instance.GenerateObjectId( m_obj.gameObject ) : 255;

				Shader.SetGlobalFloat( "_AM_OBJECT_ID", objectId * rcp255 );
				Shader.SetGlobalFloat( "_AM_MOTION_SCALE", mask ? scale : 0 );

				int qualityPass = ( quality == AmplifyMotion.Quality.Mobile ) ? 0 : 2;

				for ( int i = 0; i < m_sharedMaterials.Length; i++ )
				{
					MaterialDesc matDesc = m_sharedMaterials[ i ];
					int pass = qualityPass + ( matDesc.coverage ? 1 : 0 );

					if ( matDesc.coverage )
					{
						m_owner.Instance.SolidVectorsMaterial.mainTexture = matDesc.material.mainTexture;
						if ( matDesc.cutoff )
							m_owner.Instance.SolidVectorsMaterial.SetFloat( "_Cutoff", matDesc.material.GetFloat( "_Cutoff" ) );
					}

					var enumerator = m_particleDict.GetEnumerator();
					while ( enumerator.MoveNext() )
					{
						KeyValuePair<uint, Particle> pair = enumerator.Current;

						Matrix4x4 prevModelViewProj = m_owner.PrevViewProjMatrixRT * pair.Value.prevLocalToWorld;
						Shader.SetGlobalMatrix( "_AM_MATRIX_PREV_MVP", prevModelViewProj );

						if ( m_owner.Instance.SolidVectorsMaterial.SetPass( pass ) )
							Graphics.DrawMeshNow( m_mesh, pair.Value.currLocalToWorld, i );
					}
				}
			}
		}

		Profiler.EndSample();
	}
#else
	internal override void RenderVectors( Camera camera, CommandBuffer renderCB, float scale, AmplifyMotion.Quality quality )
	{
		Profiler.BeginSample( "Particle.Render" );

		if ( m_initialized && !m_error && m_renderer.isVisible )
		{
			bool mask = ( m_owner.Instance.CullingMask & ( 1 << m_obj.gameObject.layer ) ) != 0;
			if ( !mask || ( mask && m_moved ) )
			{
				const float rcp255 = 1 / 255.0f;
				int objectId = mask ? m_owner.Instance.GenerateObjectId( m_obj.gameObject ) : 255;

				renderCB.SetGlobalFloat( "_AM_OBJECT_ID", objectId * rcp255 );
				renderCB.SetGlobalFloat( "_AM_MOTION_SCALE", mask ? scale : 0 );

				int qualityPass = ( quality == AmplifyMotion.Quality.Mobile ) ? 0 : 2;

				for ( int i = 0; i < m_sharedMaterials.Length; i++ )
				{
					MaterialDesc matDesc = m_sharedMaterials[ i ];
					int pass = qualityPass + ( matDesc.coverage ? 1 : 0 );

					matDesc.propertyBlock.Clear();
					if ( matDesc.coverage )
					{
						matDesc.propertyBlock.AddTexture( "_MainTex", matDesc.material.mainTexture );
						if ( matDesc.cutoff )
							matDesc.propertyBlock.AddFloat( "_Cutoff", matDesc.material.GetFloat( "_Cutoff" ) );
					}

					var enumerator = m_particleDict.GetEnumerator();
					while ( enumerator.MoveNext() )
					{
						KeyValuePair<uint, Particle> pair = enumerator.Current;

						Matrix4x4 prevModelViewProj = m_owner.PrevViewProjMatrixRT * pair.Value.prevLocalToWorld;
						renderCB.SetGlobalMatrix( "_AM_MATRIX_PREV_MVP", prevModelViewProj );

						renderCB.DrawMesh( m_mesh, pair.Value.currLocalToWorld, m_owner.Instance.SolidVectorsMaterial, i, pass );
					}
				}
			}
		}

		Profiler.EndSample();
	}
#endif
}
}
