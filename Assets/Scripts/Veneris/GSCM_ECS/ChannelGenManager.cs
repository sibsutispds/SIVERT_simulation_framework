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
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using System.IO;
using System.Linq;
using System.Numerics;
using Sivert.GSCMECS;
using UnityEngine.Serialization;
using Veneris.Vehicle;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Sivert.GSCMECS
{


    public partial class ChannelGenManager : MonoBehaviour
    {
        public bool PrintRSS = false;
        private String RadiationPetternFile;

        public enum RadiationPattern
        {
            Isotropic,
            DipoleForwardBackward,
            DipoleSideways
        };

        public RadiationPattern AntennaRadiationPattern;
        public bool OmniAntenna = false;

        private float PowerInMilliWatts;
        private float Bandwidth;
        private float PowerPerSubcarrier;
        public float SpeedofLight = 299792458.0f; // m/s
        public float CarrierFrequency = (float) (5.9 * Mathf.Pow(10, 9)); // GHz
        private float fsubcarriers = (float)0.001E9; // kHz
        private Sivert_API_GSCM_ECS.Scenario V2XStack;

        private int LayerToIgnoreForRayTracing;
        private string LayerName = "Vehicle";

        /// <summary>
        ///  Data for Fourier transform
        /// </summary>

        double[] Y_output;

        double[] H_output;

        //double[] Y_noise_output;
        //double[] H_noise_output;
        double[] X_inputValues;

        [Space(10)] [Header("CHARTS FOR DRAWING")] [Space]
        public Transform tfTime;

        public Transform tfFreq;



        // for saving data
        public List<List<string>> H_save = new List<List<string>>(); // for saving data into a csv file
        public List<List<string>> h_save = new List<List<string>>(); // for saving data into a csv file
        public double[] SpectrumGainToNS3;

        // control parameters
        public bool DrawingOverlaps = false;
        public bool DrawingPath1 = false;
        public bool DrawingPath2 = false;
        public bool DrawingPath3 = false;

        // MPCs Data
        NativeArray<V6> DMC_Native;
        NativeArray<Vector3> DMC_perp;
        NativeArray<float> DMC_attenuation;
        int DMC_num;
        NativeArray<V6> MPC1_Native;
        NativeArray<Vector3> MPC1_perp;
        NativeArray<float> MPC1_attenuation;
        int MPC1_num;
        NativeArray<V6> MPC2_Native;
        NativeArray<Vector3> MPC2_perp;
        NativeArray<float> MPC2_attenuation;
        int MPC2_num;
        NativeArray<V6> MPC3_Native;
        NativeArray<Vector3> MPC3_perp;
        NativeArray<float> MPC3_attenuation;
        int MPC3_num;

        NativeArray<Vector2> Pattern;

        int MPC_num;
        NativeArray<V6> MPC_Native;
        NativeArray<Vector3> MPC_perp;
        NativeArray<float> MPC_attenuation;

        // LookUp Table Data
        float maxdistance;
        int maxNumberSeenMPC2;
        int maxNumberSeenMPC3;
        NativeArray<SeenPath2> LookUpTableMPC2; // this array shows all properties of the path2s
        NativeArray<Vector2Int> MPC2LUTID; // this array shows how many paths come from each MPC2
        NativeArray<SeenPath3> LookUpTableMPC3;
        NativeArray<Vector2Int> MPC3SeenID;

        // Coordinates of antennas
        NativeArray<Vector3> OldCoordinates;
        NativeArray<Vector3> CarCoordinates;
        NativeArray<Vector3> CarForwardVect;
        NativeArray<Vector3> CarsSpeed;

        // Creating nativearrays in this script that should be destroyed
        public NativeArray<Vector2Int> Links;

        public GameObject[] allVehicles;
        public GSCMstructECS GSCMrss;
        public GSCMspectrumChannel GSCMSpectrum;
        public float TxPowerInMilliWattsPerMHz = 20; // 20mW * 10 MHZ = 200 mW (23  dBm/10MHz)
        public int NumOfRBinSubchannel = 10;
        private int NumSubCarriersInRB = 12;
        public int NumLteV2Xsubchannles = 3; // 3 subchannels hardcoded r.n.; NB: can be changed, but also need to be changed in ns3
        private List<Vector2Int> TempLinks;
        private List<double> TempRss;

        NativeArray<Overlap> Overlaps1;
        NativeArray<Overlap> TxOverlaps2;
        NativeArray<Overlap> RxOverlaps2;
        NativeArray<Overlap> TxOverlaps3;
        NativeArray<Overlap> RxOverlaps3;
        int link_num;
        int car_num;

        //LoS
        NativeArray<RaycastCommand> commandsLoS;

        NativeArray<RaycastHit> resultsLoS;

        //DMC
        NativeArray<float> SoA0;
        NativeArray<int> Seen0; // either 0 or 1
        NativeArray<RaycastCommand> commands0; // for DMCs
        NativeArray<RaycastHit> results0; // for DMCs
        NativeArray<float> SoA1;
        NativeArray<int> Seen1; // either 0 or 1
        NativeArray<RaycastCommand> commands1; // for MPC1s
        NativeArray<RaycastHit> results1; // for MPC1s
        NativeArray<float> SoA2;
        NativeArray<int> Seen2; // either 0 or 1
        NativeArray<RaycastCommand> commands2; // for MPC2s
        NativeArray<RaycastHit> results2; // for MPC2s
        NativeArray<float> SoA3;
        NativeArray<int> Seen3; // either 0 or 1
        NativeArray<RaycastCommand> commands3; // for MPC3s
        NativeArray<RaycastHit> results3; // for MPC3s

        NativeArray<float> SoA;
        NativeArray<int> Seen; // either 0 or 1
        NativeArray<RaycastCommand> commands; // for DMCs
        NativeArray<RaycastHit> results; // for DMCs

        static int FFTNum = 1024;

        public int NActiveSubcarriers;
        public System.Numerics.Complex[] H = new System.Numerics.Complex[FFTNum]; // Half of LTE BandWidth, instead of 2048 subcarriers

        // public System.Numerics.Complex[] H;


        NativeArray<float> Subcarriers;
        NativeArray<float> InverseWavelengths;
        NativeArray<System.Numerics.Complex> H_LoS;
        NativeArray<System.Numerics.Complex> H_NLoS;


        private void OnEnable()
        {

            /// csv to record spectrum vector
            LayerToIgnoreForRayTracing = ~(1 << LayerMask.NameToLayer(LayerName));
            GameObject SimManagerRef = GameObject.Find("SimManagerSIVERT_ECS");
            V2XStack = GameObject.Find("SimManagerSIVERT_ECS").GetComponentInChildren<Sivert_API_GSCM_ECS>()
                .Stack_v2x_Tech;
            // V2XStack = Sivert_API_GSCM_ECS.Scenario.GSCM_11p;
            if (V2XStack == Sivert_API_GSCM_ECS.Scenario.GSCM_11p)
            {
                FFTNum = 64;
                NActiveSubcarriers = 52;
                Bandwidth = 10; // In MHz
                PowerInMilliWatts = TxPowerInMilliWattsPerMHz * Bandwidth;
                PowerPerSubcarrier = PowerInMilliWatts / FFTNum;
                fsubcarriers = (Bandwidth / FFTNum) * 1000000;
            }

            else if (V2XStack == Sivert_API_GSCM_ECS.Scenario.LTE_V2X)
            {
                // NumLteV2Xsubchannles = 3;
                // NumOfRBinSubchannel = 10;
                // NumSubCarriersInRB = 12;

                FFTNum = 1024;
                fsubcarriers = 15000; // In Hz
                NActiveSubcarriers = 120;
                Bandwidth = fsubcarriers * NActiveSubcarriers; // In MHz
                // PowerInMilliWatts = TxPowerInMilliWatts * Bandwidth/1000000;
                PowerInMilliWatts = (float) (TxPowerInMilliWattsPerMHz *
                                             (NumLteV2Xsubchannles * NumOfRBinSubchannel * 180000 / 1e6));
                PowerPerSubcarrier = (float) (180000 / 1e6) * TxPowerInMilliWattsPerMHz / NumOfRBinSubchannel;
                // fsubcarriers = (Bandwidth/FFTNum)*1000000;
            }
            else
            {
                Debug.LogError("Stack is currently not supported");
            }


            H = new System.Numerics.Complex[FFTNum]; // Half of LTE BandWidth, instead of 2048 subcarriers

            // SubframePower = PowerInMilliWatts / NActiveSubcarriers;
            // Debug.Log("Power per subframe = " + SubframePower);

            Subcarriers = new NativeArray<float>(FFTNum, Allocator.Persistent);
            InverseWavelengths = new NativeArray<float>(FFTNum, Allocator.Persistent);

            for (int i = 0; i < FFTNum; i++)
            {
                Subcarriers[i] = CarrierFrequency + fsubcarriers * i;
                InverseWavelengths[i] = Subcarriers[i] / SpeedofLight;
            }

            //// EADF////
            ///
#if UNITY_EDITOR
            string antennaF = Application.dataPath;
            antennaF = antennaF.Replace("Assets", "Antennas/EADF");
            Debug.Log("Reading antenna from: " + antennaF + " Antenna chosen: " + AntennaRadiationPattern.ToString());
            // p1 is path to project folder...
#endif

            switch (AntennaRadiationPattern)
            {
                case RadiationPattern.Isotropic:
                    RadiationPetternFile = antennaF + "/IsotropicEADF.csv";
                    break;
                case RadiationPattern.DipoleForwardBackward:
                    RadiationPetternFile = antennaF + "/FwrdDipoleEADF.csv";
                    break;
                case RadiationPattern.DipoleSideways:
                    RadiationPetternFile = antennaF + "/SideDipoleEADF.csv";
                    break;
            }


            List<float> listA = new List<float>();
            List<float> listB = new List<float>();

            using (var reader = new StreamReader(@RadiationPetternFile))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    //(float)Convert.ToDouble("41.00027357629127");
                    listA.Add((float) Convert.ToDouble(values[0]));
                    listB.Add((float) Convert.ToDouble(values[1]));
                }

                //Debug.Log(listA.Count);
            }

            Pattern = new NativeArray<Vector2>(listA.Count, Allocator.Persistent); //new Vector2[17];

            for (int i = 0; i < listA.Count; i++)
            {
                Pattern[i] = new Vector2(listA[i], listB[i]);
            }

        }

        private void OnDestroy()
        {
            Pattern.Dispose();
            Links.Dispose();
            Overlaps1.Dispose();
            TxOverlaps2.Dispose();
            RxOverlaps2.Dispose();
            TxOverlaps3.Dispose();
            RxOverlaps3.Dispose();

            commandsLoS.Dispose();
            resultsLoS.Dispose();

            SoA.Dispose();
            Seen.Dispose();
            commands.Dispose();
            results.Dispose();

            Subcarriers.Dispose();
            InverseWavelengths.Dispose();
            H_LoS.Dispose();

            H_NLoS.Dispose();
        }

        private void OnDisable()
        {

            string path1 = Application.persistentDataPath + "/h_time1.csv";

            using (var file = File.CreateText(path1))
            {
                foreach (var arr in h_save)
                {
                    //if (String.IsNullOrEmpty(arr)) continue;
                    file.Write(arr[0]);
                    for (int i = 1; i < arr.Count; i++)
                    {
                        file.Write(',');
                        file.Write(arr[i]);
                    }

                    file.WriteLine();
                }
            }

            string path2 = Application.persistentDataPath + "/H_freq1.csv";

            using (var file = File.CreateText(path2))
            {
                foreach (var arr in H_save)
                {
                    //if (String.IsNullOrEmpty(arr)) continue;
                    file.Write(arr[0]);
                    for (int i = 1; i < arr.Count; i++)
                    {
                        file.Write(',');
                        file.Write(arr[i]);
                    }

                    file.WriteLine();
                }
            }

            Debug.Log("The lenght of the list<list> structure " + h_save.Count);
        }

        void Start()
        {

            /// for Fourier transform
            X_inputValues = new double[H.Length];
            for (int i = 0; i < H.Length; i++)
            {
                X_inputValues[i] = i;
            }

            #region Reading Info from Scripts

            #region MPCs

            // MPCs Data
            GameObject MPC_Spawner = GameObject.Find("CorrectPolygons");
            Correcting_polygons_Native MPC_Native_Script = MPC_Spawner.GetComponent<Correcting_polygons_Native>();

            DMC_num = MPC_Native_Script.ActiveV6_DMC_NativeList.Length;
            MPC1_num = MPC_Native_Script.ActiveV6_MPC1_NativeList.Length;
            MPC2_num = MPC_Native_Script.ActiveV6_MPC2_NativeList.Length;
            MPC3_num = MPC_Native_Script.ActiveV6_MPC3_NativeList.Length;

            MPC_num = MPC_Native_Script.ActiveV6_MPC_NativeList.Length;

            DMC_Native = MPC_Native_Script.ActiveV6_DMC_NativeList;
            MPC1_Native = MPC_Native_Script.ActiveV6_MPC1_NativeList;
            MPC2_Native = MPC_Native_Script.ActiveV6_MPC2_NativeList;
            MPC3_Native = MPC_Native_Script.ActiveV6_MPC3_NativeList;

            MPC_Native = MPC_Native_Script.ActiveV6_MPC_NativeList;

            DMC_attenuation = MPC_Native_Script.ActiveV6_DMC_Power;
            MPC1_attenuation = MPC_Native_Script.ActiveV6_MPC1_Power;
            MPC2_attenuation = MPC_Native_Script.ActiveV6_MPC2_Power;
            MPC3_attenuation = MPC_Native_Script.ActiveV6_MPC3_Power;

            MPC_attenuation = MPC_Native_Script.ActiveV6_MPC_Power;

            DMC_perp = MPC_Native_Script.Active_DMC_Perpendiculars;
            MPC1_perp = MPC_Native_Script.Active_MPC1_Perpendiculars;
            MPC2_perp = MPC_Native_Script.Active_MPC2_Perpendiculars;
            MPC3_perp = MPC_Native_Script.Active_MPC3_Perpendiculars;

            MPC_perp = MPC_Native_Script.Active_MPC_Perpendiculars;

            #endregion

            #region LookUpTable

            // LookUp Table Data
            GameObject LookUpTable = GameObject.Find("LookUpTable");
            LookUpTableGenUpd LUT_Script = LookUpTable.GetComponent<LookUpTableGenUpd>();
            maxdistance = LUT_Script.MaxSeenDistance;
            maxNumberSeenMPC2 = LUT_Script.maxlengthMPC2;
            maxNumberSeenMPC3 = LUT_Script.maxlengthMPC3;

            LookUpTableMPC2 = LUT_Script.LookUpTableMPC2;
            MPC2LUTID = LUT_Script.MPC2LUTID;
            LookUpTableMPC3 = LUT_Script.LookUpTableMPC3;
            MPC3SeenID = LUT_Script.MPC3SeenID;

            #endregion

            #region Positions

            // Coordinates of antennas
            allVehicles = GameObject.FindGameObjectsWithTag("Vehicle");
            GameObject VehiclesData = GameObject.Find("AllVehiclesStatus");
            AllVehiclesControl ControlScript = VehiclesData.GetComponent<AllVehiclesControl>();

            OldCoordinates = ControlScript.OldCoordinates;
            CarCoordinates = ControlScript.CarCoordinates;
            CarForwardVect = ControlScript.CarForwardVect;
            CarsSpeed = ControlScript.CarsSpeed;

            #endregion

            #endregion

            car_num = CarCoordinates.Length;

            link_num = car_num * (car_num - 1) / 2;

            Links = new NativeArray<Vector2Int>(link_num, Allocator.Persistent);
            Overlaps1 = new NativeArray<Overlap>(link_num, Allocator.Persistent);
            TxOverlaps2 = new NativeArray<Overlap>(link_num, Allocator.Persistent);
            RxOverlaps2 = new NativeArray<Overlap>(link_num, Allocator.Persistent);
            TxOverlaps3 = new NativeArray<Overlap>(link_num, Allocator.Persistent);
            RxOverlaps3 = new NativeArray<Overlap>(link_num, Allocator.Persistent);

            // LoS
            commandsLoS = new NativeArray<RaycastCommand>(2*link_num, Allocator.Persistent);
            resultsLoS = new NativeArray<RaycastHit>(2*link_num, Allocator.Persistent);


            // MPCs
            SoA = new NativeArray<float>( (DMC_num + MPC1_num + MPC2_num + MPC3_num) * car_num, Allocator.Persistent);
            Seen = new NativeArray<int>((DMC_num + MPC1_num + MPC2_num + MPC3_num) * car_num, Allocator.Persistent);
            commands = new NativeArray<RaycastCommand>((DMC_num + MPC1_num + MPC2_num + MPC3_num) * car_num, Allocator.Persistent);
            results = new NativeArray<RaycastHit>((DMC_num + MPC1_num + MPC2_num + MPC3_num) * car_num, Allocator.Persistent);

            TempLinks = new List<Vector2Int>();
            TempRss = new List<double>();


            int link_count = 0;
            GSCMSpectrum.RSSspectrumGain = new List<List<double>>();
            for (int i = 0; i < car_num; i++)
            {

                for (int j = i + 1; j < car_num; j++)
                {
                    int TxVeh = GetVehIdByVehGO(allVehicles[i]);
                    int RxVeh = GetVehIdByVehGO(allVehicles[j]);
                    Links[link_count] = new Vector2Int(TxVeh, RxVeh);
                    TempLinks.Add(Links[link_count]);
                    TempRss.Add(Double.NegativeInfinity);
                    // GSCMrss.Links.Add(Links[link_count]);
                    // GSCMrss.RSS.Add(0.0);
                    // Links[link_count] = new Vector2Int(i, j);
                    link_count += 1;
                }

                if (V2XStack == Sivert_API_GSCM_ECS.Scenario.LTE_V2X)
                {
                    List<double> TempRssSpectrum = new List<double>();
                    for (int j = 0; j < NumOfRBinSubchannel * NumLteV2Xsubchannles; j++)
                    {
                        TempRssSpectrum.Add(Double.NegativeInfinity);
                    }

                    GSCMSpectrum.RSSspectrumGain.Add(TempRssSpectrum);

                }
                else if (V2XStack == Sivert_API_GSCM_ECS.Scenario.GSCM_11p)
                {
                    List<double> TempRssSpectrum = new List<double>();
                    for (int j = 0;
                        j < FFTNum * 3;
                        j++) // we make it 3 times bandwidth for interference model in ns3: bw +/- bw
                    {
                        TempRssSpectrum.Add(Double.NegativeInfinity);
                    }

                    GSCMSpectrum.RSSspectrumGain.Add(TempRssSpectrum);
                }
                else
                {
                    Debug.LogError("GSCM Spectrum model is currenlty suppored for only GSCM11p and LTEV2X scenarios");
                }

            }


            GSCMrss.Links = TempLinks;
            GSCMrss.RSS = TempRss;
            GSCMSpectrum.Links = TempLinks;

            // GSCMrss.Links[0] = new Vector2Int(0, 2);
            // GSCMrss.Links[1] = new Vector2Int(2, 0);




            // Channels for all links
            H_LoS = new NativeArray<Complex>(FFTNum * link_num, Allocator.Persistent);
            H_NLoS = new NativeArray<Complex>(FFTNum * link_num, Allocator.Persistent);




        }

        //private void FixedUpdate()
        private void FixedUpdate()
        {


            #region Defining edges of seen areas for all links (The duration is about 20 micro seconds)

            AreaOverlaps areaOverlaps = new AreaOverlaps
            {
                MaxDist = maxdistance,
                Coordinates = CarCoordinates,
                Links = Links,

                AreaArray1 = Overlaps1,
                TxAreaArray2 = TxOverlaps2,
                RxAreaArray2 = RxOverlaps2,
                TxAreaArray3 = TxOverlaps3,
                RxAreaArray3 = RxOverlaps3,
            };
            JobHandle findoverlaps = areaOverlaps.Schedule(link_num, 1);
            findoverlaps.Complete();

            if (DrawingOverlaps)
            {
                DrawOverlaps(Overlaps1);
                DrawOverlaps(TxOverlaps2);
                DrawOverlaps(TxOverlaps3);
            }

            #endregion


            #region DrawOverlaps Function

            void DrawOverlaps(NativeArray<Overlap> Array)
            {
                for (int i = 0; i < Array.Length; i++)
                {
                    if (Array[i].InfInf != new Vector2(0, 0))
                    {
                        Vector3 II = new Vector3(Array[i].InfInf.x, 0, Array[i].InfInf.y);
                        Vector3 IS = new Vector3(Array[i].SupSup.x, 0, Array[i].InfInf.y);
                        Vector3 SI = new Vector3(Array[i].InfInf.x, 0, Array[i].SupSup.y);
                        Vector3 SS = new Vector3(Array[i].SupSup.x, 0, Array[i].SupSup.y);

                        if (i == 0)
                        {
                            Debug.DrawLine(II, IS, Color.cyan);
                            Debug.DrawLine(IS, SS, Color.cyan);
                            Debug.DrawLine(SS, SI, Color.cyan);
                            Debug.DrawLine(SI, II, Color.cyan);
                        }
                        else if (i == 1)
                        {
                            Debug.DrawLine(II + new Vector3(0, 1, 0), IS + new Vector3(0, 1, 0), Color.red);
                            Debug.DrawLine(IS + new Vector3(0, 1, 0), SS + new Vector3(0, 1, 0), Color.red);
                            Debug.DrawLine(SS + new Vector3(0, 1, 0), SI + new Vector3(0, 1, 0), Color.red);
                            Debug.DrawLine(SI + new Vector3(0, 1, 0), II + new Vector3(0, 1, 0), Color.red);
                        }
                        else
                        {
                            Debug.DrawLine(II + new Vector3(0, 2, 0), IS + new Vector3(0, 2, 0), Color.blue);
                            Debug.DrawLine(IS + new Vector3(0, 2, 0), SS + new Vector3(0, 2, 0), Color.blue);
                            Debug.DrawLine(SS + new Vector3(0, 2, 0), SI + new Vector3(0, 2, 0), Color.blue);
                            Debug.DrawLine(SI + new Vector3(0, 2, 0), II + new Vector3(0, 2, 0), Color.blue);
                        }
                    }
                }
            }

            #endregion


            #region LoS Channel Calculation

            ParallelLoSDetection LoSDetection = new ParallelLoSDetection
            {
                CarsPositions = CarCoordinates,
                Links = Links,
                LayerMaskToIgnore = LayerToIgnoreForRayTracing, // Ignore vehicle colliders
                commands = commandsLoS,
            };
            JobHandle LoSDetectionHandle = LoSDetection.Schedule(link_num, 1);
            LoSDetectionHandle.Complete();
            // parallel raycasting
            JobHandle rayCastJobLoS = RaycastCommand.ScheduleBatch(commandsLoS, resultsLoS, 1, default);
            rayCastJobLoS.Complete();

            ParallelLoSChannel LoSChannel = new ParallelLoSChannel
            {
                OmniAntennaFlag = OmniAntenna,
                FFTSize = FFTNum,
                CarsPositions = CarCoordinates,
                CarsFwd = CarForwardVect,
                Links = Links,
                raycastresults = resultsLoS,
                inverseLambdas = InverseWavelengths,
                Pattern = Pattern,

                HLoS = H_LoS,
            };
            JobHandle LoSChannelHandle = LoSChannel.Schedule(H_LoS.Length, 64);
            LoSChannelHandle.Complete();

            #endregion

            // var layer = 1 << LayerMask.NameToLayer("AntennaRaycast");
            // var layer = ~(1 << LayerMask.NameToLayer("VehicleSafetyArea"));
            ParallelRayCastingDataCars RayCastingData = new ParallelRayCastingDataCars
            {
                CastingDistance = maxdistance,
                Cars_Positions = CarCoordinates,
                MPC_Array = MPC_Native,
                MPC_Perpendiculars = MPC_perp,
                LayerMaskToIgnore = LayerToIgnoreForRayTracing,  // Ignore vehicle colliders

                SoA = SoA,
                SeenIndicator = Seen,
                commands = commands,
            };
            JobHandle jobHandle_RayCastingData = RayCastingData.Schedule(MPC_num * car_num, 16);
            //jobHandle_RayCastingData0.Complete();

            // parallel raycasting
            JobHandle rayCastJob = RaycastCommand.ScheduleBatch(commands, results, 16, jobHandle_RayCastingData);
            rayCastJob.Complete();

            var map = new NativeMultiHashMap<int, Path_and_IDs>(1000000, Allocator.TempJob);
            var idarray = new NativeArray<int>(MPC_num * link_num, Allocator.TempJob);
            ChannelParametersAll channelParameters = new ChannelParametersAll
            {
                OmniAntennaFlag = OmniAntenna,
                MPC_Attenuation = MPC_attenuation,
                MPC_Array = MPC_Native,
                MPC_Perp = MPC_perp,
                DMCNum = DMC_num,
                MPC1Num = MPC1_num,
                MPC2Num = MPC2_num,
                MPC3Num = MPC3_num,
                MPCNum = MPC_num,

                LookUpTableMPC2 = LookUpTableMPC2,
                LUTIndexRangeMPC2 = MPC2LUTID,
                LookUpTableMPC3 = LookUpTableMPC3,
                LUTIndexRangeMPC3 = MPC3SeenID,

                ChannelLinks = Links,
                CarsCoordinates = CarCoordinates,
                CarsForwardsDir = CarForwardVect,

                Commands = commands,
                Results = results,
                SignOfArrival = SoA,
                SeenIndicator = Seen,

                Pattern = Pattern,

                IDArray = idarray,
                HashMap = map.AsParallelWriter(),
            };
            JobHandle channelParametersJob = channelParameters.Schedule(MPC_num * link_num, 4);
            channelParametersJob.Complete();

            #region Drwaing possible paths

            int Path1Count = 0;
            int Path2Count = 0;
            int Path3Count = 0;

            if (DrawingPath1 || DrawingPath2 || DrawingPath3)
            {
                for (int i = 2 * MPC_num; i < 3 * MPC_num; i++)
                {
                    if (map.TryGetFirstValue(i, out Path_and_IDs path,
                        out NativeMultiHashMapIterator<int> nativeMultiHashMapIterator))
                    {
                        do
                        {
                            if (path.PathOrder == 1 && DrawingPath1)
                            {
                                Path1Count += 1;
                                Vector3 car1_coor = CarCoordinates[path.ChainIDs.Car1];
                                Vector3 car2_coor = CarCoordinates[path.ChainIDs.Car2];
                                Vector3 MPC_coor1 = MPC_Native[path.ChainIDs.ID1].Coordinates;
                                Debug.DrawLine(car1_coor, MPC_coor1, Color.white);
                                Debug.DrawLine(MPC_coor1, car2_coor, Color.white);
                            }
                            else if (path.PathOrder == 2 && DrawingPath2)
                            {
                                Path2Count += 1;
                                Vector3 car1_coor = CarCoordinates[path.ChainIDs.Car1];
                                Vector3 car2_coor = CarCoordinates[path.ChainIDs.Car2];
                                Vector3 MPC_coor1 = MPC_Native[path.ChainIDs.ID1].Coordinates;
                                Vector3 MPC_coor2 = MPC_Native[path.ChainIDs.ID2].Coordinates;
                                Debug.DrawLine(car1_coor, MPC_coor1, Color.blue);
                                Debug.DrawLine(MPC_coor1, MPC_coor2, Color.blue);
                                Debug.DrawLine(MPC_coor2, car2_coor, Color.blue);
                            }
                            else if (path.PathOrder == 3 && DrawingPath3)
                            {
                                Path3Count += 1;
                                Vector3 car1_coor = CarCoordinates[path.ChainIDs.Car1];
                                Vector3 car2_coor = CarCoordinates[path.ChainIDs.Car2];
                                Vector3 MPC_coor1 = MPC_Native[path.ChainIDs.ID1].Coordinates;
                                Vector3 MPC_coor2 = MPC_Native[path.ChainIDs.ID2].Coordinates;
                                Vector3 MPC_coor3 = MPC_Native[path.ChainIDs.ID3].Coordinates;
                                Debug.DrawLine(car1_coor, MPC_coor1, Color.green);
                                Debug.DrawLine(MPC_coor1, MPC_coor2, Color.green);
                                Debug.DrawLine(MPC_coor2, MPC_coor3, Color.green);
                                Debug.DrawLine(MPC_coor3, car2_coor, Color.green);
                                //Debug.Log(20 * Mathf.Log10(path3.AngularGain));
                            }
                        } while (map.TryGetNextValue(out path, ref nativeMultiHashMapIterator));
                    }
                }
                Debug.Log("Pathes of order 1: " + Path1Count + " Pathes of order 2: " + Path2Count + " Pathes of order 3: " + Path3Count);
            }





            #endregion

            //float t_filt = Time.realtimeSinceStartup;
            NativeList<int> nonzero_indexes = new NativeList<int>(Allocator.TempJob);
            IndexNonZeroFilter nzindexes = new IndexNonZeroFilter
            {
                Array = idarray,
            };
            JobHandle jobHandleIndexNonZeroFilter = nzindexes.ScheduleAppend(nonzero_indexes, idarray.Length, 64);
            jobHandleIndexNonZeroFilter.Complete();

            NativeArray<Vector2Int> link_ids = new NativeArray<Vector2Int>(link_num, Allocator.TempJob);
            LinkIndexes linkIndexes = new LinkIndexes
            {
                MPCNum = MPC_num,
                NonZeroIndexes = nonzero_indexes,

                LinkIDs = link_ids,
            };
            JobHandle jobHandlelinkIndexes = linkIndexes.Schedule(link_num, 1, jobHandleIndexNonZeroFilter);
            //jobHandlelinkIndexes.Complete();
            //Debug.Log("Time spent for filtering: " + ((Time.realtimeSinceStartup - t_filt) * 1000f) + " ms");

            //float t_chan = Time.realtimeSinceStartup;
            ParallelChannel parallelChannel = new ParallelChannel
            {
                FFTSize = FFTNum,
                CarsPositions = CarCoordinates,
                CarsFwd = CarForwardVect,
                InverseLambdas = InverseWavelengths,
                HashMap = map,
                MPCNum = MPC_num,
                LinkNum = link_num,
                NonZeroIndexes = nonzero_indexes,
                LinkIDs = link_ids,

                H_NLoS = H_NLoS,
            };
            JobHandle parallelChannelJob = parallelChannel.Schedule(FFTNum * link_num, 1, jobHandlelinkIndexes);
            parallelChannelJob.Complete();
            //Debug.Log("Time spent for channel calculation: " + ((Time.realtimeSinceStartup - t_chan) * 1000f) + " ms");

            // Debug.Log("Time spent for Raycasting all at once: " + ((Time.realtimeSinceStartup - t_all) * 1000f) + " ms");

            #region FFT operation

            //float t_fft = Time.realtimeSinceStartup;
            System.Numerics.Complex[] outputSignal_Freq = FastFourierTransform.FFT(H, true);


            for (int chN = 0; chN < GSCMrss.RSS.Count; chN++)
            {
                double RSS = 0;

                for (int i = chN * H.Length; i < (chN + 1) * H.Length; i++)
                {
                    H[i % H.Length] = H_LoS[i] + H_NLoS[i];
                }

                Y_output = new double[H.Length];
                H_output = new double[H.Length];

                List<string> h_snapshot = new List<string>();
                List<string> H_snapshot = new List<string>();

                for (int i = 0; i < H.Length; i++)
                {

                    Y_output[i] = 10 *
                                  Mathf.Log10(Mathf.Pow((float) System.Numerics.Complex.Abs(outputSignal_Freq[i]), 2) +
                                              0.0000000000001f);
                    H_output[i] = 10 *
                                  Mathf.Log10(
                                      Mathf.Pow((float) System.Numerics.Complex.Abs(H[i]), 2) + 0.0000000000001f);


                    // procedure to write to a file
                    string h_string = Mathf.Pow((float) System.Numerics.Complex.Abs(outputSignal_Freq[i]), 2)
                        .ToString();
                    h_snapshot.Add(h_string); // channel in time domain

                    // apparently, we need to convert a complex number to a string using such a weird method QUICK FIX

                    string H_string;
                    if (H[i].Imaginary > 0)
                    {
                        H_string = H[i].Real.ToString() + "+" + H[i].Imaginary.ToString() + "i";
                    }
                    else // in case of negative imaginary part
                    {
                        H_string = H[i].Real.ToString() + H[i].Imaginary.ToString() + "i";
                    }

                    H_snapshot.Add(H_string); // channel in frequence domain

                }

                if (V2XStack == Sivert_API_GSCM_ECS.Scenario.LTE_V2X)
                {
                    List<double> SpectrumGainChannel = new List<double>();
                    for (int Nfr = 0; Nfr < NumOfRBinSubchannel * NumLteV2Xsubchannles; Nfr++)
                    {
                        SpectrumGainChannel.Add(0.0);
                    }

                    for (int SubCh = 0; SubCh < NumLteV2Xsubchannles; SubCh++)
                    {
                        for (int Nfr = 0; Nfr < NumOfRBinSubchannel; Nfr++)
                        {
                            double SpectrumPowerRB = 0;
                            // int SubCarPerFreqBin = NActiveSubcarriers / NumOfRBinSubchannel;
                            for (int i = 0; i < NumSubCarriersInRB; i++)
                            {
                                int ind = (SubCh + 1) * (Nfr) * NumSubCarriersInRB + i;
                                SpectrumPowerRB += Mathf.Pow((float) System.Numerics.Complex.Abs(H[ind]), 2) *
                                                   PowerPerSubcarrier;
                            }

                            SpectrumGainChannel[Nfr + NumOfRBinSubchannel * (SubCh)] =
                                SpectrumPowerRB / (NumSubCarriersInRB * PowerPerSubcarrier);

                        }

                    }

                    RSS = SpectrumGainChannel.Take(NumOfRBinSubchannel).Sum();
                    RSS *= (PowerInMilliWatts / NumOfRBinSubchannel); // adjusted after discussion
                    RSS /= NumLteV2Xsubchannles;
                    RSS /= NumOfRBinSubchannel;
                    GSCMSpectrum.RSSspectrumGain[chN] = SpectrumGainChannel;
                    GSCMrss.RSS[chN] = 10 * Mathf.Log10((float) RSS);
                }
                else if (V2XStack == Sivert_API_GSCM_ECS.Scenario.GSCM_11p)
                {
                    List<double> SpectrumGainChannel = new List<double>();
                    SpectrumGainChannel.AddRange(Enumerable.Repeat(0.0, FFTNum * 3));
                    for (int Nfr = 0; Nfr < FFTNum; Nfr++)
                    {
                        // SpectrumGainChannel.Add(Mathf.Pow((float) System.Numerics.Complex.Abs(H[Nfr]), 2) );
                        // We now duplicate the channel for interference model in wave ns3 that considers Bandwidth +/- bandwidth
                        SpectrumGainChannel[Nfr] = Mathf.Pow((float) System.Numerics.Complex.Abs(H[Nfr]), 2);
                        SpectrumGainChannel[Nfr + FFTNum] = SpectrumGainChannel[Nfr];
                        SpectrumGainChannel[Nfr + 2 * FFTNum] = SpectrumGainChannel[Nfr];
                    }

                    RSS = SpectrumGainChannel.Take(FFTNum).Sum() * (PowerInMilliWatts/ FFTNum);

                    GSCMSpectrum.RSSspectrumGain[chN] = SpectrumGainChannel;
                    // We now duplicate the channel for interference model in wave ns3 that considers Bandwidth +/- bandwidth
                    GSCMrss.RSS[chN] =
                        10 * Mathf.Log10((float) RSS /
                                         Bandwidth); // Calculate mean RSS per 1 MHz to send to NS3 for pathLoss
                }




                if (PrintRSS)
                {
                    float RSSLoS = 0;
                    float RSSNLoS = 0;

                    for (int i = chN * H.Length; i < (chN + 1) * H.Length; i++)
                    {
                        // RSS += Mathf.Pow((float) System.Numerics.Complex.Abs(H[i]), 2);
                        RSS += Mathf.Pow((float)System.Numerics.Complex.Abs(H[i]), 2) * (PowerInMilliWatts / FFTNum);
                        RSSLoS += Mathf.Pow((float)System.Numerics.Complex.Abs(H_LoS[i]), 2) * (PowerInMilliWatts / FFTNum);
                        RSSNLoS += Mathf.Pow((float)System.Numerics.Complex.Abs(H_NLoS[i]), 2) * (PowerInMilliWatts / FFTNum);
                    }
                    Debug.Log( "RSS = " + 10*Mathf.Log10((float)RSS) + " dBm" + "  RSS_LoS = " + 10*Mathf.Log10(RSSLoS) + " dBm"+ "  RSS_NLoS = " + 10*Mathf.Log10(RSSNLoS) + " dBm");


                    // Debug.Log("RSS = " + 10 * Mathf.Log10((float)RSS) + " dBm;" + " TxID: " + GSCMSpectrum.Links[chN].x + " RxID: " + GSCMSpectrum.Links[chN].y);
                    Debug.Log("Sent to NS3 RSS/MHz: RSS = " + GSCMrss.RSS[chN] + " dBm;" + " TxID: " + GSCMSpectrum.Links[chN].x +
                              " RxID: " + GSCMSpectrum.Links[chN].y);

                }

            }

            //Debug.Log("Time spent for FFT: " + ((Time.realtimeSinceStartup - t_fft) * 1000f) + " ms");

            #endregion

            link_ids.Dispose();
            nonzero_indexes.Dispose();
            map.Dispose();
            idarray.Dispose();

            //DMC_Paths.Dispose();
            //DMC_test.Dispose();
            //MPC1_Paths.Dispose();
            //MPC1_test.Dispose();
        }

        private int GetVehIdByVehGO(GameObject go)
        {
            // Debug.Log("Parsing ID by GO: " + go.name.Substring(7));
            return Int32.Parse(go.name.Substring(7));
        }




    }

}//namespace close

