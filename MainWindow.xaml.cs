using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LAB_7 {
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : WindowEx {

        private FileStream inputFileStream;
        private FileStream outputFileStream;

        public MainWindow() {
            InitializeComponent();
#if DEBUG
            AppBarButton a = new() { Label="[Debug] Save input", IsTabStop=false, MinWidth=120 };
            a.Click += (source, e) => {
                if (Utils.CheckInput(InputTextBox.Text)) {
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\debug_input.lab7";
                    FileStream fs = new(path, FileMode.OpenOrCreate);
                    Utils.WriteBinFile(fs, InputTextBox.Text);
                    fs.Dispose();
                }
            };
            CmdBar.SecondaryCommands.Add(a);
#endif
        }

        private void ProcessData() {
            // Useless variable
            string input = InputTextBox.Text;
            // Find minimal
            double? min = null;
            foreach (double d in Regex.Matches(input, "(?:-\\d*(\\.|,)\\d+)|(?:-\\d+(\\.|,)?\\d*)", RegexOptions.ExplicitCapture).Select((m) => double.Parse(m.Value))) {
                if (!min.HasValue || min > d) min = d;
            }

            // Replacing all numbers < -1 with found minimal. No array & no loop.
            string result = Regex.Replace(InputTextBox.Text, "-0*([1-9])\\d*\\.\\d+", min.ToString());

            OutputTextBox.Text = result;

            StringOutputGrid.Visibility = Visibility.Visible;
            SaveToFileButton.IsEnabled = true;
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (StringOutputGrid.Visibility == Visibility.Visible) StringOutputGrid.Visibility = Visibility.Collapsed;
            if (SaveToFileButton.IsEnabled) SaveToFileButton.IsEnabled = false;

            if (InputTextBox.Text.Length == 0) {
                InputTextBoxBorder.BorderBrush = null;
                ProcessDataButton.IsEnabled = false;
                // Don't run code below, because check will fail and display red border
                return;
            }

            if (Utils.CheckInput(InputTextBox.Text)) {
                InputTextBoxBorder.BorderBrush = new SolidColorBrush(Colors.ForestGreen);
                ProcessDataButton.IsEnabled = true;
            }
            else {
                InputTextBoxBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                ProcessDataButton.IsEnabled = false;
            }
        }

        private void InputTextBox_KeyDown(object sender, KeyRoutedEventArgs e) {
            if (e.Key == Windows.System.VirtualKey.Enter && Utils.CheckInput(InputTextBox.Text)) {
                ProcessData();
            }
        }

        // Ignore any keypress on result box
        private void OutputTextBox_PreviewKeyDown(object sender, KeyRoutedEventArgs e) => e.Handled = true;

        private void SwitchInputVisibility_Click(object sender, RoutedEventArgs e) => SwitchInputVisibility();

        private void SwitchInputVisibility() {
            if (StringInputPanel.Visibility == Visibility.Visible) {
                StringInputPanel.Visibility = Visibility.Collapsed;
                OutputTextBoxRow.Height = new GridLength(341);
            }
            else {
                StringInputPanel.Visibility = Visibility.Visible;
                OutputTextBoxRow.Height = new GridLength(235);
            }
        }

        private void ProcessData_Click(object sender, RoutedEventArgs e) => ProcessData();

        private async void OpenFileButton_Click(object sender, RoutedEventArgs e) {
            // Create the file picker
            FileOpenPicker filePicker = new() { SuggestedStartLocation = PickerLocationId.Desktop, ViewMode = PickerViewMode.List, FileTypeFilter = { ".lab7" } };

            // Associate the HWND with the file picker
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

            StorageFile file = await filePicker.PickSingleFileAsync();

            if (file != null && file.IsAvailable) {
                try {
                    //var t = 1 - 1;
                    //if (inputFileStream != null) _ = 1/t;
                    if (inputFileStream != null) {
                        inputFileStream.Dispose();
                    }

                    inputFileStream = new(file.Path, FileMode.Open, FileAccess.Read);

                    // TODO: open file and read all values in Utils
                    string res = "";
                    foreach (double v in Utils.ReadBinFile(inputFileStream)) {
                        if (res != "") res += " ";
                        res += v.ToString();
                    }
                    InputTextBox.Text = res;

                    inputFileStream.Dispose();

                    // Reveal input box if it's hidden
                    if (StringInputPanel.Visibility == Visibility.Collapsed) SwitchInputVisibility();
                }
                catch (Exception ex) {
                    ErrorTip.Subtitle = $"Unable to open/read file: {ex.Message}";
                    ErrorTip.IsOpen = true;
                    return;
                }
            }
        }

        private void RandomInputButton_Click(object sender, RoutedEventArgs e) {
            InputTextBox.Text = Utils.RandomInput();

            // Show input if it's hidden
            if (StringInputPanel.Visibility == Visibility.Collapsed) SwitchInputVisibility();
        }

        private async void SaveToFileButton_Click(object sender, RoutedEventArgs e) {
            // Create the file picker
            FileSavePicker filePicker = new() { SuggestedStartLocation = PickerLocationId.Desktop, SuggestedFileName = "result" };
            // Add file type to save as
            filePicker.FileTypeChoices.Add("Result", new List<string>() { ".lab7" });

            // Associate the HWND with the file picker
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

            StorageFile file = await filePicker.PickSaveFileAsync();

            if (file != null && file.IsAvailable) {
                try {
                    outputFileStream = new(file.Path, FileMode.OpenOrCreate, FileAccess.Write);
                    Utils.WriteBinFile(outputFileStream, OutputTextBox.Text);
                    outputFileStream.Dispose();
                }
                catch (Exception ex) {
                    ErrorTip.Subtitle = $"Unable to open/write file: {ex.Message}";
                    ErrorTip.IsOpen = true;
                    return;
                }
            }
        }

        private void ExitApplication(object sender, RoutedEventArgs e) {
            Environment.Exit(0);
        }

        // Show/hide tip
        private void InputTextBox_FocusEngaged(object sender, RoutedEventArgs e) => EnterTipTextBlock.Opacity = 1.0;
        private void InputTextBox_FocusDisengaged(object sender, RoutedEventArgs e) => EnterTipTextBlock.Opacity = 0.0;

        private bool seven = true;
        private void Hyperlink_Click(Hyperlink sender, HyperlinkClickEventArgs args) {
            if (seven)
                sender.Inlines[0] = new Run() { Text = "🤔" };
            else
                sender.Inlines[0] = new Run() { Text = "№7" };

            seven = !seven;
        }
    }
}
