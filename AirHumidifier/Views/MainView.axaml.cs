using Avalonia.Controls;
using Avalonia.Threading;


namespace AirHumidifier.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void TextInput_GotFocus(object? sender, Avalonia.Input.GotFocusEventArgs e)
    {
        var textBox = sender as TextBox;

        if (textBox == null)
        {
            return;
        }

        // Defer the SelectAll() method call using the dispatcher
        Dispatcher.UIThread.Post(() =>
        {
            textBox.SelectAll();
        });
    }
}