[BurstCompile]
public struct IndexNonZeroFilter : IJobParallelForFilter
{
    public NativeArray<int> Array;
    public bool Execute(int index)
    {
        return Array[index] == 1;
    }
}

[BurstCompile]
public struct LinkIndexes : IJobParallelFor
{
    [ReadOnly] public int MPCNum;
    [ReadOnly] public NativeArray<int> NonZeroIndexes;

    [WriteOnly] public NativeArray<Vector2Int> LinkIDs;
    public void Execute(int link_id)
    {
        int min_i = NonZeroIndexes.Length;
        int max_i = 0;
        for (int i = 0; i < NonZeroIndexes.Length; i++)
        {
            if (NonZeroIndexes[i] >= link_id * MPCNum && i < min_i)
            { min_i = i; }
            if (NonZeroIndexes[i] <= (link_id + 1) * MPCNum && i > max_i)
            { max_i = i; }
        }
        LinkIDs[link_id] = new Vector2Int(min_i, max_i);
    }
}

[BurstCompile]
public struct ParallelChannel : IJobParallelFor
{
    [ReadOnly] public int FFTSize;
    [ReadOnly] public NativeArray<Vector3> CarsPositions;
    [ReadOnly] public NativeArray<Vector3> CarsFwd;
    //[ReadOnly] public NativeArray<Vector2Int> Links;
    [ReadOnly] public NativeArray<float> InverseLambdas;
    [ReadOnly] public NativeMultiHashMap<int, Path_and_IDs> HashMap;
    [ReadOnly] public int MPCNum;
    [ReadOnly] public int LinkNum;
    [ReadOnly] public NativeArray<int> NonZeroIndexes;
    [ReadOnly] public NativeArray<Vector2Int> LinkIDs;

