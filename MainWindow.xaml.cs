using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using System.Diagnostics;

namespace AntiYandex
{
    public partial class MainWindow : Window
    {
        private static readonly string[] domainsToBlock = {
            "127.0.0.1 yandex.ru", "127.0.0.1 www.yandex.ru",
            "127.0.0.1 yandex.ua", "127.0.0.1 yandex.by", "127.0.0.1 yandex.kz",
            "127.0.0.1 browser.yandex.ru", "127.0.0.1 element.yandex.ru", "127.0.0.1 disk.yandex.ru", "127.0.0.1 browser.yandex.com"
        };

        private static readonly string[] yandexFiles = {
            "yandex.exe", "browser.exe", "ya.exe", "yandexdisk.exe", "YandexMod.exe", "YandexEnterpriseBrowser.exe"
        };

        private const string registryPath = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer";
        private const string disallowRunPath = registryPath + @"\DisallowRun";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BlockButton_Click(object sender, RoutedEventArgs e)
        {
            StatusLabel.Text = "Выполняется блокировка...";
            StatusLabel.Foreground = Brushes.Orange;

            try
            {
                BlockYandexDomains();
                BlockYandexExecutables();

                StatusLabel.Text = "Это хуйня больше не работает, я заблокировал всё!";
                StatusLabel.Foreground = Brushes.LimeGreen;
                MessageBox.Show("+1000 риса", "Си Цзиньпин", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (UnauthorizedAccessException)
            {
                StatusLabel.Text = "Брат, запусти с правами Администратора, иначе хуйня не заблокируется!";
                StatusLabel.Foreground = Brushes.Crimson;
                MessageBox.Show("Недостаточно системных прав. Пожалуйста, запустите приложение от имени Администратора.", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Ошибка: {ex.Message}";
                StatusLabel.Foreground = Brushes.Crimson;
            }
        }

        private void UnblockButton_Click(object sender, RoutedEventArgs e)
        {
            StatusLabel.Text = "Выполняется разблокировка...";
            StatusLabel.Foreground = Brushes.Orange;

            try
            {
                UnblockYandexDomains();
                UnblockYandexExecutables();

                StatusLabel.Text = "Это хуйня работает, я разблокировал всё!";
                StatusLabel.Foreground = Brushes.LimeGreen;
                MessageBox.Show("-1000 Риса", "Си Цзиньпин", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (UnauthorizedAccessException)
            {
                StatusLabel.Text = "Брат, запусти с правами Администратора, иначе не снимется!";
                StatusLabel.Foreground = Brushes.Crimson;
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Ошибка: {ex.Message}";
                StatusLabel.Foreground = Brushes.Crimson;
            }
        }

        private void BlockYandexDomains()
        {
            string hostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");
            string currentHosts = File.ReadAllText(hostsPath);
            using (StreamWriter sw = File.AppendText(hostsPath))
            {
                foreach (string domain in domainsToBlock)
                {
                    if (!currentHosts.Contains(domain))
                    {
                        sw.WriteLine(domain);
                    }
                }
            }
        }

        private void UnblockYandexDomains()
        {
            string hostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");
            var lines = File.ReadAllLines(hostsPath)
                             .Where(line => !domainsToBlock.Contains(line.Trim()))
                             .ToArray();
            File.WriteAllLines(hostsPath, lines);
        }

        private void BlockYandexExecutables()
        {
            using (RegistryKey explorerKey = Registry.CurrentUser.CreateSubKey(registryPath))
            {
                explorerKey.SetValue("DisallowRun", 1, RegistryValueKind.DWord);
            }

            using (RegistryKey disallowKey = Registry.CurrentUser.CreateSubKey(disallowRunPath))
            {
                int index = 1;
                foreach (string exe in yandexFiles)
                {
                    disallowKey.SetValue(index.ToString(), exe);
                    index++;
                }
            }
        }

                private void TrafficLightButton_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://vpn.maximkatz.com");
        }

        private void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://ko-fi.com/jasonleee");
        }

        private void GitHubButton_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://github.com");
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть ссылку: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UnblockYandexExecutables()
        {
            using (RegistryKey explorerKey = Registry.CurrentUser.OpenSubKey(registryPath, true))
            {
                explorerKey?.DeleteValue("DisallowRun", false);
            }

            using (RegistryKey disallowKey = Registry.CurrentUser.OpenSubKey(disallowRunPath, true))
            {
                if (disallowKey != null)
                {
                    foreach (string name in disallowKey.GetValueNames())
                    {
                        disallowKey.DeleteValue(name, false);
                    }
                }
            }

            // Удаляем сам подключ DisallowRun, если он пуст
            using (RegistryKey explorerKey = Registry.CurrentUser.OpenSubKey(registryPath, true))
            {
                try { explorerKey?.DeleteSubKey("DisallowRun", false); } catch { }
            }
        }
    }
}