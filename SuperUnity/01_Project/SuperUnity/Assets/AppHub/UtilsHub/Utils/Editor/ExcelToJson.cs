//using ExcelDataReader;
//using Games.MCGame.Data;
//using Hi.Common;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using UnityEditor;
//using UnityEngine;
//using static Hi.Hubs.HiItemHub;

//namespace SuperUnity.Utils
//{
//    public class ExcelToJson : Editor
//    {
//        class SheetInfo
//        {
//            public string SheetName;
//            public string ConfigName;
//            //public string ConfigItemTypeName;
//            public int HeaderLine;
//            //public string GenerateClassFolder;
//            public List<ColumnInfo> ColumnInfoList;
//        }
//        class ColumnInfo
//        {
//            public string FullName;
//            public string Name;
//            public string DictionaryKey;
//            public int ListIndex;
//            public string Type;
//            public string TypeCollection;
//            public int SheetColIndex;
//            public string Description;
//        }

//        private const string LOG_TAG = "ExcelToJson";

//        private const string ExcelConfigFilePath = "../../Config/config.xlsx"; //Excel 位置

//        private const string OutPutConfigErrorInfoPath = "../../Config/";  //Config 文件位置
//        private const string OutPutClassPath = "Configs/ConfigClass";      //自动生成类位置 

//        private const string FormatedConfigFilePath = "../../Config/FormatedConfigs/";
//        private const string ExportJsonConfigPath = "Configs/Resources/Configs/";
//        private const string BatchPath = "../../Config/MoveConfigFromDownloadToProject.bat";


//        private static readonly Dictionary<string, string> BaseFieldTypes = new Dictionary<string, string>
//        {
//            //支持的数据类型 key=短名 value=全名
//            { "string" ,"string"},
//            { "Prefab" ,"string"}, //动态加载资源Prefa
//            { "Image" ,"string"},  //动态加载资源Image
//            { "Audio" ,"string"},  //动态加载资源Image
//            { "int" , "int"},
//            { "long" , "long"},
//            { "bool" , "bool"},
//            { "float" , "float"},
//            { "Vector2" , "UnityEngine.Vector2"},
//            { "Vector2Int" , "UnityEngine.Vector2Int"},
//            { "Vector3" , "UnityEngine.Vector3"},
//            { "Vector3Int" , "UnityEngine.Vector3Int"},
//            { "Color" , "UnityEngine.Color"},
//        };

//        private static readonly Dictionary<string, EnumFieldType> enumFieldTypes = new Dictionary<string, EnumFieldType>
//        {
//            {"AnimationType",new EnumFieldType{FullName ="DG.Tweening.DOTweenAnimation.AnimationType", ConvertFunc=(strValue)=>ConvertStringtoEnum(strValue,DG.Tweening.DOTweenAnimation.AnimationType.None)}},
//            {"Ease",new EnumFieldType{FullName ="DG.Tweening.Ease",ConvertFunc=(strValue)=>ConvertStringtoEnum(strValue, DG.Tweening.Ease.Linear)}},
//            {"TextAlignmentOptions",new EnumFieldType{FullName ="TMPro.TextAlignmentOptions", ConvertFunc=(strValue)=>ConvertStringtoEnum(strValue, TMPro.TextAlignmentOptions.Center)}},
//            {"HiModalResult",new EnumFieldType{FullName ="Hi.Common.HiModalResult", ConvertFunc=(strValue)=>ConvertStringtoEnum<HiModalResult>(strValue, HiModalResult.Ok)}},
//            {"HiAdType",new EnumFieldType{FullName ="Hi.Common.HiAdType", ConvertFunc=(strValue)=>ConvertStringtoEnum(strValue, HiAdType.s)}},
//            {"MovePathType",new EnumFieldType{FullName ="Hi.UI.Utils.HiUIPathUtils.MovePathType",ConvertFunc=(strValue)=>ConvertStringtoEnum(strValue, Hi.UI.Utils.HiUIPathUtils.MovePathType.Line)}},
//            {"ChooseContinuousType",new EnumFieldType{FullName ="Games.MCGame.Data.ChooseContinuousType",ConvertFunc=(strValue)=>ConvertStringtoEnum(strValue, Games.MCGame.Data.ChooseContinuousType.None)}},
//            {"CardMapUpdateRuleType",new EnumFieldType{FullName ="Games.MCGame.Data.CardMapUpdateRuleType",ConvertFunc=(strValue)=>ConvertStringtoEnum(strValue, Games.MCGame.Data.CardMapUpdateRuleType.None)}},
//            {"GameItemActionType",new EnumFieldType{FullName ="Games.MCGame.Data.GameItemActionType", ConvertFunc=(strValue)=>ConvertStringtoEnum(strValue, GameItemActionType.None)}},
//            {"CardFillType",new EnumFieldType{FullName ="Games.MCGame.Data.CardFillType", onvertFunc=(strValue)=>ConvertStringtoEnum(strValue, CardFillType.None)}},
//            {"ShopItemType",new EnumFieldType{FullName ="Hi.Common.ShopItemType", ConvertFunc=(strValue)=>ConvertStringtoEnum<ShopItemType>(strValue, ShopItemType.Free)}},
//            {"HudShowState",new EnumFieldType{FullName ="Hi.Common.HudShowState",  ConvertFunc=(strValue)=>ConvertStringtoEnum<HudShowState>(strValue, HudShowState.Ignore)}},
//            {"CardSpecialStateMoveType",new EnumFieldType{FullName ="Games.MCGame.Data.CardSpecialStateMoveType",ConvertFunc=(strValue)=>ConvertStringtoEnum<CardSpecialStateMoveType>(strValue, CardSpecialStateMoveType.None)}},
//            {"ItemUseActionType",new EnumFieldType{FullName ="Hi.Hubs.HiItemHub.ItemUseActionType",ConvertFunc=(strValue)=>ConvertStringtoEnum<ItemUseActionType>(strValue, ItemUseActionType.None)}},
//            {"GameButtonType",new EnumFieldType{FullName ="Hi.Common.GameButtonType",ConvertFunc=(strValue)=>ConvertStringtoEnum<GameButtonType>(strValue, GameButtonType.None)}},
//            {"GameItemUseType",new EnumFieldType{FullName ="Hi.Common.GameItemUseType",ConvertFunc=(strValue)=>ConvertStringtoEnum<GameItemUseType>(strValue, GameItemUseType.None)}},
//        };

