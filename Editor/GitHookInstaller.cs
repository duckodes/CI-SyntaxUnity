using Debug = UnityEngine.Debug;
using UnityEditor;
using System.Diagnostics;
using System.IO;

[InitializeOnLoad]
public class GitHookInstaller
{
    static GitHookInstaller()
    {
        InstallToolsAndGitHookOnce();
    }

    static void InstallToolsAndGitHookOnce()
    {
        string projectRoot = Directory.GetCurrentDirectory();
        string repoRoot = Directory.GetParent(projectRoot).FullName;

        string gitHookPath = Path.Combine(repoRoot, ".git", "hooks");
        string flagFile = Path.Combine(gitHookPath, "pre-push.githookinstalled");
        string installScript = Path.Combine(repoRoot, ".githooks", "install-hooks.sh");

        if (File.Exists(flagFile))
        {
            return;
        }

        // run dotnet new tool-manifest
        if (!RunShellCommand("dotnet", "new tool-manifest", projectRoot)) return;
        // run dotnet tool install dotnet-format
        if (!RunShellCommand("dotnet", "tool install dotnet-format", projectRoot)) return;

        if (!File.Exists(installScript))
        {
            Debug.LogError("GitHookInstaller: 找不到 install-hooks.sh，請確認路徑是否正確");
            return;
        }

        if (!RunShellCommand("cmd.exe", $"/c \"{installScript}\"", repoRoot)) return;

        File.WriteAllText(flagFile, "Git hooks installed");
        Debug.Log("GitHookInstaller: 安裝完成");
    }

    static bool RunShellCommand(string fileName, string arguments, string workingDirectory)
    {
        try
        {
            var process = new Process();
            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.WorkingDirectory = workingDirectory;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Debug.LogError($"GitHookInstaller: 指令失敗：{fileName} {arguments}");
                return false;
            }

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"GitHookInstaller: 執行指令失敗：{ex.Message}");
            return false;
        }
    }
}