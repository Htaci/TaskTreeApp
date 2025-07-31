using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTree
{
    public class DirectionLine
    {
        public Grid gridTest;
        public bool 动画;

        private DispatcherTimer _timer;
        private double _frameInterval;
        double frameRate = 180;

        public TaskData start;
        public TaskData end;

        public async Task StartAsyncTask()
        {
            // 停止动画
            _timer?.Stop();
            _timer = null;
            // 计算每帧的时间间隔（毫秒）
            _frameInterval = 1000.0 / frameRate;

            // 初始化 DispatcherTimer
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(_frameInterval);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // 保持任务运行，直到动画停止
            while (动画)
            {
                await Task.Delay(100); // 避免阻塞主线程
            }
            // 停止动画
            _timer?.Stop();
            _timer = null;

        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            // 获取 Border 的宽度和高度
            var parentBorder = gridTest.Parent as Border;
            double borderWidth = 0;
            if (parentBorder != null)
            {
                borderWidth = parentBorder.Width;
                double borderHeight = parentBorder.Height;
            }

            int loopCount = (int)Math.Floor((double)borderWidth / 15) + 1;
            int lineWidth = loopCount * 15;

            foreach (var child in gridTest.Children)
            {
                if (child is Line line)
                {
                    if (line.Margin.Left < lineWidth)
                    {
                        line.Margin = new Thickness(line.Margin.Left + 0.5, 0, 0, 0);
                    }
                    else
                    {
                        line.Margin = new Thickness(line.Margin.Left - lineWidth, 0, 0, 0);
                    }
                }
            }
        }

        public DirectionLine(TaskData start , TaskData end)
        {
            Debug.WriteLine($"指向线初始化，开始点：{start.Width}/{start.Height}/任务编号{start.TaskSerial}，" +
                $"结束点：{end.Width}/{end.Height}/任务编号{end.TaskSerial}");
            this.start = start;
            this.end = end;
            NewDirectionLine();
        }
        /// <summary>
        /// 创建一个指向线
        /// </summary>
        public void NewDirectionLine()
        {
            //Debug.WriteLine($"指向线被创建,开始位置是x:{start.Width},y:{start.Height},结束位置是x:{end.Width},y:{end.Height}");
            // 计算中心点
            int p1 = (start.Width + end.Width) / 2;
            int p2 = (start.Height + end.Height) / 2;

            // 计算线长
            double distance = Math.Sqrt(Math.Pow(end.Width - start.Width, 2) + Math.Pow(end.Height - start.Height, 2));
            int integerDistance = (int)distance;

            // 计算斜率
            double slope = (end.Height - start.Height) / (double)(end.Width - start.Width);

            // 计算角度（弧度）
            double angleRad = Math.Atan2(end.Height - start.Height, end.Width - start.Width);

            // 将弧度转换为度数
            double angleDeg = angleRad * (180 / Math.PI);

            var Border = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(p1 - integerDistance / 2 + 21, p2 + 18, 0, 0),
                Background = new SolidColorBrush(Colors.Gray),
                //BorderThickness = new Thickness(1),
                //BorderBrush = new SolidColorBrush(Colors.White),
                Height = 8,
                Width = integerDistance,
                RenderTransform = new RotateTransform
                {
                    Angle = angleDeg
                }
            };

            var grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Tag = integerDistance
            };

            Border.Child = grid;

            // 用 Line 画的箭头
            for (int i = 0; i <= integerDistance; i += 15)
            {
                var Line1 = new Line
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(2, 4),
                    StrokeThickness = 0.8,
                    Margin = new Thickness(i, 0, 0, 0),
                    Stroke = new SolidColorBrush(Colors.White),
                };
                var Line2 = new Line
                {
                    StartPoint = new Point(2, 4),
                    EndPoint = new Point(0, 8),
                    StrokeThickness = 0.8,
                    Margin = new Thickness(i, 0, 0, 0),
                    Stroke = new SolidColorBrush(Colors.White),
                };

                grid.Children.Add(Line1);
                grid.Children.Add(Line2);
            }
            // 把指向线添加到 指向线容器(Grid)
            MainWindow.instance?.DirectionLinePanel.Children.Add(Border);


            gridTest = grid;
        }

        /// <summary>
        /// 删除该指向线
        /// </summary>
        public void RemoveDirectionLine()
        {
            // 检查指向线是否已经被添加到 TaskTreePanel
            if (gridTest != null && gridTest.Parent is Border border)
            {
                // 从 TaskTreePanel 中移除指向线
                MainWindow.instance?.DirectionLinePanel.Children.Remove(border);
            }

            // 停止动画
            _timer?.Stop();
            _timer = null;
        }

        // 从数据中删除指向线
        public void RemoveDirectionLineFromData()
        {
            // 因为指向线是在开始任务中记录的，所以检索开始任务的指向线列表
            //for (int s ; start.TaskTargetLine)
            //{
            //    //Debug.WriteLine($"删除指向线，开始点(任务编号)：{s}，结束点(任务编号)：{end.TaskSerial}");
            //    //Debug.WriteLine($"数据结束点(任务编号)：{s}，指向线结束点(任务编号)：{end.TaskSerial}");
            //    if (s == end.TaskSerial)
            //    {
            //        Debug.WriteLine($"满足条件，指向线{start.TaskSerial} -> {end.TaskSerial}被删除");
            //        // 删除该对象
            //        start.TaskTargetLine.Remove(s);
            //    }
            //}
            for (int i = start.TaskTargetLine.Count - 1; i >= 0; i--)
            {
                if (start.TaskTargetLine[i] == end.TaskSerial)
                {
                    start.TaskTargetLine.RemoveAt(i);
                    Debug.WriteLine($"满足条件，指向线{start.TaskSerial} -> {end.TaskSerial}被删除");
                }
            }
        }
    }
}