//        private static Dictionary<string, List<string>> ConfigKeyList;

//        private static Dictionary<string, string> SupportFieldTypes;
//        private static readonly List<string> configItemBaseFields = new List<string>() { "PrefixKey", "Key", "FilterGroup", "FilterTarget", "FilterInstallVersionLE" };

//        [MenuItem("Phoenix/ExcelConfigToJson", false, 10)]
//        public static void ExcelConfigToJson()
//        {
//            ErrorStringBuilder = null;

//            Dictionary<string, string> BaseAndEnumFieldTypes =
//            BaseFieldTypes.Union(enumFieldTypes.ToDictionary(e => e.Key, e => e.Value.FullName)).ToDictionary(e => e.Key, e => e.Value);
//            SupportFieldTypes = BaseAndEnumFieldTypes.ToDictionary(e => e.Key, e => e.Value);
//            SupportFieldTypes = SupportFieldTypes.Union(BaseAndEnumFieldTypes.ToDictionary(e => "List<" + e.Key + ">", e => "List<" + e.Value + ">")).ToDictionary(e => e.Key, e => e.Value);
//            SupportFieldTypes = SupportFieldTypes.Union(BaseAndEnumFieldTypes.ToDictionary(e => "Dictionary<string," + e.Key + ">", e => "Dictionary<string, " + e.Value + ">")).ToDictionary(e => e.Key, e => e.Value);

//            DataSet excel = LoadExcel();
//            List<SheetInfo> infoList = new List<SheetInfo>();
//            foreach (DataTable sheet in excel.Tables)
//            {
//                SheetInfo info = GetSheetInfo(sheet);
//                if (info != null)
//                {
//                    infoList.Add(info);
//                }
//            }
//            IEnumerable<IGrouping<string, SheetInfo>> groups = infoList.GroupBy(item => item.ConfigName);

//            ConfigKeyList = GetConfigKeyList(excel, groups);
//            SupportFieldTypes = SupportFieldTypes.Union(ConfigKeyList.ToDictionary(e => e.Key, e => e.Key)).ToDictionary(e => e.Key, e => e.Value);
//            SupportFieldTypes = SupportFieldTypes.Union(ConfigKeyList.ToDictionary(e => "List<" + e.Key + ">", e => "List<" + e.Key + ">")).ToDictionary(e => e.Key, e => e.Value);
//            SupportFieldTypes = SupportFieldTypes.Union(ConfigKeyList.ToDictionary(e => "Dictionary<string," + e.Key + ">", e => "Dictionary<string, " + e.Key + ">")).ToDictionary(e => e.Key, e => e.Value);
//            SupportFieldTypes = SupportFieldTypes.Union(ConfigKeyList.ToDictionary(e => $"Dictionary<{e.Key},int>", e => $"Dictionary<{e.Key},int>").Except(SupportFieldTypes)).ToDictionary(e => e.Key, e => e.Value);
//            SupportFieldTypes = SupportFieldTypes.Union(ConfigKeyList.ToDictionary(e => $"Dictionary<{e.Key},string>", e => $"Dictionary<{e.Key},string>").Except(SupportFieldTypes)).ToDictionary(e => e.Key, e => e.Value);

