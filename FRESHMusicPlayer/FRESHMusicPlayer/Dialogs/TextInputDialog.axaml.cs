using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FRESHMusicPlayer;

public partial class TextInputDialog : Window
{
    public TextInputDialog()
    {
        InitializeComponent();
    }

    public TextInputDialog(string prompt, string? initialText = null)
    {
        InitializeComponent();
        PromptTextBlock.Text = prompt;
        InputTextBox.Text = initialText;
        InputTextBox.Focus();
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(InputTextBox.Text);


    private void Button_Click_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(null);
}