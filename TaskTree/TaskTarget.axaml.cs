using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TaskTree;

public partial class TaskTarget : UserControl
{
    public int ListSerial;

    public TaskTarget()
    {
        InitializeComponent();
    }
    // ���/ȡ�� ��ť
    private void Click_Submit(object? sender, RoutedEventArgs e)
    {
        MainWindow.instance.OpenTargetPlane(ListSerial);
    }
    // ɾ�� ��ť
    private void Click_Del (object? sender, RoutedEventArgs e)
    {
        MainWindow.instance.OpenTargetDelPlane(ListSerial);
    }

    // ������ɺ�ĳ�ʼ��
    private void TaskTargetTag_Load(object sender, RoutedEventArgs e)
    {
        ListSerial = (int)TaskTargetTag.Tag;
        //MainWindow.instance.OpenTaskSerial
        Name1.Text = MainWindow.instance.jsonData.Tasks[MainWindow.instance.OpenTaskSerial].TaskTarget[ListSerial].Target;
        
        if (MainWindow.instance.jsonData.Tasks[MainWindow.instance.OpenTaskSerial].TaskTarget[ListSerial].IsComplete)
        {
            Taskbool1.Content = "ȡ��";
            Taskbool1.Foreground = new SolidColorBrush(Colors.Red);
        }
        else
        {
            Taskbool1.Content = "���";
            Taskbool1.Foreground = new SolidColorBrush(Colors.Green);
        }

        if (MainWindow.instance.EditMode)
        {
            Taskbool1.Content = "�޸�";
            Taskbool1.Foreground = new SolidColorBrush(Colors.Green);

            Task2.IsVisible = true;
            Task2.Content = "ɾ��";
            Task2.Foreground = new SolidColorBrush(Colors.Red);
        }


    }

}