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

using System.Collections;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using UnityEngine;
using System.Threading;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UIElements;

// ZeroMQ dependency for API's TCP/IP connection
using ZeroMQ;

// Flatbuffers used as API cross-platform data structures
using FlatBuffers;

// APIs datastructures definition
using SivertAPI.PosUpd;
using SivertAPI.MsgReceived;


using Sivert.GSCMECS;
using Veneris.Vehicle.Sivert;
using Debug = UnityEngine.Debug;

namespace Veneris.Vehicle
{


    [InitializeOnLoad]
    public class Sivert_API_GSCM_ECS : MonoBehaviour
    {
        // Start is called before the first frame update

        private GameObject RSSsignPtr;
        private double SignFlashTimer;
        private bool ActivateFlashingSign = false;
        public bool UseNS3Binary = true;
        public bool EnableCamerasBehindVehicles = true;
        private GameObject WarningSignPtr;
        private GameObject WarningSignPtrLoS;
        public float timeScale = 1.0f;
        private Transform goTransform;
        public GameObject[] allVehicles;
        public List<int> allVehiclesID;
        private List<GameObject> leftLights;
        private List<GameObject> rightLights;
        public List<MOBILIDMPathTracker> aiPointer;
        public bool ColorIntersections = true;
        public GameObject[] juncts;
        public Material asp;


        public List<Vector3> vehPos;
        private List<AILogic> vehControllers;
        public float tm;
        public string topicToNS = "toNS";
        public string topicFromNS = "fromNS";


        private ZeroMQ.ZContext context;
        private ZeroMQ.ZSocket server;
        private ZeroMQ.ZSocket client;


        private ZeroMQ.ZSocket initNSserver;
        private ZeroMQ.ZSocket intitNSresponse;

        // private Thread ns3Thread;
        private Process NS3thread;
        StringBuilder outputBuilder;
        private String output;

        private float Delay;
        public float SimStart = 3.0f;
        public int VehNumToNS = 2;
        public string PathToNs3Project = "/Users/xxx/xxx/ns-3";
        public enum Scenario{GSCM_11p, LTE_V2X};
        public Scenario Stack_v2x_Tech;

        public enum AppV2x
        {
            Lund_intersection,
            Demo_intersection_assist,
            None
        };

        public AppV2x CITS_scenario;

        // public bool TwoAntennas = false;
        public dynamic GSCMpointer;
        public GSCMstructECS RssGSCMpointer;
        public bool UseGSCM = true;
        private SivertSqLiteLogger PointerToSqLLogger;

