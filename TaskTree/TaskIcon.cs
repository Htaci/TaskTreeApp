using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Layout;
using Avalonia.Interactivity;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Shapes;
using System.Drawing;
using Point = Avalonia.Point;

namespace TaskTree
{
    public  class TaskIcon : Window
    {
        /// <summary>
        /// 任务编号
        /// </summary>
        public int TaskSerial;
        /// <summary>
        /// 指向线列表
        /// </summary>
        public List<DirectionLine> directionLines = new List<DirectionLine>();

        /// <summary>
        /// 右键菜单
        /// </summary>
        public ContextMenu contextMenu;

        public Border Taskborder;
        public TaskIcon(Thickness thickness, int Ts)
        {
            TaskSerial = Ts;
            Controls_TaskIcon(thickness, 20);
        }



        /// <summary>
        /// 生成任务图标
        /// </summary>
        /// <param name="thickness"></param>
        /// <param name="TaskSerial"></param>
        public void Controls_TaskIcon(Thickness thickness, int ts)
        {
            bool IsTaskComplete = true;
            // 判断此任务是否完成，如果没有任务条目则默认没有完成
            if (MainWindow.instance?.jsonData.Tasks[TaskSerial].TaskTarget.Count > 0)
            {
                foreach (var task in MainWindow.instance?.jsonData.Tasks[TaskSerial].TaskTarget)
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

            
            // 创建 Border
            var border = new Border
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Background = new SolidColorBrush(Colors.Gray),
                BorderThickness = new Thickness(2),
                BorderBrush = color,
                CornerRadius = new CornerRadius(8),
                Margin = thickness,
                Height = 40,
                Width = 40,
                Tag = TaskSerial,
            };

            border.PointerPressed += Button_Icon;
            border.PointerEntered += Border_PointerEnter;
            border.PointerExited += Border_PointerLeave;

            // 创建 Grid
            var grid = new Grid();

            // 创建 TextBlock
            var textBlock = new TextBlock
            {
                Text = "\ue922",
                FontSize = 16,
                FontFamily = new FontFamily("Segoe Fluent Icons"),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Thickness(2, 0, 0, 0)
            };


            // 将 TextBlock 添加到 Grid 中
            grid.Children.Add(textBlock);

            // 将 Grid 添加到 Border 中
            border.Child = grid;
            Taskborder = border;
            // 将 Border 添加到任务图
            MainWindow.instance?.TaskTreePanel.Children.Add(border);
            // 阻止事件冒泡
            border.PointerPressed += (s, e) => e.Handled = true;
        }

        private void Button_Icon(object? sender, PointerPressedEventArgs e)
        {
            Debug.WriteLine($"任务ID：{TaskSerial}，被点击");
            var point = e.GetCurrentPoint(sender as Control);
            if (point.Properties.IsLeftButtonPressed)
            {
                Debug.WriteLine("左键被按下");
                // 检查是否已经有了右键菜单
                if (MainWindow.instance._contextMenu != null)
                {
                    MainWindow.instance.MainPanel.Children.Remove(MainWindow.instance._contextMenu);
                    MainWindow.instance._contextMenu = null;
                }


                if (TaskSerial >= 0 && !MainWindow.instance.isConnectionStatus)
                {
                    TaskData taskData = MainWindow.instance.jsonData.Tasks[TaskSerial];
                    //MainWindow.instance.OpenTaskPlanel(taskData);
                    MainWindow.instance.OpenTaskPlanel(TaskSerial);
                }

                // 如果是正在连接状态 , 且该图标不是发起任务的图标时, 触发连接
                if (MainWindow.instance.isConnectionStatus && MainWindow.instance.connectionTaskSerial.TaskSerial != TaskSerial)
                {
                    // 清除预览
                    MainWindow.instance.previewDirectionLine.RemoveDirectionLine();
                    MainWindow.instance.previewDirectionLine = null;
                    MainWindow.instance.isConnectionStatus = false;
                    // 创建一个新的指向线
                    DirectionLine line = new DirectionLine(MainWindow.instance.jsonData.Tasks[MainWindow.instance.connectionTaskSerial.TaskSerial], MainWindow.instance.jsonData.Tasks[TaskSerial]);
                    directionLines.Add(line);
                    MainWindow.instance.connectionTaskSerial.directionLines.Add(line);
                    // 添加到json
                    MainWindow.instance.jsonData.Tasks[MainWindow.instance.connectionTaskSerial.TaskSerial].TaskTargetLine.Add(TaskSerial);
                    Debug.WriteLine($"开始任务{MainWindow.instance.connectionTaskSerial.TaskSerial}的指向线列表有{MainWindow.instance.connectionTaskSerial.directionLines.Count}个");
                    Debug.WriteLine($"被指向任务{TaskSerial}的指向线列表有{directionLines.Count}个");
                    // 重置任务
                    MainWindow.instance.connectionTaskSerial = null;
                }


            }
            if (point.Properties.IsRightButtonPressed)
            {
                Debug.WriteLine("右键被按下（图标）");

                创建右键菜单(e.GetPosition(MainWindow.instance));
            }


        }

