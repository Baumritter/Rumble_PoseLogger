using MelonLoader;
using Il2CppRUMBLE.Managers;
using Il2CppRUMBLE.MoveSystem;
using Il2CppRUMBLE.Players.Subsystems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using RumbleModUI;
using UnityEngine.Playables;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using static RumbleModUI.Baum_API;

namespace PoseLogger
{    
    /// <summary>
     /// Contains global assembly information.
     /// </summary>
    public static class BuildInfo
    {
        public const string ModName = "PoseLogger";
        public const string ModVersion = "1.4.0";
        public const string Description = "Logs Poses";
        public const string Author = "Baumritter";
        public const string Company = "";
    }
    public static class Globals
    {
        internal enum StackType
        {
            Structure,
            Modifier,
            Movement
        }
        internal static Dictionary<string, string> stackDict = new Dictionary<string, string>()
        {
            {"Explode"      ,"Explode"},
            {"Flick"        ,"Flick"},
            {"HoldRight"    ,"Hold"},
            {"Parry"        ,"Parry"},
            {"HoldLeft"     ,"Hold"},
            {"RockSlide"    ,"Dash"},
            {"SpawnCube"    ,"Cube"},
            {"Uppercut"     ,"Uppercut"},
            {"SpawnWall"    ,"Wall"},
            {"Jump"         ,"Jump"},
            {"Kick"         ,"Kick"},
            {"SpawnBall"    ,"Ball"},
            {"Stomp"        ,"Stomp"},
            {"Disc"         ,"Disc"},
            {"SpawnPillar"  ,"Pillar"},
            {"Straight"     ,"Straight"}
        };
        internal static Dictionary<string, StackType> typeDict = new Dictionary<string, StackType>()
        {
            {"Straight" ,StackType.Modifier},
            {"Explode"  ,StackType.Modifier},
            {"Flick"    ,StackType.Modifier},
            {"Hold"     ,StackType.Modifier},
            {"Parry"    ,StackType.Modifier},
            {"Uppercut" ,StackType.Modifier},
            {"Kick"     ,StackType.Modifier},
            {"Stomp"    ,StackType.Modifier},
            {"Disc"     ,StackType.Structure},
            {"Ball"     ,StackType.Structure},
            {"Cube"     ,StackType.Structure},
            {"Wall"     ,StackType.Structure},
            {"Pillar"   ,StackType.Structure},
            {"Jump"     ,StackType.Movement},
            {"Dash"     ,StackType.Movement},
        };
        internal static Dictionary<string, string> structDict = new Dictionary<string, string>()
        {
            {"Disc","Disc"},
            {"SmallRock","Rock"},
            {"Ball","Ball"},
            {"Pillar","Pillar"},
            {"RockCube","Cube"},
            {"Wall","Wall"},
            {"LargeRock","Boulder"},
            {"BoulderBall","BallToy"},

        };
    }
    public class StackData
    {
        public string StackName;
        public DateTime TimeStamp;
        public int ProcessorID;

        public bool HitSomething;
        public string TargetIdentifier;

        public string LeftString;
        public string RightString;

        public void TargetIdentification(ProcessableComponent Target)
        {
            if (Target == null)
                TargetIdentifier = "";

            string Name = Target.name;

            if (!Target.gameObject.transform.parent.name.Contains("Pool"))
                if (Globals.structDict.ContainsKey(Target.name))
                    TargetIdentifier = Globals.structDict[Target.name];
                else
                    TargetIdentifier = Target.name;
            else
                TargetIdentifier = Globals.structDict[Target.name] + "#" + Target.gameObject.transform.GetSiblingIndex();
        }

        public override string ToString()
        {
            return $"ProcessorID: {ProcessorID} StackName: {StackName} TimeStamp: {TimeStamp} HitSomething: {HitSomething} Target: {TargetIdentifier}";
        }
    }
    public class PoseLoggerClass : MelonMod
    {
        #region Board Positions
        private Vector3 Howard_BoardMainPosition = new Vector3(-2.615f, -3.52f, -19.05f);
        private Vector3 Howard_BoardMainRotation = new Vector3(0, 250, 0);
        private Vector3 Training_BoardMainPosition = new Vector3(-42.0299f, 3.194f, -1.0598f);
        private Vector3 Training_BoardMainRotation = new Vector3(0, 250, 0);
        private Vector3 Park_BoardMainPosition = new Vector3(-5.9f, -5.9f, -10f);
        private Vector3 Park_BoardMainRotation = new Vector3(0, 196, 0);
        #endregion

