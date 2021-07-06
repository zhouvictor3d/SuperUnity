using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#if HI_TARGET_APPLE || HI_TARGET_GOOGLE || HI_TARGET_AMAZON
using UnityEngine;
#endif

using Object = System.Object;

namespace SuperUnity.Utils
{
    public class HiFileUtils
    {
        private static readonly byte[] encryptHeader = new byte[] { 0x21, 0x21, 0x50, 0x68, 0x6f, 0x65, 0x6e, 0x69, 0x78, 0x21, 0x21 };

        private static readonly byte[] encryptKey = new byte[]
        {
            0x48, 0x69, 0x20, 0x50, 0x68, 0x6f, 0x65, 0x6e, 0x69,
            0x78, 0x21, 0xc8, 0x48, 0x6f, 0x77, 0x20, 0xF1, 0x72,
            0x65, 0x20, 0x79, 0x6f, 0x75, 0x3f, 0x20, 0x13, 0x20,
            0x61, 0x6d, 0x20, 0x47, 0x6f, 0x6f, 0x64, 0x21, 0x65,
            0x32, 0x01, 0xab, 0xcb, 0xcc, 0x6f, 0x64, 0xbc, 0x06
        };

        private static readonly string encryptSrKey = "Jbjhhzstsl,Bldhbfh.";

        private const string LOG_TAG = "HiFileUtils";

        public enum FileLocation
        {
            Resource,
            Storage,
            StreamingAssets,
            Network,
            Editor
        }

        private static void LogWarning(string message)
        {
#if HI_TARGET_APPLE || HI_TARGET_GOOGLE || HI_TARGET_AMAZON
            Debug.unityLogger.LogWarning(LOG_TAG, message);
#else
            Console.Write(message);
#endif
        }

        private static void Log(string message)
        {
#if HI_TARGET_APPLE || HI_TARGET_GOOGLE || HI_TARGET_AMAZON
            Debug.unityLogger.Log(LOG_TAG, message);
#else
            Console.Write(message);
#endif
        }

        public static T LoadJsonFile<T>(FileLocation location, string filepath)
        {
            string text = Encoding.UTF8.GetString(LoadBytesFile(location, filepath));
            return JsonConvert.DeserializeObject<T>(text);
        }

        public static void LoadJsonFileAsync<T>(FileLocation location, string filepath, Action<T> successCallback, Action failCallback = null)
        {
            try
            {
                LoadBytesFileAsync(location, filepath, successCallback: (bytes) =>
            {
                try
                {
                    string text = Encoding.UTF8.GetString(bytes);
                    successCallback?.Invoke(JsonConvert.DeserializeObject<T>(text));
                }
                catch (Exception e)
                {
                    LogWarning($"LoadJsonFile Error {location} {filepath}{e.Message}");
                    failCallback?.Invoke();
                }
            });
            }
            catch (Exception e)
            {
                LogWarning($"LoadJsonFile Error {location} {filepath}{e.Message}");
                failCallback?.Invoke();
            }
        }

        public static string LoadStringFile(FileLocation location, string filepath)
        {
            try
            {
                return Encoding.UTF8.GetString(LoadBytesFile(location, filepath));
            }
            catch (Exception e)
            {
                LogWarning($"LoadStringListFile Error {location} {filepath}");
                return null;
            }
        }

        public static List<string> LoadStringListFile(FileLocation location, string filepath)
        {
            try
            {
                string text = Encoding.UTF8.GetString(LoadBytesFile(location, filepath));
                return text.Split('\n').ToList();
            }
            catch (Exception e)
            {
                LogWarning($"LoadStringListFile Error {location} {filepath}");
                return null;
            }
        }

        public static void LoadStringListFileAsync(FileLocation location, string filepath, Action<List<string>> successCallback, Action failCallback = null)
        {
            try
            {
                LoadBytesFileAsync(location, filepath, successCallback: (bytes) =>
                 {
                     string text = Encoding.UTF8.GetString(bytes);
                     List<string> strsplit = text.Split('\n').ToList();
                     successCallback?.Invoke(strsplit);
                 });
            }
            catch (Exception e)
            {
                LogWarning($"LoadStringListFile Error {location} {filepath}");
                failCallback?.Invoke();
            }
        }

