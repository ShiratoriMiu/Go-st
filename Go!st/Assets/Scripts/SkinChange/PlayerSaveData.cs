using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class EquippedSkinData
{
    public string skinId;
    public float[] color; // RGBA

    public EquippedSkinData(string skinId, Color c)
    {
        this.skinId = skinId;
        this.color = new float[] { c.r, c.g, c.b, c.a };
    }

    public Color ToColor()
    {
        if (color != null && color.Length == 4)
            return new Color(color[0], color[1], color[2], color[3]);
        return Color.white;
    }
}

[System.Serializable]
public class MaterialData
{
    public string textureName; // �e�N�X�`����
    public float[] color;      // RGBA

    public MaterialData(string texName, Color c)
    {
        textureName = texName;
        color = new float[] { c.r, c.g, c.b, c.a };
    }

    public Color ToColor()
    {
        if (color != null && color.Length == 4)
            return new Color(color[0], color[1], color[2], color[3]);
        return Color.white;
    }
}


/// <summary>
/// �Z�[�u����f�[�^
/// </summary>
[System.Serializable]
public class PlayerSaveData
{
    public List<string> ownedSkins = new List<string>();//�����A�C�e��
    public List<EquippedSkinData> equippedSkins = new List<EquippedSkinData>();//�����A�C�e��
    public List<MaterialData> materials = new List<MaterialData>();//�}�e���A��
    public List<string> equippedMakes = new List<string>();//�������C�N
    // �R�C��
    public int coin = 0;

    public List<string> allItemNames = new List<string>();//�S�Ă̒����ւ��A�C�e���̖��O
}

/// <summary>
/// JSON�ۑ��E�ǂݍ��݃}�l�[�W���[
/// </summary>
public static class SaveManager
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "playerSave.json");

    public static void Save(PlayerSaveData data)
    {
        // �ۑ���f�B���N�g�����m�F���āA�Ȃ���΍쐬
        string directory = Path.GetDirectoryName(SavePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"�f�[�^�ۑ�����: {SavePath}");
    }


    public static PlayerSaveData Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("�Z�[�u�f�[�^�����݂��܂���B�V�K�쐬���܂��B");
            return new PlayerSaveData();
        }

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<PlayerSaveData>(json);
    }

    public static void Delete()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("�Z�[�u�f�[�^���폜���܂����B");
        }
    }

    //�R�C�������ۑ�
    public static void SaveCoin(int coin)
    {
        PlayerSaveData data = Load();   // �����f�[�^��ǂݍ���
        data.coin = coin;              // �R�C�������X�V
        Save(data);                    // �㏑���ۑ�
    }

    //�R�C�������擾
    public static int LoadCoin()
    {
        PlayerSaveData data = Load();
        if (data == null)   // ����N���ȂǂŃf�[�^�����݂��Ȃ��ꍇ
        {
            return 0;
        }
        return data.coin;
    }

    //�A�C�e�����ۑ�
    public static void SaveAllItemName(string name)
    {
        PlayerSaveData data = Load();   // �����f�[�^��ǂݍ���

        if (data.allItemNames == null)
        {
            data.allItemNames = new List<string>();
        }

        // �d���`�F�b�N
        if (!data.allItemNames.Contains(name))
        {
            data.allItemNames.Add(name);
        }

        Save(data); // �㏑���ۑ�
    }

    //�A�C�e�����擾
    public static List<string> AllItemNames()
    {
        PlayerSaveData data = Load();
        if (data == null)   // ����N���ȂǂŃf�[�^�����݂��Ȃ��ꍇ
        {
            return null;
        }
        return data.allItemNames;
    }

    //�����A�C�e�����ۑ�
    public static void SaveOwnedItemName(string name)
    {
        PlayerSaveData data = Load();

        if (data.ownedSkins == null)
        {
            data.ownedSkins = new List<string>();
        }

        if (!data.ownedSkins.Contains(name))
        {
            data.ownedSkins.Add(name);
        }

        Save(data);
    }
}

