/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace Opal
{

	//TODO: Is there some more efficient method to marshal the Unity structures to Optix?? The meshes are only passed during initialization (at least, they should not be passed very frequently), but 
	//the translations matrices are expected to be passed very frequently
	//We cannot modify the declaration of Matrix4x4, Vector3, Vector4... to marshal them sequentially (they are structs) 
	[StructLayout(LayoutKind.Sequential)]
	public struct Matrix4x4ToMarshal {
		public float m00;

		public float m01;

		public float m02;

		public float m03;

		public float m10;

		public float m11;

		public float m12;

		public float m13;

		public float m20;

		public float m21;

		public float m22;

		public float m23;

		public float m30;

		public float m31;

		public float m32;

		public float m33;

		//
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Vector3ToMarshal {
		public float x;
		public float y;
		public float z;

	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Vector2ToMarshal {
		public float x;
		public float y;

	}
	[System.Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct MaterialEMProperties {
		//ITU parameters: depend on frequency
		//RelativePermitivity
		public float a;
		public float b; 
		//Conductivity
		public float c;
		public float d;

	}


	/// <summary>
	/// The interface for accessing the Opal library.
	/// Library must be called in order: 
	/// Init(), add meshes and structures, FinishSceneContext (), transmit, updates.. Exit()
	/// The dll is called opal.dll or opal_s.dll
	/// </summary>
	public class OpalInterface {
		#region Opal Functions
		const string dllName="opal_s";
		[DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Exit ();

		[DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Init ([In] float frequency,    [In] bool useExactSpeedOfLight, [In] bool multiTransmitter);

		[DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SetMaxReflections ([In] uint m);

		[DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SetPrintEnabled ([In] int m);

		[DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SetUsageReport ();

		[DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FinishSceneContext ();

		[DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int AddStaticMeshFromUnity ([In] int meshVertexCount, [In] Vector3ToMarshal[] meshVertices, [In] int meshTriangleCount,[In] int[] meshTriangles, [In] Matrix4x4ToMarshal transformationMatrix, [In] MaterialEMProperties emProp);

		[DllImport(dllName, CallingConvention = CallingConvention.StdCall)]
		public static extern int AddReceiverFromUnity ([In] int id, [In] Vector3ToMarshal position, [In] float radius, [In] IntPtr callback);

		[DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FillRaySphere2D ([In] int elevationSteps, [In] int azimuthSteps, [In] Vector3ToMarshal[] rayDirections);

		[DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int CreateRaySphere2D ([In] int elevationDelta, [In] int azimuthDelta);

		[DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int CreateRaySphere2DSubstep ([In] int elevationDelta, [In] int azimuthDelta);

		[DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int Transmit ([In] int txId, [In] float txPower, [In] Vector3ToMarshal origin,  [In] Vector3ToMarshal polarization);

		//[DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
		//public static extern int Transmit ([In] int txId, [In] float txPower, [In] float ox, [In] float oy, [In] float oz,  [In] float px, [In] float py, [In] float pz);

		[DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int UpdateReceiver ([In] int id,  [In] Vector3ToMarshal position);

		[DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int UpdateReceiverWithRadius ([In] int id,  [In] Vector3ToMarshal position, [In] float radius); 

		[DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int RemoveReceiverFromUnity ([In] int id);

		[DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int AddDynamicMeshGroup ([In] int id);

		[DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int AddMeshToGroupFromUnity ([In] int id , [In] int meshVertexCount, [In] Vector3ToMarshal[] meshVertices, [In] int meshTriangleCount,[In] int[] meshTriangles,  [In] MaterialEMProperties emProp);

		[DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int UpdateTransformInGroup ([In] int id, [In] Matrix4x4ToMarshal transformationMatrix);

		[DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FinishDynamicMeshGroup ([In] int id);

		[DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int RemoveDynamicMeshGroup ([In] int id);





		#endregion
		#region Util
		public static void MarshalMatrix4x4(ref Matrix4x4 u, ref Matrix4x4ToMarshal m) {
			m.m00 = u [0, 0];
			m.m01 = u [0, 1];
			m.m02 = u [0, 2];
			m.m03 = u [0, 3];
			m.m10 = u [1, 0];
			m.m11 = u [1, 1];
			m.m12 = u [1, 2];
			m.m13 = u [1, 3];
			m.m20 = u [2, 0];
			m.m21 = u [2, 1];
			m.m22 = u [2, 2];
			m.m23 = u [2, 3];
			m.m30 = u [3, 0];
			m.m31 = u [3, 1];
			m.m32 = u [3, 2];
			m.m33 = u [3, 3];
			/*Debug.Log (u [0, 0].ToString ("F5"));
			Debug.Log (u [0, 1].ToString ("F5"));
			Debug.Log (u [0, 2].ToString ("F5"));
			Debug.Log (u [0, 3].ToString ("F5"));
			Debug.Log (u [1, 0].ToString ("F5"));
			Debug.Log (u [1, 1].ToString ("F5"));
			Debug.Log (u [1, 2].ToString ("F5"));
			Debug.Log (u [1, 3].ToString ("F5"));
			Debug.Log (u [2, 0].ToString ("F5"));
			Debug.Log (u [2, 1].ToString ("F5"));
			Debug.Log (u [2, 2].ToString ("F5"));
			Debug.Log (u [2, 3].ToString ("F5"));
			Debug.Log (u [3, 0].ToString ("F5"));
			Debug.Log (u [3, 1].ToString ("F5"));
			Debug.Log (u [3, 2].ToString ("F5"));
			Debug.Log (u [3, 3].ToString ("F5"));
			*/



		}
		public static Vector3 ToVector3(Vector3ToMarshal m) {
			return new Vector3 (m.x, m.y, m.z);

		}
		public static Vector3ToMarshal ToMarshal(Transform t) {
			Vector3ToMarshal v;
			v.x = t.position.x;
			v.y = t.position.y;
			v.z = t.position.z;
			return v;
		}
		public static Vector3ToMarshal ToMarshal(Vector3 t) {
			Vector3ToMarshal v;
			v.x = t.x;
			v.y = t.y;
			v.z = t.z;
			return v;
		}
		#endregion
	}
}
