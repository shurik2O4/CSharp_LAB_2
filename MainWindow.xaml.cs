using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Runtime.Serialization;

// Message dialog
// WinForms Message
using Windows.UI.Popups;

namespace Lab2
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        private IntPtr hwnd;

        public MainWindow()
        {
            InitializeComponent();


            // Fix message dialog
            hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        }

        private void TextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (ResultTextBox.Text != "") {
                ResultTextBox.Text = "";
            }
        }

        private async void Calculate_Clicked(object sender, RoutedEventArgs e)
        {
            bool handleExceptions = !(ErrorCatchSelection.SelectedIndex == 0);

            if (TextBoxA.Text == "" || TextBoxB.Text == "" || TextBoxC.Text == "" || TextBoxD.Text == "" ) {
                // Message dialog
                MessageDialog messageDialog = new("One or more inputs are empty", "Error");
                // OK button
                messageDialog.Commands.Add(new UICommand("OK", new UICommandInvokedHandler((_) => { })));
                // Fix message dialog 2
                WinRT.Interop.InitializeWithWindow.Initialize(messageDialog, hwnd);

                await messageDialog.ShowAsync();

                return;
            }

            try
            {
                CalculateResult();
            }
            catch (Exception ex)
            {
                // Pass exception up the stack to, uh.... no one... 
                // Great time to crash I guess. ¯\_(ツ)_/¯
                if (handleExceptions) { throw; }

                MessageDialog messageDialog = new("", "Error");

                UICommand ok_action = new("OK", new UICommandInvokedHandler((_) => { }));
                messageDialog.Commands.Add(ok_action);

                UICommand cancel_action = new("Cancel", new UICommandInvokedHandler((_) => { }));
                messageDialog.Commands.Add(cancel_action);

                // Set default behaviour
                messageDialog.DefaultCommandIndex = 0;
                // Escape behaviour
                messageDialog.CancelCommandIndex = 0;

                switch (ex)
                {
                    case DivideByZeroException:
                        messageDialog.Content = $"Error: division by zero occured while evaluating expression.";
                        break;
                    case InvalidOperationException:
                        messageDialog.Content = $"Error: {ex.Message}";
                        break;
                    case FormatException:
                        messageDialog.Content = "Error: one of the provided variables cannot be converted to number.";
                        break;
                    case ValueError:
                        messageDialog.Content = "Error: negative value in logarithm.";
                        break;

                    default:
                        messageDialog.Content = $"Unknown error occured. Application will be closed.\nError: {ex}";
                        messageDialog.Commands[0].Label = "Close";
                        messageDialog.Commands[0].Invoked = (_) => Environment.Exit(1);
                        break;
                }

                // Fix message dialog 2
                WinRT.Interop.InitializeWithWindow.Initialize(messageDialog, hwnd);

                await messageDialog.ShowAsync();
            }
        }

        private void CalculateResult()
        {
            // Get all values
            int a = int.Parse(TextBoxA.Text);
            int b = int.Parse(TextBoxB.Text);
            int c = int.Parse(TextBoxC.Text);
            int d = int.Parse(TextBoxD.Text);

            // Display result

            if (double.IsNaN(Math.Log10((4 * b) - c)))
            {
                throw new ValueError();
            }

            double result = Math.Round(Math.Log10((4 * b) - c) * a / (b + (c / d) - 1), 4);
            ResultTextBox.Text = result.ToString();

            // Switch visibility if needed
            if (ResultStackPanel.Visibility == Visibility.Collapsed)
            {
                ResultStackPanel.Visibility = Visibility.Visible;
            }
        }

        private void Exit_Clicked(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }

    [Serializable]
    internal class ValueError : Exception
    {
        public ValueError() {}

        public ValueError(string message) : base(message) {}

        public ValueError(string message, Exception innerException) : base(message, innerException) {}

        protected ValueError(SerializationInfo info, StreamingContext context) : base(info, context) {}
    }
}