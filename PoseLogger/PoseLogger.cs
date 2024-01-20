using MelonLoader;
using RUMBLE.Players.Subsystems;
using RUMBLE.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using PoseLogger;
using RUMBLE.Players;
using RUMBLE.Poses;

namespace PoseLogger
{
    public class PoseLoggerClass : MelonMod
    {
        //variables
        private bool init = false;
        private bool logging = true;

        private string PoseDataName = "";
        private string PoseName = "";
        private string currentScene = "";
        private string LogString = "";

        private float CurrTimestamp = 0;
        private float LastTimestamp = 0;
        private float TimeDiff = 0;

        //Objects
        private PlayerPoseSystem Player_Obj;

        //initializes things
        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
        }

        public override void OnLateInitializeMelon()
        {
            base.OnLateInitializeMelon();
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
                        Player_Obj = GameObject.Find("Player Controller(Clone)/Poses").GetComponent<PlayerPoseSystem>();
                        init = true;
                    }
                    catch
                    {
                    }

                }

                //Enable/Disable Logging to avoid spam
                if (Input.GetKeyDown(KeyCode.L))
                {
                    logging = !logging;
                    LogString = "Logging = " + logging.ToString();
                    MelonLogger.Msg(LogString);
                }


                //Only do in Gym / Park to avoid spam
                if (logging && init && (currentScene == "Gym" || currentScene == "Park"))
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
                            MelonLogger.Msg("-----------------------------------------------------");
                        }

                            //Output
                            LogString = "Pose Name: " + PadString(PoseName, 10) + " | Time since last Pose:" + PadString((TimeDiff).ToString("0"), 6) + "ms";
                            MelonLogger.Msg(LogString);
                        }
                        LastTimestamp = Player_Obj.lastPoseUsedTimestamp;
                    }
                    catch
                    {
                    }
                }
            }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            init = false;
            currentScene = sceneName;
        }

        public string PadString(string Input,int Length)
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
    }
}

