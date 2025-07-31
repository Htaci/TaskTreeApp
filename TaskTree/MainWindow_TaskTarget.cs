//using Avalonia;
//using Avalonia.Controls;
//using Avalonia.Interactivity;
//using Avalonia.Layout;
//using Avalonia.Media;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace TaskTree
//{
//    public partial class MainWindow : Window
//    {
//        public void Controls_TaskTarget(string targetName,bool Taskbool,int ListSerial)
//        {
//            // 创建 StackPanel
//            var stackPanel = new StackPanel
//            {
//                Orientation = Orientation.Horizontal
//            };

//            // 创建 TextBlock
//            var textBlock = new TextBlock
//            {
//                Text = targetName,
//                Margin = new Thickness(5)
//            };

//            string str;
//            SolidColorBrush solidColorBrush;
//            if (Taskbool)
//            {
//                str = "取消";
//                solidColorBrush = new SolidColorBrush(Colors.Red);
//            }
//            else
//            {
//                str = "完成";
//                solidColorBrush = new SolidColorBrush(Colors.Green);
//            }

//            if (EditMode)
//            {
//                str = "修改";
//                solidColorBrush = new SolidColorBrush(Colors.Green);
//            }
//            // 创建 HyperlinkButton
//            var hyperlinkButton = new HyperlinkButton
//            {
//                Content = str,
//                Foreground = solidColorBrush,
//                Margin = new Thickness(5),
//                Tag = ListSerial
//            };

//            // 为 HyperlinkButton 添加点击事件处理器
//            hyperlinkButton.Click += Click_Submit;




//            // 将控件添加到 StackPanel 中
//            stackPanel.Children.Add(textBlock);
//            stackPanel.Children.Add(hyperlinkButton);

//            if (EditMode)
//            {
//                // 创建 HyperlinkButton : 删除按钮
//                var hyperlinkButton_Del = new HyperlinkButton
//                {
//                    Content = "删除",
//                    Foreground = new SolidColorBrush(Colors.Red),
//                    Margin = new Thickness(5)
//                };
//                hyperlinkButton_Del.Click += Click_Submit;
//                stackPanel.Children.Add(hyperlinkButton_Del);
//            }

//            // 将 StackPanel 放置到面板内
//            TaskTargetPanel.Children.Add(stackPanel);
//        }

//        private void Click_Submit(object? sender, RoutedEventArgs e)
//        {
//            // 获取当前点击的 HyperlinkButton
//            var hyperlinkButton = sender as HyperlinkButton;

//            int ListSerial = (int)hyperlinkButton.Tag;


//            if (hyperlinkButton != null && Convert.ToString(hyperlinkButton.Content) == "删除")
//            {
//                // 移除当前元素
//                jsonData.Data[OpenTaskSerial].TaskTarget.RemoveAt(ListSerial);

//                TaskTargetPanel.Children.Clear(); // 清除任务条目
//                // 重新加载
//                for (int i = 0; i < jsonData.Data[OpenTaskSerial].TaskTarget.Count; i++)
//                {
//                    Controls_TaskTarget(jsonData.Data[OpenTaskSerial].TaskTarget[i].Target,
//                        jsonData.Data[OpenTaskSerial].TaskTarget[i].IsComplete,
//                        i);
//                }
//            }
//            else
//            {
//                OpenTargetPlane(ListSerial);
//            }
//        }


//    }
//}
