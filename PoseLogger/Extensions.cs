using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PoseLogger
{
    public static class Vector3Extension
    {
        public static void InvertAndSet(this Vector3 obj,Vector3 Input)
        {
            obj.x = 1 / Input.x;
            obj.y = 1 / Input.y;
            obj.z = 1 / Input.z;
        }
    }
    public static class StringExtension
    {
        public enum PaddingMode
        {
            Left,
            Right
        }
        public static void Pad(this string str, PaddingMode PaddingMode, int StringLength)
        {
            if (str.Length > StringLength) throw new ArgumentException("Padding not possible: Input Length > Pad Length");

            string Pad = "";
            int PadLength = StringLength - str.Length;

            if (PadLength > 0) return;

            for (int i = 0; i < PadLength; i++)
            {
                Pad += " ";
            }

            if (PaddingMode == PaddingMode.Left)
                str = Pad + str;
            else
                str = str + Pad;
        }
    }
    public static class ListExtension
    {
        public static void AddAtStart(this List<string> strings, string input,int maxLength)
        {
            if (strings.Count == 0)
                strings.Add(input);
            else
                strings.Insert(0, input);
            if (strings.Count > maxLength)
                strings.RemoveAt(strings.Count - 1);
        }
        public static void Fill(this List<string> strings, int maxLength)
        {
            int fill = maxLength - strings.Count;
            for (int i = 0;i < maxLength; i++)
            {
                strings.Add("");
            }
        }
    }
}