        public void 创建右键菜单(Point point)
        {
            // 检查是否已经有了右键菜单
            if (MainWindow.instance?._contextMenu != null)
            {
                MainWindow.instance.MainPanel.Children.Remove(MainWindow.instance._contextMenu);
                MainWindow.instance._contextMenu = null;
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
                Margin = new Thickness(point.X, point.Y -40, 0, 0),
                Width = 150,
                
            };
            右键菜单.Items.Add(右键菜单选项);
            右键菜单.Items.Add(右键菜单选项2);
            右键菜单.Items.Add(右键菜单选项3);


            MainWindow.instance.MainPanel.Children.Add(右键菜单);
            MainWindow.instance._contextMenu = 右键菜单;

            // 阻止事件冒泡
            右键菜单.PointerPressed += (s, e) => e.Handled = true;
        }

        private void On连接任务Clicked(object? sender, RoutedEventArgs e)
        {
            // 在这里处理点击事件
            Debug.WriteLine("连接任务触发");
            MainWindow.instance.connectionTaskSerial = this;
            MainWindow.instance.isConnectionStatus = true;

            // 检查是否已经有了右键菜单
            if (MainWindow.instance?._contextMenu != null)
            {
                MainWindow.instance.MainPanel.Children.Remove(MainWindow.instance._contextMenu);
                MainWindow.instance._contextMenu = null;
            }
        }

        private void On移动任务Clicked(object? sender, RoutedEventArgs e)
        {
            // 在这里处理点击事件
            // 隐藏该控件
            Taskborder.IsVisible = false;
            // 创建一个新的预览图标
            new TaskIconPreview(MainWindow.instance.jsonData.Tasks[TaskSerial]);

            // 检查是否已经有了右键菜单
            if (MainWindow.instance?._contextMenu != null)
            {
                MainWindow.instance.MainPanel.Children.Remove(MainWindow.instance._contextMenu);
                MainWindow.instance._contextMenu = null;
            }
        }

        private void On删除任务Clicked(object? sender, RoutedEventArgs e)
        {
            MainWindow.instance.DeleteTask(TaskSerial);

            // 检查是否已经有了右键菜单
            if (MainWindow.instance?._contextMenu != null)
            {
                MainWindow.instance.MainPanel.Children.Remove(MainWindow.instance._contextMenu);
                MainWindow.instance._contextMenu = null;
            }
        }
        // 鼠标进入时
        private void Border_PointerEnter(object? sender, PointerEventArgs e)
        {
            // 设置鼠标指针为手型图标
            MainWindow.instance.Cursor = new Cursor(StandardCursorType.Hand);

            // 如果是正在连接状态 , 且该图标不是发起任务的图标
            if (MainWindow.instance.isConnectionStatus && MainWindow.instance.connectionTaskSerial.TaskSerial != TaskSerial)
            {
                DirectionLine directionLine = new DirectionLine(MainWindow.instance.jsonData.Tasks[MainWindow.instance.connectionTaskSerial.TaskSerial], MainWindow.instance.jsonData.Tasks[TaskSerial]);
                directionLine.动画 = true;
                directionLine.StartAsyncTask();

                MainWindow.instance.previewDirectionLine = directionLine;
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
            MainWindow.instance.Cursor = new Cursor(StandardCursorType.Arrow);

            // 如果是正在连接状态 , 且该图标不是发起任务的图标
            if (MainWindow.instance.isConnectionStatus && MainWindow.instance.connectionTaskSerial.TaskSerial != TaskSerial)
            {
                //MainWindow.instance.previewDirectionLine.动画 = false;
                MainWindow.instance.previewDirectionLine.RemoveDirectionLine();
                MainWindow.instance.previewDirectionLine = null;
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
            Taskborder.Margin = new Thickness(MainWindow.instance.jsonData.Tasks[TaskSerial].Width, 
                MainWindow.instance.jsonData.Tasks[TaskSerial].Height,0,0);


            bool IsTaskComplete = true;
            // 判断此任务是否完成，如果没有任务条目则默认没有完成
            if (MainWindow.instance?.jsonData.Tasks[TaskSerial].TaskTarget.Count > 0)
            {
                foreach (var task in MainWindow.instance?.jsonData.Tasks[TaskSerial].TaskTarget)
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
            //foreach (var s in directionLines)
            //{
            //    Debug.WriteLine($"1");
            //    // 删除指向线自身
            //    s.RemoveDirectionLine();
            //    Debug.WriteLine($"2");
            //    // 删除目标对象directionLines列表中的指向线
            //    s.end.taskIcon.directionLines.Remove(s);
            //    Debug.WriteLine($"3");
            //    // 删除指向线在 jsonData 中的数据
            //    s.RemoveDirectionLineFromData();
            //    Debug.WriteLine($"4");
            //}
            for (int i = directionLines.Count - 1; i >= 0; i--)
            {
                var s = directionLines[i];
                s.RemoveDirectionLine();
                s.end.taskIcon.directionLines.Remove(s);
                s.start.taskIcon.directionLines.Remove(s);
                s.RemoveDirectionLineFromData();
            }
            //Debug.WriteLine($"5");
            MainWindow.instance.TaskTreePanel.Children.Remove(Taskborder);
            //Debug.WriteLine($"6");
        }
    }
}