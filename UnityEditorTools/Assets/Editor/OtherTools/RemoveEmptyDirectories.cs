using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;

public class RemoveEmptyDirectories
{
    [MenuItem("Tools/RemoveEmptyDir")]
    public static void RemoveEmptyDir()
    {
        DirectoryInfo di = new DirectoryInfo("Assets/");
        List<DirectoryInfo> dis = new List<DirectoryInfo>();

        DoRemoveEmptyDirectory(di, dis);

        if (dis.Count == 0)
        {
            EditorUtility.DisplayDialog("delete", "No empty directory", "OK");
            return;
        }

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < dis.Count; ++i)
        {
            sb.AppendLine(dis[i].FullName);
        }

        if (EditorUtility.DisplayDialog("delete", sb.ToString(), "OK", "Cancel"))
        {
            foreach (DirectoryInfo target in dis)
            {
                if (File.Exists(target.FullName + ".meta"))
                {
                    File.Delete(target.FullName + ".meta");
                }

                target.Delete(true);
            }

            AssetDatabase.Refresh();
        }
    }

    private static bool DoRemoveEmptyDirectory(DirectoryInfo target, List<DirectoryInfo> dis)
    {
        bool hasDirOrFile = false;
        foreach (DirectoryInfo di in target.GetDirectories())
        {
            bool result = DoRemoveEmptyDirectory(di, dis);
            if (result)
            {
                hasDirOrFile = true;
            }
        }

        if (!hasDirOrFile)
        {
            foreach (FileInfo fi in target.GetFiles())
            {
                if (!fi.Name.StartsWith(".") && !fi.FullName.EndsWith(".meta"))
                {
                    hasDirOrFile = true;
                    break;
                }
            }
        }

        if (!hasDirOrFile)
        {
            if (!dis.Contains(target))
            {
                dis.Add(target);
            }
        }

        return hasDirOrFile;
    }
}