        private void Awake()
        {
            Time.timeScale = timeScale;

            // GSCMpointer.SimStart = SimStart;

            if (ColorIntersections)
            {
                juncts = GameObject.FindGameObjectsWithTag("Junction");
                foreach (var jun in juncts)
                {
                    if (jun.GetComponent<MeshRenderer>() == null)
                    {
                        MeshRenderer m = jun.AddComponent<MeshRenderer>();
                        m.sharedMaterial = asp;
                    }
                    else
                    {
                        MeshRenderer mes = jun.GetComponent<MeshRenderer>();
                        mes.sharedMaterial = asp;
                    }
                }

            }



            /*
             * NOTE: We run the NS3 process first, when Unity awakes. Thus, we also synchronise the zero time of Unity3D and NS3.
             */



            // TODO: rewrite shell script to pass scenario module name parameter to it directly, thus, having one call shell script.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) // If we running MacOS
            {
                Debug.Log("We're running MacOS");

                if (Stack_v2x_Tech == Scenario.GSCM_11p)
                {

                    string arg;
                    if (UseNS3Binary) {
                      #if UNITY_EDITOR
                          string LibPath = Application.dataPath;
                          LibPath = LibPath.Replace("Assets", "Packages/SIVERT_NS3_bin");

                      #endif
                      arg = "-e 'tell app \"Terminal\" to do script \"" + LibPath +  "/NS3DaemonForUnity.sh " + LibPath +  " ns3-dev-CITS-SIVERT-11p-spectrum-debug" + "\"'";

                    }
                    else { // Specify correct path to NS3 is want to add another scenario from NS3
                       arg = "-e 'tell app \"Terminal\" to do script \"" + PathToNs3Project +  "/NS3DaemonForUnity.sh " + PathToNs3Project + " build/src/wave/examples/ns3-dev-CITS-SIVERT-11p-spectrum-debug" + "\"'";
                    }

                    NS3thread = System.Diagnostics.Process.Start ("osascript",arg);
                }
                else if (Stack_v2x_Tech == Scenario.LTE_V2X)
                {
                  string arg;

                  if (UseNS3Binary) {
                    #if UNITY_EDITOR
                        string LibPath = Application.dataPath;
                        LibPath = LibPath.Replace("Assets", "Packages/SIVERT_NS3_bin");

                    #endif
                    arg = "-e 'tell app \"Terminal\" to do script \"" + LibPath +  "/NS3DaemonForUnity.sh " + LibPath +  " ns3-dev-CITS-GSCM-SIVERT-lteV2X-spectrum-debug" + "\"'";

                  }
                  else { // Specify correct path to NS3 is want to add another scenario from NS3
                    arg = "-e 'tell app \"Terminal\" to do script \"" + PathToNs3Project +  "/NS3DaemonForUnity.sh " + PathToNs3Project + " build/src/wave/examples/ns3-dev-CITS-GSCM-SIVERT-lteV2X-spectrum-debug" + "\"'";
                  }

                    NS3thread = System.Diagnostics.Process.Start ("osascript",arg);

                }
                else
                {
                    Debug.LogError("Scenario for NS3 is incorrectly specified!");
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) // If we running Linux -- Not yet updated and verified!
            {
                Debug.Log("We're running Linux");
                if (Stack_v2x_Tech == Scenario.GSCM_11p)
                {
                    UseGSCM = true;
                    NS3thread = new Process();
                    NS3thread.StartInfo.UseShellExecute = false;
                    NS3thread.StartInfo.CreateNoWindow = true;
                    NS3thread.StartInfo.RedirectStandardOutput = true;
                    NS3thread.StartInfo.FileName = "/bin/bash";
                    NS3thread.StartInfo.Arguments = PathToNs3Project + "/NS3DaemonForUnity.sh " + PathToNs3Project + " src/wave/examples/ns3-dev-CITS-SIVERT-11p-spectrum-debug";

                    Debug.Log("Starting the: " + NS3thread.StartInfo);
                    NS3thread.Start();
                }
                else if (Stack_v2x_Tech == Scenario.LTE_V2X)
                {
                    NS3thread = new Process();
                    NS3thread.StartInfo.UseShellExecute = false;
                    NS3thread.StartInfo.CreateNoWindow = true;
                    NS3thread.StartInfo.RedirectStandardOutput = true;
                    NS3thread.StartInfo.FileName = "/bin/bash";
                    NS3thread.StartInfo.Arguments = PathToNs3Project + "/NS3DaemonForUnity.sh " + PathToNs3Project + " src/lte/examples/ns3-dev-CITS-GSCM-SIVERT-lteV2X-spectrum-debug";

                    Debug.Log("Starting the: " + NS3thread.StartInfo);
                    NS3thread.Start();

                }
                else
                {
                    Debug.LogError("Scenario for NS3 is incorrectly specified!");
                }
            }
            else
            {
                Debug.LogError("MacOS and Linux are currently only 2 supported OSs :(");
            }

            RSSsignPtr = GameObject.Find("RSS Chart");

            GameObject SumoVehManagerPointer = GameObject.Find("SumoVehicleManager");
            VehNumToNS = SumoVehManagerPointer.GetComponent<SumoVehicleManager>().vehicleGenerationList.Capacity;
            SimStart = SumoVehManagerPointer.GetComponent<SumoVehicleManager>().vehicleGenerationList[VehNumToNS - 1]
                .departTime + 1;
            // Invoke(nameof(StartAPI), SimStart-1);
        }


        void Start()
        {

            PointerToSqLLogger = this.GetComponent<SivertSqLiteLogger>();

            AsyncIO.ForceDotNet.Force();

            context = ZContext.Create();


            try
            {
                Debug.Log("ZeroMQ socket is setup...");
                initNSserver = new ZSocket(context, ZSocketType.REQ);
                initNSserver.Connect("tcp://127.0.0.1:5555");

                Debug.Log("Sending init parameters...");
                initNSserver.SendMore(new ZFrame(Time.fixedDeltaTime.ToString()));
                initNSserver.SendMore(new ZFrame(SimStart.ToString()));
                initNSserver.SendMore(new ZFrame(VehNumToNS.ToString()));
                initNSserver.Send(new ZFrame(UseGSCM.ToString()));

            }
            catch (Exception e)
            {
                initNSserver.Unbind("tcp://127.0.0.1:5555");
                initNSserver.Disconnect("tcp://127.0.0.1:5555");
                initNSserver.Close();
                initNSserver.Dispose();
                Debug.LogError("Init socket gave following error: " + e);
                throw;
            }

            /*
             Until we receive any response from NS, we will be in a blocking mode. Since we're doing this
             in the main unity thread - we actually block Unity engine, until we receive the message from NS.
             This trick should allow us to synch time progress in NS and Unity.
            */

            Invoke(nameof(InitVehicles), SimStart-Time.fixedDeltaTime);

        }

        private void InitVehicles()
        {
            server = new ZSocket(context, ZSocketType.PUB);
            server.Linger = TimeSpan.Zero;
            server.Connect("tcp://127.0.0.1:8002");
            client = new ZSocket(context, ZSocketType.SUB);
            client.Linger = TimeSpan.Zero;
            client.ReceiveTimeout = TimeSpan.FromMilliseconds(1);
            client.Connect("tcp://127.0.0.1:8001");
            client.Subscribe(topicFromNS);


            // Getting pointers to all vehicles
            allVehicles = GameObject.FindGameObjectsWithTag("Vehicle");
            // Getting pointer to GSCM channel
            GameObject gscmGO = GameObject.Find("ManagerGSCM");
            gscmGO.GetComponent<ActivateGSCM>().SimStart = SimStart;
            GSCMpointer = gscmGO.GetComponentInChildren<ChannelGenManager>();



            // Getting current vehicle parameters
            int k = 0;
            foreach (var veh in allVehicles)
            {

                try
                {

                    allVehiclesID[k] = Int16.Parse(veh.name.Substring(8, 1));
                    vehPos[k] = veh.transform.position;

                    k++;
                }
                catch (Exception e)
                {
                    Debug.Log(e + "Growing the size of the vehicles dynamically");
                    vehPos.Add(veh.transform.position);
                    allVehiclesID.Add(Int16.Parse(veh.name.Substring(8, 1)));
                    aiPointer.Add(allVehicles[k].GetComponentInChildren<MOBILIDMPathTracker>());


                    k++;

                }

            }

            k = 0;
            if (EnableCamerasBehindVehicles)
            {
                foreach (var veh in allVehicles)
                {
                    veh.GetComponentInChildren<Camera>().targetDisplay = k + 1;
                    k += 1;
                }
                WarningSignPtr = allVehicles[1].transform.GetChild (13).gameObject;
                WarningSignPtrLoS = allVehicles[0].transform.GetChild (14).gameObject;

            }


        }


        private void FixedUpdate()
        {
            // AsyncIO.ForceDotNet.Force();
            tm = Time.time;

            if (tm >= SimStart)
            {
                // Vehicle 0 brake on key "B" press

                if (Input.GetKey(KeyCode.B))
                {
                    // aiPointer[0].setEnableScenario = true;
                    aiPointer[1].IsEmergencyEnabled = true;
                    // Debug.LogWarning("Vehicle 0 is in emergency brake. Time: " + Time.time);
                }

                if (Input.GetKey(KeyCode.V))
                {
                    // aiPointer[0].setEnableScenario = true;
                    aiPointer[0].IsEmergencyEnabled = true;
                    // Debug.LogWarning("Vehicle 0 is in emergency brake. Time: " + Time.time);
                }

                // Sending the update to API
                var message = new ZMessage();
                message.Add(new ZFrame(string.Format(topicToNS)));
                message.Add(new ZFrame(makeFBposAPI(vehPos, false)));
                server.Send(message);

                // Logging vehicle parameters to database
                LogKinematicsToSqlDB();
                LogRSStoSqlDB();
                if (Stack_v2x_Tech == Scenario.GSCM_11p)
                {
                    RSSsignPtr.GetComponent<TextMesh>().text = "RSS NLoS: " + Math.Round((Double)GSCMpointer.GSCMrss.RSS[2], 2) + " dBm/MHz" + '\u000a' + "RSS LoS: " + Math.Round((Double)GSCMpointer.GSCMrss.RSS[0], 2) + " dBm/MHz"; // Lund scenraio
                    // RSSsignPtr.GetComponent<TextMesh>().text = "RSS NLoS: " + (int)GSCMpointer.GSCMrss.RSS[2] + " dBm/MHz" + '\u000a' + "RSS LoS: " + (int)GSCMpointer.GSCMrss.RSS[0] + " dBm/MHz";
                    // RSSsignPtr.GetComponent<TextMesh>().text = "RSS NLoS: " + (int)GSCMpointer.GSCMrss.RSS[0] + " dBm/MHz" + '\u000a' + "RSS LoS: " + (int)GSCMpointer.GSCMrss.RSS[2] + " dBm/MHz";
                    // RSSsignPtr.GetComponent<TextMesh>().text = "RSS NLoS: " + (int)GSCMpointer.GSCMrss.RSS[0] + " dBm/MHz" + '\u000a';
                }
                
                else
                {
                    // RSSsignPtr.GetComponent<TextMesh>().text = "RSS NLoS: " + Math.Round((Double)GSCMpointer.GSCMrss.RSS[2], 2) + " dBm/15kHz" + '\u000a' + "RSS LoS: " + Math.Round((Double)GSCMpointer.GSCMrss.RSS[0], 2) + " dBm/15kHz";
                    RSSsignPtr.GetComponent<TextMesh>().text = "RSS NLoS: " + Math.Round((Double)GSCMpointer.GSCMrss.RSS[2], 2) + " dBm/15kHz" + '\u000a' + "RSS LoS: " + Math.Round((Double)GSCMpointer.GSCMrss.RSS[0], 2) + " dBm/15kHz";
                }


                //////////////// API messages reception
                ///
                ZError error;
                ZMessage msg;
                ZPollItem pollItem = ZPollItem.CreateReceiver();

                while (client.PollIn(pollItem, out msg, out error, TimeSpan.Zero))
                {
                    try
                    {

                        var content = msg.Unwrap().ReadString();
                        byte[] bytesMSG;
                        bytesMSG = msg.Unwrap().Read();

                        // Read flatbuffer
                        try
                        {
                            ByteBuffer buf = new ByteBuffer(bytesMSG);
                            var MsgFromNS3 = MsgRecAPI.GetRootAsMsgRecAPI(buf);
                            // var pos = MsgFromNS3.Pos.Value;
                            var RxInfo = MsgFromNS3.PackContent.Value;
                            var RxID = MsgFromNS3.PackContent.Value.ReceiverID;
                            var TxID = MsgFromNS3.PackContent.Value.SenderID;
                            var Sent = MsgFromNS3.PackContent.Value.Sent;
                            var NS3time = MsgFromNS3.PackContent.Value.TimeReceived; // NS3 time in nanoseconds
                            string BeaconContent = MsgFromNS3.MsgContent;
                            // int TxID = Int32.Parse(RxInfo.ToString());

                            LogMessageToSqlDB((int)TxID, Stack_v2x_Tech.ToString(), Time.time, (int) TxID, (int) RxID, BeaconContent,
                                (float) GSCMpointer.GSCMrss.RSS[0], Convert.ToInt32(Sent), NS3time*1e-9);
                            if (Sent)
                            {
                                Debug.Log("Message sent by vehicle: " + TxID +  " at time: " +
                                          RxInfo.TimeReceived + " with content: " + BeaconContent + " UnityTime is: " + tm + " NS3 time: " + NS3time
                                          );
                            }
                            else
                            {

                                switch (CITS_scenario)
                                {
                                    case AppV2x.Lund_intersection:
                                        LundScenarioCITSlogic(buf);
                                        break;
                                    case AppV2x.Demo_intersection_assist:
                                        DemoCITSlogic (buf);
                                        break;
                                    case AppV2x.None:
                                        Debug.Log("No CITS scenario is activated");
                                        break;
                                }


                                Debug.Log("Message received by vehicle: " + RxID + " From vehicle: " + TxID + " at time: " +
                                          RxInfo.TimeReceived + " with content: " + BeaconContent + " UnityTime is: " + tm + " NS3 time: " + NS3time);
                            }

                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Can't decode flatbuffer." + " Error: " + e);
                            Time.timeScale = 0; // To pause the run.
                        }

                    }
                    catch (Exception e)
                    {
                        Debug.Log("No message was received at: " + Time.time + " ZError: " + e);
                    }
                }

                ///////////////// end API message receiving



            }
        }


        public void LundScenarioCITSlogic(ByteBuffer buf)
        {
            var MsgFromNS3 = MsgRecAPI.GetRootAsMsgRecAPI(buf);
            var pos = MsgFromNS3.Pos.Value;
            var RxInfo = MsgFromNS3.PackContent.Value;
            var RxID = MsgFromNS3.PackContent.Value.ReceiverID;
            var TxID = MsgFromNS3.PackContent.Value.SenderID;
            string BeaconContent = MsgFromNS3.MsgContent;

            if (TxID == 1 && RxID == 2)
            {
                aiPointer[1].IsEmergencyEnabled = true;
            }

            if (BeaconContent.Substring(0, 5) == "Brake")
            {

                if (BeaconContent.Substring(5, 1) == 2.ToString() && RxID == 0)
                {

                    aiPointer[0].IsEmergencyEnabled = true;
                    if (EnableCamerasBehindVehicles)
                    {
                        // WarningSignPtrLoS.SetActive(true);
                        WarningSignPtr.SetActive(true);
                    }
                    
                }

                // Debug.Log("EEBL from veh: " + TxID + " Recieved by: " + RxID);
                Debug.Log("EEBL from veh: " + TxID + " Recieved by: " + RxID + " at time: " +
                          RxInfo.TimeReceived);
            }

        }


        public void DemoCITSlogic(ByteBuffer buf)
        {
            var MsgFromNS3 = MsgRecAPI.GetRootAsMsgRecAPI(buf);
            var pos = MsgFromNS3.Pos.Value;
            var RxInfo = MsgFromNS3.PackContent.Value;
            var RxID = MsgFromNS3.PackContent.Value.ReceiverID;
            var TxID = MsgFromNS3.PackContent.Value.SenderID;
            string BeaconContent = MsgFromNS3.MsgContent;

            if (TxID == 1 && RxID == 2)
            {

                ActivateFlashingSign = true;

            }

            if (ActivateFlashingSign)
            {
                SignFlashTimer = SignFlashTimer + Time.deltaTime;
                if(SignFlashTimer >= 0.15)
                {
                    WarningSignPtr.SetActive(true);
                }
                if(SignFlashTimer >= 0.3)
                {
                    WarningSignPtr.SetActive(false);
                    SignFlashTimer = 0;
                }

            }

            if (BeaconContent.Substring(0, 5) == "Brake")
            {

                if (BeaconContent.Substring(5, 1) == 2.ToString() && RxID == 0)
                {

                    aiPointer[0].IsEmergencyEnabled = true;
                    WarningSignPtrLoS.SetActive(true);

                }

                // Debug.Log("EEBL from veh: " + TxID + " Recieved by: " + RxID);
                Debug.Log("EEBL from veh: " + TxID + " Recieved by: " + RxID + " at time: " +
                          RxInfo.TimeReceived);
            }

        }



        private byte[] makeFBposAPI(List<Vector3> UnityPos, bool CloseNS3)
        {

            FlatBufferBuilder fbb = new FlatBufferBuilder(128);
            var GSCMrss = GSCMstruct.CreateGSCMstruct(fbb, UseGSCM, 0.0);
            if (UseGSCM)
            {
                GSCMrss = GSCMstruct.CreateGSCMstruct(fbb, UseGSCM, 0);
            }

            PosAPI.StartGSCMvectorVector(fbb, GSCMpointer.GSCMrss.RSS.Count);

            for (int i = 0; i < GSCMpointer.GSCMrss.RSS.Count; i++)
            {
                GscmInfo.CreateGscmInfo(fbb, GSCMpointer.GSCMrss.Links[i].x, GSCMpointer.GSCMrss.Links[i].y, GSCMpointer.GSCMrss.RSS[i]);
            }

            var GSCMoffset = fbb.EndVector();


            int vehId = 0;
            PosAPI.StartCITSVector(fbb, aiPointer.Count);

            for (int i = 0; i < aiPointer.Count; i++)
            {
                MOBILIDMPathTracker ai = aiPointer[GetVehIdByAiPointer(aiPointer[i])];
                if (ai.IsEmergencyEnabled)
                {
                    EEBL.CreateEEBL(fbb, ai.IsEmergencyEnabled, GetVehIdByAiPointer(ai));
                }
                else
                {
                    EEBL.CreateEEBL(fbb, ai.IsEmergencyEnabled, 0);
                }

                vehId++;
            }

            var EEBLoffset = fbb.EndVector();


            PosAPI.StartPosVector(fbb, UnityPos.Count);
            for (int i = 0; i < allVehicles.Length; i++)
            {

                Vector3 p = allVehicles[i].transform.position;
                Vec3API.CreateVec3API(fbb, p.x, p.y, p.z, allVehiclesID[i]);
            }

            var posOffset = fbb.EndVector();


            PosAPI.StartSupplVector(fbb, allVehicles.Length);
            for (int i = 0; i < allVehicles.Length; i++)
            {
                // VehInfo.CreateVehInfo(fbb, GetVehIdByVehGO(allVehicles[i]), tm);
                VehInfo.CreateVehInfo(fbb, allVehiclesID[i], tm);
            }
            var infoOffset = fbb.EndVector();

            // int[] RB = new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            // double[] PSD = new[] {1e-5, 1e-5, 1e-5, 1e-5, 1e-5, 1e-5, 1e-5, 1e-5, 1e-5, 1e-5 };
            // FlatBuffers.Offset<SivertAPI.PosUpd.GSCMChannelPSD>[] spe_offsets =
            //     new FlatBuffers.Offset<SivertAPI.PosUpd.GSCMChannelPSD>[3];


            //////////// GSCM Spectrum
            // int FreqNum = 30;
            // int ChNum = 3;
            int FreqNum = GSCMpointer.GSCMSpectrum.RSSspectrumGain[0].Count;
            int ChNum = GSCMpointer.GSCMSpectrum.Links.Count;

            PosAPI.StartGSCMSpectruChannelsVector(fbb, FreqNum*ChNum);

            for (int ch = 0; ch < ChNum; ch++)
            {
                List<double> SpChan= GSCMpointer.GSCMSpectrum.RSSspectrumGain[ch];
                for (int f = 0; f < FreqNum; f++)
                {
                    SpectrumValue.CreateSpectrumValue(fbb, ch, SpChan[f], f);
                }
            }
            var gscMspectrumOffset = fbb.EndVector();


            PosAPI.StartSpectrumInfoVector(fbb,ChNum);
            for (int i = 0; i < ChNum; i++)
            {
                int TxID = GSCMpointer.GSCMSpectrum.Links[i].x;
                int RxID = GSCMpointer.GSCMSpectrum.Links[i].y;
                Channel.CreateChannel(fbb, TxID, RxID, i);
            }

            var channelInfo = fbb.EndVector();


            ////////////////////////////

            PosAPI.StartPosAPI(fbb);
            PosAPI.AddTerminateNS3(fbb, CloseNS3);
            PosAPI.AddPos(fbb, posOffset);
            PosAPI.AddGSCMvector(fbb, GSCMoffset);
            PosAPI.AddSuppl(fbb, infoOffset);
            PosAPI.AddCITS(fbb, EEBLoffset);
            PosAPI.AddGSCM(fbb, GSCMrss);
            PosAPI.AddGSCMSpectruChannels(fbb, gscMspectrumOffset);
            PosAPI.AddSpectrumInfo(fbb, channelInfo);
            PosAPI.AddNumberOfFrequnciesPerChannel(fbb, FreqNum);

            var offset = PosAPI.EndPosAPI(fbb);
            PosAPI.FinishPosAPIBuffer(fbb,offset);

            return fbb.SizedByteArray();
        }


        private void OnApplicationQuit()
        {
            var message = new ZMessage();
            message.Add(new ZFrame(string.Format(topicToNS)));
            message.Add(new ZFrame(makeFBposAPI(vehPos, true)));
            server.Send(message);

            // server.SendMoreFrame(topicToNS).SendFrame(makeFBposAPI(vehPos, true));
            Debug.Log("Destroying thread for TCP/IP socket between Unity3D and ns3");
            NS3thread.Close();
            server.Unbind("tcp://127.0.0.1:8002");
            client.Unbind("tcp://127.0.0.1:8001");
            server.Close();
            client.Close();

            initNSserver.Unbind("tcp://127.0.0.1:5555");
            initNSserver.Close();

            context.Shutdown();
        }

        private int GetVehIdByVehGO(GameObject go)
        {
            // Debug.Log("Parsing ID by GO: " + go.name.Substring(7));
            return Int32.Parse(go.name.Substring(7));
        }

        private int GetVehIdByAiPointer(MOBILIDMPathTracker aiP)
        {
            // Debug.Log("Parsing ID by AI pointer: " + aiP.transform.root.name.Substring(7));
            return Int32.Parse(aiP.transform.root.name.Substring(7));
        }


        private void LogKinematicsToSqlDB ()
        {
            for (int i = 0; i < allVehicles.Length; i++)
            {

                Vector3 p = allVehicles[i].transform.position;
                MOBILIDMPathTracker ai = aiPointer[GetVehIdByAiPointer(aiPointer[i])];
                PointerToSqLLogger.InsertKinematicsData(allVehiclesID[i], allVehicles[i].name,  Time.fixedTime, p.x, p.z, p.y, allVehicles[i].GetComponent<Rigidbody>().velocity.magnitude);
            }


        }

        private void LogRSStoSqlDB()
        {
            for (int i = 0; i < GSCMpointer.GSCMrss.Links.Count; i++)
            {
                string chName = "GSCMChannel" + GSCMpointer.GSCMrss.Links[i].x.ToString() +
                                GSCMpointer.GSCMrss.Links[i].y.ToString();
                PointerToSqLLogger.InserGSCMData(chName, GSCMpointer.GSCMrss.Links[i].x, GSCMpointer.GSCMrss.Links[i].y, Time.fixedTime, (float)GSCMpointer.GSCMrss.RSS[i]);
            }


        }

        private void LogMessageToSqlDB(int VehID, string V2XStack, float time, int TxID, int RxID, string MessageBody,
            float RSS, int Sent, double NS3time)
        {
            PointerToSqLLogger.InsertMessageData(VehID, V2XStack, time, TxID, RxID, MessageBody, RSS, Sent, NS3time);
        }

    }

}