        public static byte[] LoadBytesFile(FileLocation location, string filepath)
        {
            byte[] dataArray = null;
            Log($"loading file {location} : {filepath}");
            if (location == FileLocation.Resource)
            {
#if HI_TARGET_APPLE || HI_TARGET_GOOGLE || HI_TARGET_AMAZON
                TextAsset textAsset = Resources.Load<TextAsset>(filepath);
                dataArray = textAsset.bytes;
#else
                dataArray = File.ReadAllBytes(GetPathForLocation(location, filepath));
#endif
            }
            else
            {
                dataArray = File.ReadAllBytes(GetPathForLocation(location, filepath));
            }

            if (IsEncrypt(dataArray))
            {
                dataArray = DecryptBytes(dataArray);
            }
            return dataArray;
        }

        public static void LoadBytesFileAsync(FileLocation location, string filepath, Action<byte[]> successCallback, Action failCallback = null)
        {
            byte[] dataArray = null;
            Log($"loading file {location} : {filepath}");
            if (location == FileLocation.Resource)
            {
#if HI_TARGET_APPLE || HI_TARGET_GOOGLE || HI_TARGET_AMAZON
                ResoucesLoadAsync<TextAsset>(filepath, (textAsset) =>
                {
                    dataArray = textAsset.bytes;
                    if (IsEncrypt(dataArray))
                    {
                        dataArray = DecryptBytes(dataArray);
                    }
                    successCallback?.Invoke(dataArray);
                });

#else
             string file = GetPathForLocation(location, filepath);
             Task.Run(() =>
                {
                    dataArray = File.ReadAllBytes(file);
                    if (IsEncrypt(dataArray))
                    {
                        dataArray = DecryptBytes(dataArray);
                    }
                    successCallback?.Invoke(dataArray);
                });
#endif
            }
            else
            {
                string file = GetPathForLocation(location, filepath);
                Task.Run(() =>
                {
                    dataArray = File.ReadAllBytes(file);
                    if (IsEncrypt(dataArray))
                    {
                        dataArray = DecryptBytes(dataArray);
                    }
                    successCallback?.Invoke(dataArray);
                });
            }
        }

        private static IEnumerator ResoucesLoadAsync<T>(string path, Action<T> successCallback = null, Action failCallback = null) where T : UnityEngine.Object
        {
            ResourceRequest request = Resources.LoadAsync<T>(path);
            yield return request;
            if (request != null && request.isDone)
            {
                T res = request.asset as T;
                successCallback?.Invoke(res);
            }
            else
            {
                failCallback?.Invoke();
            }
        }

