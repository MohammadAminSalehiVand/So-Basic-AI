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

public class CrawlerWikiPedia
{
    public List<string> urlListPersian { get; set; } = new List<string>();
    public List<string> urlListForgien { get; set; } = new List<string>();
    public List<string> urlFailedToGet { get; set; } = new List<string>();
    public List<string> innerHref { get; set; } = new List<string>();
    public string mainUrl { get; set; } = "";
    private string proccessingUrl = "";
    private string jsonObjectPath = "JsonFiles/learningObject.json";
    private string jsonStopWordsPath = "JsonFiles/nunsenseWordsTemprery.json";


    public async Task CrawlerCreatorWikiPedia()
    {
        try
        {
            if (File.Exists(jsonObjectPath))
            {
                string jsonText = File.ReadAllText(jsonObjectPath);
                CrawlerWikiPedia loadedObject = JsonSerializer.Deserialize<CrawlerWikiPedia>(jsonText);
                if (loadedObject != null)
                {
                    System.Console.WriteLine("loaded Object is not null");
                    this.urlFailedToGet = loadedObject.urlFailedToGet;
                    this.urlListForgien = loadedObject.urlListForgien;
                    this.urlListPersian = loadedObject.urlListPersian;
                    this.innerHref = loadedObject.innerHref;
                    this.mainUrl = loadedObject.mainUrl;
                }
            }

            if (this.mainUrl == "" || this.mainUrl == null)
            {
                System.Console.WriteLine("Main Url is emprty please Enter a New one => ");
                this.mainUrl = Console.ReadLine();
            }
            else
            {
                System.Console.WriteLine($"your main url is => \n {this.mainUrl}");
                System.Console.WriteLine("Do you wanna change the Main url? (say [y] if you want)");
                if (Console.ReadLine() == "y")
                {
                    System.Console.WriteLine("Enter the New url => ");
                    try
                    {
                        this.mainUrl = Console.ReadLine();
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine("maybe your new url is wrong!!!");
                        System.Console.WriteLine(ex);
                    }
                }
            }
            await DetectingTheInnerLinks();
            await GettingWebPageInformation(this.mainUrl);
            System.Console.WriteLine("your Main url has Learned. you can proccess your inner links too. do you want? (say [y])");
            if (Console.ReadLine() == "y")
            {
                await ProccessingTheInnerLinks();
            }
            else
            {
                ClearingTheNunsenseFile();
            }
            WordsRecognition objectWord = new WordsRecognition();
            objectWord.WordsRecognitionCreator();
        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex);
            System.Console.WriteLine("Theres Something Wrong with Creating");
        }
    }
    private async Task GettingWebPageInformation(string url)
    {
        try
        {
            this.proccessingUrl = url;
            using HttpClient client = new HttpClient();
            string htmlContent = await client.GetStringAsync(url);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);
            HtmlNodeCollection paragraphs = doc.DocumentNode.SelectNodes("//p");
            if (paragraphs != null)
            {
                WordProccessor wordProccessorObject = new WordProccessor();
                MergingUrlLists(wordProccessorObject.WordProccessorCreator(paragraphs));
            }
        }
        catch (Exception ex)
        {
            urlFailedToGet.Add(url);
            System.Console.WriteLine(ex);
            System.Console.WriteLine("ERROR in GettinWebPage!!");
        }
    }
    private void MergingUrlLists(string status)
    {
        switch (status)
        {
            case "forgien":
                this.urlListForgien.Add(proccessingUrl);
                break;
            case "persian":
                this.urlListPersian.Add(proccessingUrl);
                break;
            case "failed":
                this.urlFailedToGet.Add(proccessingUrl);
                break;
        }
        SavingThisObject();
    }
    private async Task DetectingTheInnerLinks()
    {
        using HttpClient client = new HttpClient();
        string htmlContent = await client.GetStringAsync(this.mainUrl);
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);
        try
        {
            HtmlNodeCollection innerLinks = doc.DocumentNode.SelectNodes("//a[@href]");
            doc = null;
            htmlContent = null;
            this.innerHref = [];
            if (innerLinks != null)
            {
                foreach (var link in innerLinks)
                {
                    string linkText = link.GetAttributeValue("href", "");
                    if (!string.IsNullOrEmpty(linkText))
                    {
                        if (!linkText.Contains(";uselang=") && !linkText.StartsWith("#") && !linkText.Contains(".php"))
                        {
                            if (linkText.StartsWith("/wiki") || linkText.StartsWith("/w/"))
                            {
                                linkText = "https://fa.wikipedia.org/" + linkText;
                            }
                            if (linkText.Contains("//fa."))
                            {
                                if (!linkText.StartsWith("http"))
                                {
                                    linkText = "https:" + linkText;
                                }
                                this.innerHref.Add(linkText);
                            }
                        }
                    }
                }
                SavingThisObject();
            }
            else
            {
                System.Console.WriteLine("Inner Links is null!!!");
            }
        }
        catch (Exception ex)
        {

            System.Console.WriteLine(ex);
            System.Console.WriteLine("Somthing went wrong!!!");
        }

    }
    private async Task ProccessingTheInnerLinks()
    {
        if (this.innerHref != null)
        {
            if (this.innerHref.Count != 0)
            {
                foreach (string link in this.innerHref)
                {
                    string input = @link;
                    string output = Regex.Replace(input, @"\\u([0-9A-Fa-f]{4})", m =>
                    {
                        string hex = m.Groups[1].Value;
                        int code = Convert.ToInt32(hex, 16);
                        char unicodeChar = (char)code;
                        return unicodeChar.ToString();
                    });
                    if (output.Contains(";"))
                    {
                        output = output.Substring(0, output.IndexOf(";"));
                    }
                    Process currentProcess = Process.GetCurrentProcess();
                    long memoryUsed = currentProcess.WorkingSet64;
                    TimeSpan cpuTime = currentProcess.TotalProcessorTime;
                    System.Console.WriteLine(output);
                    Console.WriteLine($"\nRAM: {memoryUsed / (1024 * 1024)} MB");
                    Console.WriteLine($"TIME ON CPU: {cpuTime.TotalMilliseconds} ms\n");
                    await GettingWebPageInformation(output);
                    this.innerHref = [];
                    SavingThisObject();
                    ClearingTheNunsenseFile();
                }
            }
        }
        else
        {
            System.Console.WriteLine("innerHref is Empty");
        }
    }
    private void SavingThisObject()
    {
        string jsontext = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(jsonObjectPath, jsontext);
    }
    private void ClearingTheNunsenseFile()
    {
        File.WriteAllText(jsonStopWordsPath, "{}");
    }
}