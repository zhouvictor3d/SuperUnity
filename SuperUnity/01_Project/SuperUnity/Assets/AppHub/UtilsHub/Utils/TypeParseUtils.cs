using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuperUnity.Utils
{
    public class TypeParseUtils
    {
        const string LOG_TAG = nameof(TypeParseUtils);
        public static bool SafeBoolParse(string s, bool defaultValue = false)
        {
            if (!Boolean.TryParse(s, out bool value))
            {
                if (!Int32.TryParse(s, out int iValue))
                {
                    return defaultValue;
                }
                return Convert.ToBoolean(iValue);
            }

            return value;
        }

        public static float SafeFloatParse(string s, float defaultValue = 0.0f)
        {
            if (!Single.TryParse(s, out float value)) return defaultValue;
            return value;
        }

        public static long SafeLongParse(string s, long defaultValue = 0)
        {
            if (!Int64.TryParse(s, out long value)) return defaultValue;
            return value;
        }

        public static int SafeIntParse(string s, int defaultValue = 0)
        {
            if (!Int32.TryParse(s, out int value)) return defaultValue;
            return value;
        }

        public static Vector3 SafeVectorParse(string s, Vector3 defaultValue = new Vector3())
        {
            Vector3 value = new Vector3();
            string[] stringDatas = s.Split(',');
            for (int i = 0; i < stringDatas.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        {
                            value.x = SafeFloatParse(stringDatas[i], defaultValue.x);
                        }
                        break;
                    case 1:
                        {
                            value.y = SafeFloatParse(stringDatas[i], defaultValue.y);
                        }
                        break;
                    case 2:
                        {
                            value.z = SafeFloatParse(stringDatas[i], defaultValue.z);
                        }
                        break;
                    default:
                        break;
                }
            }

            return value;
        }

        public static Vector3Int SafeVectorIntParse(string s, Vector3Int defaultValue = new Vector3Int())
        {
            Vector3Int value = new Vector3Int();
            string[] stringDatas = s.Split(',');
            for (int i = 0; i < stringDatas.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        {
                            value.x = SafeIntParse(stringDatas[i], defaultValue.x);
                        }
                        break;
                    case 1:
                        {
                            value.y = SafeIntParse(stringDatas[i], defaultValue.y);
                        }
                        break;
                    case 2:
                        {
                            value.z = SafeIntParse(stringDatas[i], defaultValue.z);
                        }
                        break;
                    default:
                        break;
                }
            }

            return value;
        }

        public static TEnum SafeEnumParse<TEnum>(string s, TEnum defaultValue) where TEnum : struct
        {
            if (!Enum.TryParse(s, out TEnum value)) return defaultValue;
            return value;
        }

        public static Color SafeColorParse(string s)
        {
            if (!ColorUtility.TryParseHtmlString(s, out Color value))
                return Color.black;
            return value;
        }

        /// <summary>
        /// only basic Types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Dictionary<string, T> SafeDictionaryParse<T>(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Trim() == "")
            {
                return new Dictionary<string, T>();
            }
            Dictionary<string, string> dictRaw = value.Split(';').Select(x => x.Split(new char[] { ':' }, 2)).ToDictionary(l => l[0], l => l.Length > 1 ? l[1] : null);
            Dictionary<string, T> dict = dictRaw.ToDictionary(e => e.Key, e => (T)Convert.ChangeType(e.Value, typeof(T))).Where(e => e.Value != null).ToDictionary(e => e.Key, e => e.Value);
            return dict;
        }

        /// <summary>
        ///  only basic Types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static List<T> SafeListParse<T>(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Trim() == "")
            {
                return new List<T>();
            }
            List<string> listRaw = value.Split(';').ToList();
            List<T> list = listRaw.Select(item => (T)Convert.ChangeType(item, typeof(T))).Where(item => item != null).ToList();
            return list;
        }

        public static TEnum EnumParse<TEnum>(string s) where TEnum : struct
        {
            if (Enum.TryParse(s, out TEnum value))
            {
                return value;
            }
            Debug.unityLogger.LogError(LOG_TAG, $"EnumParse<{typeof(TEnum).FullName}>({s}) Unknow Value");
            return (TEnum)Enum.ToObject(typeof(TEnum), 0);
        }
    }
}