        public static void SaveJsonFile(FileLocation location, string filepath, Object obj, bool isEncrypt = false, bool formatJson = false, bool isSafe = false, bool isAsync = false, Action<bool> asyncSaveResult = null)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());//Enum 采用字符串保存
            string text = JsonConvert.SerializeObject(obj, formatJson ? Formatting.Indented : Formatting.None, settings);
            SaveTextFile(location, filepath, text, isEncrypt, isSafe, isAsync, asyncSaveResult);
        }

        public static void SaveStringListFile(FileLocation location, string filepath, List<string> stringList, bool isEncrypt = false, bool isSafe = false, bool isAsync = false, Action<bool> asyncSaveResult = null)
        {
            string text = string.Join("\n", stringList);
            SaveTextFile(location, filepath, text, isEncrypt, isSafe, isAsync, asyncSaveResult);
        }

        public static void SaveTextFile(FileLocation location, string filepath, string text, bool isEncrypt = false, bool isSafe = false, bool isAsync = false, Action<bool> asyncSaveResult = null)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);
            SaveBytesFile(location, filepath, bytes, isEncrypt, isSafe, isAsync, asyncSaveResult);
        }

        public static void SaveBytesFile(FileLocation location, string filepath, byte[] bytes, bool isEncrypt = false, bool isSafe = false, bool isAsync = false, Action<bool> asyncSaveResult = null)
        {
            byte[] dataArray = bytes;
            if (isEncrypt)
            {
                dataArray = EncryptBytes(dataArray);
            }
            string file = GetPathForLocation(location, filepath);
            if (isAsync)
            {
                Task.Run(() =>
                {
                    SafeSaveFile(file, dataArray, isSafe);
                    asyncSaveResult?.Invoke(true);
                });
            }
            else
            {
                SafeSaveFile(file, dataArray, isSafe);
            }
        }

        private static void SafeSaveFile(string filepath, byte[] dataArray, bool isSafe)
        {
            //TODO 会保留一个Temp文件
            if (isSafe)
            {
                string newTempFile = $"{filepath}_newtemp";
                string lastTempFile = $"{filepath}_lasttemp";
                File.WriteAllBytes(newTempFile, dataArray);
                if (File.Exists(filepath))
                {
                    File.Replace(newTempFile, filepath, lastTempFile);
                }
                else
                {
                    File.Copy(newTempFile, filepath);
                }
            }
            else
            {
                File.WriteAllBytes(filepath, dataArray);
            }
        }

        /// <summary>
        /// 删除缓存文件夹内所有文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="deleteDirectory"></param>
        public static void DeleteAllFile(FileLocation location, string path, bool deleteDirectory = false)
        {
            path = GetPathForLocation(location, path);
            DeleteAllFile(path, deleteDirectory);
        }

        private static void DeleteAllFile(string path, bool deleteDirectory = false)
        {
            if (Directory.Exists(path))
            {
                var pathEnumtor = Directory.GetFileSystemEntries(path).GetEnumerator();
                while (pathEnumtor.MoveNext())
                {
                    string curPath = (pathEnumtor.Current as string).Replace("\\", "/");
                    if (Directory.Exists(curPath))
                    {
                        Directory.Delete(curPath, true);
                    }
                    else if (File.Exists(curPath))
                    {
                        File.Delete(curPath);
                    }
                }

                if (deleteDirectory)
                {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path);
                    }
                }
            }
        }

        /// <summary>
        /// 设置路径 如果路径不存在就创建路径
        /// </summary>
        /// <param name="filePath"></param>
        public static void CreateFilePath(FileLocation location, string filePath)
        {
            string realfilePath = GetPathForLocation(location, filePath);
            realfilePath = Path.GetDirectoryName(realfilePath);
            if (!Directory.Exists(realfilePath))
            {
                Directory.CreateDirectory(realfilePath);
            }
        }

        /// <summary>
        /// 查看目录是否存在,如果不存在则创建
        /// </summary>
        /// <param name="path"></param>
        public static void CreateDirectoryPath(FileLocation location, string path)
        {
            string realfilePath = GetPathForLocation(location, path);
            if (!Directory.Exists(realfilePath))
            {
                Directory.CreateDirectory(realfilePath);
            }
        }

        public static string CombinePath(string dirName, string fileName)
        {
            dirName = dirName.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            fileName = fileName.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return Path.Combine(dirName, fileName);
        }

        public static string GetPathForLocation(FileLocation location, string filepath)
        {
#if HI_TARGET_APPLE || HI_TARGET_GOOGLE || HI_TARGET_AMAZON
            switch (location)
            {
                case FileLocation.Resource:
                    throw new ArgumentException("Try to Save File to Resource");
                case FileLocation.Storage:
                    return CombinePath(Application.persistentDataPath, filepath);
                case FileLocation.StreamingAssets:
                    return CombinePath(Application.streamingAssetsPath, filepath);
                case FileLocation.Network: //完整的网络地址
                    return filepath;
                case FileLocation.Editor:
                    return CombinePath(Application.dataPath, filepath);
            }
            throw new ArgumentException($"Unknow FileLocation {location}");
#else
            return CombinePath(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), filepath);
#endif
        }

        private static bool IsEncrypt(byte[] data)
        {//检查加密头
            if (data.Length < encryptHeader.Length)
            {
                return false;
            }
            for (int i = 0; i < encryptHeader.Length; i++)
            {
                if (data[i] != encryptHeader[i])
                {
                    return false;
                }
            }
            return true;
        }

        private static byte[] EncryptBytes(byte[] data)
        {
            byte[] result = new byte[data.Length + encryptHeader.Length];
            for (int i = 0; i < encryptHeader.Length; i++)
            {//加入加密头
                result[i] = encryptHeader[i];
            }
            for (int i = 0; i < data.Length; i++)
            {
                result[i + encryptHeader.Length] = (byte)(data[i] ^ encryptKey[(i + (1 / 7)) % encryptKey.Length]);
            }
            return result;
        }

        private static byte[] DecryptBytes(byte[] data)
        {
            byte[] result = new byte[data.Length - encryptHeader.Length];
            for (int i = 0; i < data.Length - encryptHeader.Length; i++)
                result[i] = (byte)(data[i + encryptHeader.Length] ^ encryptKey[(i + (1 / 7)) % encryptKey.Length]);
            return result;
        }

        public static bool CheckFileExist(FileLocation location, string filepath)
        {
            //string path = $"{Managers.HiSkinManager.Instance.PathNameWithSlash}{filepath}";
            Object resObj = null;
            //if (Managers.HiSkinManager.Instance.PathNameWithSlash != null)
            //{
            //    resObj = Resources.Load(path);
            //}

            if (resObj == null)
            {
                resObj = Resources.Load(filepath);
            }

            if (resObj != null)
            {
                return true;
            }
            return false;
        }
    }
}