    [WriteOnly] public NativeArray<System.Numerics.Complex> H_NLoS;
    public void Execute(int index)
    {
        int i_link = Mathf.FloorToInt(index / FFTSize);
        int i_sub = index - i_link * FFTSize; // calculating index within FFT array

        // defining zero temp value
        System.Numerics.Complex temp_HNLoS = new System.Numerics.Complex(0, 0);

        //for (int i = i_link * MPCNum; i < (i_link + 1) * MPCNum; i++)
        for (int i = LinkIDs[i_link].x; i <= LinkIDs[i_link].y; i++)
        {
            if (HashMap.TryGetFirstValue(NonZeroIndexes[i], out Path_and_IDs path, out NativeMultiHashMapIterator<int> nativeMultiHashMapIterator))
            {
                // if there are many paths, then sum them up
                do
                {
                    float HNLoS_dist = path.PathParameters.Distance;
                    // float HNLoS_dist_gain = 1 / (InverseLambdas[i_sub] * 4 * Mathf.PI * HNLoS_dist); // Free space loss
                    float HNLoS_dist_gain = 1 / (HNLoS_dist); // Free space loss
                    float HNLoS_attnuation = path.PathParameters.Attenuation;

                    double ReExpHLoS = Mathf.Cos(2 * Mathf.PI * InverseLambdas[i_sub] * HNLoS_dist);
                    double ImExpHLoS = -Mathf.Sin(2 * Mathf.PI * InverseLambdas[i_sub] * HNLoS_dist);
                    // defining exponent
                    System.Numerics.Complex ExpHLoS = new System.Numerics.Complex(ReExpHLoS, ImExpHLoS);

                    temp_HNLoS += HNLoS_attnuation * HNLoS_dist_gain * ExpHLoS;
                }
                while (HashMap.TryGetNextValue(out path, ref nativeMultiHashMapIterator));
            }
        }
        H_NLoS[index] = temp_HNLoS;
    }
}

