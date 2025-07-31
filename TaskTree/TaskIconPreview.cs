using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTree
{
    /// <summary>
    /// 放置图标时的预览图标，仅能存在一个
    /// </summary>
    public class TaskIconPreview : Window
    {
        bool move = false;
        private Point _startPoint;
        private Border borderPreview;
        private Point contextPoint;
        // 是否启用网格对齐
        public bool isGridAlign = true;

        // 预览任务图标编号
        public TaskData taskSerial;
        // 预览任务图标实例
        public TaskIcon taskIcon;
        // 鼠标是否在圆形按钮上
        public bool isPointerInCircle = false;
        // 两个圆形按钮的索引
        public Ellipse circle;
        public Ellipse circle2;

        // 原本的位置
        public int originX;
        public int originY;

        public TaskIconPreview(TaskData TaskSerial)
        {
            taskSerial = TaskSerial;
            taskIcon = TaskSerial.taskIcon;
            //Thickness thickness = new Thickness(TaskSerial.Width
            //    , TaskSerial.Height, 0, 0);
            originX = TaskSerial.Width;
            originY = TaskSerial.Height;
            Controls_TaskIcon(taskIcon.Taskborder.Margin);
        }

        /// <summary>
        /// 生成预览任务图标
        /// </summary>
        /// <param name="thickness"></param>
        public void Controls_TaskIcon(Thickness thickness)
        {
            //// 测试
            //thickness = new Thickness(0, 0, 0, 0);
            // 创建 Border
            var border = new Border
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Background = new SolidColorBrush(Colors.Green),
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(Colors.White),
                CornerRadius = new CornerRadius(8),
                Margin = thickness,
                Height = 40,
                Width = 40,
            };
            border.PointerMoved += Form_OnDragMove;
            border.PointerEntered += Border_PointerEnter;
            border.PointerExited += Border_PointerLeave;
            border.PointerPressed += Border_PointerPressed;
            border.PointerReleased += Border_PointerReleased;

             
            // 创建一个绿色圆形代表确定
            var circle = new Ellipse
            {
                Width = 15,
                Height = 15,
                Margin =  new Thickness(thickness.Left + 35, thickness.Top +35, 0, 0),
                Fill = Brushes.Green, // 绿色填充
                Stroke = Brushes.White, // 可选：白色边框
                StrokeThickness = 1.5,   // 边框厚度
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            };
            circle.PointerPressed += 确认_PointerPressed;
            circle.PointerEntered += 确认_PointerEnter;
            circle.PointerExited += 确认_PointerLeave;

            // 创建一个红色圆形代表取消
            var circle2 = new Ellipse
            {
                Width = 15,
                Height = 15,
                Margin = new Thickness(thickness.Left - 10, thickness.Top + 35, 0, 0),
                Fill = Brushes.Red, // 绿色填充
                Stroke = Brushes.White, // 可选：白色边框
                StrokeThickness = 1.5,   // 边框厚度
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            };
            circle2.PointerEntered += 确认_PointerEnter;
            circle2.PointerExited += 确认_PointerLeave;


            // 将 Border 添加到任务图
            MainWindow.instance?.TaskTreePanel.Children.Add(border);
            MainWindow.instance?.TaskTreePanel.Children.Add(circle);
            MainWindow.instance?.TaskTreePanel.Children.Add(circle2);
            borderPreview = border;
            this.circle = circle;
            this.circle2 = circle2;
            // 阻止事件冒泡
            border.PointerPressed += (s, e) => e.Handled = true; 
            circle2.PointerPressed += (s, e) => e.Handled = true;
            circle.PointerPressed += (s, e) => e.Handled = true;
        }
        // 当鼠标进入图标时
        private void Border_PointerEnter(object? sender, PointerEventArgs e)
        {
            if(isPointerInCircle == false)
            {
                // 设置鼠标指针为四方向（移动）图标
                MainWindow.instance.Cursor = new Cursor(StandardCursorType.SizeAll);
                Debug.WriteLine("预览图标进入了");
            }
        }
        // 当鼠标离开图标时
        private void Border_PointerLeave(object? sender, PointerEventArgs e)
        {
            // 恢复默认鼠标指针
            MainWindow.instance.Cursor = new Cursor(StandardCursorType.Arrow);
        }
        // 当鼠标在图标内按下时
        private void Border_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            move = true;
            contextPoint = e.GetPosition(borderPreview);
        }
        // 当鼠标在图标内松开时
        private void Border_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            move = false;
        }

        // 当鼠标进入图标时
        private void 确认_PointerEnter(object? sender, PointerEventArgs e)
        {
            isPointerInCircle = true;
            // 设置鼠标指针为手型图标
            MainWindow.instance.Cursor = new Cursor(StandardCursorType.Hand);
            Debug.WriteLine("确认圆形按钮进入了");
        }
        
        // 当鼠标离开图标时
        private void 确认_PointerLeave(object? sender, PointerEventArgs e)
        {
            isPointerInCircle = false;
            // 恢复默认鼠标指针
            MainWindow.instance.Cursor = new Cursor(StandardCursorType.Arrow);
        }
        // 当鼠标在图标内按下时
        private void 确认_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // 设置目标图标的位置
            taskIcon.Taskborder.Margin = borderPreview.Margin;
            taskIcon.Taskborder.IsVisible = true;
            // 更新数据中的信息
            taskSerial.Width = (int)taskIcon.Taskborder.Margin.Left;
            taskSerial.Height = (int)taskIcon.Taskborder.Margin.Top;
            Delete();
            taskIcon.刷新任务指向线();
        }
        private void 取消_PointerPressed(object? sender, PointerPressedEventArgs e)
        {

        }

        // 当鼠标移动时
        private void Form_OnDragMove(object? sender, PointerEventArgs e)
        {
            if (move)
            {
                var point = e.GetPosition(MainWindow.instance);
                
                // 计算经过偏移后，鼠标相对于任务图的位置，也就是图标的真实位置
                var deltaX = point.X - MainWindow.instance.TaskTreePanelX - contextPoint.X;
                var deltaY = point.Y - MainWindow.instance.TaskTreePanelY -50 - contextPoint.Y;

                // 网格对齐
                if (isGridAlign)
                {
                    deltaX = (int)deltaX / 10 * 10;
                    deltaY = (int)deltaY / 10 * 10;
                }

                // 更新 Border 的位置
                borderPreview.Margin = new Thickness(deltaX, deltaY, 0, 0);
                circle.Margin = new Thickness(deltaX + 35, deltaY + 35, 0, 0);
                circle2.Margin = new Thickness(deltaX -10, deltaY + 35, 0, 0);

                // 设置目标图标的位置
                taskIcon.Taskborder.Margin = borderPreview.Margin;
                // 更新数据中的信息
                taskSerial.Width = (int)taskIcon.Taskborder.Margin.Left;
                taskSerial.Height = (int)taskIcon.Taskborder.Margin.Top;
                taskIcon.刷新任务指向线();
            }
        }

        // 隐藏自身
        public void Hide()
        {
            borderPreview.IsVisible = false;
            circle.IsVisible = false;
            circle2.IsVisible = false;
        }
        // 删除自身全部控件包括类实例
        public void Delete()
        {
            MainWindow.instance?.TaskTreePanel.Children.Remove(borderPreview);
            MainWindow.instance?.TaskTreePanel.Children.Remove(circle);
            MainWindow.instance?.TaskTreePanel.Children.Remove(circle2);
            borderPreview = null;
            circle = null;
            circle2 = null;
        }
    }
}
