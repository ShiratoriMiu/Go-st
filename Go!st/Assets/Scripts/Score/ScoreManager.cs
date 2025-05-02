using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private string filePath;

    private void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, "ScoreData.csv");
    }

    public async Task WriteScoreDataAsync(int newScore)
    {
        List<int> scores = LoadScoreData();

        scores.Add(newScore);
        scores.Sort((a, b) => b.CompareTo(a));

        if (scores.Count > 10)
        {
            scores = scores.GetRange(0, 10);
        }

        var lines = scores.ConvertAll(s => s.ToString());

        using StreamWriter writer = new StreamWriter(filePath, false); // è„èëÇ´
        foreach (var line in lines)
        {
            await writer.WriteLineAsync(line);
        }
    }

    public List<int> LoadScoreData()
    {
        List<int> scores = new List<int>();

        if (!File.Exists(filePath))
            return scores;

        string[] lines = File.ReadAllLines(filePath);
        foreach (string line in lines)
        {
            if (int.TryParse(line.Trim(), out int result))
            {
                scores.Add(result);
            }
        }

        return scores;
    }
}
