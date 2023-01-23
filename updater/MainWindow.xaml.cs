using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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

            CheckForUpdates();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CheckForUpdates()
        {
            StatusText.Text = "Проверка";
            DetailsText.Text = "Проверяем на наличие обновлений";
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

            string path = Path.Combine(appFolder, "/app");
            if (!Directory.Exists(path)) return false;

            return true;
        }

        private void Update()
        {
            StatusText.Text = "Обновление";
            DetailsText.Text = "Не выключайте компьютер и интернет";
        }

        private async Task Install(string appFolder)
        {
            StatusText.Text = "Установка";
            DetailsText.Text = "Не выключайте компьютер и интернет";

            if (Directory.Exists(appFolder))
                Directory.Delete(appFolder, true);
            Console.Write(appFolder);
            Directory.CreateDirectory(appFolder);

            var client = new HttpClient();
            var res = await client.GetAsync("https://api.github.com/repos/Report-Helper/updater/releases/latest");
        }
    }
}