//            ExcelConfigToConfigItem(excel, groups);
//            ExcelConfigToData(excel, groups);

//            OutPutConfigErrorToCSV();   //整理配置错误信息

//            AssetDatabase.SaveAssets();
//            AssetDatabase.Refresh();
//        }

//        [MenuItem("Phoenix/ExcelConfigToJson(Clean)", false, 12)]
//        public static void ExcelConfigToJsonClean()
//        {
//            HiFileUtils.DeleteAllFile(HiFileUtils.FileLocation.Editor, OutPutClassPath);
//            HiFileUtils.DeleteAllFile(HiFileUtils.FileLocation.Editor, ExportJsonConfigPath);
//            HiFileUtils.DeleteAllFile(HiFileUtils.FileLocation.Editor, FormatedConfigFilePath);
//            ExcelConfigToJson();
//        }

//        [MenuItem("Phoenix/ExcelConfigToJson(Copy)", false, 10)]
//        public static void ExcelConfigCopyFromDownloads()
//        {
//            string cmd = HiFileUtils.GetPathForLocation(HiFileUtils.FileLocation.Editor, BatchPath);
//            string configfile = HiFileUtils.GetPathForLocation(HiFileUtils.FileLocation.Editor, ExcelConfigFilePath);

//            ShellHelper.ShellRequest req = ShellHelper.ProcessCommand(cmd, "");
//            req.onLog += (int logType, string log) =>
//            {
//                Debug.Log(log);
//            };
//            req.onDone += () =>
//            {
//                Debug.Log("Copy Done");
//                ExcelConfigToJson();
//                DateTime modification = File.GetLastWriteTime(configfile);
//                Debug.LogWarning(modification.ToLongDateString());
//            };
//            req.onError += () =>
//            {
//                Debug.LogError("Copy Error");
//            };
//        }

//        private static Dictionary<string, List<string>> GetConfigKeyList(DataSet excel, IEnumerable<IGrouping<string, SheetInfo>> groups)
//        {
//            Dictionary<string, List<string>> ConfigKey = new Dictionary<string, List<string>>();
//            foreach (var groupItem in groups)
//            {
//                var groupItemList = groupItem.ToList();
//                List<Dictionary<string, object>> listConfigData = new List<Dictionary<string, object>>();
//                foreach (var si in groupItemList)
//                {
//                    LoadSheetToList(excel.Tables[si.SheetName], si, listConfigData, true);
//                }

//                List<string> keyList = listConfigData.Select(item => item["Key"].ToString()).Distinct()
//                    .OrderBy(item => item).ToList();

//                keyList.Insert(0, "None");
//                keyList = keyList.Distinct().ToList();
//                ConfigKey.Add(groupItem.Key + "Key", keyList);
//            }
//            return ConfigKey;
//        }

//        private static void ExcelConfigToData(DataSet excel, IEnumerable<IGrouping<string, SheetInfo>> groups)
//        {
//            Debug.Log($"ExcelConfigToJson Begin");
//            int fileCount = 0;
//            foreach (var groupItem in groups)
//            {
//                var groupItemList = groupItem.ToList();
//                Debug.Log($"Config: {groupItem.Key} = Sheets[" + string.Join(",", groupItemList.Select(info => info.SheetName)) + "]");
//                List<Dictionary<string, object>> listConfigData = new List<Dictionary<string, object>>();
//                foreach (var sheetInfo in groupItemList)
//                {
//                    LoadSheetToList(excel.Tables[sheetInfo.SheetName], sheetInfo, listConfigData);
//                }
//                HiFileUtils.SaveJsonFile(HiFileUtils.FileLocation.Editor, ExportJsonConfigPath + groupItemList[0].ConfigName + ".bytes", listConfigData, true);
//                HiFileUtils.SaveJsonFile(HiFileUtils.FileLocation.Editor, FormatedConfigFilePath + groupItemList[0].ConfigName + ".json", listConfigData, false, true);//项目外部写一份格式化好的,对比使用
//                fileCount++;
//            }

