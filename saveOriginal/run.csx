#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"
#r "System.Drawing"
 
using System.Net;
using System.IO; 
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Bindings.Runtime;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System.Text;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, Stream saveBlob, Binder binder, TraceWriter log)
{
    string name = req.GetQueryNameValuePairs()
                     .FirstOrDefault(q => string.Compare(q.Key, "imgName", true) == 0)
                     .Value;
         
    string filename = name;
    
    if (filename.Contains('.')) {
        filename = name.Substring(0, name.IndexOf('.'));
    }

    var attributes = new Attribute[]
    {
        new BlobAttribute($"originals/{filename}/{name}"),
        new StorageAccountAttribute("AzureWebJobsStorage")
    };

    using (var writer = await binder.BindAsync<CloudBlobStream>(attributes).ConfigureAwait(false))
    {
        var bytes = req.Content.ReadAsByteArrayAsync().Result;
        writer.Write(bytes, 0, bytes.Length);
    }


    var response = new HttpResponseMessage()
    {
        StatusCode = HttpStatusCode.OK,
    };

    var content = createResponse(filename, name);

    response.Content = new StringContent(
        content: JsonConvert.SerializeObject(content),
        encoding: Encoding.UTF8,
        mediaType: "application/json");
    
    return response;
}

public static Dictionary<string,string> createResponse (string filename, string name) {
    //e.g: https://mycoolresizingapp.blob.core.windows.net
    string baseURL = "https://imgresizeservicestorage.blob.core.windows.net";

    string sizedBaseURL = $"{baseURL}/sized";
    int[] sizes = new int[] {200,100,80,64,48,24};
    Dictionary<string, string> responseContent = new Dictionary<string, string>();

    string originalBaseURL = $"{baseURL}/originals";
    responseContent.Add("original", $"{originalBaseURL}/{filename}/{name}");

    foreach (int size in sizes) {
        string url = $"{sizedBaseURL}/{size}/{filename}/{name}";
        responseContent.Add($"{size}", url);
    }

    return responseContent;
}