using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Text.Json;
using System.Numerics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net;

public class WordProccessor
{
    private string jsonStopWordsPath = "JsonFiles/nunsenseWordsTemprery.json";
    private string proccessingUrl = "";
    public List<char> englishLetters = new List<char>
{
    'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i',
    'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r',
    's', 't', 'u', 'v', 'w', 'x', 'y', 'z','A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I',
    'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R',
    'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
};

    private List<char> persianLetters = new List<char>
{
    'ا', 'ب', 'پ', 'ت', 'ث', 'ج', 'چ', 'ح', 'خ',
    'د', 'ذ', 'ر', 'ز', 'ژ', 'س', 'ش', 'ص', 'ض',
    'ط', 'ظ', 'ع', 'غ', 'ف', 'ق', 'ک', 'گ', 'ل',
    'م', 'ن', 'و', 'ه', 'ی', 'آ'
};
    public string WordProccessorCreator(HtmlNodeCollection paragraphs)
    {
        return SeprateProccessing(paragraphs);
    }
    private string SeprateProccessing(HtmlNodeCollection paragraphs) //Word Detector
    {
        if (paragraphs != null)
        {
            if (paragraphs.Count() != 0)
            {
                string orginalLabel = "";
                foreach (HtmlNode p in paragraphs)
                {
                    string label = p.InnerText.Trim();
                    label = System.Text.RegularExpressions.Regex.Replace(label, @"[^\u0600-\u06FF0-9\s]", "");
                    if (label.Length > 20)
                    {
                        orginalLabel += label;
                    }
                }
                return PersianChecking(orginalLabel);
            }
            else
            {
                System.Console.WriteLine("Paragraph is empty");
                return "failed";
            }
        }
        else
        {
            System.Console.WriteLine("Paragraph is empty");
            return "failed";
        }

    }
    private string PersianChecking(String label)//Word Detector
    {
        int englishWords = 0;
        int persianWords = 0;
        foreach (char p in label)
        {
            if (persianLetters.Contains(p))
            {
                persianWords++;
            }
            else if (englishLetters.Contains(p))
            {
                englishWords++;
            }
        }
        if (englishWords + persianWords != 0)
        {
            float persentage = (float)(persianWords / (englishWords + persianWords));
            if (persentage > 0.85)
            {
                return StopWordsProccessing(label);
            }
            else
            {
                System.Console.WriteLine("it have so many English words !!");
                return "forgien";
            }
        }
        else
        {
            System.Console.WriteLine("it have so many forgien words !!");
            return "forgien";
        }
    }
    private string StopWordsProccessing(string text)//Word Detector
    {
        List<string> wordsList = text
        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .ToList();
        Dictionary<string, int> stopWords = new Dictionary<string, int>();
        foreach (string word in wordsList)
        {
            if (stopWords.ContainsKey(word))
            {
                stopWords[word]++;
            }
            else
            {
                stopWords.TryAdd(word, 1);
            }
        }
        int step = wordsList.Count / 30;
        if (step > 5)
        {
            // foreach (var p in stopWords)
            // {
            //     if (p.Value < step || p.Key.Length > 4)
            //     {
            //         stopWords.Remove(p.Key);
            //     }
            // }
            // if (stopWords != null)
            // {
            //     Merging(stopWords);
            // }

            Merging(stopWords);
            return "persian";
        }
        else
        {
            System.Console.WriteLine("this page is not usefull");
            return "short";
        }
    }
    private void Merging(Dictionary<string, int> stopWords)//Word Detector
    {
        Dictionary<string, int> loadedDict = new Dictionary<string, int>();
        if (!File.Exists(jsonStopWordsPath))
        {
            string jsonText = JsonSerializer.Serialize(stopWords, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(jsonStopWordsPath, jsonText);
        }
        else
        {
            try
            {
                string jsonText = File.ReadAllText(jsonStopWordsPath);
                loadedDict = JsonSerializer.Deserialize<Dictionary<string, int>>(jsonText);
                if (loadedDict != null)
                {
                    if (loadedDict.Count() != 0)
                    {

                        foreach (var pair in loadedDict)
                        {
                            if (!stopWords.ContainsKey(pair.Key))
                            {
                                stopWords[pair.Key] = pair.Value;
                            }
                            else
                            {
                                stopWords[pair.Key] += pair.Value;
                            }
                        }
                    }
                    jsonText = JsonSerializer.Serialize(stopWords);
                    File.WriteAllText(jsonStopWordsPath, jsonText);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
                System.Console.WriteLine("Could not read the file");
            }
        }
    }

}