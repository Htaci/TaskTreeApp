using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = Avalonia.Point;
namespace TaskTree;

public partial class TaskIcon : UserControl
{
    /// <summary>
    /// 任务编号
    /// </summary>
    public int TaskSerial { get; set; }
    /// <summary>
    /// 指向线列表
    /// </summary>
    public List<DirectionLine> directionLines = new List<DirectionLine>();

    /// <summary>
    /// 右键菜单
    /// </summary>
    public ContextMenu contextMenu;

    //public Border Taskborder;
    /// <summary>
    /// 任务图标的坐标
    /// </summary>
    public Thickness thickness { get; set; }

    public TaskIcon()
    {
        InitializeComponent();
        // 阻止事件冒泡
        Taskborder.PointerPressed += (s, e) => e.Handled = true;
    }
    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        // 设置UserControl的Margin
        this.Margin = thickness;
        //Debug.WriteLine($"任务：{TaskSerial}，坐标：{Margin}");

        bool IsTaskComplete = true;
        // 判断此任务是否完成，如果没有任务条目则默认没有完成
        if (TaskTreePanel.instance?.jsonData.Tasks[TaskSerial].TaskTarget.Count > 0)
        {
            foreach (var task in TaskTreePanel.instance?.jsonData.Tasks[TaskSerial].TaskTarget)
            {
                if (!task.IsComplete)
                {
                    IsTaskComplete = false;
                }

            }
        }
        else
        {
            IsTaskComplete = false;
        }
        // 根据任务是否完成设置颜色
        if (IsTaskComplete)
            Taskborder.BorderBrush = new SolidColorBrush(Colors.Green);
        else
            Taskborder.BorderBrush = new SolidColorBrush(Colors.White);


