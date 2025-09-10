using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

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

[System.Serializable]
public class ItemData
{
    public string name;
    public string IconName;
    public float[] color;

    public bool isOwned;        // �������Ă��邩
    public bool isEquipped;     // �������Ă��邩
    public bool canColorChange;  // �F�ς��\��
    public bool isColorChangeOn; //����F�ς��\���i�K�`���ȂǂŃ_�u�������j

    public ItemData(string _name, string _iconName, Color _color, bool _isOwned, bool _isEquipped, bool _canColorChange, bool _isColorChangeOn)
    {
        this.name = _name;
        IconName = _iconName;
        color = new float[] { _color.r, _color.g, _color.b, _color.a };
        isOwned = _isOwned;
        isEquipped = _isEquipped;
        canColorChange = _canColorChange;
        isColorChangeOn = _isColorChangeOn;
    }

    public Color ToColor()
    {
        if (color != null && color.Length == 4)
            return new Color(color[0], color[1], color[2], color[3]);
        return Color.white;
    }
}

public enum PlayerIconStyle
{
    BackGround,
    Chara,
}

[System.Serializable]
public class PlayerIconData
{
    [HideInInspector]public string name;
    public PlayerIconStyle style;

    public PlayerIconData(string _name, PlayerIconStyle _style)
    {
        name = _name;
        this.style = _style;
    }
}

/// <summary>
/// �Z�[�u����f�[�^
/// </summary>
[System.Serializable]
public class PlayerSaveData
{
    public List<MaterialData> materials = new List<MaterialData>();//�}�e���A��

    // �R�C��
    public int coin = 0;

    public List<ItemData> allItems = new List<ItemData>();//�S�Ă̒����ւ��A�C�e���̖��O

    public List<string> gachaItems = new List<string>();//�K�`���Ŏ擾�\�ȃA�C�e���̖��O��ۑ�

    public List<PlayerIconData> ownedPlayerIcons = new List<PlayerIconData>();//�����v���C���[�A�C�R���̖��O

    public List<PlayerIconData> equippedPlayerIcons = new List<PlayerIconData>();
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

    //�A�C�e���ۑ�
    public static void SaveAllItem(string _name, string _iconName, Color _color, bool _isOwned, bool _isEquipped, bool _canColorChange, bool _isColorChangeOn)
    {
        // �����f�[�^��ǂݍ��݁inull �΍�j
        PlayerSaveData data = Load() ?? new PlayerSaveData();

        if (data.allItems == null)
        {
            data.allItems = new List<ItemData>();
        }

        // ���ɓ����̃A�C�e�������邩�`�F�b�N
        bool exists = data.allItems.Exists(x => x.name == _name);

        if (!exists)
        {
            // �V���� ItemData �𐳂����������Ēǉ�
            ItemData item = new ItemData(_name, _iconName, _color, _isOwned, _isEquipped, _canColorChange, _isColorChangeOn);

            data.allItems.Add(item);
            Save(data); // �㏑���ۑ�

            Debug.Log($"SaveAllItem: added '{_name}' (total:{data.allItems.Count})");
        }
        else
        {
            Debug.Log($"SaveAllItem: '{_name}' already exists, skip add.");
        }
    }


    //�A�C�e���擾
    public static List<ItemData> AllItems()
    {
        PlayerSaveData data = Load();
        if (data == null)   // ����N���ȂǂŃf�[�^�����݂��Ȃ��ꍇ
        {
            return null;
        }
        return data.allItems;
    }

    public static void UpdateItemFlags(string itemName, bool? owned = null, bool? colorChangeOn = null)
    {
        PlayerSaveData data = Load() ?? new PlayerSaveData();

        if (data.allItems == null)
            data.allItems = new List<ItemData>();

        // ���O�ŃA�C�e��������
        ItemData item = data.allItems.Find(i => i.name == itemName);

        if (item != null)
        {
            // null �łȂ��ꍇ�̂ݍX�V
            if (owned.HasValue)
                item.isOwned = owned.Value;

            if (colorChangeOn.HasValue)
                item.isColorChangeOn = colorChangeOn.Value;

            Save(data); // �ۑ�
            Debug.Log($"�A�C�e�� '{itemName}' �̃t���O���X�V���܂��� (isOwned={item.isOwned}, isColorChangeOn={item.isColorChangeOn})");
        }
        else
        {
            Debug.LogWarning($"�A�C�e�� '{itemName}' ��������܂���");
        }
    }

