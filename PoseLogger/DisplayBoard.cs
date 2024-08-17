using Il2CppRUMBLE.Managers;
using Il2CppTMPro;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace PoseLogger
{
    public static class BoardRef
    {
        private static bool debug = false;

        public static GameObject Reference;
        
        public static GameObject GetBoard()
        {
            if (Reference != null) return Reference;
            else
            {
                InitBoardRef();
                return Reference;
            }
        }
        public static void InitBoardRef()
        {
            if (Reference != null) return;

            Vector3 BoardMainPosition = new Vector3(0f, 50f, 0f);
            Vector3 BoardMainRotation = new Vector3(0, 0, 0);
            Vector3 BoardBGOffset = new Vector3(0, 1.175f, 0);
            Vector3 BoardBGScale = new Vector3(0.95f, 0.6f, 0.2f);
            Vector3 BoardTxOffsetHead = new Vector3(0, 0.1f, -0.25f);
            Vector3 BoardTxOffsetBody = new Vector3(0, -0.1f, -0.25f);

            GameObject Text;

            GameObject temp = GameObject.Find("--------------LOGIC--------------/Heinhouser products/Leaderboard/Frame");
            if (debug) MelonLogger.Msg("Board Object get");

            GameObject Board = GameObject.Instantiate(temp);
            if (debug) MelonLogger.Msg("Board Object instantiate");

            Board.transform.position = BoardMainPosition;
            Board.transform.eulerAngles = BoardMainRotation;
            if (debug) MelonLogger.Msg("Position changed");

            Board.name = "Template";
            Board.transform.GetChild(0).name = "TopLog";
            Board.transform.GetChild(2).name = "FootLeft";
            Board.transform.GetChild(3).name = "FootRight";
            Board.transform.GetChild(4).name = "LogLeft";
            Board.transform.GetChild(5).name = "LogRight";
            Board.transform.GetChild(6).name = "InnerWall";
            Board.transform.GetChild(11).name = "LogBigTop";
            Board.transform.GetChild(12).name = "BackPlane";
            if (debug) MelonLogger.Msg("Objects renamed");

            Board.transform.GetChild(6).localPosition = BoardBGOffset;
            Board.transform.GetChild(6).localScale = BoardBGScale;
            if (debug) MelonLogger.Msg("Frame resized");

            temp = GameObject.Find("--------------LOGIC--------------/Heinhouser products/Leaderboard/Text Objects/Titleplate/Leaderboard");
            if (debug) MelonLogger.Msg("Sample Text Object get");

            for (int i = 0; i < 4; i++)
            {
                Text = new GameObject { name = "Text" + i.ToString() };
                Text.transform.SetParent(Board.transform.GetChild(6));
                if (debug) MelonLogger.Msg("Text Object " + i.ToString() + " created");

                if (i <= 1) { Text.transform.localPosition = BoardTxOffsetHead; }
                else { Text.transform.localPosition = BoardTxOffsetBody; }

                Text.transform.localScale.InvertAndSet(BoardBGScale);
                Text.transform.eulerAngles = BoardMainRotation;
                Text.AddComponent<TextMeshPro>();
                Text.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
                if (debug) MelonLogger.Msg("Text created and transformed");

                Text.transform.GetComponent<TextMeshPro>().fontSize = 0.6f;
                Text.transform.GetComponent<TextMeshPro>().font = temp.GetComponent<TextMeshPro>().font;
                Text.transform.GetComponent<TextMeshPro>().fontMaterial = temp.GetComponent<TextMeshPro>().fontMaterial;

                switch (i)
                {
                    case 0:
                        Text.transform.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.TopLeft;
                        Text.transform.GetComponent<TextMeshPro>().text = "Pose/Structure Tracker";
                        break;
                    case 1:
                        Text.transform.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.TopRight;
                        Text.transform.GetComponent<TextMeshPro>().text = "Time since last";
                        break; 
                    case 2:
                        Text.transform.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.BottomLeft;
                        Text.transform.GetComponent<TextMeshPro>().text = "Initialization Text";
                        break;
                    case 3:
                        Text.transform.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.BottomRight;
                        Text.transform.GetComponent<TextMeshPro>().text = "Initialization Text";
                        break;
                }

                if (debug) MelonLogger.Msg("Text Object " + i.ToString() + " set");
            }

            GameObject.Destroy(Board.transform.GetChild(1).gameObject);
            GameObject.Destroy(Board.transform.GetChild(7).gameObject);
            GameObject.Destroy(Board.transform.GetChild(8).gameObject);
            GameObject.Destroy(Board.transform.GetChild(9).gameObject);
            GameObject.Destroy(Board.transform.GetChild(10).gameObject);
            if (debug) MelonLogger.Msg("Children destroyed");

            GameObject.DontDestroyOnLoad(Board.transform);

            Reference = Board;
            Reference.gameObject.SetActive(false);
        }
    }
    public class DisplayBoard
    {
        public enum TextFields
        {
            Head_Left,
            Head_Right,
            Body_Left,
            Body_Right
        }

        private bool debug = false;

        public string 
            LeftHeader = "Pose/Structure Tracker",
            RightHeader = "Time since last";

        public GameObject Board;
        public TextMeshPro HeaderLeft, HeaderRight, BodyLeft, BodyRight;

        public void Init(string Name, Vector3 BoardMainPosition, Vector3 BoardMainRotation)
        {
            if (Board != null) return;

            Board = GameObject.Instantiate(BoardRef.GetBoard());

            Board.name = Name;  
            Board.transform.position = BoardMainPosition;
            Board.transform.eulerAngles = BoardMainRotation;

            HeaderLeft  = Board.transform.GetChild(5).GetChild(0).GetComponent<TextMeshPro>();
            HeaderRight = Board.transform.GetChild(5).GetChild(1).GetComponent<TextMeshPro>();
            BodyLeft    = Board.transform.GetChild(5).GetChild(2).GetComponent<TextMeshPro>();
            BodyRight   = Board.transform.GetChild(5).GetChild(3).GetComponent<TextMeshPro>();

            Board.gameObject.SetActive(true);

            if (debug) MelonLogger.Msg("Board initialized.");
        }
        public void SetAllTextOnBoard(List<string> LeftBuffer, List<string> RightBuffer)
        {
            SetTextOnBoard(TextFields.Head_Left, LeftHeader);
            SetTextOnBoard(TextFields.Head_Right, RightHeader);

            string LeftBody = "", RightBody = "";

            for (int i = 0; i < 17; i++)
            {
                LeftBody += LeftBuffer[i];
                RightBody += RightBuffer[i];

                if (i != 16)
                {
                    LeftBody += Environment.NewLine;
                    RightBody += Environment.NewLine;
                }
            }

            SetTextOnBoard(TextFields.Body_Left, LeftBody); 
            SetTextOnBoard(TextFields.Body_Right, RightBody);

            if (debug) MelonLogger.Msg("Board refreshed.");
        }
        public void SetTextOnBoard(TextFields Field, string Input)
        {
            switch (Field)
            {
                case TextFields.Head_Left:
                    HeaderLeft.text = Input;
                    break;
                case TextFields.Head_Right:
                    HeaderRight.text = Input;
                    break;
                case TextFields.Body_Left:
                    BodyLeft.text = Input;
                    break;
                case TextFields.Body_Right:
                    BodyRight.text = Input;
                    break;
            }
            if (debug) MelonLogger.Msg("Text applied.");
        }
    }
}
