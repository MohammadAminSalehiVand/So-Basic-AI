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
    static CrawlerWikiPedia mainObject = new CrawlerWikiPedia();

    public static async Task Main()
    {
        while (true)
        {
            if (File.Exists(objectpath))
            {
                try
                {
                    System.Console.WriteLine("the object file exist");
                    string jsontext = File.ReadAllText(objectpath);
                    CrawlerWikiPedia loadedObject = JsonSerializer.Deserialize<CrawlerWikiPedia>(jsontext);
                    await mainObject.CrawlerCreatorWikiPedia();
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
                await mainObject.CrawlerCreatorWikiPedia();
            }
            string response = "";
            System.Console.WriteLine("wanna Exit? (Enter exit)");
            response = Console.ReadLine();
            if (response == "exit")
            {
                break;
            }
            else
            {
                continue;
            }
        }
    }

}