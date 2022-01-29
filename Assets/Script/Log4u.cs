using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SeolMJ
{
    public static class Log4u
    {
        public static void Log(LogPreset preset, string content, byte state = 0)
        {
#if UNITY_EDITOR
            Debug.Log(preset.Get(content, state));
#endif
        }

        public static void Error(LogPreset preset, string content, Exception exception, byte state = 0)
        {
#if UNITY_EDITOR
            Debug.LogError(preset.GetError(content, state) + "\n" + exception.Message);
#endif
        }

        public static void Error(LogPreset preset, string content, byte state = 0)
        {
#if UNITY_EDITOR
            Debug.LogError(preset.GetError(content, state));
#endif
        }
    }

    public struct LogPreset
    {
        public string prefix;
        public string hex;
        public string prefixHex;
        public string gradientLine;
        public int width;

        public LogPreset(string prefix, Color32 color, int padding = 0)
        {
            this.prefix = prefix;
            this.hex = string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", color.r, color.g, color.b, color.a);
            this.prefixHex = hex[..6] + string.Format("{0:X2}", color.a / 2);
            this.width = padding;
            if (padding != 0) gradientLine = string.Format(gradient, hex[..6]);
            else gradientLine = null;
        }

        public void Line(int padding)
        {
            this.width = padding;
            if (padding != 0) gradientLine = string.Format(gradient, hex[..6]);
            else gradientLine = null;
        }

        public string Get(string content, byte state = 0)
        {
            return state switch
            {
                1 => "  <color=#" + prefixHex + ">¦£ " + prefix + ":</color> <color=#" + hex + "><b>" + content + gradientLine + "</b></color>",
                2 => "  <color=#" + prefixHex + ">¦¢ " + prefix + ":</color> <color=#" + hex + "><b>" + content + "</b></color>",
                3 => "  <color=#" + prefixHex + ">¦¦ " + prefix + ":</color> <color=#" + hex + "><b>" + content + gradientLine + "</b></color>",
                4 => "  <color=#" + prefixHex + ">¦§ " + prefix + ":</color> <color=#" + hex + "><b>" + content + gradientLine + "</b></color>",
                _ => "  <color=#" + prefixHex + "> " + prefix + ":</color> <color=#" + hex + "><b>" + content + "</b></color>"
            };
        }

        public string GetError(string content, byte state = 0)
        {
            return state switch
            {
                1 => "  <color=#C1392B>¦£ " + prefix + ":</color> <color=#FF6352><b>" + content + gradientLine + "</b></color>",
                2 => "  <color=#C1392B>¦¢ " + prefix + ":</color> <color=#FF6352><b>" + content + "</b></color>",
                3 => "  <color=#C1392B>¦¦ " + prefix + ":</color> <color=#FF6352><b>" + content + gradientLine + "</b></color>",
                4 => "  <color=#C1392B>¦§ " + prefix + ":</color> <color=#FF6352><b>" + content + gradientLine + "</b></color>",
                _ => "  <color=#C1392B> " + prefix + ":</color> <color=#FF6352><b>" + content + "</b></color>"
            };
        }

        public const string gradient = " <color=#{0}FF>-</color><color=#{0}E0>-</color><color=#{0}C0>-</color><color=#{0}A0>-</color><color=#{0}80>-</color><color=#{0}60>-</color><color=#{0}40>-</color><color=#{0}20>-</color>";
    }
}
