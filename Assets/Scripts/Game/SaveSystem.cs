using System;
using System.IO;
using UnityEngine;

// Reads and writes the save file. A "static class" is just a group of helper functions with
// no object to create, like a module of exported functions in JS.
// The file lives in Application.persistentDataPath, a per-user folder Unity provides on every
// platform (on Windows: C:\Users\<you>\AppData\LocalLow\<company>\<game>).
public static class SaveSystem
{
    private const string FileName = "save.json";

    public static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    public static void Save(SaveData data)
    {
        // "true" = pretty-print, so the file is easy to read if you open it.
        string json = JsonUtility.ToJson(data, true);
        try
        {
            File.WriteAllText(SavePath, json);
        }
        catch (Exception e)
        {
            // Disk full, no permission, etc. Don't crash the game over a failed save.
            Debug.LogError($"Couldn't save the game to {SavePath}: {e.Message}");
        }
    }

    // Returns null if there's no save yet (first time playing) or the file can't be read.
    public static SaveData Load()
    {
        if (!File.Exists(SavePath))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Couldn't read the save file, starting fresh: {e.Message}");
            return null;
        }
    }

    public static void Delete()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }
    }
}
