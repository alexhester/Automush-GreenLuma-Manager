using Automush.Pacman;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;

namespace Automush;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class AutomushWindow : Window
{
    private string? automushFilePath;

    private string appIdsPath = "\\appIds.txt", configPath = "\\config.ini", steamDir = "C:\\Program Files (x86)\\Steam";

    private readonly string separator = "&/?*";

    private readonly int maxAppIds = 146;

    public AutomushWindow()
    {
        Loaded += Automush_Loaded;
        InitializeComponent();
    }

    /// <summary>
    /// Called after window is finished loading. Sets current directory
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="Exception"></exception>
    private void Automush_Loaded(object sender, RoutedEventArgs e)
    {
        Task.Run(Logging.Clear);
        // Set current directory to exe directory
        string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); // GetExecutingAssembly().Location can return null
        if (path != null)
        {
            automushFilePath = path;
            Directory.SetCurrentDirectory(automushFilePath);
        }
        else
            throw new Exception("Automush File Path returned null");

        // Find/create config.ini and read/write the file path to DLLInjector
        configPath = automushFilePath + configPath;
        appIdsPath = automushFilePath + appIdsPath;
        if (File.Exists(configPath))
            steamDir = File.ReadAllText(configPath);
        else
            File.WriteAllText(configPath, steamDir);

        // Update UI on the UI thread
        Dispatcher.Invoke(() => filePathTextBox.Text = steamDir);