//            Debug.Log($"ExcelConfigToJson Done. {fileCount} Files");
//        }

//        private static void ExcelConfigToConfigItem(DataSet excel, IEnumerable<IGrouping<string, SheetInfo>> groups)
//        {
//            Debug.Log($"ExcelConfigToConfigItem Begin");

//            foreach (var groupItem in groups)
//            {
//                var groupItemList = groupItem.ToList();
//                Debug.Log($"Config: {groupItem.Key} = Sheets[" + string.Join(",", groupItemList.Select(info => info.SheetName)) + "]");

//                var sheetInfo = groupItemList[0];
//                Dictionary<string, string> sheetFieldList = GetSheetFieldList(excel.Tables[sheetInfo.SheetName], sheetInfo);

//                string strClassName = sheetInfo.ConfigName;
//                string strEnumName = sheetInfo.ConfigName + "Key";
//                string strStableEnum = sheetInfo.ConfigName + "StableEnum";
//                //  string strNameSpace = string.Join(".", names.ToList().GetRange(0, names.Length - 1));


//                string strClassCode = "";

//                strClassCode += "//Auto Generated by Unity Editor ExcelToJson.cs\r\n";
//                strClassCode += "using System.Collections.Generic;\r\n";
//                strClassCode += "using System;\r\n";
//                strClassCode += "using UnityEngine;\r\n";
//                strClassCode += "namespace Configs\r\n";

//                strClassCode += "{\r\n";

//                List<Dictionary<string, object>> listConfigData = new List<Dictionary<string, object>>();
//                foreach (var si in groupItemList)
//                {
//                    LoadSheetToList(excel.Tables[si.SheetName], si, listConfigData);
//                }

//                List<string> keyList = listConfigData.Select(item => item["Key"].ToString()).Distinct().OrderBy(item => item).ToList();
//                //enum
//                strClassCode += $"    public enum {strEnumName}\r\n";
//                strClassCode += "    {\r\n";
//                strClassCode += "        None=0,\r\n";
//                keyList.ForEach(item =>
//                {
//                    if (item != "None")
//                    {
//                        strClassCode += $"        {item},\r\n";

//                    }
//                });

//                strClassCode += "    }\r\n";


//                //enum field
//                strClassCode += "    [Serializable]\r\n";
//                strClassCode += $"    public class {strStableEnum} : StableEnum< {strEnumName}>\r\n";
//                strClassCode += "    {\r\n";
//                strClassCode += $"        public static implicit operator {strStableEnum}( {strEnumName} value)\r\n";
//                strClassCode += "        {\r\n";
//                strClassCode += $"            var stableEnum = new {strStableEnum}()\r\n";
//                strClassCode += "            {\r\n";
//                strClassCode += "                _value = value,\r\n";
//                strClassCode += "                _proxy = value.ToString()\r\n";
//                strClassCode += "            };\r\n";
//                strClassCode += "            return stableEnum;\r\n";
//                strClassCode += "        }\r\n";
//                strClassCode += "    }\r\n";



//                strClassCode += $"    public class {strClassName} : ConfigBase<{strClassName}, {strEnumName}>\r\n";
//                strClassCode += "    {\r\n";
//                foreach (var e in sheetFieldList)
//                {
//                    if (configItemBaseFields.Contains(e.Key))
//                    {
//                        continue;
//                    }
//                    strClassCode += "\r\n";
//                    strClassCode += "        /// <summary>\r\n";
//                    strClassCode += "        /// " + sheetInfo.ColumnInfoList.Find(i => i.Name == e.Key).Description.Replace("\n", "").Replace("\r", "") + "\r\n";
//                    strClassCode += "        /// </summary>\r\n";
//                    strClassCode += $"        public {SupportFieldTypes[e.Value]} {e.Key};\r\n";
//                }
//                strClassCode += "    }\r\n";
//                strClassCode += "}";

//                HiFileUtils.SaveTextFile(HiFileUtils.FileLocation.Editor, HiFileUtils.CombinePath(OutPutClassPath, strClassName + ".cs"), strClassCode, false);
//            }
//            Debug.Log($"ExcelConfigToConfigItem Done. {groups.ToList().Count} Files");
//        }

//        private static DataSet LoadExcel()
//        {
//            using (FileStream stream = File.Open(HiFileUtils.CombinePath(Application.dataPath, ExcelConfigFilePath), FileMode.Open, FileAccess.Read))
//            {
//                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
//                return excelReader.AsDataSet();
//            }
//        }

