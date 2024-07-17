using System.Web;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;

const int year = 2024;
var fileName = $"diemTHPT{year}.csv";
var httpClient = new HttpClient
{
    BaseAddress = new Uri($"https://vietnamnet.vn/giao-duc/diem-thi/tra-cuu-diem-thi-tot-nghiep-thpt/{year}/")
};
var results = new ConcurrentBag<string>();

await File.WriteAllTextAsync(fileName, """
                                       "SBD","Toán","Ngữ văn","Ngoại ngữ","Vật lý","Hóa học","Sinh học","Lịch sử","Địa lý","GDCD"
                                       """);

Console.WriteLine("Working....");

await Parallel.ForAsync(1, 65, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 }, async (i, _) =>
{
    var maTinh = i.ToString("D2");
    var nfrqCount = 0;
    for (var j = 1; j <= 999999; j++)
    {
        var sbd = maTinh + j.ToString("D6");
        var success = await CrawlAsync(sbd);
        if (success)
            nfrqCount = 0;
        else
            nfrqCount++;
        if (nfrqCount > 100)
            break;
    }
});

Console.WriteLine("Finished");

// Sort results before writing to file
var sortedResults = results.OrderBy(x => x).ToList();
await File.AppendAllLinesAsync(fileName, sortedResults);
return;

async Task<bool> CrawlAsync(string sbd)
{
    try
    {
        var html = await httpClient.GetStringAsync(sbd + ".html");
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);
        var tds = htmlDocument.DocumentNode.Descendants("td").Select(td => HttpUtility.HtmlDecode(td.InnerHtml)).ToList();
        var subject = new Dictionary<string, string>
        {
            { "Toán", "" },
            { "Văn", "" },
            { "Ngoại ngữ", "" },
            { "Lí", "" },
            { "Hóa", "" },
            { "Sinh", "" },
            { "Sử", "" },
            { "Địa", "" },
            { "GDCD", "" }
        };
        for (var i = 0; i < tds.Count; i += 2) subject[tds[i]] = tds[i + 1];
        var result = subject.Aggregate(sbd, (current, sub) => current + "," + sub.Value);

        results.Add(result);
        Console.WriteLine(result);
        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine(sbd + ": " + ex.Message);
        return false;
    }
}
