using Il2CppSystem;
using MelonLoader;
using TMPro;
using UnityEngine;

namespace Poselogger_BoardExtension
{
    public class Poselogger_BoardExtensionClass : MelonMod
    {
        //constants
        private const bool debuglog = true;


        //variables
        private Vector3 Howard_BoardMainPosition = new Vector3(-2.615f, -3.52f, -19.05f);
        private Vector3 Howard_BoardMainRotation = new Vector3(0, 250, 0);
        private Vector3 Training_BoardMainPosition = new Vector3(-42.0299f, 3.194f, -1.0598f);
        private Vector3 Training_BoardMainRotation = new Vector3(0, 250, 0);

        private string Output;

        private string currentScene;

        //objects
        private GameObject Text_Howard;
        private GameObject Text_Training;
        private GameObject Logger;

        //initializes things
        public override void OnLateInitializeMelon()
        {
            base.OnLateInitializeMelon();
        }

        //Run every update
        public override void OnUpdate()
        {
            //Base Updates
            base.OnUpdate();

            if (currentScene == "Gym")
            {
                if (Text_Howard == null || Text_Training == null)
                {
                    Text_Howard = SpawnLeaderBoard("Gym_Howard_Board", Howard_BoardMainPosition, Howard_BoardMainRotation, debuglog);
                    Text_Training = SpawnLeaderBoard("Gym_Training_Board", Training_BoardMainPosition, Training_BoardMainRotation, debuglog);
                }
                else
                {
                    try
                    {
                        if (Logger == null) { Logger = GameObject.Find("Game Instance/UI/OnScreenLogger"); }

                        Output =
                            "Last Move: " + Environment.NewLine +
                            Logger.transform.GetChild(19).GetComponent<TextMeshProUGUI>().text + Environment.NewLine +
                            "Time since last Move: " + Environment.NewLine +
                            Logger.transform.GetChild(39).GetComponent<TextMeshProUGUI>().text;

                        SetBoardText(Text_Howard, Output);
                        SetBoardText(Text_Training, Output);
                    }
                    catch
                    {

                    }
                }
            }


        }

        //Functions
        private GameObject SpawnLeaderBoard(string Name, Vector3 BoardMainPosition, Vector3 BoardMainRotation, bool debuglog)
        {
            Vector3 BoardBGOffset = new Vector3(0, 1.175f, 0);
            Vector3 BoardBGScale = new Vector3(0.95f, 0.6f, 0.2f);
            Vector3 BoardTxOffset = new Vector3(0, 0, -0.25f);
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

            Text = new GameObject { name = "Text" };
            Text.transform.SetParent(Board.transform.GetChild(6));
            if (debuglog) MelonLogger.Msg(Name + ": Text Object created");

            Text.transform.localPosition = BoardTxOffset;
            Text.transform.localScale = Invert_Values(BoardBGScale);
            Text.transform.eulerAngles = BoardMainRotation;
            Text.AddComponent<TextMeshPro>();
            Text.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
            if (debuglog) MelonLogger.Msg(Name + ": Text created and transformed");

            temp = GameObject.Find("--------------LOGIC--------------/Heinhouser products/Leaderboard/Text Objects/Titleplate/Leaderboard");
            if (debuglog) MelonLogger.Msg(Name + ": Sample Text Object get");

            Text.transform.GetComponent<TextMeshPro>().text = "Initialization Text";
            Text.transform.GetComponent<TextMeshPro>().fontSize = 1;
            Text.transform.GetComponent<TextMeshPro>().font = temp.GetComponent<TextMeshPro>().font;
            Text.transform.GetComponent<TextMeshPro>().fontMaterial = temp.GetComponent<TextMeshPro>().fontMaterial;
            if (debuglog) MelonLogger.Msg(Name + ": Text Object set");

            GameObject.Destroy(Board.transform.GetChild(1).gameObject);
            GameObject.Destroy(Board.transform.GetChild(7).gameObject);
            GameObject.Destroy(Board.transform.GetChild(8).gameObject);
            GameObject.Destroy(Board.transform.GetChild(9).gameObject);
            GameObject.Destroy(Board.transform.GetChild(10).gameObject);
            if (debuglog) MelonLogger.Msg(Name + ": Children destroyed");
            return Text;
        }
        private void SetBoardText(GameObject Text, string Input)
        {
            Text.transform.GetComponent<TextMeshPro>().text = Input;
        }
        private Vector3 Invert_Values(Vector3 Input)
        {
            Vector3 temp;

            temp.x = 1 / Input.x;
            temp.y = 1 / Input.y;
            temp.z = 1 / Input.z;

            return temp;
        }



        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            currentScene = sceneName;
        }

    }
}