//        private static SheetInfo GetSheetInfo(DataTable sheet)
//        {
//            if (sheet.Rows.Count < 1 || sheet.Columns.Count < 6)
//                return null;//表太小连基础信息都没有

//            if (int.Parse(sheet.Rows[0][1].ToString()) != 1)
//                return null;//不需要导出为配置文件

//            SheetInfo sheetInfo = new SheetInfo();
//            sheetInfo.SheetName = sheet.TableName;
//            sheetInfo.ConfigName = sheet.Rows[0][3].ToString().Trim();
//            sheetInfo.HeaderLine = int.Parse(sheet.Rows[0][7].ToString()); //行数，注意他是从1开始，不是从0


//            sheetInfo.ColumnInfoList = new List<ColumnInfo>();
//            for (int i = 1; i < sheet.Columns.Count; i++)//第一列都是注释
//            {
//                string strType = sheet.Rows[sheetInfo.HeaderLine - 2][i].ToString().Trim();
//                if (strType == "" || strType == "ignore")//header上一行，列的类型， ignore表示不导出
//                    continue;
//                ColumnInfo info = new ColumnInfo();
//                info.FullName = sheet.Rows[sheetInfo.HeaderLine - 1][i].ToString().Trim();
//                info.Name = info.FullName;
//                info.SheetColIndex = i;
//                info.Type = strType;
//                info.Description = sheet.Rows[sheetInfo.HeaderLine - 3][i].ToString().Trim();
//                if (info.FullName.Contains("."))
//                {
//                    List<String> dotNames = info.FullName.Split(new[] { '.' }, 2).ToList();
//                    if (dotNames.Count == 2)
//                    {
//                        info.TypeCollection = "Dictionary";
//                        info.Name = dotNames[0];
//                        info.DictionaryKey = dotNames[1];
//                    }
//                    else
//                    {
//                        Debug.unityLogger.LogError(LOG_TAG,
//                            $"FiledName  is not supported! Sheet[ {sheet.TableName}]  ColumnIndex[{info.SheetColIndex}] CloumnName[{info.FullName}] fieldType[{info.Type}]");
//                    }
//                }
//                else if (info.FullName.Contains("["))
//                {
//                    List<String> bracketNames = info.FullName.Replace("]", "").Split(new[] { '[' }, 2).ToList();
//                    if (bracketNames.Count == 2)
//                    {
//                        info.TypeCollection = "List";
//                        info.Name = bracketNames[0];
//                        info.ListIndex = Int32.Parse(bracketNames[1]);
//                    }
//                    else
//                    {
//                        Debug.unityLogger.LogError(LOG_TAG,
//                            $"FiledName is not supported! Sheet[ {sheet.TableName}]  ColumnIndex[{info.SheetColIndex}] CloumnName[{info.FullName}] fieldType[{info.Type}]");
//                    }
//                }
//                sheetInfo.ColumnInfoList.Add(info);
//            }
//            return sheetInfo;
//        }

//        private static Dictionary<string, string> GetSheetFieldList(DataTable sheet, SheetInfo sheetInfo)
//        {
//            Dictionary<string, string> fieldList = new Dictionary<string, string>();
//            var columnGroupList = sheetInfo.ColumnInfoList.GroupBy(item => item.Name).ToDictionary(grp => grp.Key, grp => grp.ToList());

//            //检查所有类型
//            foreach (var info in sheetInfo.ColumnInfoList)
//            {
//                if (!SupportFieldTypes.ContainsKey(info.Type))
//                {
//                    Debug.unityLogger.LogError(LOG_TAG,
//                        $"FiledType in Sheet is not supported! Sheet[ {sheet.TableName}]  ColumnIndex[{info.SheetColIndex}] CloumnName[{info.FullName}] fieldType[{info.Type}]");
//                }
//            }

//            foreach (var e in columnGroupList)
//            {
//                var info = e.Value[0];
//                if (e.Value.Count == 1)
//                {
//                    if (info.TypeCollection == null)
//                    {//非集合类型处理
//                        fieldList.Add(info.Name, info.Type);
//                        continue;
//                    }
//                }
//                //集合类型处理
//                if (e.Value.Select(item => item.Type).Distinct().Count() > 1)
//                {//集合类型的子类型多于一种

//                    Debug.unityLogger.LogError(LOG_TAG,
//                        $"Sub FiledType in One Name is not Same! Sheet[ {sheet.TableName}]  Collection  Field[{e.Key}] " + string.Join(", ", e.Value.Select(item => item.Type)));
//                }

//                if (info.TypeCollection == "List")
//                {

