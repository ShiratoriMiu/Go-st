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
    public string textureName; // テクスチャ名
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

    public ItemData(string name, string iconName, Color c)
    {
        this.name = name;
        IconName = iconName;
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
/// セーブするデータ
/// </summary>
[System.Serializable]
public class PlayerSaveData
{
    public List<string> ownedSkins = new List<string>();//所持アイテム
    public List<EquippedSkinData> equippedSkins = new List<EquippedSkinData>();//装備アイテム
    public List<MaterialData> materials = new List<MaterialData>();//マテリアル
    public List<string> equippedMakes = new List<string>();//装備メイク
    // コイン
    public int coin = 0;

    public List<ItemData> allItems = new List<ItemData>();//全ての着せ替えアイテムの名前
}

/// <summary>
/// JSON保存・読み込みマネージャー
/// </summary>
public static class SaveManager
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "playerSave.json");

    public static void Save(PlayerSaveData data)
    {
        // 保存先ディレクトリを確認して、なければ作成
        string directory = Path.GetDirectoryName(SavePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"データ保存完了: {SavePath}");
    }


    public static PlayerSaveData Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("セーブデータが存在しません。新規作成します。");
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
            Debug.Log("セーブデータを削除しました。");
        }
    }

    //コインだけ保存
    public static void SaveCoin(int coin)
    {
        PlayerSaveData data = Load();   // 既存データを読み込み
        data.coin = coin;              // コインだけ更新
        Save(data);                    // 上書き保存
    }

    //コインだけ取得
    public static int LoadCoin()
    {
        PlayerSaveData data = Load();
        if (data == null)   // 初回起動などでデータが存在しない場合
        {
            return 0;
        }
        return data.coin;
    }

    //アイテム名保存
    public static void SaveAllItem(string name, string iconName, Color color)
    {
        // 既存データを読み込み（null 対策）
        PlayerSaveData data = Load() ?? new PlayerSaveData();

        if (data.allItems == null)
        {
            data.allItems = new List<ItemData>();
        }

        // 既に同名のアイテムがあるかチェック
        bool exists = data.allItems.Exists(x => x.name == name);

        if (!exists)
        {
            // 新しい ItemData を正しく生成して追加
            ItemData item = new ItemData(name, iconName, color);

            data.allItems.Add(item);
            Save(data); // 上書き保存

            Debug.Log($"SaveAllItem: added '{name}' (total:{data.allItems.Count})");
        }
        else
        {
            Debug.Log($"SaveAllItem: '{name}' already exists, skip add.");
        }
    }


    //アイテム名取得
    public static List<ItemData> AllItems()
    {
        PlayerSaveData data = Load();
        if (data == null)   // 初回起動などでデータが存在しない場合
        {
            return null;
        }
        return data.allItems;
    }

    //所持アイテム名保存
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

