using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Automush
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class AutomushWindow : Window
    {
        private string? automushFilePath;

        private string appIdsPath = "\\appIds.txt", configPath = "\\config.ini", dllInjectorFilePath = "C:\\Program Files (x86)\\Steam\\DLLInjector.exe";

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
                dllInjectorFilePath = File.ReadAllText(configPath);
            else
                File.WriteAllText(configPath, dllInjectorFilePath);
            filePathTextBox.Text = dllInjectorFilePath;

            ReadAppIDs();
        }

        /// <summary>
        /// Reads each line from appIds.txt and adds them to listBoxes
        /// </summary>
        private void ReadAppIDs()
        {
            if (!File.Exists(appIdsPath))
                File.Create(appIdsPath);
            else
            {
                string[] data = File.ReadAllLines(appIdsPath);
                for (int i = 0; i < data.Length; i++)
                {
                    string[] split = data[i].Split(separator, StringSplitOptions.None); // Split app Ids and game names using separator: &/?*

                    // Add app Ids and game names to their repective listBoxes
                    if (IsNumbersOnly(split[0]))
                    {
                        gameNameListBox.Items.Add(split[1]);
                        appIdListBox.Items.Add(split[0]);
                    }
                    else
                    {
                        gameNameListBox.Items.Add(split[0]);
                        appIdListBox.Items.Add(split[1]);
                    }

                }
            }
            GetAppIdsCount();
        }

        /// <summary>
        /// Returns true if string is only numbers and has a length > 0
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private bool IsNumbersOnly(string str)
        {
            if (str.Length == 0)
                return false;
            foreach(char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }
            return true;
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

        /// <summary>
        /// Gets App ID and game name from text boxes and adds to listBoxes, then saves listBox contents to appIds.txt
        /// </summary>
        private void AddAppID()
        {
            string newAppId = appIdTextBox.Text;

            if (newAppId == null || newAppId == "")
                MessageBox.Show("No App ID entered");

            else if (!newAppId.All(Char.IsAsciiDigit))
                MessageBox.Show("App ID must be numbers only");

            else
            {
                appIdListBox.Items.Add(newAppId);
                string newGameName = gameNameTextBox.Text;

                // Checks that Game Name is only letters, digits, and white space
                //if (newGameName.All(c => Char.IsAsciiLetterOrDigit(c) || Char.IsWhiteSpace(c)))

                if (newGameName == null || newGameName == "")
                {
                    newGameName = "null";
                    gameNameListBox.Items.Add("");
                }
                else
                    gameNameListBox.Items.Add(newGameName);

                appIdTextBox.Clear();
                gameNameTextBox.Clear();

                WriteAppIDs();
            }
        }

        /// <summary>
        /// Deletes an app Id and game name from the listBoxes then saves the listBox contents to appIds.txt
        /// </summary>
        private void RemoveAppID()
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

            WriteAppIDs();
        }

        /// <summary>
        /// Writes App IDs and game names to appIds.txt
        /// </summary>
        private void WriteAppIDs()
        {
            List<string> data = [];

            for (int i = 0; i < appIdListBox.Items.Count; i++)
                data.Add(gameNameListBox.Items[i].ToString() + separator + appIdListBox.Items[i].ToString());

            File.WriteAllLines(appIdsPath, data);

            GetAppIdsCount();
        }

        /// <summary>
        /// Starts OpenFileDialog and sets TextBox to show file path
        /// </summary>
        private void BrowseFiles()
        {
            OpenFileDialog openFileDialog = new();

            Nullable<bool> result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string file = openFileDialog.FileName;
                try
                {
                    filePathTextBox.Text = file;
                    File.WriteAllText(configPath, file);
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
        private static bool IsProcessRunning(string processName)
        {
            Process[] process = Process.GetProcessesByName(processName);
            return process.Length > 0;
        }

        /// <summary>
        /// Checks that Steam and DLLInjector are not currently running, then launches DLLInjector
        /// </summary>
        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessRunning("DLLInjector"))
            {
                MessageBox.Show("DLLInjector is already running!");
                Console.WriteLine("DLLInjector.exe is running");
                return;
            }
            else if (IsProcessRunning("Steam"))
            {
                MessageBox.Show("Close steam before running DLLInjector!");
                Console.WriteLine("steam.exe is running");
                return;
            }
            else
                LaunchDLLInjector();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e) => BrowseFiles();

        private void AddButton_Click(object sender, RoutedEventArgs e) => AddAppID();

        private void RemoveButton_Click(object sender, RoutedEventArgs e) => RemoveAppID();

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
                FrameworkElement? frameworkElement = element as FrameworkElement;
                if (frameworkElement != null)
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

        /// <summary>
        /// Launches DLLInjector and uses kbinput.dll to input app Ids, then closes Automush.NET
        /// </summary>
        private async void LaunchDLLInjector()
        {
            if (appIdListBox.Items.Count == 0)
            {
                MessageBox.Show("You don't have any app IDs");
                return;
            }
            string dllPath = File.ReadAllText(configPath);

            Process.Start(dllPath);

            // Wait for either GreenLuma or app Id input to open
            IntPtr hwnd = IntPtr.Zero;
            bool foundGreenLuma = false;
            while (hwnd == IntPtr.Zero)
            {
                foreach (Process process in Process.GetProcesses())
                {
                    if (process.MainWindowTitle.Contains("GreenLuma"))
                    {
                        hwnd = process.MainWindowHandle;
                        foundGreenLuma = true;
                        await Task.Delay(250);
                    }
                    else if (process.MainWindowTitle.Contains("\\Steam\\Steam.exe"))
                    {
                        hwnd = process.MainWindowHandle;
                        foundGreenLuma = false;
                        await Task.Delay(250);
                    }
                }
            }

            // Only do this if the window found was GreenLuma
            if (foundGreenLuma)
            {
                KBInput.InputLeft();

                await Task.Delay(50);

                KBInput.InputReturn();
            }

            //Wait
            await Task.Delay(250);

            int appIdCount = appIdListBox.Items.Count;

            // Input the total games to add
            string lineCountStr = appIdCount.ToString();
            for (int i = 0; i < lineCountStr.Length; i++)
            {
                char c = lineCountStr[i];
                KBInput.InputChar(c);
                await Task.Delay(5);
            }

            // Input the enter key
            KBInput.InputReturn();

            // For each appId
            for (int i = 0; i < appIdCount; i++)
            {
                string appId = appIdListBox.Items[i].ToString() ?? throw new Exception("App ID returned null");

                // For each character in appId
                for (int j = 0; j < appId.Length; j++)
                {
                    char c = appId[j];
                    KBInput.InputChar(c);
                }

                // Input the enter key after each appId
                KBInput.InputReturn();
            }

            Close();
        }
    }

    class KBInput
    {
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
        [DllImport("KBInput.dll", CallingConvention = CallingConvention.StdCall)] public static extern void InputChar(int c);
        [DllImport("KBInput.dll", CallingConvention = CallingConvention.StdCall)] public static extern void InputReturn();
        [DllImport("KBInput.dll", CallingConvention = CallingConvention.StdCall)] public static extern void InputLeft();
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
    }
}