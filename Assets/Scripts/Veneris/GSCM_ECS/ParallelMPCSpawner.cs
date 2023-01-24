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
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;

[BurstCompile]
public struct ParallelMPCSpawner : IJobParallelFor
{
    public Vector3 Corner_bottom_left;
    public float Height;
    public float Width;



    //public Unity.Mathematics.Random random;
    [NativeDisableContainerSafetyRestriction]
    public NativeArray<Unity.Mathematics.Random> rngs;
    [NativeSetThreadIndex]
    readonly int threadId;

    [WriteOnly] public NativeArray<V4> Array;
    public void Execute(int index)
    {
        var rng = rngs[threadId];


        float x_value = Corner_bottom_left.x + rng.NextFloat(0, Width);
        float y_value = 1.2f + rng.NextFloat(0f, 1f);
        float z_value = Corner_bottom_left.z + rng.NextFloat(0, Height);

        rngs[threadId] = rng;

        // Preparing V4 format of the nativearray
        Array[index] = new V4(new Vector3(x_value, y_value, z_value), 0);
    }
}