        textBlock.Text = "\ue922";
    }


    private void Button_Icon(object? sender, PointerPressedEventArgs e)
    {
        Debug.WriteLine($"任务ID：{TaskSerial}，被点击");

        // 检查是否已经有了右键菜单
        if (TaskTreePanel.instance._contextMenu != null)
        {
            TaskTreePanel.instance.MainPanel.Children.Remove(TaskTreePanel.instance._contextMenu);
            TaskTreePanel.instance._contextMenu = null;
        }

        var point = e.GetCurrentPoint(sender as Control);
        if (point.Properties.IsLeftButtonPressed)   // 左键点击
        {
            Debug.WriteLine("左键被按下");



            if (!TaskTreePanel.instance.isConnectionStatus)
            {
                TaskData taskData = TaskTreePanel.instance.jsonData.Tasks[TaskSerial];
                //MainWindow.instance.OpenTaskPlanel(taskData);
                Debug.WriteLine($"任务ID：{TaskSerial} 触发任务：打开任务面板");
                TaskTreePanel.instance.OpenTaskPlanel(TaskSerial);
            }

            // 如果是正在连接状态 , 且该图标不是发起任务的图标时, 触发连接
            if (TaskTreePanel.instance.isConnectionStatus && TaskTreePanel.instance.connectionTaskSerial.TaskSerial != TaskSerial)
            {
                // 清除预览
                TaskTreePanel.instance.previewDirectionLine.RemoveDirectionLine();
                TaskTreePanel.instance.previewDirectionLine = null;
                TaskTreePanel.instance.isConnectionStatus = false;
                // 创建一个新的指向线
                DirectionLine line = new DirectionLine(TaskTreePanel.instance.jsonData.Tasks[TaskTreePanel.instance.connectionTaskSerial.TaskSerial], TaskTreePanel.instance.jsonData.Tasks[TaskSerial]);
                directionLines.Add(line);
                TaskTreePanel.instance.connectionTaskSerial.directionLines.Add(line);
                // 添加到json
                TaskTreePanel.instance.jsonData.Tasks[TaskTreePanel.instance.connectionTaskSerial.TaskSerial].TaskTargetLine.Add(TaskSerial);
                Debug.WriteLine($"开始任务{TaskTreePanel.instance.connectionTaskSerial.TaskSerial}的指向线列表有{TaskTreePanel.instance.connectionTaskSerial.directionLines.Count}个");
                Debug.WriteLine($"被指向任务{TaskSerial}的指向线列表有{directionLines.Count}个");
                // 重置任务
                TaskTreePanel.instance.connectionTaskSerial = null;
            }


        }
        if (point.Properties.IsRightButtonPressed)  // 右键点击
        {
            Debug.WriteLine("右键被按下（图标）");



            创建右键菜单(e.GetPosition(TaskTreePanel.instance));
        }

        
    }

    public void 创建右键菜单(Point point)
    {
        // 检查是否已经有了右键菜单
        if (TaskTreePanel.instance?._contextMenu != null)
        {
            TaskTreePanel.instance.MainPanel.Children.Remove(TaskTreePanel.instance._contextMenu);
            TaskTreePanel.instance._contextMenu = null;
        }
        var 右键菜单选项 = new MenuItem
        {
            Header = "连接任务",
        };
        var 右键菜单选项2 = new MenuItem
        {
            Header = "移动位置",
        };
        var 右键菜单选项3 = new MenuItem
        {
            Header = "删除任务",
        };
        // 绑定点击事件
        右键菜单选项.Click += On连接任务Clicked;
        右键菜单选项2.Click += On移动任务Clicked;
        右键菜单选项3.Click += On删除任务Clicked;
        var 右键菜单 = new ContextMenu
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(point.X, point.Y - 40, 0, 0),
            Width = 150,

        };
        右键菜单.Items.Add(右键菜单选项);
        右键菜单.Items.Add(右键菜单选项2);
        右键菜单.Items.Add(右键菜单选项3);


        TaskTreePanel.instance.MainPanel.Children.Add(右键菜单);
        TaskTreePanel.instance._contextMenu = 右键菜单;

        // 阻止事件冒泡
        右键菜单.PointerPressed += (s, e) => e.Handled = true;
    }

    private void On连接任务Clicked(object? sender, RoutedEventArgs e)
    {
        // 在这里处理点击事件
        Debug.WriteLine("连接任务触发");
        TaskTreePanel.instance.connectionTaskSerial = this;
        TaskTreePanel.instance.isConnectionStatus = true;

        // 检查是否已经有了右键菜单
        if (TaskTreePanel.instance?._contextMenu != null)
        {
            TaskTreePanel.instance.MainPanel.Children.Remove(TaskTreePanel.instance._contextMenu);
            TaskTreePanel.instance._contextMenu = null;
        }
    }

    private void On移动任务Clicked(object? sender, RoutedEventArgs e)
    {
        // 在这里处理点击事件
        // 隐藏该控件
        Taskborder.IsVisible = false;
        // 创建一个新的预览图标
        new TaskIconPreview(TaskTreePanel.instance.jsonData.Tasks[TaskSerial]);

        // 检查是否已经有了右键菜单
        if (TaskTreePanel.instance?._contextMenu != null)
        {
            TaskTreePanel.instance.MainPanel.Children.Remove(TaskTreePanel.instance._contextMenu);
            TaskTreePanel.instance._contextMenu = null;
        }
    }

    private void On删除任务Clicked(object? sender, RoutedEventArgs e)
    {
        TaskTreeData.DeleteTask(TaskSerial);

        // 检查是否已经有了右键菜单
        if (TaskTreePanel.instance?._contextMenu != null)
        {
            TaskTreePanel.instance.MainPanel.Children.Remove(TaskTreePanel.instance._contextMenu);
            TaskTreePanel.instance._contextMenu = null;
        }
    }
    // 鼠标进入时
    private void Border_PointerEnter(object? sender, PointerEventArgs e)
    {
        // 设置鼠标指针为手型图标
        TaskTreePanel.instance.Cursor = new Cursor(StandardCursorType.Hand);

        // 如果是正在连接状态 , 且该图标不是发起任务的图标
        if (TaskTreePanel.instance.isConnectionStatus && TaskTreePanel.instance.connectionTaskSerial.TaskSerial != TaskSerial)
        {
            DirectionLine directionLine = new DirectionLine(TaskTreePanel.instance.jsonData.Tasks[TaskTreePanel.instance.connectionTaskSerial.TaskSerial], TaskTreePanel.instance.jsonData.Tasks[TaskSerial]);
            directionLine.动画 = true;
            directionLine.StartAsyncTask();

            TaskTreePanel.instance.previewDirectionLine = directionLine;
        }

        foreach (var s in directionLines)
        {
            s.动画 = true;
            s.StartAsyncTask();
        }
    }
    // 鼠标离开时
    private void Border_PointerLeave(object? sender, PointerEventArgs e)
    {
        // 恢复默认鼠标指针
        TaskTreePanel.instance.Cursor = new Cursor(StandardCursorType.Arrow);

        // 如果是正在连接状态 , 且该图标不是发起任务的图标
        if (TaskTreePanel.instance.isConnectionStatus && TaskTreePanel.instance.connectionTaskSerial.TaskSerial != TaskSerial)
        {
            //MainWindow.instance.previewDirectionLine.动画 = false;
            TaskTreePanel.instance.previewDirectionLine.RemoveDirectionLine();
            TaskTreePanel.instance.previewDirectionLine = null;
        }

        foreach (var s in directionLines)
        {
            s.动画 = false;
        }
    }
    // 刷新任务指向线
    public void 刷新任务指向线()
    {
        foreach (var s in directionLines)
        {
            // 删除指向线自身
            s.RemoveDirectionLine();

            //Debug.WriteLine($"指向线测试，开始点：{s.start.Width}/{s.start.Height}/任务编号{s.start.TaskSerial}，" +
            //    $"结束点：{s.end.Width}/{s.end.Height}/任务编号{s.end.TaskSerial}");

            // 重新生成指向线
            s.NewDirectionLine();
        }
    }

    public void 刷新任务图标()
    {
        Taskborder.Margin = new Thickness(TaskTreePanel.instance.jsonData.Tasks[TaskSerial].Width,
            TaskTreePanel.instance.jsonData.Tasks[TaskSerial].Height, 0, 0);


        bool IsTaskComplete = true;
        // 判断此任务是否完成，如果没有任务条目则默认没有完成
        if (TaskTreePanel.instance?.jsonData.Tasks[TaskSerial].TaskTarget.Count > 0)
        {
            foreach (var task in TaskTreePanel.instance?.jsonData.Tasks[TaskSerial].TaskTarget)
            {
                if (!task.IsComplete)
                {
                    IsTaskComplete = false;
                }

            }
        }
        else
        {
            IsTaskComplete = false;
        }
        SolidColorBrush color;
        if (IsTaskComplete)
        {
            color = new SolidColorBrush(Colors.Green);
        }
        else
        {
            color = new SolidColorBrush(Colors.White);
        }

        Taskborder.BorderBrush = color;
    }

    public void 删除任务图标()
    {
        Debug.WriteLine($"任务：{TaskSerial} 被删除");

        for (int i = directionLines.Count - 1; i >= 0; i--)
        {
            var s = directionLines[i];
            s.RemoveDirectionLine();
            s.end.taskIcon.directionLines.Remove(s);
            s.start.taskIcon.directionLines.Remove(s);
            s.RemoveDirectionLineFromData();
        }
        //Debug.WriteLine($"5");
        TaskTreePanel.instance.TaskTreeGrid.Children.Remove(Taskborder);
        //Debug.WriteLine($"6");
    }
}
