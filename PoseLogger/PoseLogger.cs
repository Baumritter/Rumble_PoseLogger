using MelonLoader;
using RUMBLE.MoveSystem;
using RUMBLE.Players.Subsystems;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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

        private bool structconsole = false;
        private bool structfile = false;
        private bool poseconsole = false;
        private bool posefile = false;

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


        private float CurrTimestamp = 0;
        private float LastTimestamp = 0;
        private float TimeDiff = 0;

        private List<StructureTracking> Tracker = new List<StructureTracking>();
        //--------------------------------------------------
        //--------------------------------------------------

        //--------------------------------------------------
        //--------------------------------------------------
        //Objects
        private PlayerPoseSystem Player_Obj;
        private GameObject PoolManager;
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
            CheckandCreateFolder(BaseFolder + @"\" + ModFolder);
            CheckandCreateFolder(BaseFolder + @"\" + ModFolder + @"\" + LogFolder);
            CheckandCreateFolder(BaseFolder + @"\" + ModFolder + @"\" + SettingsFolder);
            PoseLogFileString = BaseFolder + @"\" + ModFolder + @"\" + LogFolder + @"\" + PoseLogFileName + DateTime.Now.ToString("yyyyMMddHHmmss") + LogFileSuffix;
            StructLogFileString = BaseFolder + @"\" + ModFolder + @"\" + LogFolder + @"\" + StructLogFileName + DateTime.Now.ToString("yyyyMMddHHmmss") + LogFileSuffix;
            MelonLogger.Msg("PoseLogFile: " + PoseLogFileString);
            MelonLogger.Msg("PoseLogFile: " + StructLogFileString);
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
                        //For Pose Tracking
                        Player_Obj = GameObject.Find("Player Controller(Clone)/Poses").GetComponent<PlayerPoseSystem>();
                        if (debug) MelonLogger.Msg("Player Object found.");

                        //For Structure Tracking
                        PoolManager = GameObject.Find("Game Instance/Pre-Initializable/PoolManager");
                        if (debug) MelonLogger.Msg("PoolManager found.");

                        //Get all Structure Pool Objects
                        if(discenb) Discs = PoolManager.transform.GetChild(DiscPool).GetComponentsInChildren<ProcessableComponent>(true);
                        if(ballenb) Balls = PoolManager.transform.GetChild(BallPool).GetComponentsInChildren<ProcessableComponent>(true);
                        if(pillenb) Pills = PoolManager.transform.GetChild(PillPool).GetComponentsInChildren<ProcessableComponent>(true);
                        if(cubeenb) Cubes = PoolManager.transform.GetChild(CubePool).GetComponentsInChildren<ProcessableComponent>(true);
                        if(wallenb) Walls = PoolManager.transform.GetChild(WallPool).GetComponentsInChildren<ProcessableComponent>(true);
                        if(lgrkenb) LgRck = PoolManager.transform.GetChild(LgRkPool).GetComponentsInChildren<ProcessableComponent>(true);


                    init = true;
                        if (debug) MelonLogger.Msg(init);
                    }
                    catch
                    {
                    }

                }

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
                    //Structure based logging
                    if(discenb) CheckActivityOfStructurePool(Discs);
                    if(ballenb) CheckActivityOfStructurePool(Balls);
                    if(pillenb) CheckActivityOfStructurePool(Pills);
                    if(cubeenb) CheckActivityOfStructurePool(Cubes);
                    if(wallenb) CheckActivityOfStructurePool(Walls);
                    if(lgrkenb) CheckActivityOfStructurePool(LgRck);

                //Pose based logging
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
                                break;
                            case "DoublePunch":
                                PoseName = "Flick";
                                break;
                            case "Poses_Hold":
                                PoseName = "Hold";
                                break;
                            case "Poses_SpawnCube":
                                PoseName = "Cube";
                                break;
                            case "Poses_Dash":
                                PoseName = "Dash";
                                break;
                            case "Poses_Uppercut_015":
                                PoseName = "Uppercut";
                                break;
                            case "PosesSpawnWall":
                                PoseName = "Wall";
                                break;
                            case "Poses_Jump":
                                PoseName = "Jump";
                                break;
                            case "Poses_Kick_015":
                                PoseName = "Kick";
                                break;
                            case "Poses_SpawnBall":
                                PoseName = "Ball";
                                break;
                            case "Poses_Stomp":
                                PoseName = "Stomp";
                                break;
                            case "Poses_Pillar_015":
                                PoseName = "Pillar";
                                break;
                            case "Poses_Straight_015":
                                PoseName = "Straight";
                                break;
                            case "MoveSystemDisc 1":
                                PoseName = "Disc";
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
                                WriteToLogFile(PoseLogFileString,"-----------------------------------------------------",poseconsole,posefile);
                            }

                            //Output
                            LogString = "Pose Name: " + PadString(PoseName, 10) + " | Time since last Pose:" + PadString((TimeDiff).ToString("0"), 6) + "ms";
                            
                            WriteToLogFile(PoseLogFileString, LogString, poseconsole, posefile);  
                        }
                        LastTimestamp = Player_Obj.lastPoseUsedTimestamp;
                    }
                    catch
                    {
                    }
                }
            }

        //Functions
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
                    "Boulder Tracked: True"
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
                MelonLogger.Msg("Default Settings applied");
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

        public void CheckActivityOfStructurePool(ProcessableComponent[] Children)
        {
            StructureTracking current = new StructureTracking();
            StructureTracking last = new StructureTracking();
            int index;
            string temp;

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

                        if (current.LastActive != last.LastActive)
                        {
                            if (current.LastActive)
                            {
                                temp = PadString(current.StructureName,10) + " spawned.";
                                WriteToLogFile(StructLogFileString,temp, structconsole, structfile);
                            }
                            else
                            {
                                temp = PadString(current.StructureName, 10) + " broke.";
                                WriteToLogFile(StructLogFileString,temp, structconsole, structfile);
                            }
                        }

                        if (current.LastActive && (current.Timestamp != last.Timestamp || current.MoveName != last.MoveName))
                        {
                            temp = PadString(current.StructureName, 10) + " affected by " + FillString(current.MoveName, 12);
                            if (last.Timestamp == -1)
                            {
                                temp += "| First Modifier  ";
                                temp += "| Player: ";
                                try
                                {
                                    temp += SanitizeName(Children[i].latestInfluencedProcessor.Cast<PlayerStackProcessor>().ParentController.AssignedPlayer.Data.GeneralData.PublicUsername.ToString());
                                }
                                catch
                                {
                                    temp += "No Player";
                                }
                                WriteToLogFile(StructLogFileString,temp,structconsole,structfile);
                            }
                            else
                            {
                                if (currentScene == "Gym")
                                {
                                    temp += FillString("| Delay: " + ((current.Timestamp - last.Timestamp) * 1000).ToString("0") + "ms", 18);
                                }
                                if (currentScene == "Park")
                                {
                                    if(current.Timestamp-last.Timestamp == 0)
                                    {
                                        temp += FillString("| Delay: <128ms", 18);
                                    }
                                    else
                                    {
                                        temp += FillString("| Delay: ~" + ((current.Timestamp - last.Timestamp)).ToString("0") + "ms", 18);
                                    }
                                }
                                temp += "| Player: ";
                                try
                                {
                                    temp += SanitizeName(Children[i].latestInfluencedProcessor.Cast<PlayerStackProcessor>().ParentController.AssignedPlayer.Data.GeneralData.PublicUsername.ToString());
                                }
                                catch 
                                {
                                    temp += "No Player";
                                }
                                WriteToLogFile(StructLogFileString,temp, structconsole, structfile);
                            }
                        }
                        Tracker[index] = new StructureTracking { StructureName = current.StructureName, LastActive = current.LastActive, Timestamp = current.Timestamp, MoveName = current.MoveName };
                    }
                }
            }
            catch
            {

            }

        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            init = false;
            currentScene = sceneName;
        }

    }
}

