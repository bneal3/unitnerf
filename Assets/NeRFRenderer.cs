using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using UnityEngine;
using System.Text;
using UnityStandardAssets.Utility;

public class NeRFRenderer : MonoBehaviour
{
    // VARIABLES: Public
    public Material nerfMaterial;
    public Camera viewCamera;
    
    // VARIABLES: Mesh
    private MeshFilter mf; 
    private MeshRenderer mr;
    private Texture2D nerfTex;
    
    // VARIABLES: Paths
    private string outputDirName;
    private string model;
    private string modelPath;
    private string intrinsicPath;
    private string poseFileName;
    private string poseMatrixPath;

    // VARIABLES: Process
    private bool pollWaiting = false;
    private bool processLoaded = false;
    private System.Diagnostics.Process process;

    // VARIABLES: FPS tracking
    private DateTime benchmarkTime;
    private float timer = 2f;
    private StringBuilder csv;
    private int counter = 0;
    private double averageFps = 0;
    private DateTime processStart;
    private double processTime;
    private double averageProcessFps;
    
    void Start()
    {
        mf = this.gameObject.AddComponent<MeshFilter>();
        mf.mesh.name = "NeRF_Render";
        mr = this.gameObject.AddComponent<MeshRenderer>();

        nerfTex = new Texture2D(1,1);
        nerfMaterial.SetTexture("_MainTex", nerfTex);    
        mr.material = nerfMaterial;

        outputDirName = Application.persistentDataPath + "/unitnerf";
        model = "ficus";
        modelPath = Application.dataPath + @$"/nerf-volrend/models/plenoctree/{model}/{model}.npz";
        intrinsicPath = Application.dataPath + @$"/nerf-volrend/models/plenoctree/{model}/intrinsics.txt";

        // FLOW: process variable instantiation
        process = new System.Diagnostics.Process();
        // FLOW: System settings
        process.EnableRaisingEvents = false;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        // FLOW: I/O settings
        process.Exited += new EventHandler(ProcessExited);
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        benchmarkTime = DateTime.Now;
        csv = new StringBuilder();

        Matrix4x4 camMat = new Matrix4x4();
        camMat[0, 0] = -1;
        camMat[0, 1] = 0;
        camMat[0, 2] = 0;
        camMat[0, 3] = 0;
        camMat[1, 0] = 0;
        camMat[1, 1] = -0.7341f;
        camMat[1, 2] = 0.67903f;
        camMat[1, 3] = 2.73f;
        camMat[2, 0] = 0;
        camMat[2, 1] = 0.67903f;
        camMat[2, 2] = 0.734109f;
        camMat[2, 3] = 2.959f;
        camMat[3, 0] = 0;
        camMat[3, 1] = 0;
        camMat[3, 2] = 0;
        camMat[3, 3] = 1;
        // viewCamera.worldToCameraMatrix = camMat;
    }

    void Update()
    {
        // FLOW: Record FPS
        if(DateTime.Now.Subtract(benchmarkTime).TotalSeconds > timer) {
            try 
            {
                counter += 1;
                var fps = Math.Ceiling((float)(1f / Time.unscaledDeltaTime) * 100) / 100;
                var processFps = Math.Ceiling((float)(1f / processTime) * 100) / 100;
                if(double.IsInfinity(processFps)) { processFps = 0; }
                if(counter == 1) 
                {
                    averageFps = fps;
                    averageProcessFps = processFps;
                }
                else
                {
                    averageFps = Math.Ceiling(((averageFps * ((counter - 1f)/counter)) + (fps * (1f/counter))) * 100) / 100;
                    averageProcessFps = Math.Ceiling(((averageProcessFps * ((counter - 1f)/counter)) + (processFps * (1f/counter))) * 100) / 100;
                }

                var newLine = string.Format("{0},{1},{2},{3},{4}", counter, fps, averageFps, processFps, averageProcessFps);
                csv.AppendLine(newLine);
                File.WriteAllText(@$"{outputDirName}/fps_data.csv", csv.ToString());

                benchmarkTime = DateTime.Now;
            }
            catch(Exception e)
            {
                // FLOW: Log error
                Debug.Log("Record fps: " + e.Message);
            }
        }

        // FLOW: Update viewing image to be in front of the camera
        // if (viewCamera.WorldToViewportPoint(gameObject.transform.position).x >= 0 
        // && viewCamera.WorldToViewportPoint(gameObject.transform.position).x <= 1
        // && viewCamera.WorldToViewportPoint(gameObject.transform.position).y >= 0 
        // && viewCamera.WorldToViewportPoint(gameObject.transform.position).y <= 1
        // && viewCamera.WorldToViewportPoint(gameObject.transform.position).z >= 0)
        // {
        //     mr.enabled = true;
        // }
        // else
        // {
        //     mr.enabled = false;
        // }
        gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, Quaternion.LookRotation(viewCamera.transform.position - gameObject.transform.position), 1/(Time.deltaTime * 1000f));
        Quad quad = new Quad(/*-Vector3.Normalize(viewCamera.transform.position - new Vector3(0, 0, 5))*/Vector3.Normalize(gameObject.transform.position - viewCamera.transform.position), gameObject.transform.position, viewCamera);
        Mesh[] sideMesh = new Mesh[1];
        sideMesh[0] = quad.mesh;
        mf.mesh = MeshUtils.MergeMeshes(sideMesh);