//                    fieldList.Add(info.Name, $"List<{info.Type}>");
//                }
//                else //"Dictionary"
//                {
//                    fieldList.Add(info.Name, $"Dictionary<string,{info.Type}>");
//                }


//            }
//            return fieldList;
//        }

//        private static string DecodeString(string value)
//        {// 采用了urlencode 
//            if (value == null)
//                return null;
//            string str = value;
//            str = str.Trim();
//            str = Regex.Unescape(str);
//            return str;
//        }

//        public static object ConvertRawStringtoType(string value, string strType, bool defaultValueReturnNull = true, string sheetName = null)
//        {
//            if (value == null)
//                return null;
//            Regex regDictionary = new Regex("^Dictionary<(.*),(.*)>$", RegexOptions.IgnoreCase);
//            Match matchDictionary = regDictionary.Match(strType);
//            if (matchDictionary.Success)
//            {//字典类
//                string strKeyType = matchDictionary.Groups[1].Value;
//                string strSubType = matchDictionary.Groups[2].Value;
//                if (string.IsNullOrEmpty(value))
//                    return null;
//                if (value.Trim() == "")
//                    return null;
//                Dictionary<string, string> dictRaw = value.Split(';').Select(x => x.Split(new char[] { ':' }, 2))
//                    .ToDictionary(l => l[0], l => l.Length > 1 ? l[1] : null);
//                Dictionary<string, object> dict = dictRaw.ToDictionary(e => e.Key, e => ConvertRawStringtoType(e.Value, strSubType)).Where(e => e.Value != null).ToDictionary(e => e.Key, e => e.Value);

//                if (dict.Count == 0)
//                    return null;
//                return dict;
//            }
//            Regex regList = new Regex("^List<(.*)>$", RegexOptions.IgnoreCase);
//            Match matchList = regList.Match(strType);
//            if (matchList.Success)
//            //      if (strType.StartsWith("List<"))
//            {//列表类
//                string strSubType = matchList.Groups[1].Value;
//                if (string.IsNullOrEmpty(value))
//                    return null;
//                if (value.Trim() == "")
//                    return null;
//                List<string> listRaw = value.Split(';').ToList();
//                List<object> list = listRaw.Select(item => ConvertRawStringtoType(item, strSubType)).Where(item => item != null).ToList();
//                if (list.Count == 0)
//                    return null;
//                return list;
//            }

//            string strValue = DecodeString(value);
//            switch (strType)
//            {
//                //对于基础类型类型，如果是0那么就不会加入json ,节约默认值
//                case "string":
//                    {
//                        if (strValue == "" && defaultValueReturnNull)
//                        {
//                            return null;
//                        }
//                        return strValue;
//                    }
//                case "Prefab":
//                    {
//                        if (strValue == "" && defaultValueReturnNull)
//                        {
//                            return null;
//                        }
//                        if (!HiFileUtils.CheckFileExist(HiFileUtils.FileLocation.Resource, strValue))
//                        {
//                            Debug.unityLogger.LogWarning(LOG_TAG, $"【Prefab】  There's nothing under the '{strValue}' ");
//                        }
//                        return strValue;
//                    }
//                case "Image":
//                    {
//                        if (strValue == "" && defaultValueReturnNull)
//                        {
//                            return null;
//                        }

//                        if (!HiFileUtils.CheckFileExist(HiFileUtils.FileLocation.Resource, strValue))
//                        {
//                            Debug.unityLogger.LogWarning(LOG_TAG, $" 【Image】  There's nothing under the '{strValue}' ");
//                        }

//                        return strValue;
//                    }
//                case "Audio":
//                    {
//                        if (strValue == "" && defaultValueReturnNull)
//                        {
//                            return null;
//                        }
//                        if (!HiFileUtils.CheckFileExist(HiFileUtils.FileLocation.Resource, $"Audio/{strValue}"))
//                        {
//                            Debug.unityLogger.LogWarning(LOG_TAG, $" 【Audio】  There's nothing under the '{strValue}' ");
//                        }
//                        return strValue;
//                    }

