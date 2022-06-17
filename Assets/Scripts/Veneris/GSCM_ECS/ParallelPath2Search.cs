/*
Copyright (c) <2022> <Aleksei Fedorov, aleksei.fedorov@eit.lth.se; Nikita Lyamin, nikita.lyamin@volvocars.com>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Sivert.GSCMECS;

[BurstCompile]
public struct ParallelPath2Search : IJobParallelFor
{
    [ReadOnly] public NativeArray<V6> MPC_Array;
    [ReadOnly] public NativeArray<Vector3> MPC_Dir;

    [ReadOnly] public NativeArray<RaycastCommand> commands;
    [ReadOnly] public NativeArray<RaycastHit> results;
    [ReadOnly] public NativeArray<Vector2Int> ID;

    [ReadOnly] public float maxDistance;
    [ReadOnly] public float angleThreshold;

    [WriteOnly] public NativeArray<SeenPath2> PossiblePath2;
    public void Execute(int index)
    {
        if (results[index].distance == 0)
        {
            float temp_dist = commands[index].distance;
            if (temp_dist < maxDistance)
            {
                Vector3 c1 = MPC_Array[ID[index].x].Coordinates;
                Vector3 n1 = MPC_Array[ID[index].x].Normal;
                Vector3 p1 = MPC_Dir[ID[index].x];
                Vector3 c2 = MPC_Array[ID[index].y].Coordinates;
                Vector3 n2 = MPC_Array[ID[index].y].Normal;
                Vector3 p2 = MPC_Dir[ID[index].y];
                Vector3 dir = commands[index].direction;
                if (Vector3.Dot(dir, n1) > angleThreshold && Vector3.Dot(dir, n2) < -angleThreshold)
                {
                    float aod = Mathf.Acos(Vector3.Dot(dir, n1));
                    float signDep = Mathf.Sign(Vector3.Dot(dir, p1));
                    float aoa = Mathf.Acos(Vector3.Dot(-dir, n2));
                    float signArr = Mathf.Sign(Vector3.Dot(-dir, p2));
                    float thr = (float)1.22;// Mathf.Acos(angleThreshold);

                    // calculating angular gain
                    float gd = AngularGainFunc(aod, thr);
                    float ga = AngularGainFunc(aoa, thr);
                    //float g0 = gd * ga; // according to the Carl's paper, it should be commented
                    float g0 = 1; // according to Carl's Matlab code

                    PossiblePath2[index] = new SeenPath2(ID[index], temp_dist, aod, aoa, signDep, signArr, g0);
                }
            }

        }
    }
    private float AngularGainFunc(float angle, float threshold)
    {
        float Gain = 1;
        if (Mathf.Abs(angle) > threshold)
        { Gain = Mathf.Exp(-12 * (Mathf.Abs(angle) - threshold)); }

        return Gain;
    }
}
