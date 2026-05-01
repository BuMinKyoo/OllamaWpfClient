using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OllamaWpfClient.Behaviors
{
    public static class EnterKey
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(EnterKey),
                new PropertyMetadata(null, OnCommandChanged));

        public static ICommand? GetCommand(DependencyObject obj)
        {
            return (ICommand?)obj.GetValue(CommandProperty);
        }

        public static void SetCommand(DependencyObject obj, ICommand? value)
        {
            obj.SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty AllowShiftForNewlineProperty =
            DependencyProperty.RegisterAttached(
                "AllowShiftForNewline",
                typeof(bool),
                typeof(EnterKey),
                new PropertyMetadata(false));

        public static bool GetAllowShiftForNewline(DependencyObject obj)
        {
            return (bool)obj.GetValue(AllowShiftForNewlineProperty);
        }

        public static void SetAllowShiftForNewline(DependencyObject obj, bool value)
        {
            obj.SetValue(AllowShiftForNewlineProperty, value);
        }

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBox? textBox = d as TextBox;
            if (textBox == null)
            {
                return;
            }

            textBox.PreviewKeyDown -= OnPreviewKeyDown;
            if (e.NewValue != null)
            {
                textBox.PreviewKeyDown += OnPreviewKeyDown;
            }
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            TextBox textBox = (TextBox)sender;

            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (!GetAllowShiftForNewline(textBox))
                {
                    return;
                }

                int caretIndex = textBox.CaretIndex;
                textBox.Text = textBox.Text.Insert(caretIndex, "\n");
                textBox.CaretIndex = caretIndex + 1;
                e.Handled = true;
                return;
            }

            ICommand? command = GetCommand(textBox);
            if (command != null && command.CanExecute(null))
            {
                command.Execute(null);
                e.Handled = true;
            }
        }
    }
}
