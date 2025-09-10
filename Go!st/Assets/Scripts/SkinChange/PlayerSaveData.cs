using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

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

    public bool isOwned;        // 所持しているか
    public bool isEquipped;     // 装備しているか
    public bool canColorChange;  // 色変え可能か
    public bool isColorChangeOn; //現状色変え可能か（ガチャなどでダブったか）

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
/// セーブするデータ
/// </summary>
[System.Serializable]
public class PlayerSaveData
{
    public List<MaterialData> materials = new List<MaterialData>();//マテリアル

    // コイン
    public int coin = 0;

    public List<ItemData> allItems = new List<ItemData>();//全ての着せ替えアイテムの名前

    public List<string> gachaItems = new List<string>();//ガチャで取得可能なアイテムの名前を保存

    public List<PlayerIconData> ownedPlayerIcons = new List<PlayerIconData>();//所持プレイヤーアイコンの名前

    public List<PlayerIconData> equippedPlayerIcons = new List<PlayerIconData>();
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

    //アイテム保存
    public static void SaveAllItem(string _name, string _iconName, Color _color, bool _isOwned, bool _isEquipped, bool _canColorChange, bool _isColorChangeOn)
    {
        // 既存データを読み込み（null 対策）
        PlayerSaveData data = Load() ?? new PlayerSaveData();

        if (data.allItems == null)
        {
            data.allItems = new List<ItemData>();
        }

        // 既に同名のアイテムがあるかチェック
        bool exists = data.allItems.Exists(x => x.name == _name);

        if (!exists)
        {
            // 新しい ItemData を正しく生成して追加
            ItemData item = new ItemData(_name, _iconName, _color, _isOwned, _isEquipped, _canColorChange, _isColorChangeOn);

            data.allItems.Add(item);
            Save(data); // 上書き保存

            Debug.Log($"SaveAllItem: added '{_name}' (total:{data.allItems.Count})");
        }
        else
        {
            Debug.Log($"SaveAllItem: '{_name}' already exists, skip add.");
        }
    }


    //アイテム取得
    public static List<ItemData> AllItems()
    {
        PlayerSaveData data = Load();
        if (data == null)   // 初回起動などでデータが存在しない場合
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

        // 名前でアイテムを検索
        ItemData item = data.allItems.Find(i => i.name == itemName);

        if (item != null)
        {
            // null でない場合のみ更新
            if (owned.HasValue)
                item.isOwned = owned.Value;

            if (colorChangeOn.HasValue)
                item.isColorChangeOn = colorChangeOn.Value;

            Save(data); // 保存
            Debug.Log($"アイテム '{itemName}' のフラグを更新しました (isOwned={item.isOwned}, isColorChangeOn={item.isColorChangeOn})");
        }
        else
        {
            Debug.LogWarning($"アイテム '{itemName}' が見つかりません");
        }
    }

    //ガチャアイテム保存
    public static void SaveGachaItemName(string _name)
    {
        // 既存データを読み込み（null 対策）
        PlayerSaveData data = Load() ?? new PlayerSaveData();

        if (data.gachaItems == null)
        {
            data.gachaItems = new List<string>();
        }

        // 既に同名のアイテムがあるかチェック
        bool exists = data.gachaItems.Exists(x => x == _name);

        if (!exists)
        {
            data.gachaItems.Add(_name);
            Save(data); // 上書き保存

            Debug.Log($"SaveAllItem: added '{_name}' (total:{data.allItems.Count})");
        }
        else
        {
            Debug.Log($"SaveAllItem: '{_name}' already exists, skip add.");
        }
    }

    //ガチャアイテム取得
    public static List<string> GachaItems()
    {
        PlayerSaveData data = Load();
        if (data == null)   // 初回起動などでデータが存在しない場合
        {
            return null;
        }
        return data.gachaItems;
    }

    // アイコン購入（所持済みに追加）
    public static void SaveOwnedPlayerIcon(PlayerIconData iconName)
    {
        PlayerSaveData data = Load() ?? new PlayerSaveData();

        if (data.ownedPlayerIcons == null)
            data.ownedPlayerIcons = new List<PlayerIconData>();

        if (!data.ownedPlayerIcons.Contains(iconName))
        {
            data.ownedPlayerIcons.Add(iconName);
            Save(data);
            Debug.Log($"SavePlayerIcon: '{iconName}' を購入済みに追加しました");
        }
        else
        {
            Debug.Log($"SavePlayerIcon: '{iconName}' はすでに所持済みです");
        }
    }

    // 所持アイコン一覧を取得
    public static List<PlayerIconData> LoadOwnedPlayerIcons()
    {
        PlayerSaveData data = Load();
        return data?.ownedPlayerIcons ?? new List<PlayerIconData>();
    }

    // アイコン装備
    public static void SaveEquippedPlayerIcon(PlayerIconData newIcon)
    {
        PlayerSaveData data = Load() ?? new PlayerSaveData();

        if (data.equippedPlayerIcons == null)
            data.equippedPlayerIcons = new List<PlayerIconData>();

        // 同じ style のアイコンがすでにあるか探す
        int index = data.equippedPlayerIcons.FindIndex(icon => icon.style == newIcon.style);

        if (index >= 0)
        {
            // すでにあるなら上書き
            data.equippedPlayerIcons[index] = newIcon;
            Debug.Log($"SavePlayerIcon: '{newIcon}' を {newIcon.style} として上書きしました");
        }
        else
        {
            // まだそのスタイルがないなら追加
            data.equippedPlayerIcons.Add(newIcon);
            Debug.Log($"SavePlayerIcon: '{newIcon}' を {newIcon.style} として新規追加しました");
        }

        // 念のため最大2つに制限（BackGround と Chara）
        if (data.equippedPlayerIcons.Count > 2)
        {
            data.equippedPlayerIcons = data.equippedPlayerIcons
                .GroupBy(icon => icon.style)
                .Select(g => g.First())
                .ToList();
        }

        Save(data);
    }


    // 装備アイコン一覧を取得
    public static List<PlayerIconData> LoadEquippedPlayerIcons()
    {
        PlayerSaveData data = Load();
        return data?.equippedPlayerIcons ?? new List<PlayerIconData>();
    }
}

