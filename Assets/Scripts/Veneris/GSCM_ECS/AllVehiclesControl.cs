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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Collections;
using Veneris.Vehicle;


public class AllVehiclesControl : MonoBehaviour
{
    public NavMeshAgent[] carsArray;
    public Transform[] destinations;
    public GameObject[] allVehicles;
    public List<GameObject> GSCMAntennas;

    [HideInInspector] public NativeArray<Vector3> OldCoordinates;
    [HideInInspector] public NativeArray<Vector3> CarCoordinates;
    [HideInInspector] public NativeArray<Vector3> CarForwardVect;
    [HideInInspector] public NativeArray<Vector3> CarsSpeed;

    private void OnEnable()
    {
        allVehicles = GameObject.FindGameObjectsWithTag("Vehicle");


        CarCoordinates = new NativeArray<Vector3>(allVehicles.Length, Allocator.Persistent);
        OldCoordinates = new NativeArray<Vector3>(allVehicles.Length, Allocator.Persistent);
        CarsSpeed = new NativeArray<Vector3>(allVehicles.Length, Allocator.Persistent);
        CarForwardVect = new NativeArray<Vector3>(allVehicles.Length, Allocator.Persistent);


        foreach (var Veh in allVehicles)
        {
            GSCMAntennas.Add(Veh.transform.Find("antenna_gscm").gameObject);
        }

    }
    private void OnDestroy()
    {
        CarCoordinates.Dispose();
        OldCoordinates.Dispose();
        CarsSpeed.Dispose();
        CarForwardVect.Dispose();
    }



    void Start()
    {
        // allVehicles = GameObject.FindGameObjectsWithTag("Vehicle");

        for (int i = 0; i < GSCMAntennas.Count; i++)
        {
            // OldCoordinates[i] = GSCMAntennas[i].transform.position;
            int ind = GetVehIdByVehGO(allVehicles[i]);
            CarCoordinates[ind] = GSCMAntennas[i].transform.position;
            CarForwardVect[ind] = GSCMAntennas[i].transform.forward;
        }

    }

    private void FixedUpdate()
    {

        for (int i = 0; i < GSCMAntennas.Count; i++)
        {
            int ind = GetVehIdByVehGO(allVehicles[i]);
            // OldCoordinates[i] = GSCMAntennas[i].transform.position;
            CarCoordinates[ind] = GSCMAntennas[i].transform.position;
            CarForwardVect[ind] = GSCMAntennas[i].transform.forward;

            CarsSpeed[ind] = CarCoordinates[ind] - OldCoordinates[ind];
            OldCoordinates[ind] = CarCoordinates[ind];
        }


    }

    private int GetVehIdByVehGO(GameObject go)
    {
        // Debug.Log("Parsing ID by GO: " + go.name.Substring(7));
        return Int32.Parse(go.name.Substring(7));
    }

}