public struct GSCMspectrumChannel
{
    public List<Vector2Int> Links;
    public List<List<double>> RSSspectrumGain;
}
public struct GSCMstructECS
{
    public List<Vector2Int> Links;
    public List<double> RSS;
}

public struct Path_and_IDs
{
    public PathChain ChainIDs;
    public Path PathParameters;
    public int PathOrder;

    public Path_and_IDs(PathChain pathChain, Path pathParameters, int order)
    {
        ChainIDs = pathChain;
        PathParameters = pathParameters;
        PathOrder = order;
    }
}

public struct PathChain
{
    // for tracking the all paths inclusions
    public int Car1;
    public int ID1;
    public int ID2;
    public int ID3;
    public int Car2;
    public PathChain(int car1, int i1, int i2, int i3, int car2)
    {
        Car1 = car1;
        ID1 = i1;
        ID2 = i2;
        ID3 = i3;
        Car2 = car2;
    }
}

public struct Path
{
    public Vector2Int Car_IDs;
    public float Distance;
    public float Attenuation;
    public Path(Vector2Int link, float d, float att)
    {
        Car_IDs = link;
        Distance = d;
        Attenuation = att;
    }
}





[BurstCompile(Debug = true)]
public struct ParallelRayCastingDataCars : IJobParallelFor
{
    [ReadOnly] public float CastingDistance;
    [ReadOnly] public NativeArray<Vector3> Cars_Positions;
    [ReadOnly] public NativeArray<V6> MPC_Array;
    [ReadOnly] public NativeArray<Vector3> MPC_Perpendiculars;
    [ReadOnly] public int LayerMaskToIgnore;

