using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Text.Json;
using System.Numerics;

public class WordsRecognition
{
    public Dictionary<string, int> nameWords { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> commenWords { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> stopWordsFinalList { get; set; } = new Dictionary<string, int>();
    private string recognitionObjectPath = "JsonFiles/recognationObject.json";
    private string tempreryStopWordsPath = "JsonFiles/nunsenseWordsTemprery.json";
    public Dictionary<string, int> loadedDict = new Dictionary<string, int>();
    public void WordsRecognitionCreator()
    {
        if (File.Exists(recognitionObjectPath))
        {
            string jsonObjectText = File.ReadAllText(recognitionObjectPath);
            WordsRecognition loadedObject = JsonSerializer.Deserialize<WordsRecognition>(jsonObjectText);
            if (loadedObject != null)
            {
                this.nameWords = loadedObject.nameWords;
                this.stopWordsFinalList = loadedObject.stopWordsFinalList;
                this.commenWords = loadedObject.commenWords;
                loadedObject = null;
            }
        }
        else
        {
            ObjectSaver();
        }
        FileReader();

    }
    private void FileReader()
    {
        try
        {
            string jsonStopText = File.ReadAllText(tempreryStopWordsPath);
            loadedDict = JsonSerializer.Deserialize<Dictionary<string, int>>(jsonStopText);
            if (loadedDict != null)
            {
                int step = Math.Max(1, loadedDict.Count / 300);
                NameDetector(loadedDict, step);
                step = Math.Max(1, loadedDict.Count / 150);
                CommenDetector(loadedDict, step);
                step = Math.Max(1, loadedDict.Count / 100);
                StopDetector(loadedDict, step);
                CompareLists(loadedDict);
                ObjectSaver();
                loadedDict = null;
            }
            else
            {
                System.Console.WriteLine("Stop Words File Is Empty !!!");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex);
        }
    }
    private void NameDetector(Dictionary<string, int> name, int step)
    {
        float middleStopWordsList = 0;
        foreach (var p in name)
        {
            if (p.Value >= step)
            {
                if (nameWords.ContainsKey(p.Key))
                {
                    middleStopWordsList += 1;
                    nameWords[p.Key] += 1;
                }
                else
                {
                    middleStopWordsList += 1;
                    nameWords.TryAdd(p.Key, p.Value);
                }
            }
        }
        middleStopWordsList = middleStopWordsList / nameWords.Count();
        foreach (var p in nameWords)
        {
            if (p.Value >= middleStopWordsList)
            {
                //
            }
            else
            {
                nameWords.Remove(p.Key);
            }
        }
    }
    private void CommenDetector(Dictionary<string, int> commen, int step)
    {
        float middleStopWordsList = 0;
        foreach (var p in commen)
        {
            if (p.Value >= step)
            {
                if (commenWords.ContainsKey(p.Key))
                {
                    middleStopWordsList += 1;
                    commenWords[p.Key] += 1;
                }
                else
                {
                    middleStopWordsList += 1;
                    commenWords.TryAdd(p.Key, p.Value);
                }
            }
        }
        middleStopWordsList = middleStopWordsList / commenWords.Count();
        foreach (var p in commenWords)
        {
            if (p.Value >= middleStopWordsList)
            {
                //
            }
            else
            {
                commenWords.Remove(p.Key);
            }
        }
    }
    private void StopDetector(Dictionary<string, int> stop, int step)
    {
        float middleStopWordsList = 0;
        foreach (var p in stop)
        {
            if (p.Value >= step)
            {
                if (stopWordsFinalList.ContainsKey(p.Key))
                {
                    middleStopWordsList += 1;
                    stopWordsFinalList[p.Key] += 1;
                }
                else
                {
                    middleStopWordsList += 1;
                    stopWordsFinalList.TryAdd(p.Key, p.Value);
                }
            }
        }
        middleStopWordsList = middleStopWordsList / stopWordsFinalList.Count();
        foreach (var p in stopWordsFinalList)
        {
            if (p.Value >= middleStopWordsList)
            {
                //
            }
            else
            {
                stopWordsFinalList.Remove(p.Key);
            }
        }
    }
    private void CompareLists(Dictionary<string, int> list)
    {
        foreach (var p in list)
        {
            int enterIf = 0;
            int maxValue = int.MinValue;
            if (nameWords.ContainsKey(p.Key) && (nameWords[p.Key] > maxValue))
            {
                enterIf = 1;
                maxValue = nameWords[p.Key];
            }
            if (commenWords.ContainsKey(p.Key) && commenWords.ContainsKey(p.Key))
            {
                enterIf = 2;
                maxValue = commenWords[p.Key];

            }
            if (stopWordsFinalList.ContainsKey(p.Key) && stopWordsFinalList.ContainsKey(p.Key))
            {
                enterIf = 3;
                maxValue = stopWordsFinalList[p.Key];
            }
            if (enterIf == 0)
            {
                continue;
            }
            else if (enterIf == 1)
            {
                commenWords.Remove(p.Key);
                stopWordsFinalList.Remove(p.Key);
            }
            else if (enterIf == 2)
            {
                nameWords.Remove(p.Key);
                stopWordsFinalList.Remove(p.Key);
            }
            else if (enterIf == 3)
            {
                nameWords.Remove(p.Key);
                nameWords.Remove(p.Key);
            }
        }
    }
    private void ObjectSaver()
    {
        string jsontext = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(recognitionObjectPath, jsontext);
    }
}