        #region General Tracking
        private static List<StackData> dataList = new List<StackData>();
        private static List<string> moveList = new List<string>();
        private static List<string> delayList = new List<string>();
        private static bool AllowTracking = false;
        private static int dataListLength = 6;
        #endregion

        #region Combo Tracking
        private static bool ComboActive;
        private static int ComboLength;
        private static DateTime ComboStartTime;
        #endregion

        #region Tracker Objects
        private static GameObject OnScreenTracker;
        private static List<DisplayBoard> boards = new List<DisplayBoard>();
        #endregion

        #region General
        private Mod Mod = new Mod();
        private static ModSetting<bool> OnScreenLog, BoardLog;
        private Baum_API.Delay LoadDelay = new Baum_API.Delay();
        private static Baum_API.Folders folderHandler = new Baum_API.Folders();
        private string currentScene;
        #endregion

        [HarmonyPatch(typeof(PlayerStackProcessor), "Execute", new Type[] { typeof(Stack), typeof(StackConfiguration) })]   //This runs when the player does a move
        public static class ExecutePatch
        {
            private static void Prefix(Stack stack, StackConfiguration overrideConfig)
            {
                if (!AllowTracking) return;

                dataList.Insert(0, new StackData()
                {
                    StackName = Globals.stackDict[stack.cachedName],
                    HitSomething = false,
                    TimeStamp = DateTime.Now
                });

            }
        }

        [HarmonyPatch(typeof(PlayerStackProcessor), "OnStackExecutionStarted", new Type[] { typeof(StackConfiguration) })]  // This runs when a stack actually does something
        public static class OnStackExecutePatch
        {
            private static void Postfix(StackConfiguration config)
            {
                if (!AllowTracking) return;

                dataList[0] = new StackData()
                {
                    StackName = dataList[0].StackName,
                    ProcessorID = config.CastingPlayerActorNo,
                    TimeStamp = dataList[0].TimeStamp,
                    HitSomething = true,
                    TargetIdentifier = ""
                };

                if (Globals.typeDict[dataList[0].StackName] == Globals.StackType.Modifier)
                    dataList[0].TargetIdentification(config.TargetProcessable.TryCast<ProcessableComponent>());

                ComboTracker();

                if (dataList.Count > dataListLength)
                    dataList.RemoveAt(dataList.Count - 1);

                string moveString;
                TimeSpan delay;

                switch (ComboLength)
                {
                    case 2:
                        moveString = dataList[1].StackName + ">" + dataList[0].StackName;
                        if (dataList.Count == 2)
                            delay = TimeSpan.Zero;
                        else
                            delay = dataList[0].TimeStamp - dataList[2].TimeStamp;
                        break;
                    case 3:
                        moveString = dataList[2].StackName + ">" + dataList[1].StackName + ">" + dataList[0].StackName;
                        if (dataList.Count == 3)
                            delay = TimeSpan.Zero;
                        else
                            delay = dataList[0].TimeStamp - dataList[3].TimeStamp;
                        break;
                    case 4:
                        moveString = dataList[3].StackName + ">" + dataList[2].StackName + ">" + dataList[1].StackName + ">" +  dataList[0].StackName;
                        if (dataList.Count == 4)
                            delay = TimeSpan.Zero;
                        else
                            delay = dataList[0].TimeStamp - dataList[4].TimeStamp;
                        break;
                    case 5:
                        moveString = dataList[4].StackName + ">" + dataList[3].StackName + ">" + dataList[2].StackName + ">" + dataList[1].StackName + ">" + dataList[0].StackName;
                        if (dataList.Count == 5)
                            delay = TimeSpan.Zero;
                        else
                            delay = dataList[0].TimeStamp - dataList[5].TimeStamp;
                        break;
                    default:
                        moveString = dataList[0].StackName;
                        if (dataList.Count == 1)
                            delay = TimeSpan.Zero;
                        else
                            delay = dataList[0].TimeStamp - dataList[1].TimeStamp;
                        break;
                }

                if (dataList[0].TargetIdentifier != "")
                    moveString += " on " + dataList[0].TargetIdentifier;

                if (ComboActive)
                {
                    moveList[0] = moveString;
                    delayList[0] = delay.TotalMilliseconds.ToString("####") + "ms";
                }
                else
                {
                    moveList.AddAtStart(moveString, 20);
                    delayList.AddAtStart(delay.TotalMilliseconds.ToString("####") + "ms", 20);
                }

                ApplyToBoards();
            }
        }

