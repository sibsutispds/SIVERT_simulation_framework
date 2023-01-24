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
using Sivert.GSCMECS;

[BurstCompile]
public struct ParallelRayCastingData : IJobParallelFor
{
    [ReadOnly] public NativeArray<V4> MPC_Array;
    [ReadOnly] public NativeArray<Vector3> OBS_Array;

    [WriteOnly] public NativeArray<RaycastCommand> commands;
    public void Execute(int index)
    {
        int i_obs = Mathf.FloorToInt(index/MPC_Array.Length);
        int i_mpc = index - i_obs * MPC_Array.Length;

        Vector3 temp_direction = MPC_Array[i_mpc].Coordinates - OBS_Array[i_obs];
        commands[index] = new RaycastCommand(OBS_Array[i_obs], temp_direction.normalized, temp_direction.magnitude);
    }
}
