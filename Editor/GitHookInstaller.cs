using Debug = UnityEngine.Debug;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System;

[InitializeOnLoad]
public class GitHookInstaller
{
    static GitHookInstaller()
    {
        InstallToolsAndGitHookIfNeeded();
    }

    static void InstallToolsAndGitHookIfNeeded()
    {
        string projectRoot = Directory.GetCurrentDirectory();
        string repoRoot = Directory.GetParent(projectRoot).FullName;

        string gitHookPath = Path.Combine(repoRoot, ".git", "hooks");
        string flagFile = Path.Combine(gitHookPath, "pre-push.githookinstalled");
        string installScript = Path.Combine(repoRoot, ".githooks", "install-hooks.sh");
        string sourceHook = Path.Combine(repoRoot, ".githooks", "pre-push");
        string toolManifestPath = Path.Combine(projectRoot, ".config", "dotnet-tools.json");

        if (!File.Exists(sourceHook))
        {
            Debug.LogError("GitHookInstaller: 找不到 pre-push hook，請確認路徑是否正確");
            return;
        }

        string currentHash = ComputeFileHash(sourceHook);
        string savedHash = File.Exists(flagFile) ? File.ReadAllText(flagFile).Trim() : "";

        if (currentHash == savedHash)
        {
            return;
        }

        if (!File.Exists(toolManifestPath))
        {
            if (!RunShellCommand("dotnet", "new tool-manifest", projectRoot)) return;
        }
        if (!RunShellCommand("dotnet", "tool install dotnet-format", projectRoot)) return;

        if (!File.Exists(installScript))
        {
            Debug.LogError("GitHookInstaller: 找不到 install-hooks.sh，請確認路徑是否正確");
            return;
        }

        if (!RunShellCommand("cmd.exe", $"/c \"{installScript}\"", repoRoot)) return;

        File.WriteAllText(flagFile, currentHash);
        Debug.Log("GitHookInstaller: Git hook 已更新並安裝完成");
    }

    static string ComputeFileHash(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        byte[] hashBytes = sha256.ComputeHash(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
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
        catch (Exception ex)
        {
            Debug.LogError($"GitHookInstaller: 執行指令失敗：{ex.Message}");
            return false;
        }
    }
}