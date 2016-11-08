#r "System.Drawing"
#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"

using System;
using System.Drawing;
using ImageProcessor;
using System.IO; 
using System.Net;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Bindings.Runtime;
using Microsoft.WindowsAzure.Storage.Blob;


public static void Run(Stream myBlob, string name, Stream saveBlob, Binder binder, TraceWriter log)
{
    resizeImage(myBlob, new Size(200,200), binder);
    resizeImage(myBlob, new Size(100,100), binder);
    resizeImage(myBlob, new Size(80,80), binder);
    resizeImage(myBlob, new Size(64,64), binder);
    resizeImage(myBlob, new Size(48,48), binder);
    resizeImage(myBlob, new Size(24,24), binder);
}


public static async void resizeImage (Stream blob, Size size, Binder binder) {
    using (var imageFactory = new ImageFactory()) {
    
        var outStream = new MemoryStream();
        imageFactory.Load(blob).Resize(size).Save(outStream);
        
        var attributes = new Attribute[]
        {
            new BlobAttribute($"sized/"+size.Width+"/{name}"),
            new StorageAccountAttribute("AzureWebJobsStorage")
        };

        using (var writer = await binder.BindAsync<CloudBlobStream>(attributes).ConfigureAwait(false))
        {
            var bytes = outStream.GetBuffer();
            writer.Write(bytes, 0, bytes.Length);
        }
    } 
}