        // FLOW: Poll system
        if(!pollWaiting) {
            StartPlenoctrees();
        } else {
            KillPlenoctrees();
        }
    }

    // v_back = glm::normalize(v_back);
    // v_right = glm::normalize(glm::cross(v_world_up, v_back));
    // v_up = glm::cross(v_back, v_right);
    // v_world_up =  {0, 0, 1}
    // center = {-3.55f, 0.0, 3.55f};
    // v_back = {-0.7071068f, 0.0f, 0.7071068f};
    // v_world_up = {0.0f, 0.0f, 1.0f};

    void StartPlenoctrees() 
    {        
        // FLOW: Record process start time
        processStart = DateTime.Now;
        try
        {
            Matrix4x4 rotMat = new Matrix4x4();
            rotMat[0, 0] = 1;
            rotMat[0, 1] = 0;
            rotMat[0, 2] = 0;
            rotMat[0, 3] = 0;
            rotMat[1, 0] = 0;
            rotMat[1, 1] = 0;
            rotMat[1, 2] = 1;
            rotMat[1, 3] = 0;
            rotMat[2, 0] = 0;
            rotMat[2, 1] = -1;
            rotMat[2, 2] = 0;
            rotMat[2, 3] = 0;
            rotMat[3, 0] = 0;
            rotMat[3, 1] = 0;
            rotMat[3, 2] = 0;
            rotMat[3, 3] = 1;
            Matrix4x4 objMat = new Matrix4x4();
            objMat[0, 0] = -1.000000119209289551f;
            objMat[0, 1] = 0;
            objMat[0, 2] = 0;
            objMat[0, 3] = 0;
            objMat[1, 0] = 0;
            objMat[1, 1] = -0.7341099977493286133f;
            objMat[1, 2] = 0.6790307164192199707f;
            objMat[1, 3] = 1.737260103225708008f;
            objMat[2, 0] = 0;
            objMat[2, 1] = 0.6790306568145751953f;
            objMat[2, 2] = 0.7341099977493286133f;
            objMat[2, 3] = 1.959291696548461914f;
            objMat[3, 0] = 0;
            objMat[3, 1] = 0;
            objMat[3, 2] = 0;
            objMat[3, 3] = 1;
            // objMat[0, 3] = -2.260683f;
            // objMat[1, 3] = -0.2556345f;
            // objMat[2, 3] = 6.275355f;
            // objMat[3, 3] = 1;
            Debug.Log(objMat);
            string poseMatrixText = "";
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    poseMatrixText += objMat[i, j] + " ";
                }
                poseMatrixText += "\n";
            }


            // FLOW: Create pose matrix path
            poseFileName = @$"{viewCamera.transform.position.x}-{viewCamera.transform.position.y}-{viewCamera.transform.position.z}";
            poseMatrixPath = @$"{outputDirName}/{poseFileName}.txt";
            // string poseFileNameStable = "r_0";
            // string poseMatrixPathStable = Application.dataPath + "/nerf-volrend/models/plenoctree/ficus/pose/" + poseFileNameStable + ".txt";

            // FLOW: Create intrinsics and pose
            if (!File.Exists(poseMatrixPath))
            {
                // FLOW: Create the file and write pose matrix to it
                using (FileStream fs = File.Create(poseMatrixPath))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(poseMatrixText);
                    fs.Write(info, 0, info.Length);
                    fs.Close();
                }
            }

            // FLOW: Application settings
            process.StartInfo.FileName = Application.dataPath + "/nerf-volrend/build/Release/volrend_headless.exe";
            process.StartInfo.Arguments = $"\"{modelPath}\" -i \"{intrinsicPath}\" \"{poseMatrixPath}\" -o \"{outputDirName}\""; 
            // FLOW: Begin process
            process.Start();
            // FLOW: Read in program stream
            // Read(process.StandardOutput);
            // Read(process.StandardError);
            pollWaiting = true;
        }
        catch(Exception e)
        {
            // FLOW: Log error
            Debug.Log("Read file: " + e);
        }
    }

    void KillPlenoctrees() {
        // FLOW: Poll for saved image of NeRF, if found, set waiting = false
        string texFileName = @$"{outputDirName}/{poseFileName}.png";
        if(!processLoaded)
        { 
            try 
            {
                if(File.Exists(texFileName)) 
                {
                    // FLOW: Load image onto square as texture
                    byte[] texBytes = File.ReadAllBytes(texFileName);
                    nerfTex.LoadImage(texBytes);

                    processLoaded = true;
                } 
            }
            catch(Exception e)
            {
                // FLOW: Log error
                Debug.Log("Create file: " + e);
            }
        }
        else
        {
            try {
                if (process.HasExited) 
                {
                    // File.Delete(texFileName);
                    // File.Delete(poseMatrixPath);
                    pollWaiting = false;
                    processLoaded = false;
                    // FLOW: Record end time
                    processTime = DateTime.Now.Subtract(processStart).TotalSeconds;
                }
            }
            catch(Exception e)
            {
                // FLOW: Log error
                Debug.Log("Delete file: " + e.Message);
            }
        }
    }

    void ProcessExited(object sender, EventArgs e)
    {
        Debug.Log("Process finished");
    }

    private void Read(System.IO.StreamReader reader)
    {
        new System.Threading.Thread(() =>
        {
            while (true)
            {
                int current;
                while ((current = reader.Read()) >= 0) {
                    Debug.Log((char)current);
                }      
            }
        }).Start();
    }
}