    [WriteOnly] public NativeArray<float> SoA;
    [WriteOnly] public NativeArray<int> SeenIndicator;
    [WriteOnly] public NativeArray<RaycastCommand> commands;
    public void Execute(int index)
    {
        int i_car = Mathf.FloorToInt(index / MPC_Array.Length);
        int i_mpc = index - i_car * MPC_Array.Length;
        Vector3 temp_direction = MPC_Array[i_mpc].Coordinates - Cars_Positions[i_car];

        float cosA = Vector3.Dot(MPC_Array[i_mpc].Normal, -temp_direction.normalized); // NOTE: the sign is negative
        if (cosA > (float)0.1 && temp_direction.magnitude < CastingDistance)
        {
            //SoA[index] = Mathf.Sign(Vector3.Dot(MPC_Perpendiculars[i_mpc], -temp_direction.normalized));
            SoA[index] = Vector3.Dot(MPC_Perpendiculars[i_mpc], -temp_direction.normalized);
            SeenIndicator[index] = 1;
            commands[index] = new RaycastCommand(Cars_Positions[i_car], temp_direction.normalized, temp_direction.magnitude, LayerMaskToIgnore);
            // commands[index] = new RaycastCommand(Cars_Positions[i_car], temp_direction.normalized, temp_direction.magnitude);
        }
        else
        {
            SoA[index] = 0;
            SeenIndicator[index] = 0;
            commands[index] = new RaycastCommand(new Vector3(0, 0, 0), new Vector3(0, 0, 0), 0);
        }

    }
}

// }