        Task.Run(ReadAppIDs);
    }

    /// <summary>
    /// Reads each line from appIds.txt and adds them to listBoxes
    /// </summary>
    private async Task ReadAppIDs()
    {
        if (!File.Exists(appIdsPath))
        {
            using (File.Create(appIdsPath)) { } // Create and immediately close the file
        }
        else
        {
            string[] data = await File.ReadAllLinesAsync(appIdsPath);

            for (int i = 0; i < data.Length; i++)
            {
                string[] split = data[i].Split(separator, StringSplitOptions.None); // Split app Ids and game names using separator: &/?*

                string gameName = string.Empty;
                string appId = string.Empty;

                if (split.Length == 2)
                {
                    if (await IsNumbersOnly(split[0]))
                    {
                        appId = split[0];
                        gameName = split[1];
                    }
                    else
                    {
                        appId = split[1];
                        gameName = split[0];
                    }
                }
                else if (split.Length == 1)
                {
                    if (await IsNumbersOnly(split[0]))
                    {
                        appId = split[0];
                    }
                    else
                    {
                        await Logging.Log("Unknown string in appIds.txt:");
                        await Logging.Log(split[0]); // Log the problematic line
                        continue; // Skip processing this line
                    }
                }
                else
                {
                    await Logging.Log("Unknown string in appIds.txt:");
                    foreach (string line in split)
                        await Logging.Log(line); // Log each line of the problematic split
                    continue; // Skip processing this line
                }

                if (string.IsNullOrEmpty(gameName))
                {
                    try
                    {
                        gameName = await GetGameTitleFromSteam(appId) ?? string.Empty;
                    }
                    catch (Exception ex)
                    {
                        await Logging.Log($"Failed to fetch game title for App ID {appId}: {ex.Message}");
                        continue; // Skip to the next iteration if fetching fails
                    }
                }

                // Update UI on the UI thread
                Dispatcher.Invoke(() =>
                {
                    gameNameListBox.Items.Add(gameName);
                    appIdListBox.Items.Add(appId);
                });
            }
            await SortAppIDs();
        }
        // Update count
        GetAppIdsCount();
    }

    /// <summary>
    /// Returns true if string is only numbers and has a length > 0
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static async Task<bool> IsNumbersOnly(string str)
    {
        return await Task.Run(() =>
        {
            if (str.Length == 0)
                return false;
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }
            return true;
        });
    }

    /// <summary>
    /// Updates the count label and changes label background color if you are using too many app ids
    /// </summary>
    private void GetAppIdsCount()
    {
        int appIdCount = appIdListBox.Items.Count;
        string countText = "Using " + appIdCount + " app IDs out of " + maxAppIds;

        countLabel.Content = countText;
        if (appIdCount > maxAppIds)
            countLabel.Background = Brushes.Red;
        else
            countLabel.Background = Brushes.Transparent;
    }

    private async Task SortAppIDs()
    {
        Dictionary<string, string> dict = [];
        Dispatcher.Invoke(() =>
        {
            for (int i = 0; i < appIdListBox.Items.Count; i++)
                dict.Add(appIdListBox.Items[i].ToString() ?? string.Empty, gameNameListBox.Items[i].ToString() ?? string.Empty);
            gameNameListBox.Items.Clear();
            appIdListBox.Items.Clear();

            // Create a list of key-value pairs sorted by value
            List<KeyValuePair<string, string>> sortedList = [.. dict.OrderBy(kvp => kvp.Value, StringComparer.OrdinalIgnoreCase)];

            // Convert sorted list back to dictionary
            dict = sortedList.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (KeyValuePair<string, string> pair in dict)
            {
                gameNameListBox.Items.Add(pair.Value);
                appIdListBox.Items.Add(pair.Key);
            }
        });
        await WriteAppIDs();
    }

    private static async Task<string?> GetGameTitleFromSteam(string appId)
    {
        await Logging.Log($"Searching for title for App ID: {appId}");
        string url = $"https://store.steampowered.com/app/{appId}/";

        using HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            await Logging.Log($"Failed to fetch data from Steam store for app ID {appId}. Status code: {response.StatusCode}");
            return null;
        }

        string content = await response.Content.ReadAsStringAsync();
        HtmlDocument document = new();
        document.LoadHtml(content);

        HtmlNode? gameNameNode = document.DocumentNode.SelectSingleNode("//div[@class='apphub_HomeHeaderContent']//div[@id='appHubAppName']");
        string? gameName = gameNameNode?.InnerText.Trim() ?? null;

        if (string.IsNullOrWhiteSpace(gameName))
        {
            // Fallback to <title> if <div class="apphub_AppName"> is not found or empty
            gameNameNode = document.DocumentNode.SelectSingleNode("//title");
            string pattern = @"^Save \d+% on";
            gameName = Regex.Replace(gameNameNode.InnerText.Replace("on Steam", ""), pattern, "") ?? null;
            gameName = gameName?.Trim() ?? null;
            if (gameName != null && gameName.Contains("Welcome to Steam"))
                gameName = null;
        }

        if (string.IsNullOrWhiteSpace(gameName))
        {
            await Logging.Log($"Failed to find game title for App ID: {appId}");
            return null;
        }
        else
        {
            await Logging.Log($"Found game title: {gameName}");
            return gameName;
        }
    }

    /// <summary>
    /// Gets App ID and game name from text boxes and adds to listBoxes, then saves listBox contents to appIds.txt
    /// </summary>
    private async Task AddAppID()
    {
        string newAppId = string.Empty;
        Dispatcher.Invoke(() => newAppId = appIdTextBox.Text);

        if (newAppId == null || newAppId == "")
            MessageBox.Show("No App ID entered");

        else if (!newAppId.All(Char.IsAsciiDigit))
            MessageBox.Show("App ID must be numbers only");

        else
        {
            string? newGameName = null;
            // Update UI on the UI thread
            Dispatcher.Invoke(() =>
            {
                appIdListBox.Items.Add(newAppId);
                newGameName = gameNameTextBox.Text;
            });

            if (string.IsNullOrEmpty(newGameName))
            {
                newGameName = await GetGameTitleFromSteam(newAppId);
            }
            // Update UI on the UI thread
            Dispatcher.Invoke(() =>
            {
                if (string.IsNullOrEmpty(newGameName))
                    gameNameListBox.Items.Add("");
                else
                    gameNameListBox.Items.Add(newGameName);

                appIdTextBox.Clear();
                gameNameTextBox.Clear();
            });

            await SortAppIDs();
        }
    }

    /// <summary>
    /// Deletes an app Id and game name from the listBoxes then saves the listBox contents to appIds.txt
    /// </summary>
    private async Task RemoveAppID()
    {
        Dispatcher.Invoke(() =>
        {
            int index;
            if (appIdListBox.SelectedItems.Count > 0)
                index = appIdListBox.SelectedIndex;
            else if (gameNameListBox.SelectedItems.Count > 0)
                index = gameNameListBox.SelectedIndex;
            else
                return;

            appIdListBox.Items.RemoveAt(index);
            gameNameListBox.Items.RemoveAt(index);
        });
        
        await WriteAppIDs();
    }

    private async Task EditAppID()
    {
        Dispatcher.Invoke(() =>
        {
            if (appIdListBox.SelectedIndex == -1 || (string.IsNullOrEmpty(gameNameTextBox.Text) && string.IsNullOrEmpty(appIdTextBox.Text))) return;

            string appId = string.IsNullOrEmpty(appIdTextBox.Text) ? appIdListBox.SelectedItem.ToString()! : appIdTextBox.Text;
            string gameName = string.IsNullOrEmpty(gameNameTextBox.Text) ? gameNameListBox.SelectedItem.ToString()! : gameNameTextBox.Text;
            int index = gameNameListBox.SelectedIndex;
            gameNameListBox.Items.RemoveAt(index);
            appIdListBox.Items.RemoveAt(index);

            gameNameListBox.Items.Add(gameName);
            appIdListBox.Items.Add(appId);
            gameNameTextBox.Clear();
            appIdTextBox.Clear();
        });

        await SortAppIDs();
    }

    /// <summary>
    /// Writes App IDs and game names to appIds.txt
    /// </summary>
    private async Task WriteAppIDs()
    {
        List<string> data = [];

        // Update UI on the UI thread
        Dispatcher.Invoke(() =>
        {
            for (int i = 0; i < appIdListBox.Items.Count; i++)
                data.Add(gameNameListBox.Items[i].ToString() + separator + appIdListBox.Items[i].ToString());

            GetAppIdsCount();
        });

        await File.WriteAllLinesAsync(appIdsPath, data);
    }

    /// <summary>
    /// Starts OpenFileDialog and sets TextBox to show file path
    /// </summary>
    private async Task BrowseFiles()
    {
        OpenFolderDialog openFolderDialog = new();

        Nullable<bool> result = openFolderDialog.ShowDialog();

        if (result == true)
        {
            string folder = openFolderDialog.FolderName;
            try
            {
                // Update UI on the UI thread
                Dispatcher.Invoke(() => filePathTextBox.Text = folder);
                await File.WriteAllTextAsync(configPath, folder);
            }
            catch (IOException)
            {
            }
        }
        Console.WriteLine(result);
    }

    /// <summary>
    /// Checks if a process is currently running
    /// </summary>
    /// <param name="processName"></param>
    /// <returns></returns>
    private static async Task<bool> IsProcessRunning(string processName)
    {
        return await Task.Run(() =>
        {
            Process[] process = Process.GetProcessesByName(processName);
            return process.Length > 0;
        });
    }

    /// <summary>
    /// Checks that Steam and DLLInjector are not currently running, then launches DLLInjector
    /// </summary>
    private void RunButton_Click(object sender, RoutedEventArgs e)
    {
        if (Task.Run(() => IsProcessRunning("DLLInjector")).GetAwaiter().GetResult())
        {
            MessageBox.Show("DLLInjector is already running!");
            Console.WriteLine("DLLInjector.exe is running");
            return;
        }
        else if (Task.Run(() => IsProcessRunning("Steam")).GetAwaiter().GetResult())
        {
            MessageBox.Show("Close steam before running DLLInjector!");
            Console.WriteLine("steam.exe is running");
            return;
        }

        Task.Run(LaunchDLLInjector);
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e) => Task.Run(BrowseFiles);

    private void AddButton_Click(object sender, RoutedEventArgs e) => Task.Run(AddAppID);

    private void RemoveButton_Click(object sender, RoutedEventArgs e) => Task.Run(RemoveAppID);

    private void EditButton_Click(object sender, RoutedEventArgs e) => Task.Run(EditAppID);

    /// <summary>
    /// Syncs gameNameListBox scroll bar with appIdListBox scroll bar
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AppIDListBox_ScrollChanged(object sender, EventArgs e)
    {
        if (GetDescendantByType(gameNameListBox, typeof(ScrollViewer)) is ScrollViewer gameNameScrollViewer
            && GetDescendantByType(appIdListBox, typeof(ScrollViewer)) is ScrollViewer appIdScrollViewer)
                gameNameScrollViewer.ScrollToVerticalOffset(appIdScrollViewer.VerticalOffset);
    }

    /// <summary>
    /// Syncs appIdListBox scroll bar with gameNameListBox scroll bar
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GameNameListBox_ScrollChanged(object sender, EventArgs e)
    {
        if (GetDescendantByType(appIdListBox, typeof(ScrollViewer)) is ScrollViewer appIdScrollViewer
            && GetDescendantByType(gameNameListBox, typeof(ScrollViewer)) is ScrollViewer gameNameScrollViewer)
                appIdScrollViewer.ScrollToVerticalOffset(gameNameScrollViewer.VerticalOffset);
    }

    /// <summary>
    /// Syncs gameNameListBox selection with appIdListBox selection
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AppIdListBox_SelectedIndexChanged(object sender, SelectionChangedEventArgs e) => gameNameListBox.SelectedIndex = appIdListBox.SelectedIndex;

    /// <summary>
    /// Syncs appIdListBox selection with gameNameListBox selection
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GameNameListBox_SelectedIndexChanged(object sender, SelectionChangedEventArgs e) => appIdListBox.SelectedIndex = gameNameListBox.SelectedIndex;

    /// <summary>
    /// Use to get the ScrollViewer from a ListBox
    /// </summary>
    /// <param name="element"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Visual? GetDescendantByType(Visual? element, Type type)
    {
        if (element == null)
        {
            return null;
        }
        if (element.GetType() == type)
        {
            return element;
        }
        Visual? foundElement = null;
        if (element is FrameworkElement)
        {
            if (element is FrameworkElement frameworkElement)
                frameworkElement.ApplyTemplate();
        }
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
        {
            Visual? visual = VisualTreeHelper.GetChild(element, i) as Visual;
            foundElement = GetDescendantByType(visual, type);
            if (foundElement != null)
            {
                break;
            }
        }
        return foundElement;
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        PacmanWindow pacmanWindow = new();
        pacmanWindow.Show();
    }

    /// <summary>
    /// Launches DLLInjector and uses kbinput.dll to input app Ids, then closes Automush.NET
    /// </summary>
    private async Task LaunchDLLInjector()
    {
        if (appIdListBox.Items.Count == 0)
        {
            MessageBoxResult result = MessageBox.Show("You don't have any app IDs\nContinue anyways?", "No App IDs", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No) return;
        }
        string _steamDir = await File.ReadAllTextAsync(configPath);
        string dllPath = Path.Combine(_steamDir, "DLLInjector.exe");
        string appListDir = Path.Combine(_steamDir, "AppList");

        if (Directory.Exists(appListDir))
            foreach (string file in Directory.EnumerateFiles(appListDir))
                File.Delete(file);

        await Logging.Log("Entering app Ids");
        int appIdCount = appIdListBox.Items.Count;

        for (int i = 0; i < appIdCount; i++)
        {
            string appId = appIdListBox.Items[i].ToString() ?? throw new Exception("App ID returned null");
            string currentFile = Path.Combine(appListDir, $"{i}.txt");
            await File.WriteAllTextAsync(currentFile, appId);
            await Logging.Log($"Entering {appId} in {currentFile}");
        }

        Thread.Sleep(50);

        await Logging.Log("Starting DLLInjector");
        Process.Start(dllPath);

        IntPtr hWnd = IntPtr.Zero;

        while (hWnd == IntPtr.Zero)
        {
            hWnd = Messaging.FindWindow(null, "GreenLuma 2024");
            Thread.Sleep(50);
        }

        Messaging.SetForeground(hWnd);

        await Logging.Log("Found GreenLuma 2024");
        Thread.Sleep(50);

        Messaging.InputReturn();

        await Logging.Log("Exiting");
        Close();
    }
}

internal static class Logging
{
    private readonly static string path = "./log.txt";
    internal static async Task Log(string message)
    {
        if (!File.Exists(path))
            File.Create(path);

        await File.AppendAllTextAsync(path, $"{DateTime.Now} : {message}\n");
    }

    internal static async Task Clear() => await File.WriteAllTextAsync(path, null);
}