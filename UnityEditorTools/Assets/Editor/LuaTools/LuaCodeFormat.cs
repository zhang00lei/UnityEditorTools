using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class LuaCodeFormat
{
    private static string searchFilePath => Path.Combine(Application.dataPath, "LuaScripts");

    /// <summary>
    /// 需要互转的lua目录
    /// </summary>
    private static List<string> luaDirList = new List<string>
    {
        "UI/UIGame"
    };

    private static List<string> GetLuaFileList()
    {
        List<string> result = new List<string>();
        foreach (string dirName in luaDirList)
        {
            string path = Path.Combine(searchFilePath, dirName);
            string[] filePath = Directory.GetFiles(path, "*.lua", SearchOption.AllDirectories);
            foreach (string filePathInfo in filePath) result.Add(filePathInfo.Replace('\\', '/'));
        }

        return result;
    }

    private static string GetPackageName(string[] fileContent)
    {
        for (int i = fileContent.Length - 1; i >= 0; i--)
        {
            if (fileContent[i].Contains("{") || fileContent[i].Contains("(")) continue;

            if (fileContent[i].StartsWith("return")) return fileContent[i].Replace("return", string.Empty).Trim();
        }

        return string.Empty;
    }

    /// <summary>
    /// 点方法转冒号方法
    /// </summary>
    [MenuItem("Tools/LuaTools/To:Func")]
    private static void ToColonFunc()
    {
        StringBuilder sb = new StringBuilder();
        List<string> luaFileList = GetLuaFileList();
        foreach (string filePath in luaFileList)
        {
            sb.Clear();
            List<string> luaFuncList = new List<string>();
            string[] fileContent = File.ReadAllLines(filePath);
            string packageName = GetPackageName(fileContent);

            foreach (string luaLine in fileContent)
                if (luaLine.StartsWith("local function "))
                {
                    string luaInfo = luaLine.Trim();
                    string methodName = luaInfo.Replace("local function ", string.Empty).Split('(')[0];
                    string param = luaInfo.Split('(')[1];
                    if (param.Contains("self"))
                    {
                        param = param.Replace("self,", string.Empty)
                            .Replace("self ,", string.Empty)
                            .Replace("self", string.Empty)
                            .Replace(")", string.Empty).Trim();
                        sb.AppendLine($"function {packageName}:{methodName}({param})");
                        luaFuncList.Add($"{packageName}.{methodName} = {methodName}");
                    }
                    else
                    {
                        sb.AppendLine(luaInfo);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(luaLine) || !luaFuncList.Contains(luaLine)) sb.AppendLine(luaLine);
                }

            File.WriteAllText(filePath, sb.ToString());
        }

        AssetDatabase.Refresh();
    }


    /// <summary>
    /// 冒号方法转点方法
    /// </summary>
    [MenuItem("Tools/LuaTools/To.Func")]
    private static void ToDotFunc()
    {
        StringBuilder sb = new StringBuilder();
        List<string> luaFileList = GetLuaFileList();
        foreach (string filePath in luaFileList)
        {
            sb.Clear();
            List<string> luaFuncList = new List<string>();
            string[] fileContent = File.ReadAllLines(filePath);
            string packageName = GetPackageName(fileContent);
            foreach (string luaLine in fileContent)
                if (luaLine.StartsWith($"function {packageName}:"))
                {
                    string luaInfo = luaLine.Trim();
                    string methodName = luaInfo.Replace($"function {packageName}:", string.Empty).Split('(')[0];
                    string param = luaInfo.Split('(')[1];
                    param = param
                        .Replace(")", string.Empty).Trim();
                    param = (string.IsNullOrEmpty(param) ? "self" : "self, ") + param;
                    sb.AppendLine($"local function {methodName}({param})");
                    luaFuncList.Add($"{packageName}.{methodName} = {methodName}");
                }
                else
                {
                    if (string.IsNullOrEmpty(luaLine) || !luaFuncList.Contains(luaLine)) sb.AppendLine(luaLine);
                }

            foreach (string funInfo in luaFuncList) sb.AppendLine(funInfo);

            string content = sb.ToString();

            if (!string.IsNullOrEmpty(packageName))
            {
                string returnStr = $"return {packageName}";
                content = content.Replace("\n" + returnStr, string.Empty);
                content += $"{returnStr}";
            }

            File.WriteAllText(filePath, content);
        }

        AssetDatabase.Refresh();
    }
}