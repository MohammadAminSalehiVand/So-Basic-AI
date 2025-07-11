using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Text.Json;

public class Program
{
    static string objectpath = "JsonFiles/learningObject.json";
    static WebCrawler mainObject = new WebCrawler();

    public static async Task Main()
    {
        string url = "https://fa.wikipedia.org/wiki/%D8%B2%D8%B1%D9%87%E2%80%8C%D9%BE%D9%88%D8%B4";
        if (File.Exists(objectpath))
        {
            try
            {
                System.Console.WriteLine("the object file exist");
                string jsontext = File.ReadAllText(objectpath);
                WebCrawlerData loadedObject = JsonSerializer.Deserialize<WebCrawlerData>(jsontext);
                if (loadedObject != null)
                {
                    WebCrawler loadedObjectOrginal = new WebCrawler();
                    loadedObjectOrginal.urlListPersian = loadedObject.urlListPersian;
                    loadedObjectOrginal.urlListForgien = loadedObject.urlListForgien;
                    loadedObjectOrginal.innerHref = loadedObject.innerHref;
                    loadedObjectOrginal.mainUrl = loadedObject.mainUrl;
                    loadedObjectOrginal.urlFailedToGet = loadedObject.urlFailedToGet;
                    mainObject = loadedObjectOrginal;
                    System.Console.WriteLine("wanna try a new Url?");
                    string responce = Console.ReadLine();
                    if (responce == "yes")
                    {
                        url = Console.ReadLine();
                        await mainObject.WebCrawlerCrator(url);
                    }
                    else
                    {
                        await mainObject.GettingWebPage(mainObject.mainUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                System.Console.WriteLine("Getting file Failed!");
            }
        }
        else
        {
            System.Console.WriteLine("the object file does not exist");
            await mainObject.WebCrawlerCrator(url);
        }
        await LearningMore(mainObject);
    }
    private static async Task LearningMore(WebCrawler loadedObject)
    {
        System.Console.WriteLine("wanna Continue For Deeper Learnning?");
        string response = Console.ReadLine();
        if (response == "yes")
        {
            await loadedObject.ProccessinLinks();
            await loadedObject.StopWordsCleanning();
            System.Console.WriteLine("Its Done! I leaned Everything!");
        }
        else
        {
            System.Console.WriteLine("WE DID IT!!!");
        }

    }

}