//                case "int":
//                    {
//                        int newValue = TypeParseUtils.SafeIntParse(strValue);
//                        if (newValue == 0 && defaultValueReturnNull)
//                        {
//                            return null;
//                        }
//                        return newValue;
//                    }
//                case "long":
//                    {
//                        long newValue = TypeParseUtils.SafeLongParse(strValue);
//                        if (newValue == 0 && defaultValueReturnNull)
//                        {
//                            return null;
//                        }
//                        return newValue;
//                    }
//                case "bool":
//                    {
//                        bool newValue = TypeParseUtils.SafeBoolParse(strValue);
//                        if (newValue == false && defaultValueReturnNull)
//                        {
//                            return null;
//                        }
//                        return newValue;
//                    }
//                case "float":
//                    {
//                        float newValue = TypeParseUtils.SafeFloatParse(strValue);
//                        if (newValue == 0.0f && defaultValueReturnNull)
//                        {
//                            return null;
//                        }
//                        return newValue;
//                    }
//                case "Vector2":
//                    {
//                        Vector2 newValue = TypeParseUtils.SafeVectorParse(strValue);
//                        if (newValue == Vector2.zero && defaultValueReturnNull)
//                        {
//                            return null;
//                        }
//                        return newValue;
//                    }
//                case "Vector2Int":
//                    {
//                        Vector2Int newValue = (Vector2Int)TypeParseUtils.SafeVectorIntParse(strValue);
//                        if (newValue == Vector2Int.zero && defaultValueReturnNull)
//                        {
//                            return null;
//                        }
//                        return newValue;
//                    }
//                case "Vector3":
//                    {
//                        Vector3 newValue = TypeParseUtils.SafeVectorParse(strValue);
//                        if (newValue == Vector3.zero && defaultValueReturnNull)
//                        {
//                            return null;
//                        }
//                        return newValue;
//                    }
//                case "Vector3Int":
//                    {
//                        Vector3Int newValue = TypeParseUtils.SafeVectorIntParse(strValue);
//                        if (newValue == Vector3Int.zero && defaultValueReturnNull)
//                        {
//                            return null;
//                        }
//                        return newValue;
//                    }
//                //对于扩展类型，不会节约默认值
//                case "Color":
//                    {
//                        UnityEngine.Color color = TypeParseUtils.SafeColorParse(strValue);
//                        return new Dictionary<string, float> { { "r", color.r }, { "g", color.g }, { "b", color.b }, { "a", color.a }, };
//                    }
//                default:
//                    break;
//            }

//            if (enumFieldTypes.ContainsKey(strType))
//            {
//                return enumFieldTypes[strType].ConvertFunc(strValue);
//            }

//            if (ConfigKeyList.ContainsKey(strType))
//            {
//                if (strValue == "")
//                {
//                    return 0;
//                }
//                if (!ConfigKeyList[strType].Contains(strValue))
//                {
//                    //   throw new Exception($"Value {strValue}  is in Enum {strType}");
//                    Debug.unityLogger.LogError(LOG_TAG, $"Value {strValue}  is Not in Enum {strType}");
//                    //记录配置错误信息
//                    ErrorStringBuilder.Append($"{strType},{strValue},{sheetName}\n");
//                    return 0;
//                }

//                return ConfigKeyList[strType].IndexOf(strValue);
//            }

//            Debug.unityLogger.LogError(LOG_TAG, $"Value {strValue}  Type {strType} ia Unknown");

//            return strValue;
//        }

//        private static TEnum SafeEnumParse<TEnum>(string s, TEnum defaultValue) where TEnum : struct
//        {
//            TEnum value;
//            if (!Enum.TryParse(s, out value))
//            {
//                if (s != "")
//                {
//                    Debug.unityLogger.LogError(LOG_TAG, $"The Value[{s}] is not in {typeof(TEnum).FullName}");
//                }
//            }
//            return value;
//        }

//        private static void LoadSheetToList(DataTable sheet, SheetInfo sheetInfo, List<Dictionary<string, object>> list, bool bOnlyKey = false)
//        {
//            Debug.Log("Load Sheet" + sheet.TableName + "... ");
//            Debug.Log("configName : " + sheetInfo.ConfigName + "\t\theaderLine : " + sheetInfo.HeaderLine.ToString());

//            for (int rowIndex = sheetInfo.HeaderLine; rowIndex < sheet.Rows.Count; rowIndex++)
//            {
//                DataRow row = sheet.Rows[rowIndex];
//                Dictionary<string, object> item = new Dictionary<string, object>();
//                foreach (var info in sheetInfo.ColumnInfoList)
//                {
//                    String strValue = row[info.SheetColIndex].ToString();
//                    //强制替换0宽字符
//                    strValue = strValue.Replace("\u200B", "");
//                    if (bOnlyKey && info.Name != "Key")
//                    {
//                        continue;
//                    }

//                    if (info.Type == "string")
//                    {
//                        if (!CheckStringLanguage(ConvertRawStringtoType(strValue, info.Type) as string))
//                        {
//                            Debug.unityLogger.LogError(LOG_TAG,
//                                string.Format("Contains chinese string for {0}  ,with sheet:{1} row:{2} col:{3}", strValue,
//                                    sheet.TableName, rowIndex, info.SheetColIndex));
//                        }
//                    }