    //�K�`���A�C�e���ۑ�
    public static void SaveGachaItemName(string _name)
    {
        // �����f�[�^��ǂݍ��݁inull �΍�j
        PlayerSaveData data = Load() ?? new PlayerSaveData();

        if (data.gachaItems == null)
        {
            data.gachaItems = new List<string>();
        }

        // ���ɓ����̃A�C�e�������邩�`�F�b�N
        bool exists = data.gachaItems.Exists(x => x == _name);

        if (!exists)
        {
            data.gachaItems.Add(_name);
            Save(data); // �㏑���ۑ�

            Debug.Log($"SaveAllItem: added '{_name}' (total:{data.allItems.Count})");
        }
        else
        {
            Debug.Log($"SaveAllItem: '{_name}' already exists, skip add.");
        }
    }

    //�K�`���A�C�e���擾
    public static List<string> GachaItems()
    {
        PlayerSaveData data = Load();
        if (data == null)   // ����N���ȂǂŃf�[�^�����݂��Ȃ��ꍇ
        {
            return null;
        }
        return data.gachaItems;
    }

    // �A�C�R���w���i�����ς݂ɒǉ��j
    public static void SaveOwnedPlayerIcon(PlayerIconData iconName)
    {
        PlayerSaveData data = Load() ?? new PlayerSaveData();

        if (data.ownedPlayerIcons == null)
            data.ownedPlayerIcons = new List<PlayerIconData>();

        if (!data.ownedPlayerIcons.Contains(iconName))
        {
            data.ownedPlayerIcons.Add(iconName);
            Save(data);
            Debug.Log($"SavePlayerIcon: '{iconName}' ���w���ς݂ɒǉ����܂���");
        }
        else
        {
            Debug.Log($"SavePlayerIcon: '{iconName}' �͂��łɏ����ς݂ł�");
        }
    }

    // �����A�C�R���ꗗ���擾
    public static List<PlayerIconData> LoadOwnedPlayerIcons()
    {
        PlayerSaveData data = Load();
        return data?.ownedPlayerIcons ?? new List<PlayerIconData>();
    }

    // �A�C�R������
    public static void SaveEquippedPlayerIcon(PlayerIconData newIcon)
    {
        PlayerSaveData data = Load() ?? new PlayerSaveData();

        if (data.equippedPlayerIcons == null)
            data.equippedPlayerIcons = new List<PlayerIconData>();

        // ���� style �̃A�C�R�������łɂ��邩�T��
        int index = data.equippedPlayerIcons.FindIndex(icon => icon.style == newIcon.style);

        if (index >= 0)
        {
            // ���łɂ���Ȃ�㏑��
            data.equippedPlayerIcons[index] = newIcon;
            Debug.Log($"SavePlayerIcon: '{newIcon}' �� {newIcon.style} �Ƃ��ď㏑�����܂���");
        }
        else
        {
            // �܂����̃X�^�C�����Ȃ��Ȃ�ǉ�
            data.equippedPlayerIcons.Add(newIcon);
            Debug.Log($"SavePlayerIcon: '{newIcon}' �� {newIcon.style} �Ƃ��ĐV�K�ǉ����܂���");
        }

        // �O�̂��ߍő�2�ɐ����iBackGround �� Chara�j
        if (data.equippedPlayerIcons.Count > 2)
        {
            data.equippedPlayerIcons = data.equippedPlayerIcons
                .GroupBy(icon => icon.style)
                .Select(g => g.First())
                .ToList();
        }

        Save(data);
    }


    // �����A�C�R���ꗗ���擾
    public static List<PlayerIconData> LoadEquippedPlayerIcons()
    {
        PlayerSaveData data = Load();
        return data?.equippedPlayerIcons ?? new List<PlayerIconData>();
    }
}

