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
using Veneris.Vehicle;

public class ActivateGSCM : MonoBehaviour
{
    public float SimStart = 3.0f;
    public GameObject ChannelGenManager;
    public GameObject AllVehiclesStatus;

    // Start is called before the first frame update
    void Awake()
    {
        // ChannelGenManager = GameObject.Find("ChannelGenManager");
        // AllVehiclesStatus = GameObject.Find("AllVehiclesStatus");
        // ChannelGenManager.SetActive(false);
        // AllVehiclesStatus.SetActive(false);
        try
        {
            SimStart = GameObject.Find("SimManagerSIVERT_ECS").GetComponentInChildren<Sivert_API_GSCM_ECS>().SimStart - 0.1f;
        }
        catch (Exception e)
        {
            Debug.LogWarning("SIVERT API Manager Is not found in the scene!");
        }
        Invoke(nameof(ChannelInvoke), SimStart);
    }

    // Update is called once per frame
    void ChannelInvoke()
    {
        AllVehiclesStatus.SetActive(true);
        ChannelGenManager.SetActive(true);



    }

    private void OnApplicationQuit()
    {
        ChannelGenManager.SetActive(false);
        AllVehiclesStatus.SetActive(false);
    }
}
