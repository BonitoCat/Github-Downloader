using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FileLib;
using Github_Downloader_lib.Models;
using Github_Downloader.Enums;
using LoggerLib;

namespace Github_Downloader_lib;

public static class FileManager
{
    public static readonly string AppdataPath = Path.Join(DirectoryHelper.GetAppDataDirPath(), "github-downloader");
    public static readonly string ReposConfigFilePath = Path.Join(AppdataPath, "repos.json");
    public static readonly string CachePath = Path.Join(DirectoryHelper.GetCacheDirPath(), "github-downloader");
    public static readonly string AppImagesPath = Path.Join(DirectoryHelper.GetAppDataDirPath(), "github-downloader", "app-images");
    
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("12345678901234567890123456789012");
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("1234567890123456");
    
    public static void SaveRepos()
    {
        Logger.LogI("Saving repos");
        if (!File.Exists(ReposConfigFilePath))
        {
            FileHelper.Create(ReposConfigFilePath);
        }

        string jsonString = JsonSerializer.Serialize(UpdateManager.Repos, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        
        File.WriteAllText(ReposConfigFilePath, jsonString);
    }

    public static async Task LoadRepos(Action<string> statusText)
    {
        DirectoryHelper.CreateDir(AppdataPath);
        DirectoryHelper.CreateDir(CachePath);
        DirectoryHelper.CreateDir(AppImagesPath);
        
        if (File.Exists(ReposConfigFilePath))
        {
            string jsonString = File.ReadAllText(ReposConfigFilePath);
            UpdateManager.Repos = JsonSerializer.Deserialize<List<Repo>>(jsonString);
        }

        UpdateManager.Repos ??= [];

        if (UpdateManager.CurPlatform != Platform.Avalonia)
        {
            return;
        }
        
        await UpdateManager.UpdateRepoDetails(UpdateManager.Repos);
        SaveRepos();
    }

    public static void ExportRepoConfig(string destPath)
    {
        if (!File.Exists(ReposConfigFilePath))
        {
            Logger.LogI($"Folder {destPath} not found");
            return;
        }
        
        File.Copy(ReposConfigFilePath, Path.Join(destPath, "repos.json"), true);
    }

    public static void ImportRepoConfig(string sourceFile)
    {
        if (!File.Exists(sourceFile))
        {
            Logger.LogI($"File {sourceFile} not found");
            return;
        }
        
        File.Copy(sourceFile, Path.Join(AppdataPath, "repos.json"), true);
        LoadRepos(Console.WriteLine);

        foreach (Repo repo in UpdateManager.Repos)
        {
            repo.CurrentInstallTag = "";
        }
    }
}