//                    if (info.TypeCollection == null)
//                    {
//                        object obj = ConvertRawStringtoType(strValue, info.Type, sheetName: sheetInfo.SheetName);
//                        if (obj != null) //不在json 里放root层级的null, 减少文件大小
//                        {
//                            item[info.Name] = obj;
//                        }

//                        continue;
//                    }

//                    if (info.TypeCollection == "Dictionary")
//                    {
//                        if (strValue.Trim() != "")
//                        {
//                            if (info.Type == "string")
//                            {
//                                if (ErrorSplitCharForListAndDictionary(
//                                    ConvertRawStringtoType(strValue, info.Type) as string))
//                                {
//                                    Debug.unityLogger.LogError(LOG_TAG,
//                                        string.Format("Error split char for {0}  ,with sheet:{1} row:{2} col:{3}", strValue,
//                                            sheet.TableName, rowIndex, info.SheetColIndex));
//                                }
//                            }

//                            if (!item.ContainsKey(info.Name))
//                            {
//                                item[info.Name] = new Dictionary<string, object>();
//                            }
//                            ((Dictionary<string, object>)item[info.Name]).Add(info.DictionaryKey, ConvertRawStringtoType(strValue, info.Type, false));
//                        }
//                    }

//                    if (info.TypeCollection == "List")
//                    {
//                        if (info.Type == "string")
//                        {
//                            if (ErrorSplitCharForListAndDictionary(ConvertRawStringtoType(strValue, info.Type) as string))
//                            {
//                                Debug.unityLogger.LogError(LOG_TAG,
//                                    string.Format("Error split char for {0}  ,with sheet:{1} row:{2} col:{3}", strValue,
//                                        sheet.TableName, rowIndex, info.SheetColIndex));
//                            }
//                        }

//                        if (!item.ContainsKey(info.Name))
//                        {
//                            item[info.Name] = new List<object>();
//                        }
//                        ((List<object>)item[info.Name]).Add(ConvertRawStringtoType(strValue, info.Type));
//                    }
//                }

//                if (item.ContainsKey("Key") && !string.IsNullOrEmpty(item["Key"].ToString()))
//                {//  此行key必须不是null 或者 ""
//                    list.Add(item);
//                }
//            }
//        }

//        private static bool CheckStringLanguage(string strValue)
//        {
//            if (string.IsNullOrEmpty(strValue))
//            {
//                return true;
//            }
//            for (int i = 0; i < strValue.Length; i++)
//            {
//                if ((short)strValue[i] > 127)
//                {
//                    Debug.LogError(string.Format("chinese string: {0}  asciiCode:{1}      index: {2}     lastCharIs: {3}    nextCharIs:{4}",
//                        strValue[i],
//                        (short)strValue[i],
//                        i,
//                        i > 0 ? strValue[i - 1].ToString() : "",
//                        i < strValue.Length - 1 ? strValue[i + 1].ToString() : "")
//                        );
//                    return false;
//                }
//            }
//            return true;
//        }

//        private static bool ErrorSplitCharForListAndDictionary(string strValue)
//        {
//            if (string.IsNullOrEmpty(strValue))
//            {
//                return false;
//            }
//            if (strValue.Contains("：") || strValue.Contains("；"))
//            {
//                return true;
//            }
//            return false;
//        }

//        class EnumFieldType
//        {
//            public string FullName;
//            public Func<string, object> ConvertFunc;
//        }

//        private static object ConvertStringtoEnum<TEnum>(string value, TEnum defaultValue) where TEnum : struct
//        {
//            return TypeParseUtils.SafeEnumParse<TEnum>(value, defaultValue);
//        }

//        static StringBuilder errorStringBuilder;
//        private static StringBuilder ErrorStringBuilder
//        {
//            get
//            {
//                if (errorStringBuilder == null)
//                {
//                    errorStringBuilder = new StringBuilder();
//                    errorStringBuilder.AppendLine("TargetSheet,Key,ReferenceSheet");
//                }
//                return errorStringBuilder;
//            }
//            set
//            {
//                errorStringBuilder = value;
//            }
//        }

//        private static void OutPutConfigErrorToCSV()
//        {
//            HiFileUtils.SaveTextFile(HiFileUtils.FileLocation.Editor, HiFileUtils.CombinePath(OutPutConfigErrorInfoPath, "configError.csv"), ErrorStringBuilder.ToString(), false);
//        }
//    }
//}
