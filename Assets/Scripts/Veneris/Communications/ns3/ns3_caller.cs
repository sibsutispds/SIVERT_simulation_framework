
using System;
using System.Diagnostics;
using System.Collections;
using System.IO;
using UnityEngine;
using System.Threading;

namespace Veneris.Communications.ns3
{
    public class ns3_caller:MonoBehaviour
    {
        private Process ns3;
        public Thread ns3Thread;
        StreamWriter messageStream;
        private void Start()
        {
//            we implement the call process of the python or ns-3 through multi threading, i.e. creating a separate thread for the ns3 process. See 
            this.ns3Thread = new Thread(new ThreadStart(Ns3Start_cplus));
            this.ns3Thread.Start();
        }


//        void Ns3Start_python()
//        {
//            Process ns3 = new Process();
//            ns3.StartInfo.FileName = "/usr/local/bin/python3";
//            ns3.StartInfo.Arguments = "test_ZeroMQ.py";    
//            // Pipe the output to itself - we will catch this later
//            ns3.StartInfo.RedirectStandardError=true;
//            ns3.StartInfo.RedirectStandardOutput=true;
//            ns3.StartInfo.CreateNoWindow = false;
// 
//            // Where the script lives
//            ns3.StartInfo.WorkingDirectory = "/WRK/Simulation/ns-d2d/scratch"; 
//            ns3.StartInfo.UseShellExecute = false;
//            UnityEngine.Debug.Log("Trying to call the ns3 waf...");
//            ns3.Start();
//            UnityEngine.Debug.Log("Potentially started the NS3 process");
//            // Read the output - this will show is a single entry in the console - you could get  fancy and make it log for each line - but thats not why we're here
//            UnityEngine.Debug.Log( ns3.StandardOutput.ReadToEnd() );
//            ns3.WaitForExit();
//            ns3.Close();
//        }
        
        void Ns3Start_cplus()
        {
            ns3 = new Process();
            ns3.StartInfo.FileName = "/Users/nlyamin/WRK/Simulation/ns-d2d/waf";
            ns3.StartInfo.Arguments =" --run /Users/nlyamin/WRK/Simulation/ns-d2d/src/wave/examples/wave-simple-80211p";    
            // Pipe the output to itself - we will catch this later
            ns3.StartInfo.RedirectStandardError=true;
            ns3.StartInfo.RedirectStandardOutput=true;
            ns3.StartInfo.CreateNoWindow = false;
 
            // Where the script lives
            ns3.StartInfo.WorkingDirectory = "/WRK/Simulation/ns-d2d"; 
            ns3.StartInfo.UseShellExecute = false;
            UnityEngine.Debug.Log("Trying to call the ns3 waf...");
            ns3.Start();
            UnityEngine.Debug.Log("Potentially started the NS3 process");
            // Read the output - this will show is a single entry in the console - you could get  fancy and make it log for each line - but thats not why we're here
            UnityEngine.Debug.Log( ns3.StandardOutput.ReadToEnd() );
            ns3.WaitForExit();
            ns3.Close();
        }

                void Ns3Start_cplus_ext()
        {
            try
            {
                ns3 = new Process();
                ns3.StartInfo.FileName = "/Users/nlyamin/WRK/Simulation/ns-d2d/waf";
                ns3.StartInfo.Arguments =" --run /Users/nlyamin/WRK/Simulation/ns-d2d/src/wave/examples/wave-simple-80211p_sivert";
                ns3.StartInfo.WorkingDirectory = "/Users/nlyamin/WRK/Simulation/ns-d2d"; 
                // Pipe the output to itself - we will catch this later
                ns3.StartInfo.UseShellExecute = false;
                ns3.StartInfo.RedirectStandardError=true;
                // string eOut = null;
                // ns3.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => 
                //     { eOut += e.Data; });
                ns3.StartInfo.RedirectStandardOutput=true;
                ns3.StartInfo.RedirectStandardInput=true;
                ns3.EnableRaisingEvents = false;
                // ns3.StartInfo.CreateNoWindow = false;
                
                ns3.OutputDataReceived += new DataReceivedEventHandler( DataReceived );
                ns3.ErrorDataReceived += new DataReceivedEventHandler( ErrorReceived );
                
                
                ns3.Start();
                ns3.BeginOutputReadLine();
                
                 messageStream = ns3.StandardInput;
       
                UnityEngine.Debug.Log( "Successfully launched app" );
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            
           
            
            // enable raising events because Process does not raise events by default
            
// // attach the event handler for OutputDataReceived before starting the process
//             ns3.OutputDataReceived += new DataReceivedEventHandler
//             (
//                 delegate(object sender, DataReceivedEventArgs e)
//                 {
//                     // append the new data to the data already read-in
//                     outputBuilder.Append(e.Data);
//                 }
//             );
            
            // ns3.Start();
            // // ns3.BeginOutputReadLine();
            // UnityEngine.Debug.Log("Potentially started the NS3 process");
            // // Read the output - this will show is a single entry in the console - you could get  fancy and make it log for each line - but thats not why we're here
            // // UnityEngine.Debug.Log( ns3.StandardOutput.ReadToEnd() );
            // ns3.BeginErrorReadLine();
            // UnityEngine.Debug.Log( ns3.StandardOutput.ReadToEnd() );
            
            
            
            // string log = outputBuilder.ToString();
            
            // UnityEngine.Debug.Log(log);
            
        }
        
                void DataReceived( object sender, DataReceivedEventArgs eventArgs )
                {
                    // Handle it
                    System.IO.File.WriteAllText (@"/Users/nlyamin/WRK/Simulation/log_unity.txt", eventArgs.Data);
                    UnityEngine.Debug.Log("STD stream:" + eventArgs.Data);
                }
 
 
                void ErrorReceived( object sender, DataReceivedEventArgs eventArgs )
                {
                    UnityEngine.Debug.LogError( "Error stream: " + eventArgs.Data );
                }
                
        private void OnDestroy()
        {
            UnityEngine.Debug.Log("Destroying thread for ns3 simulation run");
            ns3Thread.Abort();
        }

        private void OnApplicationQuit()
        {
            ns3Thread.Abort();
        }
    }
}