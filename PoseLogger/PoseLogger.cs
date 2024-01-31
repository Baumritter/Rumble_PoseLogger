using MelonLoader;
using RUMBLE.Managers;
using RUMBLE.MoveSystem;
using RUMBLE.Players.Subsystems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        private const string Version = "Poselogger_v1.3.0";
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
        private Vector3 Howard_BoardMainPosition = new Vector3(-2.615f, -3.52f, -19.05f);
        private Vector3 Howard_BoardMainRotation = new Vector3(0, 250, 0);
        private Vector3 Training_BoardMainPosition = new Vector3(-42.0299f, 3.194f, -1.0598f);
        private Vector3 Training_BoardMainRotation = new Vector3(0, 250, 0);
        private Vector3 Park_BoardMainPosition = new Vector3(-5.9f, -5.9f, -10f);
        private Vector3 Park_BoardMainRotation = new Vector3(0, 196, 0);


        private bool debuglogging = false;

        private bool SceneInit = false;
        private bool logging = true;
        private bool buffer = false;
        private bool PoseStructureLink = false;
        private bool Combo = false;
        private bool BasePoseReset = false;

        private bool posefile = false;
        private bool structurefile = false;
        private bool onscreenlog = false;
        private bool onboardlog = false;

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
        private GameObject Board_Howard;
        private GameObject Board_Training;
        private GameObject Board_Template;
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
            if (debuglogging) MelonLogger.Msg("PoseLogFile: " + PoseLogFileString);
            if (debuglogging) MelonLogger.Msg("PoseLogFile: " + StructLogFileString);

            //Get and apply settings
            GetSettings();    
            
            ClearLogFolder();
            
        }
        //Run every update
        public override void OnUpdate()
        {
            //Base Updates
            base.OnUpdate();

            //Init once in actual scene to avoid spam
            if ((currentScene == "Gym" || currentScene == "Park") && !SceneInit)
            {

                try
                {
                    //Get Username of local player
                    LocalPlayerName = SanitizeName(GameObject.Find("Game Instance/Initializable/PlayerManager").GetComponent<PlayerManager>().LocalPlayer.Data.GeneralData.PublicUsername);

                    //For Pose Tracking
                    if (Player_Obj == null) Player_Obj = GameObject.Find("Game Instance/Initializable/PlayerManager").GetComponent<PlayerManager>().LocalPlayer.Controller.gameObject.transform.FindChild("Poses").GetComponent<PlayerPoseSystem>();
                    if (debuglogging) MelonLogger.Msg("Player Object found.");

                    //For Structure Tracking
                    if (PoolManager == null) PoolManager = GameObject.Find("Game Instance/Pre-Initializable/PoolManager");
                    if (debuglogging) MelonLogger.Msg("PoolManager found.");

                    //Get all Structure Pool Objects
                    if (Discs == null) Discs = PoolManager.transform.GetChild(DiscPool).GetComponentsInChildren<ProcessableComponent>(true);
                    if (Balls == null) Balls = PoolManager.transform.GetChild(BallPool).GetComponentsInChildren<ProcessableComponent>(true);
                    if (Pills == null) Pills = PoolManager.transform.GetChild(PillPool).GetComponentsInChildren<ProcessableComponent>(true);
                    if (Cubes == null) Cubes = PoolManager.transform.GetChild(CubePool).GetComponentsInChildren<ProcessableComponent>(true);
                    if (Walls == null) Walls = PoolManager.transform.GetChild(WallPool).GetComponentsInChildren<ProcessableComponent>(true);
                    if (LgRck == null) LgRck = PoolManager.transform.GetChild(LgRkPool).GetComponentsInChildren<ProcessableComponent>(true);

                    //OnScreenLogging
                    if(onscreenlog)
                    {
                        if (!buffer) CreateTrackingBuffer();
                    }

                    //OnBoardLogging
                    if (onboardlog && (Board_Howard == null || Board_Training == null))
                    {
                        if (currentScene == "Gym")
                        {
                            Board_Howard = SpawnLeaderBoard("Gym_Howard_Board", Howard_BoardMainPosition, Howard_BoardMainRotation, debuglogging);
                            Board_Training = SpawnLeaderBoard("Gym_Training_Board", Training_BoardMainPosition, Training_BoardMainRotation, debuglogging);
                            if(Board_Template == null)
                            {
                                Board_Template = SpawnLeaderBoard("Park_Training_Board", Park_BoardMainPosition, Park_BoardMainRotation, debuglogging);
                                Board_Template.SetActive(false);
                                GameObject.DontDestroyOnLoad(Board_Template);
                            }
                        }
                        if (currentScene == "Park")
                        {
                            Board_Training = GameObject.Instantiate(Board_Template);
                            Board_Training.name = "Park_Training_Board";
                            Board_Training.SetActive(true);
                        }
                    }

                    //Buffer
                    BufferList.Add("Pose/Structure Tracker");
                    BufferList2.Add("Time since last");
                    if (debuglogging) MelonLogger.Msg("Text added to Buffer.");

                    for (int i = 0; i < TBChildren.Length; i++)
                    {
                        WriteToBufferList("", BufferList);
                        WriteToBufferList("", BufferList2);
                    }
                    if (debuglogging) MelonLogger.Msg("Buffer filled.");

                    if (onscreenlog && buffer)
                    {
                        TrackingBuffer.SetActive(true);
                        if (debuglogging) MelonLogger.Msg("OnScreenLog visibility enabled.");
                    }

                    if (onscreenlog)
                    {
                        AddToBuffer(BufferList, 0, 19);
                        AddToBuffer(BufferList2, 20, 39);
                        if (debuglogging) MelonLogger.Msg("Buffer pushed to OnScreenLog");
                    }

                    if (onboardlog) UpdateBoard();
                    if (debuglogging) MelonLogger.Msg("Buffer pushed to Board");

                    SceneInit = true;
                    if (debuglogging) MelonLogger.Msg(SceneInit);
                }
                catch
                {
                }

            }

            if((currentScene != "Gym" || currentScene != "Park") && onscreenlog && buffer) { TrackingBuffer.SetActive(false); }
            if ((currentScene == "Gym" || currentScene == "Park") && onscreenlog && buffer) { TrackingBuffer.SetActive(true); }



            //Enable/Disable Logging to avoid spam (Key is "L")
            if (Input.GetKeyDown(KeyCode.L))
                {
                    logging = !logging;
                    LogString = "Logging = " + logging.ToString();
                    MelonLogger.Msg(LogString);
                }

            //Only do in Gym / Park to avoid spam
            if (logging && SceneInit && (currentScene == "Gym" || currentScene == "Park"))
            {                
                
                //Pose based logging
                PoseLogging();

                //Structure based logging
                StructureLogging(Discs);
                StructureLogging(Balls);
                StructureLogging(Pills);
                StructureLogging(Cubes);
                StructureLogging(Walls);
                StructureLogging(LgRck);

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
            string SettingsString = (
                    Version + Environment.NewLine +
                    "<Logging Types>" + Environment.NewLine +
                    "<File Logging>" + Environment.NewLine +
                    "Enable Pose Log : True" + Environment.NewLine +
                    "Enable Structure Log: True" + Environment.NewLine +
                    "<OnScreenLog>" + Environment.NewLine +
                    "Enable: True" + Environment.NewLine +
                    "<BoardLog>" + Environment.NewLine +
                    "Enable: True"
                    );

            if (System.IO.File.Exists(SettingsFile))
            {
                string[] fileContents = System.IO.File.ReadAllLines(SettingsFile);

                if (fileContents[0].Contains(Version))
                {
                    if (fileContents[3].Contains("True"))
                    {
                        posefile = true;
                    }
                    if (fileContents[4].Contains("True"))
                    {
                        structurefile = true;
                    }
                    if (fileContents[6].Contains("True"))
                    {
                        onscreenlog = true;
                    }
                    if (fileContents[8].Contains("True"))
                    {
                        onboardlog = true;
                    }
                    MelonLogger.Msg("Settings File applied");
                }
                else
                {
                    File.WriteAllText(SettingsFile, SettingsString);
                    posefile = true;
                    structurefile = true;
                    onscreenlog = true;
                    onboardlog = true;
                    MelonLogger.Msg("Default Settings applied");
                }

            }
            else
            {
                File.WriteAllText(SettingsFile, SettingsString);
                posefile = true;
                structurefile = true;
                onscreenlog = true;
                onboardlog = true;
                MelonLogger.Msg("Default Settings applied");
            }
        }
        public void ClearLogFolder()
        {
            foreach (var fi in new DirectoryInfo(BaseFolder + @"\" + ModFolder + @"\" + LogFolder).GetFiles().OrderByDescending(x => x.LastWriteTime).Skip(10))
                fi.Delete();
        }
        public void WriteToLogFile(string FileName,string Output,bool file)
        {
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
                        WriteToLogFile(PoseLogFileString, "-----------------------------------------------------", posefile);
                        for (int i = 0; i < TBChildren.Length; i++)
                        {
                            WriteToBufferList("", BufferList);
                            WriteToBufferList("", BufferList2);
                        }
                        if (onscreenlog)
                        {
                            AddToBuffer(BufferList, 0, 19);
                            AddToBuffer(BufferList2, 20, 39);
                        }
                        if (onboardlog) UpdateBoard();
                        LastTime = DateTime.Now;
                        if(PoseName == "Base Pose") BasePoseReset = true;
                    }

                    //Output
                    LogString = "Pose Name: " + PadString(PoseName, 10) + " | Time since last Pose:" + PadString((TimeDiff).ToString("0"), 6) + "ms";

                    if (!PoseStructureLink && !PoseName.Contains("Base Pose"))
                    {
                        AddToScreenLogger("Pose struck: " + PoseName);
                    }

                    WriteToLogFile(PoseLogFileString, LogString, posefile);

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
                                WriteToLogFile(StructLogFileString, Output, structurefile);

                                if (PoseStructureLink)
                                {
                                    AddToScreenLogger("Spawned " + current.StructureName);
                                }
                            }
                            else
                            {
                                Output = Structure + " broke.";
                                WriteToLogFile(StructLogFileString, Output, structurefile);
                            }
                        }

                        if (current.LastActive && (current.Timestamp != last.Timestamp || current.MoveName != last.MoveName))
                        {
                            Output = Structure + " affected by " + MoveName;
                            if (LocalPlayerName == Player && !MoveName.Contains("Nothing") && !MoveName.Contains("Spawn Move"))
                            {
                                AddToScreenLogger(ComboTracker(current.StructureName, current.MoveName) + " on " + current.StructureName);
                            }
                            if (last.Timestamp == -1)
                            {

                                Output += "| First Modifier  ";
                                Output += "| Player: ";
                                Output += Player;
                                WriteToLogFile(StructLogFileString,Output,structurefile);
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
                                WriteToLogFile(StructLogFileString,Output, structurefile);
                            }
                        }
                        Tracker[index] = new StructureTracking { StructureName = current.StructureName, LastActive = current.LastActive, Timestamp = current.Timestamp, MoveName = current.MoveName };
                    }
                }
            }
            catch
            {
                MelonLogger.Msg("StructureLogging broke.");
            }

        }

        public void AddToScreenLogger(string I1)
        {
            string temp;
            if (BasePoseReset) { LastTime = DateTime.Now; BasePoseReset = false; }
            if (Combo) LastDelay += DateTime.Now - LastTime;
            else LastDelay = DateTime.Now - LastTime;

            if (LastDelay.TotalMilliseconds < 20) temp = "First";
            else temp = LastDelay.TotalMilliseconds.ToString("0") + "ms";

            if (Combo) { BufferList.RemoveAt(BufferList.Count - 1); BufferList.Add(I1); }
            else WriteToBufferList(I1, BufferList);
            if (Combo) { BufferList2.RemoveAt(BufferList.Count - 1); BufferList2.Add(temp); }
            else WriteToBufferList(temp, BufferList2);

            if (onscreenlog)
            {
                AddToBuffer(BufferList, 0, 19);
                AddToBuffer(BufferList2, 20, 39);
            }
            if (onboardlog) UpdateBoard();

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
                    TBChildren[i].transform.GetComponent<TMPro.TextMeshProUGUI>().text = "Initialization Text";
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
            if (debuglogging) MelonLogger.Msg("Buffer Object created.");
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
            string Output;
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


        private GameObject SpawnLeaderBoard(string Name, Vector3 BoardMainPosition, Vector3 BoardMainRotation, bool debuglog)
        {
            Vector3 BoardBGOffset = new Vector3(0, 1.175f, 0);
            Vector3 BoardBGScale = new Vector3(0.95f, 0.6f, 0.2f);
            Vector3 BoardTxOffsetHead = new Vector3(0, 0.1f, -0.25f);
            Vector3 BoardTxOffsetBody = new Vector3(0, -0.1f, -0.25f);
            GameObject Board;
            GameObject Text;

            GameObject temp = GameObject.Find("--------------LOGIC--------------/Heinhouser products/Leaderboard/Frame");
            if (debuglog) MelonLogger.Msg(Name + ": Board Object get");

            Board = GameObject.Instantiate(temp);
            if (debuglog) MelonLogger.Msg(Name + ": Board Object instantiate");

            Board.transform.position = BoardMainPosition;
            Board.transform.eulerAngles = BoardMainRotation;
            if (debuglog) MelonLogger.Msg(Name + ": Position changed");

            Board.name = Name;
            Board.transform.GetChild(0).name = "TopLog";
            Board.transform.GetChild(2).name = "FootLeft";
            Board.transform.GetChild(3).name = "FootRight";
            Board.transform.GetChild(4).name = "LogLeft";
            Board.transform.GetChild(5).name = "LogRight";
            Board.transform.GetChild(6).name = "InnerWall";
            Board.transform.GetChild(11).name = "LogBigTop";
            Board.transform.GetChild(12).name = "BackPlane";
            if (debuglog) MelonLogger.Msg(Name + ": Objects renamed");

            Board.transform.GetChild(6).localPosition = BoardBGOffset;
            Board.transform.GetChild(6).localScale = BoardBGScale;
            if (debuglog) MelonLogger.Msg(Name + ": Frame resized");

            temp = GameObject.Find("--------------LOGIC--------------/Heinhouser products/Leaderboard/Text Objects/Titleplate/Leaderboard");
            if (debuglog) MelonLogger.Msg(Name + ": Sample Text Object get");

            for(int i = 0; i < 4; i++)
            {
                Text = new GameObject { name = "Text" + i.ToString() };
                Text.transform.SetParent(Board.transform.GetChild(6));
                if (debuglog) MelonLogger.Msg(Name + ": Text Object " + i.ToString() + " created");
                if(i <= 1) { Text.transform.localPosition = BoardTxOffsetHead; }
                else { Text.transform.localPosition = BoardTxOffsetBody; }
                Text.transform.localScale = Invert_Values(BoardBGScale);
                Text.transform.eulerAngles = BoardMainRotation;
                Text.AddComponent<TextMeshPro>();
                Text.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
                if (debuglog) MelonLogger.Msg(Name + ": Text created and transformed");

                Text.transform.GetComponent<TextMeshPro>().text = "Initialization Text";
                Text.transform.GetComponent<TextMeshPro>().fontSize = 0.6f;
                Text.transform.GetComponent<TextMeshPro>().font = temp.GetComponent<TextMeshPro>().font;
                Text.transform.GetComponent<TextMeshPro>().fontMaterial = temp.GetComponent<TextMeshPro>().fontMaterial;

                switch (i)
                {
                    case 0:
                        Text.transform.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.TopLeft;
                        break;
                    case 1:
                        Text.transform.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.TopRight;
                        break;
                    case 2:
                        Text.transform.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.BottomLeft;
                        break;
                    case 3:
                        Text.transform.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.BottomRight;
                        break;
                }
                    
                if (debuglog) MelonLogger.Msg(Name + ": Text Object " + i.ToString() + " set");
            }

            GameObject.Destroy(Board.transform.GetChild(1).gameObject);
            GameObject.Destroy(Board.transform.GetChild(7).gameObject);
            GameObject.Destroy(Board.transform.GetChild(8).gameObject);
            GameObject.Destroy(Board.transform.GetChild(9).gameObject);
            GameObject.Destroy(Board.transform.GetChild(10).gameObject);
            if (debuglog) MelonLogger.Msg(Name + ": Children destroyed");
            return Board;
        }
        private void SetTextOnBoard(GameObject Board,int TextNumber,string Output)
        {
            Board.transform.FindChild("InnerWall").GetChild(TextNumber).GetComponent<TextMeshPro>().text = Output;
        }
        private void UpdateBoard()
        {
            string Head1;
            string Head2;
            string Body1 = "";
            string Body2 = "";

            Head1 = 
                BufferList[0];
            Head2 = 
                BufferList2[0];
            for (int i = 3;i < 20; i++)
            {
                if (i != 19)
                {
                    Body1 += BufferList[i] + Environment.NewLine;
                }
                else
                {
                    Body1 += BufferList[i];
                }
            }
            for (int i = 3; i < 20; i++)
            {
                if (i != 19)
                {
                    Body2 += BufferList2[i] + Environment.NewLine;
                }
                else
                {
                    Body2 += BufferList2[i];
                }
            }
            if (debuglogging) MelonLogger.Msg("Text Blocks created");


            //Board at Howard
            if (currentScene == "Gym")
            {
                SetTextOnBoard(Board_Howard, 0, Head1);
                SetTextOnBoard(Board_Howard, 1, Head2);
                SetTextOnBoard(Board_Howard, 2, Body1);
                SetTextOnBoard(Board_Howard, 3, Body2);
                if (debuglogging) MelonLogger.Msg("Text pushed to Howard Board");
            }

            //Training Area Board
            SetTextOnBoard(Board_Training, 0, Head1);
            SetTextOnBoard(Board_Training, 1, Head2);
            SetTextOnBoard(Board_Training, 2, Body1);
            SetTextOnBoard(Board_Training, 3, Body2);
            if (debuglogging) MelonLogger.Msg("Text pushed to Training Board");

        }
        private Vector3 Invert_Values(Vector3 Input)
        {
            Vector3 temp;

            temp.x = 1 / Input.x;
            temp.y = 1 / Input.y;
            temp.z = 1 / Input.z;

            return temp;
        }

        //Overrides
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            SceneInit = false;
            currentScene = sceneName;
        }

    }
}

