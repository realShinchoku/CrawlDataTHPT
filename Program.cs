
using HtmlAgilityPack;
using System.Web;

HttpClient httpClient = new HttpClient();
httpClient.BaseAddress = new Uri("https://vietnamnet.vn/giao-duc/diem-thi/tra-cuu-diem-thi-tot-nghiep-thpt/2022/");
await using (StreamWriter line = new("diemTHPT2022.csv"))
{
    _ = line.WriteLineAsync(@"""SBD"",""Toán"",""Ngữ văn"",""Ngoại ngữ"",""Vật lý"",""Hóa học"",""Sinh học"",""Lịch sử"",""Địa lý"",""GDCD""");
    line.Close();
}
Console.WriteLine("Working....");
int nfrqCount = 0;
for (int i = 1; i <= 64; i++)
{
    string maTinh = i.ToString();
    if (i < 10)
        maTinh = "0" + maTinh;
    for (int j = 1; j <= 999999; j++)
    {
        string mahs = j.ToString();
        string maHs = "000000".Remove(6 - mahs.Length) + mahs;
        var sbd = maTinh + maHs;
        bool sussed = await Crawl(sbd);
        if (sussed)
            nfrqCount = 0;
        else
            nfrqCount++;
        if (nfrqCount > 100)
            break;
    }
}
Console.WriteLine("Finished");
async Task<bool> Crawl(string sbd)
{
    try
    {
        string html = await httpClient.GetStringAsync(sbd + ".html");
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);
        var tbody = htmlDocument.DocumentNode.Descendants("tbody").First();
        var tds = htmlDocument.DocumentNode.Descendants("td").Select(td => HttpUtility.HtmlDecode(td.InnerHtml)).ToList();
        var subject = new Dictionary<string, string> {
            {"Toán", "" },
            {"Văn", "" },
            {"Ngoại ngữ", "" },
            {"Lí", "" },
            {"Hóa", "" },
            {"Sinh", "" },
            {"Sử", "" },
            {"Địa", "" },
            {"GDCD", "" },
        };
        for (int i = 0; i < tds.Count; i+=2)
        {
            subject[tds[i]] = tds[i+1];
        }
        string result = sbd;
        foreach (var sub in subject)
        {
            result = result + "," + sub.Value;
        }
        Console.WriteLine(result);
        await using StreamWriter line = new("diemTHPT2022.csv", append: true);
        _ = line.WriteLineAsync(result);
        line.Close();
    }
    catch (Exception ex)
    {
        Console.WriteLine(sbd+": "+ex.Message);
        return false;
    }
    return true;
    
}