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

public partial class TaskIconMenuItem : UserControl
{
    public TaskTreePanel taskTreePanel;
    
    public TaskIconMenuItem()
    {
        InitializeComponent();
    }
    
    // 鼠标进入时
    private void _PointerEnter(object? sender, PointerEventArgs e)
    {
        // 设置鼠标指针为手型图标
        TaskTreePanel.instance.Cursor = new Cursor(StandardCursorType.Hand);
        var grid = sender as Border;
        if (grid is null)
            return;
        
        grid.Background = new SolidColorBrush(Colors.DarkGray);
    }

    private void _PointerLeave(object? sender, PointerEventArgs e)
    {
        // 设置鼠标指针为默认图标
        TaskTreePanel.instance.Cursor = new Cursor(StandardCursorType.Arrow);
        
        var grid = sender as Border;
        if (grid is null)
            return;
        
        grid.Background = new SolidColorBrush(Colors.Transparent);
    }

    /// <summary>
    /// 创建新任务，随机任务编号
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void NewTask_Clicked(object? sender, PointerPressedEventArgs e)
    {
        var margin = Margin;
        
        int x = (int)(margin.Left - taskTreePanel.TaskTreePanelX);
        int y = (int)(margin.Top - taskTreePanel.TaskTreePanelY);

        // 添加任务并刷新
        int newTaskId = TaskTreeData.AddTask(x, y);
        TaskTreeData.jsonData.Tasks[newTaskId].taskIcon = new TaskIcon 
        {
            TaskSerial = newTaskId,
            thickness = new Thickness(x, y, 0, 0)
        };//new TaskIcon(new Thickness(x, y, 0, 0), newTaskId);
        
        Debug.WriteLine($"创建任务触发:创建了新任务,X{x},Y{y}");
        // 清除任务容器，重新生成
        taskTreePanel.TaskTreePanelUpdate();
        // 删除右键菜单
        taskTreePanel.MenuLtemGrid.Children.Clear();
    }
    
    
}