        public void CreateOnScreenLogger()
        {
            GameObject[] TBChildren = new GameObject[20];
            GameObject[] TBDelay = new GameObject[20];
            GameObject temp = GameObject.Find("Game Instance/UI/LegacyRecordingCameraUI/Panel");
            GameObject blank = new GameObject();
            float Offset = 16f;
            OnScreenTracker = GameObject.Instantiate(temp);
            //Background Panel
            OnScreenTracker.SetActive(false);                                                    //Deactivate the Object
            OnScreenTracker.name = "OnScreenLogger";                                             //Change object name
            OnScreenTracker.transform.SetParent(GameObject.Find("Game Instance/UI").transform);  //Move object to DDOL Area
            OnScreenTracker.transform.position = new Vector3(220f, 790f, 0);                     //Move object to top left corner 
            OnScreenTracker.transform.localScale = new Vector3(1.041625f, 1.25f, 0);             //Move object to top left corner 
            OnScreenTracker.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);     //Change Color and Alpha of picture
            for (int i = 0; i < OnScreenTracker.transform.childCount; i++)
            {
                GameObject.Destroy(OnScreenTracker.transform.GetChild(i).gameObject);
            }                     //Destroy all Children

            //Tracking
            for (int i = 0; i < TBChildren.Length; i++)
            {
                TBChildren[i] = GameObject.Instantiate(blank);
                TBChildren[i].gameObject.AddComponent<TextMeshProUGUI>();
                TBChildren[i].transform.SetParent(OnScreenTracker.transform);
                if (i == 0)
                {
                    TBChildren[i].name = "Line_Head";
                    TBChildren[i].transform.localPosition = new Vector3(-35f, 160f, 0);
                    TBChildren[i].transform.GetComponent<RectTransform>().sizeDelta = new Vector2(520f, 28f);
                    TBChildren[i].transform.GetComponent<TextMeshProUGUI>().text = "Pose/Structure Tracker";
                    TBChildren[i].transform.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
                }
                else
                {
                    TBChildren[i].name = "Line" + i.ToString();
                    TBChildren[i].transform.localPosition = new Vector3(-35f, 150f + Offset - Offset * (i + 1), 0);
                    TBChildren[i].transform.GetComponent<RectTransform>().sizeDelta = new Vector2(520f, 28f);
                    TBChildren[i].transform.GetComponent<TextMeshProUGUI>().text = "";
                    TBChildren[i].transform.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
                }
                TBChildren[i].transform.localScale = new Vector3(0.6f, 0.5f, 1f);
                TBChildren[i].transform.GetComponent<TextMeshProUGUI>().fontSize = 24;
                TBChildren[i].transform.GetComponent<TextMeshProUGUI>().autoSizeTextContainer = false;
            }
            //Delay
            for (int i = 0; i < TBDelay.Length; i++)
            {
                TBDelay[i] = GameObject.Instantiate(blank);
                TBDelay[i].gameObject.AddComponent<TextMeshProUGUI>();
                TBDelay[i].transform.SetParent(OnScreenTracker.transform);
                if (i == 0)
                {
                    TBDelay[i].name = "Delay_Head";
                    TBDelay[i].transform.localPosition = new Vector3(135f, 160f, 0);
                    TBDelay[i].transform.GetComponent<TextMeshProUGUI>().text = "Time since last";
                }
                else
                {
                    TBDelay[i].name = "Delay" + i.ToString();
                    TBDelay[i].transform.localPosition = new Vector3(135f, 150f + Offset - Offset * (i + 1), 0);
                    TBDelay[i].transform.GetComponent<TextMeshProUGUI>().text = "";
                }
                TBDelay[i].transform.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
                TBDelay[i].transform.GetComponent<RectTransform>().sizeDelta = new Vector2(170f, 28f);
                TBDelay[i].transform.localScale = new Vector3(0.6f, 0.5f, 1f);
                TBDelay[i].transform.GetComponent<TextMeshProUGUI>().fontSize = 24;
                TBDelay[i].transform.GetComponent<TextMeshProUGUI>().autoSizeTextContainer = false;
            }
            GameObject.Destroy(blank);
        }
        public static void ComboTracker()
        {
            if (dataList.Count <= 1) return;

            TimeSpan timeSpan = dataList[0].TimeStamp - ComboStartTime;

            if (dataList[0].TargetIdentifier == dataList[1].TargetIdentifier && timeSpan.TotalMilliseconds <= 350)
            {
                //Name Shortening
                switch (dataList[1].StackName)
                {
                    case "Straight":
                        if (dataList[0].StackName == "Uppercut")
                        {
                            dataList[1].StackName = "Struppercut";
                            dataList.RemoveAt(0);
                        }
                        if (dataList[0].StackName == "Kick")
                        {
                            dataList[1].StackName = "Hop";
                            dataList.RemoveAt(0);
                        }
                        break;
                    case "Struppercut":
                        if (dataList[0].StackName == "Kick")
                        {
                            dataList[1].StackName = "Strupperkick";
                            dataList.RemoveAt(0);
                        }
                        break;
                    default:
                        ComboLength++;
                        break;
                }
                ComboActive = true;
                if (ComboLength >= 5)
                    ComboLength = 5;
            }
            else
            {
                ComboActive = false;
                ComboLength = 1;
                ComboStartTime = dataList[0].TimeStamp;
            }
        }
        public static void ApplyToBoards()
        {
            if ((bool)OnScreenLog.SavedValue)
            {
                for (int i = 0; i < moveList.Count - 1; i++)
                {
                    OnScreenTracker.transform.GetChild(i + 1).GetComponent<TextMeshProUGUI>().text = moveList[i];
                    OnScreenTracker.transform.GetChild(i + 21).GetComponent<TextMeshProUGUI>().text = delayList[i];
                }
            }
            if ((bool)BoardLog.SavedValue)
            {
                foreach (DisplayBoard board in boards)
                {
                    board.SetAllTextOnBoard(moveList, delayList);
                }
            }
        }

