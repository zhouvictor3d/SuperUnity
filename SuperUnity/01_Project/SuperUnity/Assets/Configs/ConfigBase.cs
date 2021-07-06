using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SuperUnity.Utils;
using UnityEngine;

namespace SuperUnity.Hub.Configs
{
    public class ConfigBase<T, TEnum> where T : ConfigBase<T, TEnum> where TEnum : struct
    {
        public int FilterTarget;
        public string FilterGroup;
        public int FilterInstallVersionLE;
        public string PrefixKey;
        public string Key;
        public static string ConfigName;


        [JsonIgnore] private const string LOG_TAG = "Config";
        [JsonIgnore] private static Dictionary<string, T> dictData = null;

        [JsonIgnore]
        private static Dictionary<string, T> Data
        {
            get
            {
                if (dictData == null)
                {
                    dictData = Load(typeof(T).Name);
                }
                return dictData;
            }
        }

        public TEnum EnumKey
        {
            get => TypeParseUtils.EnumParse<TEnum>(Key);
        }

        protected string GetPrimeKey()
        {
            return MakePrimeKey(Key, PrefixKey);
        }

        protected static string MakePrimeKey(string key, string prefixKey)
        {
            if (string.IsNullOrEmpty(prefixKey))
                return key;
            return key + "." + prefixKey;
        }

        private static Dictionary<string, T> Load(string strConfigName)
        {
            ConfigName = strConfigName;
            List<T> listConfig = HiFileUtils.LoadJsonFile<List<T>>(HiFileUtils.FileLocation.Resource, $"Configs/{strConfigName}");
            List<List<T>> Groups = listConfig.GroupBy(t => t.GetPrimeKey()).Select(grp => grp.ToList()).ToList();
            List<T> listDataFiltered = Groups.Select(items => items.OrderBy(item => -CalcFilterScore(item)).ElementAt(0)).ToList();
            return listDataFiltered.ToDictionary(item => item.GetPrimeKey(), item => item);
        }

        private static void LogWarning(string message)
        {
#if HI_TARGET_APPLE || HI_TARGET_GOOGLE || HI_TARGET_AMAZON
            Debug.unityLogger.LogWarning(LOG_TAG, message);
#else
            Console.Write(message);
#endif
        }

        public static bool ContainsKey(TEnum key, string prefixKey = null)
        {
            return Data.ContainsKey(MakePrimeKey(key.ToString(), prefixKey));
        }

        public static bool ContainsKey(string key, string prefixKey = null)
        {
            return Data.ContainsKey(MakePrimeKey(key.Trim(), prefixKey));
        }

        public static T GetItem(TEnum key, string prefixKey = null)
        {
            return GetItem(key.ToString(), prefixKey);
        }

        public static List<T> GetItemsbyPrefixKey(string PrefixKey)
        {
            return Data.Where(e => e.Value.PrefixKey == PrefixKey).Select(e => e.Value).ToList();
        }

        public static T GetItem(string key, string prefixKey = null)
        {
            T item;

            if (!Data.TryGetValue(MakePrimeKey(key.Trim(), prefixKey), out item))
            {
                item = null;
            }
            if (item == null)
            {
                LogWarning($"{ConfigName} key [{key.Trim()}] prefixKey [{prefixKey}] config not found");
            }

            return item;
        }

        private static int CalcFilterScore(T configitem)
        {
            int score = 0;
            if (configitem.FilterTarget != 0 && configitem.FilterTarget == AppInfo.Instance.Target)
            {
                score += 80000000; //最重要 完美符合
            }

            if (configitem.FilterTarget == 0)
            {
                score += 40000000; //最重要  通配符合
            }

            if (configitem.FilterGroup != null && configitem.FilterGroup.Trim() != "" && configitem.FilterGroup
                .ToString().Contains(AppInfo.Instance.Group.ToString()))
            {
                score += 20000000; //次重要 完美符合
            }

            if (configitem.FilterGroup == null || configitem.FilterGroup.Trim() == "")
            {
                score += 10000000; //次重要  统配符合
            }

            if (configitem.FilterInstallVersionLE != 0 &&
                configitem.FilterInstallVersionLE > AppInfo.Instance.InstallVersion)
            {
                score += 1000000 - configitem.FilterInstallVersionLE; //三重要  完美符合 这里假设咱们的版本号不超过6位数，挑符合而版本小的
            }

            if (configitem.FilterInstallVersionLE == 0)
            {
                score += 0; //三重要 统配符合
            }

            return score;
        }

        public static List<T> All()
        {
            return Data.Values.ToList();
        }

        public static string GetString(TEnum key, string prefixKey = null, string defaultValue = "")
        {
            return GetString(key.ToString(), prefixKey, defaultValue);
        }

        protected static string GetString(string key, string prefixKey = null, string defaultValue = "")
        {
            if (!typeof(T).IsSubclassOf(typeof(AppConfig)))
            {
                throw new Exception("Not AppConfigItem cannot get as a single value");
            }

            AppConfig item = GetItem(key, prefixKey) as AppConfig;
            if (item == null)
            {
                return defaultValue;
            }

            if (item.Value == null)
            {
                return defaultValue;
            }

            return item.Value;
        }

        public static long GetLong(TEnum key, string prefixKey = null, long defaultValue = 0)
        {
            return GetLong(key.ToString(), prefixKey, defaultValue);
        }

        protected static long GetLong(string key, string prefixKey = null, long defaultValue = 0)
        {
            return TypeParseUtils.SafeLongParse(GetString(key, prefixKey), defaultValue);
        }

        public static int GetInt(TEnum key, string prefixKey = null, int defaultValue = 0)
        {
            return GetInt(key.ToString(), prefixKey, defaultValue);
        }

        protected static int GetInt(string key, string prefixKey = null, int defaultValue = 0)
        {
            return TypeParseUtils.SafeIntParse(GetString(key, prefixKey), defaultValue);
        }

        public static bool GetBool(TEnum key, string prefixKey = null, bool defaultValue = false)
        {
            return GetBool(key.ToString(), prefixKey, defaultValue);
        }

        protected static bool GetBool(string key, string prefixKey = null, bool defaultValue = false)
        {
            return TypeParseUtils.SafeBoolParse(GetString(key, prefixKey), defaultValue);
        }

        public static float GetFloat(TEnum key, string prefixKey = null, float defaultValue = 0.0f)
        {
            return GetFloat(key.ToString(), prefixKey, defaultValue);
        }

        protected static float GetFloat(string key, string prefixKey = null, float defaultValue = 0.0f)
        {
            return TypeParseUtils.SafeFloatParse(GetString(key, prefixKey), defaultValue);
        }
    }
}