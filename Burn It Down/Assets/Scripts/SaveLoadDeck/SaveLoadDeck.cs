using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

public static class SaveLoadDeck
{
    public static void Write(SaveData data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = $"{Application.persistentDataPath}/SaveFile.es3";
        FileStream stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static SaveData Read()
    {
        string path = $"{Application.persistentDataPath}/SaveFile.es3";

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            SaveData data = formatter.Deserialize(stream) as SaveData;
            stream.Close();
            return data;
        }
        else
        {
            Debug.Log("either no save file, or save file failed to load");
            return null;
        }
    }

    public static void DeleteSaveData()
    {
        string path = $"{Application.persistentDataPath}/SaveFile.es3";
        File.Delete(path);
    }
}