        public override void OnLateInitializeMelon()
        {
            base.OnLateInitializeMelon();

            UI.instance.UI_Initialized += OnUIInit;
            Baum_API.LoadHandler.PlayerLoaded += WaitAfterInit;

            folderHandler.SetModFolderCustom(BuildInfo.ModName);
            folderHandler.AddSubFolder("Debug");
            folderHandler.CheckAllFoldersExist();

            dataList.Add(new StackData() { ProcessorID = 99, StackName = "Filler", TimeStamp = DateTime.Now });
            moveList.Fill(20);
            delayList.Fill(20);
        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            currentScene = sceneName;
            AllowTracking = false;
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (currentScene == "Gym" && BoardRef.Reference == null)
                BoardRef.InitBoardRef();
        }


        private void OnUIInit()
        {
            Mod.ModName = BuildInfo.ModName;
            Mod.ModVersion = BuildInfo.ModVersion;
            Mod.SetFolder(BuildInfo.ModName);
            Mod.SetSubFolder("Settings");
            Mod.AddDescription("Description", "", BuildInfo.Description, new Tags { IsSummary = true });
            OnScreenLog = Mod.AddToList("OnScreen_Log", true, 0, "Shows the log window in the top left corner.", new Tags());
            BoardLog = Mod.AddToList("PhysicalBoard_Log", true, 0, "Enables the physical boards in the Gym/Park", new Tags());

            Mod.GetFromFile();

            Mod.ModSaved += OnSaveOrInit;

            UI.instance.AddMod(Mod);

            Baum_API.MelonLoggerExtension.Log("Added Mod.");
        }
        public void WaitAfterInit()
        {
            LoadDelay.Start(2, false, new System.Action(() => { OnSaveOrInit(); }));
        }
        private void OnSaveOrInit()
        {
            if (OnScreenTracker == null)
                CreateOnScreenLogger();

            if (currentScene == "Gym" || currentScene == "Park")
            {
                OnScreenTracker.SetActive((bool)OnScreenLog.SavedValue);

                if (boards.Count > 0)
                {
                    foreach (DisplayBoard board in boards)
                    {
                        GameObject.Destroy(board.Board);
                    }
                }

                if ((bool)BoardLog.SavedValue)
                {
                    switch (currentScene)
                    {
                        case "Gym":
                            boards.Clear();

                            boards.Add(new DisplayBoard());
                            boards.Add(new DisplayBoard());

                            boards[0].Init("HowardBoard", Howard_BoardMainPosition, Howard_BoardMainRotation);
                            boards[1].Init("RingBoard", Training_BoardMainPosition, Training_BoardMainRotation);
                            break;
                        case "Park":
                            boards.Clear();

                            boards.Add(new DisplayBoard());

                            boards[0].Init("ParkBoard", Park_BoardMainPosition, Park_BoardMainRotation);
                            break;
                    }
                }

                if (!(bool)BoardLog.SavedValue && !(bool)OnScreenLog.SavedValue)
                    AllowTracking = false;
                else
                    AllowTracking = true;
            }
            else
            {
                AllowTracking = false;
                OnScreenTracker.gameObject.SetActive(false);
            }

        }
    }
}

