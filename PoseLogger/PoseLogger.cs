using MelonLoader;
using RUMBLE.MoveSystem;
using RUMBLE.Players.Subsystems;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using RUMBLE.Managers;
using UnityEngine.UI;
using RUMBLE.Players;
using RootMotion.Dynamics;
using static UnityEngine.ExpressionEvaluator;

namespace PoseLogger
{
    public class StructureTracking : MonoBehaviour
    {
        public string StructureName { get; set; }
        public bool LastActive { get; set; }
        public float Timestamp { get; set; }
        public string MoveName { get; set; }
    }
    public class PoseLoggerClass : MelonMod
    {
        //--------------------------------------------------
        //--------------------------------------------------
        //constants
        private const int LgRkPool = 17;
        private const int DiscPool = 43;
        private const int BallPool = 51;
        private const int PillPool = 42;
        private const int CubePool = 50;
        private const int WallPool = 49;

        private const string SettingsFile = @"UserData\PoseLogger\Settings\Settings.txt";
        private const string BaseFolder = "UserData";
        private const string ModFolder = "PoseLogger";
        private const string LogFolder = "Logs";
        private const string SettingsFolder = "Settings";
        private const string PoseLogFileName = "PoseLog";
        private const string StructLogFileName = "StructLog";
        private const string LogFileSuffix = ".txt";
        //--------------------------------------------------
        //--------------------------------------------------

        //--------------------------------------------------
        //--------------------------------------------------
        //variables
        private bool init = false;
        private readonly bool debug = false;
        private bool logging = true;
        private bool buffer = false;
        private bool PoseStructureLink = false;
        private bool Combo = false;
        private bool BasePoseReset = false;

        private bool structconsole = false;
        private bool structfile = false;
        private bool poseconsole = false;
        private bool posefile = false;
        private bool streamermode = false;

        private bool discenb = false;
        private bool ballenb = false;
        private bool pillenb = false;
        private bool cubeenb = false;
        private bool wallenb = false;
        private bool lgrkenb = false;

        private string PoseDataName = "";
        private string PoseName = "";
        private string currentScene = "";
        private string LogString = "";
        private string PoseLogFileString = "";
        private string StructLogFileString = "";
        private string LocalPlayerName = "";
        private string LastStructure = "";


        private float CurrTimestamp = 0;
        private float LastTimestamp = 0;
        private float TimeDiff = 0;

        private DateTime LastTime;
        private TimeSpan LastDelay;
        private DateTime ComboTime;

        private List<StructureTracking> Tracker = new List<StructureTracking>();
        private List<String> BufferList = new List<String>();
        private List<String> BufferList2 = new List<String>();
        private List<String> MoveBuffer = new List<String>();
        //--------------------------------------------------
        //--------------------------------------------------

        //--------------------------------------------------
        //--------------------------------------------------
        //Objects
        private PlayerPoseSystem Player_Obj;
        private GameObject PoolManager;
        private GameObject TrackingBuffer;
        private GameObject[] TBChildren = new GameObject[20];
        private GameObject[] TBDelay = new GameObject[20];
        private ProcessableComponent[] Discs;
        private ProcessableComponent[] Balls;
        private ProcessableComponent[] Pills;
        private ProcessableComponent[] Cubes;
        private ProcessableComponent[] Walls;
        private ProcessableComponent[] LgRck;
        //--------------------------------------------------
        //--------------------------------------------------

