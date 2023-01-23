using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            new Thread(CheckForUpdates).Start();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CheckForUpdates()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appData, "./Report Helper");

            if (!IsInstalled(appFolder))
                Install(appFolder).Wait();
            else
                Update();
        }

        private bool IsInstalled(string appFolder)
        {
            if (!Directory.Exists(appFolder)) return false;

            string path = Path.Combine(appFolder, "./app");
            if (!Directory.Exists(path)) return false;

            return true;
        }

        private void Update(string appFolder)
        {

        }

        private async Task Install(string appFolder)
        {
            if (Directory.Exists(appFolder))
                Directory.Delete(appFolder, true);
            Directory.CreateDirectory(appFolder);

            var json = await GetJson("https://api.github.com/repos/Report-Helper/updater/releases/latest");
            var fileToDownload = json.AsObject()["assets"].AsArray()[0].AsObject();
            Trace.WriteLine(fileToDownload["browser_download_url"].AsValue().ToString());

            string tempPath = Path.Combine(appFolder, "./temp");
            Directory.CreateDirectory(tempPath);

            Download(fileToDownload["browser_download_url"].AsValue().ToString(), Path.Join(tempPath, "download.zip"));
            ZipFile.ExtractToDirectory(Path.Join(tempPath, "download.zip"), Path.Join(tempPath, "./unpacked"));
            
            var files = Directory.GetFiles(Path.Join(tempPath, "./unpacked"));
            foreach (string file in files)
            {
                var info = new FileInfo(file);
                info.MoveTo(Path.Combine(appFolder, info.Name));
            }
            Directory.Delete(Path.Combine(tempPath, "./unpacked"), true);
            File.Delete(Path.Join(tempPath, "download.zip"));

            Update();
        }

        private async Task<JsonNode> GetJson(string url)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "report-helper-update-checker 1.0.0");

            var res = await client.GetAsync(url);
            res.EnsureSuccessStatusCode();
            var rawJson = await res.Content.ReadAsStringAsync();
            var json = JsonObject.Parse(rawJson);

            return json;
        }

        private void Download(string url, string outputPath)
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(new Uri(url), outputPath);
            }
            Trace.WriteLine("Downloaded");
        }
    }
}
