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
    // 完成/取消 按钮
    private void Click_Submit(object? sender, RoutedEventArgs e)
    {
        MainWindow.instance.OpenTargetPlane(ListSerial);
    }
    // 删除 按钮
    private void Click_Del (object? sender, RoutedEventArgs e)
    {
        MainWindow.instance.OpenTargetDelPlane(ListSerial);
    }

    // 加载完成后的初始化
    private void TaskTargetTag_Load(object sender, RoutedEventArgs e)
    {
        ListSerial = (int)TaskTargetTag.Tag;
        //MainWindow.instance.OpenTaskSerial
        Name1.Text = MainWindow.instance.jsonData.Tasks[MainWindow.instance.OpenTaskSerial].TaskTarget[ListSerial].Target;
        
        if (MainWindow.instance.jsonData.Tasks[MainWindow.instance.OpenTaskSerial].TaskTarget[ListSerial].IsComplete)
        {
            Taskbool1.Content = "取消";
            Taskbool1.Foreground = new SolidColorBrush(Colors.Red);
        }
        else
        {
            Taskbool1.Content = "完成";
            Taskbool1.Foreground = new SolidColorBrush(Colors.Green);
        }

        if (MainWindow.instance.EditMode)
        {
            Taskbool1.Content = "修改";
            Taskbool1.Foreground = new SolidColorBrush(Colors.Green);

            Task2.IsVisible = true;
            Task2.Content = "删除";
            Task2.Foreground = new SolidColorBrush(Colors.Red);
        }


    }

}