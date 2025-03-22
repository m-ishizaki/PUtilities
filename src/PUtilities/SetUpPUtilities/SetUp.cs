using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;

namespace RKSoftware.SetUpPUtilities
{
    public class SetUp
    {
        static string url = "https://github.com/m-ishizaki/PUtilities/releases/download/0.0.1/RKSoftware.PUtilities.zip";
        Lazy<HttpClient> client = new Lazy<HttpClient>(() => new HttpClient());

        public async Task Execute(string[] addonurls)
        {
            var download = Path.Combine(Environment.CurrentDirectory, "tmp");
            if (!Directory.Exists(download)) Directory.CreateDirectory(download);

            var file = await Download(download, url);
            var addonsTasks = addonurls.AsParallel().Select(async a => await Download(download, a)).ToArray();
            Task.WaitAll(addonsTasks);
            var addonsFiles = addonsTasks.Select(a => a.Result).ToArray();

            var unzipFiles = Unzip(file);
            var unzipAddonsFiles = addonsFiles.Select(x => Unzip(x));

            var putilities = Path.Combine(Environment.CurrentDirectory, "PUtilities");
            if (!Directory.Exists(putilities)) Directory.CreateDirectory(putilities);
            var addons = Path.Combine(putilities, "Addons");

            Copy(unzipFiles, putilities);
            foreach (var addon in unzipAddonsFiles) Copy(addon, addons);
        }

        async Task<string> Download(string download, string url)
        {
            var bytes = await client.Value.GetByteArrayAsync(url);
            var file = Path.Combine(download, Path.GetFileName(url));
            File.WriteAllBytes(file, bytes);
            return file;
        }

        string Unzip(string file)
        {
            var extract = Path.Combine(Path.GetDirectoryName(file)!, Path.GetFileNameWithoutExtension(file));
            if (!Directory.Exists(extract)) Directory.CreateDirectory(extract);
            ZipFile.ExtractToDirectory(file, extract, true);
            return extract;
        }

        void Copy(string source, string destination)
        {
            if(!Directory.Exists(destination)) Directory.CreateDirectory(destination);
            foreach (var file in Directory.GetFiles(source, "*"))
            {
                File.Copy(file, Path.Combine( destination, Path.GetFileName(file)), true);
            }
        }

    }
}
