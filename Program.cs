using System.Web;
using HtmlAgilityPack;

const int year = 2023;
var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri($"https://vietnamnet.vn/giao-duc/diem-thi/tra-cuu-diem-thi-tot-nghiep-thpt/{year}/");
await using (StreamWriter line = new($"diemTHPT{year}.csv"))
{
    _ = line.WriteLineAsync(
        @"""SBD"",""Toán"",""Ngữ văn"",""Ngoại ngữ"",""Vật lý"",""Hóa học"",""Sinh học"",""Lịch sử"",""Địa lý"",""GDCD""");
    line.Close();
}

Console.WriteLine("Working....");
var nfrqCount = 0;
for (var i = 1; i <= 64; i++)
{
    var maTinh = i.ToString();
    if (i < 10)
        maTinh = "0" + maTinh;
    for (var j = 1; j <= 999999; j++)
    {
        var mahs = j.ToString();
        var maHs = "000000".Remove(6 - mahs.Length) + mahs;
        var sbd = maTinh + maHs;
        var sussed = await Crawl(sbd);
        if (sussed)
            nfrqCount = 0;
        else
            nfrqCount++;
        if (nfrqCount > 100)
            break;
    }
}

Console.WriteLine("Finished");
return;

async Task<bool> Crawl(string sbd)
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
        Console.WriteLine(result);
        await using StreamWriter line = new("diemTHPT2022.csv", true);
        await line.WriteLineAsync(result);
        line.Close();
    }
    catch (Exception ex)
    {
        Console.WriteLine(sbd + ": " + ex.Message);
        return false;
    }

    return true;
}