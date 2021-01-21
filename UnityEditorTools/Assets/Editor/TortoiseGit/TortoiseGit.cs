using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;

public enum GitType
{
    Log,
    Pull,
    Commit,
    Push,
    StashSave,
    StashPop
}

public static class TortoiseGit
{
    private const string quota = "\"";

    public const string COMMAND_TORTOISE_LOG = @"/command:log /path:{0} /findtype:0 /closeonend:0";
    public const string COMMAND_TORTOISE_PULL = @"/command:pull /path:{0} /closeonend:0";
    public const string COMMAND_TORTOISE_COMMIT = @"/command:commit /path:{0} /closeonend:0";
    public const string COMMAND_TORTOISE_PUSH = @"/command:push /path:{0} /closeonend:0";
    public const string COMMAND_TORTOISE_STASHSAVE = @"/command:stashsave /path:{0} /closeonend:0";
    public const string COMMAND_TORTOISE_STASHPOP = @"/command:stashpop /path:{0} /closeonend:0";

    public static Process CreateProcess(string path, string arguments)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo(path, arguments)
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = false,
            ErrorDialog = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            LoadUserProfile = true
        };
        return new Process {StartInfo = startInfo};
    }

    public static void GitCommand(GitType gitType, string path, string tortoiseGitPath)
    {
        if (!File.Exists(tortoiseGitPath))
        {
            Debug.LogError("TortoiseGitPath can't find.");
            return;
        }

        switch (gitType)
        {
            case GitType.Log:
                GitLog(path, tortoiseGitPath);
                break;
            case GitType.Commit:
                GitCommmit(path, tortoiseGitPath);
                break;
            case GitType.Pull:
                GitPull(path, tortoiseGitPath);
                break;
            case GitType.Push:
                GitPush(path, tortoiseGitPath);
                break;
            case GitType.StashSave:
                GitStashSave(path, tortoiseGitPath);
                break;
            case GitType.StashPop:
                GitStashPop(path, tortoiseGitPath);
                break;
        }
    }

    private static void GitPush(string path, string tortoiseGitPath)
    {
        var args = quota + path + quota;
        args = string.Format(COMMAND_TORTOISE_PUSH, args);
        Process process = CreateProcess(tortoiseGitPath, args);
        process.Start();
    }

    public static void GitStashPop(string path, string tortoiseGitPath)
    {
        var args = quota + path + quota;
        args = string.Format(COMMAND_TORTOISE_STASHPOP, args);
        Process process = CreateProcess(tortoiseGitPath, args);
        process.Start();
    }

    public static void GitStashSave(string path, string tortoiseGitPath)
    {
        var args = quota + path + quota;
        args = string.Format(COMMAND_TORTOISE_STASHSAVE, args);
        Process process = CreateProcess(tortoiseGitPath, args);
        process.Start();
    }

    public static void GitLog(string path, string tortoiseGitPath)
    {
        var args = quota + path + quota;
        args = string.Format(COMMAND_TORTOISE_LOG, args);
        Process process = CreateProcess(tortoiseGitPath, args);
        process.Start();
    }

    public static void GitPull(string path, string tortoiseGitPath)
    {
        var args = quota + path + quota;
        args = string.Format(COMMAND_TORTOISE_PULL, args);
        Process process = CreateProcess(tortoiseGitPath, args);
        process.Start();
    }

    public static void GitCommmit(string path, string tortoiseGitPath)
    {
        var args = quota + path + quota;
        args = string.Format(COMMAND_TORTOISE_COMMIT, args);
        Process process = CreateProcess(tortoiseGitPath, args);
        process.Start();
    }
}