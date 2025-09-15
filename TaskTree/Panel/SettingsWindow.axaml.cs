using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace TaskTree;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();

        // ��С������󻯣��رմ���
        this.FindControl<Button>("Minimize").Click += (s, e) => this.WindowState = WindowState.Minimized;
        this.FindControl<Button>("Maximize").Click += (s, e) =>
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                Maximize.Content = "\ue922";
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                Maximize.Content = "\ue923";
            }
        };
        this.FindControl<Button>("Close").Click += (s, e) =>
        {
            this.Close(); // �˳�����
        };
    }
    private void Form_OnDrag(object? sender, PointerPressedEventArgs e)
    {
        if (e.Pointer.Type == PointerType.Mouse) this.BeginMoveDrag(e);
    }





}