        //initializes things
        public override void OnLateInitializeMelon()
        {
            base.OnLateInitializeMelon();
            //Create Folders if they don't exist
            CheckandCreateFolder(BaseFolder + @"\" + ModFolder);
            CheckandCreateFolder(BaseFolder + @"\" + ModFolder + @"\" + LogFolder);
            CheckandCreateFolder(BaseFolder + @"\" + ModFolder + @"\" + SettingsFolder);

            //Get Paths
            PoseLogFileString = BaseFolder + @"\" + ModFolder + @"\" + LogFolder + @"\" + PoseLogFileName + DateTime.Now.ToString("yyyyMMddHHmmss") + LogFileSuffix;
            StructLogFileString = BaseFolder + @"\" + ModFolder + @"\" + LogFolder + @"\" + StructLogFileName + DateTime.Now.ToString("yyyyMMddHHmmss") + LogFileSuffix;
            MelonLogger.Msg("PoseLogFile: " + PoseLogFileString);
            MelonLogger.Msg("PoseLogFile: " + StructLogFileString);

            //Get and apply settings
            GetSettings();                        
            
        }

        //Run every update
        public override void OnUpdate()
        {
            //Base Updates
            base.OnUpdate();

            //Init once in actual scene to avoid spam
            if ((currentScene == "Gym" || currentScene == "Park") && !init)
            {

                try
                {
                    //Get Username of local player
                    LocalPlayerName = SanitizeName(GameObject.Find("Game Instance/Initializable/PlayerManager").GetComponent<PlayerManager>().LocalPlayer.Data.GeneralData.PublicUsername);

                    //For Pose Tracking
                    if(poseconsole || posefile || streamermode)
                    {
                        Player_Obj = GameObject.Find("Game Instance/Initializable/PlayerManager").GetComponent<PlayerManager>().LocalPlayer.Controller.gameObject.transform.FindChild("Poses").GetComponent<PlayerPoseSystem>();

                        if (debug) MelonLogger.Msg("Player Object found.");
                    }

                    //For Structure Tracking
                    if (structconsole || structfile || streamermode)
                    {
                        PoolManager = GameObject.Find("Game Instance/Pre-Initializable/PoolManager");
                        if (debug) MelonLogger.Msg("PoolManager found.");

                        //Get all Structure Pool Objects
                        if (discenb) Discs = PoolManager.transform.GetChild(DiscPool).GetComponentsInChildren<ProcessableComponent>(true);
                        if (ballenb) Balls = PoolManager.transform.GetChild(BallPool).GetComponentsInChildren<ProcessableComponent>(true);
                        if (pillenb) Pills = PoolManager.transform.GetChild(PillPool).GetComponentsInChildren<ProcessableComponent>(true);
                        if (cubeenb) Cubes = PoolManager.transform.GetChild(CubePool).GetComponentsInChildren<ProcessableComponent>(true);
                        if (wallenb) Walls = PoolManager.transform.GetChild(WallPool).GetComponentsInChildren<ProcessableComponent>(true);
                        if (lgrkenb) LgRck = PoolManager.transform.GetChild(LgRkPool).GetComponentsInChildren<ProcessableComponent>(true);

                    }
                    //OnScreenTracking
                    if(streamermode)
                    {
                        try
                        {
                            if (!buffer)
                            {
                                CreateTrackingBuffer();
                                BufferList.Add("Pose/Structure Tracker");
                                BufferList2.Add("Time since last");
                            }
                            for (int i = 0; i < TBChildren.Length; i++)
                            {
                                WriteToBufferList("",BufferList);
                                WriteToBufferList("",BufferList2);
                            }
                            TrackingBuffer.SetActive(true);
                            AddToBuffer(BufferList, 0, 19);
                            AddToBuffer(BufferList2, 20, 39);
                        }
                        catch
                        {
                        }
                    }

                    init = true;
                    if (debug) MelonLogger.Msg(init);
                }
                catch
                {
                }

            }

            if((currentScene != "Gym" || currentScene != "Park") && streamermode && buffer) { TrackingBuffer.SetActive(false); }
            if ((currentScene == "Gym" || currentScene == "Park") && streamermode && buffer) { TrackingBuffer.SetActive(true); }



            //Enable/Disable Logging to avoid spam (Key is "L")
            if (Input.GetKeyDown(KeyCode.L))
                {
                    logging = !logging;
                    LogString = "Logging = " + logging.ToString();
                    MelonLogger.Msg(LogString);
                }

            //Only do in Gym / Park to avoid spam
            if (logging && init && (currentScene == "Gym" || currentScene == "Park"))
            {                
                
                //Pose based logging
                if (poseconsole || posefile || streamermode)
                {
                    PoseLogging();
                }

                //Structure based logging
                if (structconsole || structfile || streamermode)
                {
                    if (discenb) StructureLogging(Discs);
                    if (ballenb) StructureLogging(Balls);
                    if (pillenb) StructureLogging(Pills);
                    if (cubeenb) StructureLogging(Cubes);
                    if (wallenb) StructureLogging(Walls);
                    if (lgrkenb) StructureLogging(LgRck);
                }


                PoseStructureLink = false;
            }
        }

        //---Functions---

        //String Manipulation
        public string PadString(string Input, int Length)
        {
            try
            {
                for (int i = Input.Length; i < Length; i++)
                {
                    Input = " " + Input;
                }
            }
            catch
            {
                Input = "PadError";
            }
            return Input;
        }
        public string FillString(string Input, int Length)
        {
            try
            {
                for (int i = Input.Length; i < Length; i++)
                {
                    Input += " ";
                }
            }
            catch
            {
                Input = "FillError";
            }
            return Input;
        }
        public string SanitizeName(string Input)
        {
            bool RemoveChars = false;
            char[] chars = Input.ToCharArray();
            string Output = "";

            for (int c = 0; c < Input.Length; c++)
            {
                if (chars[c] == '<')
                {
                    if (chars[c + 1] == '#')
                    {
                        RemoveChars = true;
                    }
                }
                if (!RemoveChars)
                {
                    Output += chars[c];
                }
                if (chars[c] == '>')
                {
                    RemoveChars = false;
                }
            }
            return Output;
        }

        //Settings and File Manipulation
        public void GetSettings()
        {
            if (System.IO.File.Exists(SettingsFile))
            {
                string[] fileContents = System.IO.File.ReadAllLines(SettingsFile);
                //PoseSettings
                if (fileContents[1].Contains("True"))
                {
                    posefile = true;
                }
                if (fileContents[2].Contains("True"))
                {
                    poseconsole = true;
                }
                //StructSetttings
                if (fileContents[4].Contains("True"))
                {
                    structfile = true;
                }
                if (fileContents[5].Contains("True"))
                {
                    structconsole = true;
                }
                //Toggles
                if (fileContents[7].Contains("True"))
                {
                    discenb = true;
                }
                if (fileContents[8].Contains("True"))
                {
                    ballenb = true;
                }
                if (fileContents[9].Contains("True"))
                {
                    pillenb = true;
                }
                if (fileContents[10].Contains("True"))
                {
                    cubeenb = true;
                }
                if (fileContents[11].Contains("True"))
                {
                    wallenb = true;
                }
                if (fileContents[12].Contains("True"))
                {
                    lgrkenb = true;
                }
                if (fileContents[14].Contains("True"))
                {
                    streamermode = true;
                }
                MelonLogger.Msg("Settings File applied");
            }
            else
            {
                string SettingsString = (
                    "<Pose Based Logging>" + Environment.NewLine +
                    "Write Pose Log to File: True" + Environment.NewLine +
                    "Write Pose Log to Console: False" + Environment.NewLine +
                    "<Structure Based Logging>" + Environment.NewLine +
                    "Write Structure Log to File: True" + Environment.NewLine +
                    "Write Structure Log to Console: True" + Environment.NewLine +
                    "<Toggle Structures>" + Environment.NewLine +
                    "Disc Tracked: True" + Environment.NewLine +
                    "Ball Tracked: True" + Environment.NewLine +
                    "Pillar Tracked: True" + Environment.NewLine +
                    "Cube Tracked: True" + Environment.NewLine +
                    "Wall Tracked: True" + Environment.NewLine +
                    "Boulder Tracked: True" + Environment.NewLine +
                    "<Streamer Mode>" + Environment.NewLine +
                    "Enable OnScreenLogging: True"
                    );
                File.WriteAllText(SettingsFile, SettingsString);
                posefile = true;
                poseconsole = false;
                structfile = true;
                structconsole = true;
                discenb = true;
                ballenb = true;
                pillenb = true;
                cubeenb = true;
                wallenb = true;
                lgrkenb = true;
                streamermode = true;
                MelonLogger.Msg("Default Settings applied");
            }
        }
        public void WriteToLogFile(string FileName,string Output,bool console,bool file)
        {
            if (console)
            {
                MelonLogger.Msg(Output);
            }
            if (file)
            {
                Output += Environment.NewLine;
                File.AppendAllText(FileName, Output);
            }
        }
        public void CheckandCreateFolder(string Input)
        {
            if (!Directory.Exists(Input))
            {
                Directory.CreateDirectory(Input);
                MelonLogger.Msg("Folder: " + Input.ToString() + " created.");

            }
        }


        //Logging Functions
        public void PoseLogging()
        {
            try
            {
                CurrTimestamp = Player_Obj.lastPoseUsedTimestamp;
                if (CurrTimestamp != LastTimestamp)
                {
                    //Get Name of Pose
                    PoseDataName = Player_Obj.lastPoseForSFX.name;

                    //Get Timediff in ms
                    TimeDiff = (CurrTimestamp - LastTimestamp) * 1000;

                    //Set Name based on PoseDataName
                    switch (PoseDataName)
                    {
                        case "Sprint":
                            PoseName = "Sprint";
                            break;
                        case "EXPLODE":
                            PoseName = "Explode";
                            PoseStructureLink = true;
                            break;
                        case "DoublePunch":
                            PoseName = "Flick";
                            PoseStructureLink = true;
                            break;
                        case "Poses_Hold":
                            PoseName = "Hold";
                            PoseStructureLink = true;
                            break;
                        case "Poses_SpawnCube":
                            PoseName = "Cube";
                            PoseStructureLink = true;
                            break;
                        case "Poses_Dash":
                            PoseName = "Dash";
                            break;
                        case "Poses_Uppercut_015":
                            PoseName = "Uppercut";
                            PoseStructureLink = true;
                            break;
                        case "PosesSpawnWall":
                            PoseName = "Wall";
                            PoseStructureLink = true;
                            break;
                        case "Poses_Jump":
                            PoseName = "Jump";
                            break;
                        case "Poses_Kick_015":
                            PoseName = "Kick";
                            PoseStructureLink = true;
                            break;
                        case "Poses_SpawnBall":
                            PoseName = "Ball";
                            PoseStructureLink = true;
                            break;
                        case "Poses_Stomp":
                            PoseName = "Stomp";
                            PoseStructureLink = true;
                            break;
                        case "Poses_Pillar_015":
                            PoseName = "Pillar";
                            PoseStructureLink = true;
                            break;
                        case "Poses_Straight_015":
                            PoseName = "Straight";
                            PoseStructureLink = true;
                            break;
                        case "MoveSystemDisc 1":
                            PoseName = "Disc";
                            PoseStructureLink = true;
                            break;
                        case "Poses_StructureX":
                            PoseName = "Base Pose";
                            break;
                        default:
                            PoseName = "No Name ?";
                            break;
                    }

                    //Place Buffer String after delay
                    if ((TimeDiff >= 1500 && PoseName != "Sprint") || (TimeDiff >= 3500 && PoseName == "Sprint"))
                    {
                        WriteToLogFile(PoseLogFileString, "-----------------------------------------------------", poseconsole, posefile);
                        if (streamermode)
                        {
                            for (int i = 0; i < TBChildren.Length; i++)
                            {
                                WriteToBufferList("", BufferList);
                                WriteToBufferList("", BufferList2);
                            }
                            AddToBuffer(BufferList, 0, 19);
                            AddToBuffer(BufferList2, 20, 39);
                            LastTime = DateTime.Now;
                            if(PoseName == "Base Pose") BasePoseReset = true;
                        }
                    }

                    //Output
                    LogString = "Pose Name: " + PadString(PoseName, 10) + " | Time since last Pose:" + PadString((TimeDiff).ToString("0"), 6) + "ms";

                    if (streamermode && !PoseStructureLink && !PoseName.Contains("Base Pose"))
                    {
                        AddToScreenLogger("Pose struck: " + PoseName);
                    }


                    WriteToLogFile(PoseLogFileString, LogString, poseconsole, posefile);
                }
                LastTimestamp = Player_Obj.lastPoseUsedTimestamp;
            }
            catch
            {
            }
        }
        public void StructureLogging(ProcessableComponent[] Children)
        {
            StructureTracking current = new StructureTracking();
            StructureTracking last = new StructureTracking();
            int index;
            string Output;
            string Player;
            string Structure;
            string MoveName;
            string TimeSinceLast;

            try
            {
                for (int i = 0; i < Children.Length; i++)
                {

                    //Get current Data
                    switch (Children[i].name)
                    {
                        case "Disc":
                            current.StructureName = "Disc#";
                            break;
                        case "Ball":
                            current.StructureName = "Ball#";
                            break;
                        case "Pillar":
                            current.StructureName = "Pillar#";
                            break;
                        case "RockCube":
                            current.StructureName = "Cube#";
                            break;
                        case "Wall":
                            current.StructureName = "Wall#";
                            break;
                        case "LargeRock":
                            current.StructureName = "Boulder#";
                            break;
                    }
                    //Padding
                    if(i < 10)
                    {
                        current.StructureName += "0" + i.ToString();
                    }
                    else
                    {
                        current.StructureName += i.ToString();
                    }

                    current.LastActive = Children[i].gameObject.activeSelf;

                    //MoveName
                    try
                    {
                        current.MoveName = Children[i].previousStackConfiguration.ExecutedStack.name;
                        if(current.MoveName == "Disc")
                        {
                            current.MoveName = "Spawn Move";
                        }
                    }
                    catch
                    {
                        current.MoveName = "Nothing";

                    }
                    //TimeStamp
                    try
                    {
                        if(currentScene == "Gym")
                        {
                            current.Timestamp = Children[i].previousStackConfiguration.OfflineTimestamp;
                        }
                        if (currentScene == "Park")
                        {
                            current.Timestamp = Children[i].previousStackConfiguration.Timestamp;
                        }
                    }
                    catch
                    {

                        current.Timestamp = -1;
                    }

                    index = Tracker.FindIndex(x => x.StructureName == current.StructureName);

                    if (index == -1)
                    {
                        Tracker.Add(new StructureTracking { StructureName = current.StructureName, LastActive = current.LastActive , Timestamp = current.Timestamp, MoveName = current.MoveName});

                        index = Tracker.FindIndex(x => x.StructureName == current.StructureName);
                    }
                    else
                    {

                        //Get old data from list via index
                        last.StructureName = Tracker[index].StructureName;
                        last.LastActive = Tracker[index].LastActive;
                        last.Timestamp = Tracker[index].Timestamp;
                        last.MoveName = Tracker[index].MoveName;
                        try { Player = SanitizeName(Children[i].latestInfluencedProcessor.Cast<PlayerStackProcessor>().ParentController.AssignedPlayer.Data.GeneralData.PublicUsername); }
                        catch { Player = "No Player"; }

                        Structure = PadString(current.StructureName, 10);
                        MoveName = FillString(current.MoveName, 12);
                        TimeSinceLast = ((current.Timestamp - last.Timestamp) * 1000).ToString("0");

                        if (current.LastActive != last.LastActive)
                        {
                            if (current.LastActive)
                            {
                                Output = Structure + " spawned.";
                                WriteToLogFile(StructLogFileString, Output, structconsole, structfile);

                                if (PoseStructureLink && streamermode)
                                {
                                    AddToScreenLogger("Spawned " + current.StructureName);
                                }
                            }
                            else
                            {
                                Output = Structure + " broke.";
                                WriteToLogFile(StructLogFileString, Output, structconsole, structfile);
                            }
                        }

                        if (current.LastActive && (current.Timestamp != last.Timestamp || current.MoveName != last.MoveName))
                        {
                            Output = Structure + " affected by " + MoveName;
                            if (LocalPlayerName == Player && streamermode && !MoveName.Contains("Nothing") && !MoveName.Contains("Spawn Move"))
                            {
                                AddToScreenLogger(ComboTracker(current.StructureName, current.MoveName) + " on " + current.StructureName);
                            }
                            if (last.Timestamp == -1)
                            {

                                Output += "| First Modifier  ";
                                Output += "| Player: ";
                                Output += Player;
                                WriteToLogFile(StructLogFileString,Output,structconsole,structfile);
                            }
                            else
                            {
                                if (currentScene == "Gym")
                                {
                                    Output += FillString("| Delay: " + TimeSinceLast + "ms", 18);
                                }
                                if (currentScene == "Park")
                                {
                                    if(current.Timestamp-last.Timestamp == 0)
                                    {
                                        Output += FillString("| Delay: <128ms", 18);
                                    }
                                    else
                                    {
                                        Output += FillString("| Delay: ~" + TimeSinceLast + "ms", 18);
                                    }
                                }
                                Output += "| Player: ";
                                Output += Player;
                                WriteToLogFile(StructLogFileString,Output, structconsole, structfile);
                            }
                        }
                        Tracker[index] = new StructureTracking { StructureName = current.StructureName, LastActive = current.LastActive, Timestamp = current.Timestamp, MoveName = current.MoveName };
                    }
                }
            }
            catch
            {
                MelonLogger.Msg("FUCK");
            }

        }

        public void AddToScreenLogger(string I1)
        {
            string temp = "";
            if (BasePoseReset) { LastTime = DateTime.Now; BasePoseReset = false; }
            if (Combo) LastDelay += DateTime.Now - LastTime;
            else LastDelay = DateTime.Now - LastTime;

            if (LastDelay.TotalMilliseconds < 20) temp = "First";
            else temp = LastDelay.TotalMilliseconds.ToString("0") + "ms";

            if (Combo) { BufferList.RemoveAt(BufferList.Count - 1); BufferList.Add(I1); }
            else WriteToBufferList(I1, BufferList);
            if (Combo) { BufferList2.RemoveAt(BufferList.Count - 1); BufferList2.Add(temp); }
            else WriteToBufferList(temp, BufferList2);
            AddToBuffer(BufferList, 0, 19);
            AddToBuffer(BufferList2, 20, 39);
            LastTime = DateTime.Now;
            Combo = false;
        }
        public void CreateTrackingBuffer()
        {
            GameObject temp = GameObject.Find("Game Instance/UI/LegacyRecordingCameraUI/Panel");
            GameObject blank = new GameObject();
            float Offset = 16f;
            TrackingBuffer = GameObject.Instantiate(temp);
            //Background Panel
            TrackingBuffer.SetActive(false);                                                    //Deactivate the Object
            TrackingBuffer.name = "OnScreenLogger";                                             //Change object name
            TrackingBuffer.transform.SetParent(GameObject.Find("Game Instance/UI").transform);  //Move object to DDOL Area
            TrackingBuffer.transform.position = new Vector3(220f, 790f, 0);                     //Move object to top left corner 
            TrackingBuffer.transform.localScale = new Vector3(1.041625f, 1.25f, 0);             //Move object to top left corner 
            TrackingBuffer.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);     //Change Color and Alpha of picture
            for (int i = 0;i < TrackingBuffer.transform.childCount; i++)
            {
                GameObject.Destroy(TrackingBuffer.transform.GetChild(i).gameObject);
            }                     //Destroy all Children
            
            //Tracking
            for (int i = 0; i < TBChildren.Length; i++)
            {
                TBChildren[i] = GameObject.Instantiate(blank);
                TBChildren[i].gameObject.AddComponent<TMPro.TextMeshProUGUI>();
                TBChildren[i].transform.SetParent(TrackingBuffer.transform);
                if (i == 0)
                {
                    TBChildren[i].name = "Line_Head";
                    TBChildren[i].transform.localPosition = new Vector3(-35f, 160f, 0);
                    TBChildren[i].transform.GetComponent<RectTransform>().sizeDelta = new Vector2(520f, 28f);
                    TBChildren[i].transform.GetComponent<TMPro.TextMeshProUGUI>().text = "Pose/Structure Tracker";
                    TBChildren[i].transform.GetComponent<TMPro.TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
                }
                else
                {
                    TBChildren[i].name = "Line" + i.ToString();
                    TBChildren[i].transform.localPosition = new Vector3(-35f, 150f + Offset - Offset * (i + 1), 0);
                    TBChildren[i].transform.GetComponent<RectTransform>().sizeDelta = new Vector2(520f, 28f);
                    TBChildren[i].transform.GetComponent<TMPro.TextMeshProUGUI>().text = "";
                    TBChildren[i].transform.GetComponent<TMPro.TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
                }
                TBChildren[i].transform.localScale = new Vector3(0.6f, 0.5f, 1f);
                TBChildren[i].transform.GetComponent<TMPro.TextMeshProUGUI>().fontSize = 24;
                TBChildren[i].transform.GetComponent<TMPro.TextMeshProUGUI>().autoSizeTextContainer = false;
            }
            //Delay
            for (int i = 0; i < TBDelay.Length; i++)
            {
                TBDelay[i] = GameObject.Instantiate(blank);
                TBDelay[i].gameObject.AddComponent<TMPro.TextMeshProUGUI>();
                TBDelay[i].transform.SetParent(TrackingBuffer.transform);
                if (i == 0)
                {
                    TBDelay[i].name = "Delay_Head";
                    TBDelay[i].transform.localPosition = new Vector3(135f, 160f, 0);
                    TBDelay[i].transform.GetComponent<TMPro.TextMeshProUGUI>().text = "Time since last";
                }
                else
                {
                    TBDelay[i].name = "Delay" + i.ToString();
                    TBDelay[i].transform.localPosition = new Vector3(135f, 150f + Offset - Offset * (i + 1), 0);
                    TBDelay[i].transform.GetComponent<TMPro.TextMeshProUGUI>().text = "";
                }
                TBDelay[i].transform.GetComponent<TMPro.TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
                TBDelay[i].transform.GetComponent<RectTransform>().sizeDelta = new Vector2(170f, 28f);
                TBDelay[i].transform.localScale = new Vector3(0.6f, 0.5f, 1f);
                TBDelay[i].transform.GetComponent<TMPro.TextMeshProUGUI>().fontSize = 24;
                TBDelay[i].transform.GetComponent<TMPro.TextMeshProUGUI>().autoSizeTextContainer = false;
            }
            GameObject.Destroy(blank);
            MelonLogger.Msg("Buffer Object created.");
            buffer = true;
        }
        public void WriteToBufferList(string Input, List<string> List)
        {
            if (List.Count > (TBChildren.Length - 1))
            {
                List.RemoveAt(1);
                List.Add(Input);
            }
            else
            {
                List.Add(Input);
            }
        }
        public void AddToBuffer(List<string> Input,int Start,int End)
        {
            int j = 0;
            for (int i = Start; i <= End ; i++)
            {
                TrackingBuffer.transform.GetChild(i).GetComponent<TMPro.TextMeshProUGUI>().text = Input[j];
                j++;
            }
        }

        public string ComboTracker(string Structure, string Move) 
        {
            string Output = "";
            TimeSpan timeSpan = DateTime.Now - ComboTime;

            switch (Move)
            {
                case "Straight":
                    break;
                case "Kick":
                    break;
                case "Uppercut":
                    break;
                case "Stomp":
                    break;
                case "Explode":
                    break;
                case "HoldRight":
                    Move = "Hold";
                    break;
                case "HoldLeft":
                    Move = "Hold";
                    break;
                case "Flick":
                    break;
            }

            if (LastStructure != Structure || timeSpan.TotalMilliseconds >= 350)
            {
                LastStructure = Structure;
                MoveBuffer.Clear();
                MoveBuffer.Add(Move);
                Output = Move;
                Combo = false;
                MelonLogger.Msg("Buffer: " + Move);
            }
            else
            {
                //Name Shortening
                switch (MoveBuffer[0])
                {
                    case "Straight":
                        if (Move == "Uppercut") { Move = "Struppercut"; MoveBuffer.RemoveAt(0); break; }
                        if (Move == "Kick") { Move = "Hop"; MoveBuffer.RemoveAt(0); break; }
                        break;
                    case "Struppercut":
                        if (Move == "Kick") { Move = "Strupperkick"; MoveBuffer.RemoveAt(0); break; }
                        break;
                    default:
                        break;
                }
                if (MoveBuffer.Count > 4)
                {
                    MoveBuffer.RemoveAt(4);
                    MoveBuffer.Insert(0, Move);
                }
                else
                {
                    MoveBuffer.Insert(0, Move);
                }

                switch (MoveBuffer.Count)
                {
                    case 1:
                        Output = MoveBuffer[0];
                        Combo = true;
                        break;
                    case 2:
                        Output = MoveBuffer[1] + ">" + MoveBuffer[0];
                        Combo = true;
                        break;
                    case 3:
                        Output = MoveBuffer[2] + ">" + MoveBuffer[1] + ">" + MoveBuffer[0];
                        Combo = true;
                        break;
                    case 4:
                        Output = MoveBuffer[3] + ">" + MoveBuffer[2] + ">" + MoveBuffer[1] + ">" + MoveBuffer[0];
                        Combo = true;
                        break;
                    case 5:
                        Output = MoveBuffer[4] + ">" + MoveBuffer[3] + ">" + MoveBuffer[2] + ">" + MoveBuffer[1] + ">" + MoveBuffer[0];
                        Combo = true;
                        break;
                    default:
                        Output = "Error";
                        Combo = true;
                        break;

                }

            }
            ComboTime = DateTime.Now;
            return Output;
        }

        //Overrides
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            init = false;
            currentScene = sceneName;
        }

    }
}

