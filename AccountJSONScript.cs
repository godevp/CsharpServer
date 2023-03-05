namespace CsharpServer;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;


public class ListOfAccounts
{
    public List<Account> accounts { get; set; }
}
public class AccountJSONScript
{
    
    private static readonly string SolutionDirectory = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName).FullName).FullName).FullName;
    private static readonly string FilePath = Path.Combine(SolutionDirectory, "accounts.json");
    public static void Save(ListOfAccounts listOfAccounts)
    {
        var json = JsonConvert.SerializeObject(listOfAccounts);
        File.WriteAllText(FilePath, json);
    }
    public static ListOfAccounts Load()
    {
        if (File.Exists(FilePath))
        {
            var json = File.ReadAllText(FilePath);
            return JsonConvert.DeserializeObject<ListOfAccounts>(json);
        }

        return new ListOfAccounts { accounts = new List<Account>() };
    }
}