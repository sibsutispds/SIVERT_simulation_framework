using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEditor;

public class RunNsProcessBeforeMainThread
{
    [MenuItem("SIVERT/Run NS3 daemon in separate thread")]
    public static void PlayFromPrelaunchScene()
    {
        if ( EditorApplication.isPlaying == true )
        {
            EditorApplication.isPlaying = false;
            return;
        }

        Thread ns3Thread = new Thread(Ns3Start_cplus);
        ns3Thread.Start();
        EditorApplication.isPlaying = true;
    }

    
    
            static void Ns3Start_cplus()
        {
            try
            {
                Process ns3 = new Process();
                ns3.StartInfo.FileName = "/bin/zsh";
                ns3.StartInfo.Arguments ="waf --run /Users/nlyamin/WRK/Simulation/ns-d2d/src/wave/examples/wave-simple-80211p";
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
                
                // ns3.OutputDataReceived += new DataReceivedEventHandler( DataReceived );
                // ns3.ErrorDataReceived += new DataReceivedEventHandler( ErrorReceived );
                
                
                ns3.Start();
                ns3.BeginOutputReadLine();
                StreamWriter messageStream = ns3.StandardInput;
       
                UnityEngine.Debug.Log( "Successfully launched NS3 Daemon" );
                ns3.WaitForExit();
                ns3.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                UnityEngine.Debug.Log( "Something went wrong with the daemon" );
                throw;
            }
            

            
        }
   
    
    
} 