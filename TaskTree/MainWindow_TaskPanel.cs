using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace TaskTree
{
    public partial class MainWindow : Window
    {
        public int SubmitTaskListSerial; // 选中的任务条目
        public int OpenTaskSerial; // 当前打开的任务

        // 打开任务卡片
        public void OpenTaskPlanel(int TaskSerial)
        {
            // Debug.WriteLine("调用成功");
            OpenTaskSerial = TaskSerial;
            TaskPanelTitle.Text = jsonData.Tasks[TaskSerial].TaskTitle;

            Debug.WriteLine("任务卡片:当前任务:" + TaskSerial);
            //Edit_AddTaskTarget(taskData.TaskSerial);

            for (int i = 0; i < jsonData.Tasks[TaskSerial].TaskTarget.Count; i++)
            {
                TaskTargetPanel.Children.Add(new TaskTarget()
                {
                    Tag = i
                });
                //Controls_TaskTarget(jsonData.Data[TaskSerial].TaskTarget[i].Target,
                //    jsonData.Data[TaskSerial].TaskTarget[i].IsComplete,
                //    i);
                //Debug.WriteLine("任务卡片:当前添加任务条目:" + i);
                //Debug.WriteLine("任务卡片:添加任务条目的名称:" + MainWindow.instance.jsonData.Data[taskData.TaskSerial].TaskTarget[i].Target);
            }

            TaskPanel.IsVisible = true;
            TaskPanelBackground.IsVisible = true;
        }

        // 任务卡片 : 关闭按钮
        private void Click_TaskPanelClose(object? sender, RoutedEventArgs e)
        {
            TaskPanel.IsVisible = false;
            TaskPanelBackground.IsVisible = false;
            TaskTargetPanel.Children.Clear(); // 清除任务条目

            // 关闭卡片编辑模式
            EditMode = false;
            Edit_TaskPanelButton.Foreground = new SolidColorBrush(Colors.Black);
            TaskPanel.BorderBrush = new SolidColorBrush(Colors.LightGray);

            TaskPanelTitle.IsVisible = true; // 显示标题
            TaskPanelTitle_Edit.IsVisible = false;

            New_TaskTarget.IsVisible = false; // 隐藏添加新任务的按钮
        }

        // 任务卡片 : 编辑按钮
        private void Click_TaskPanelEdit(object? sender, RoutedEventArgs e)
        {
            if (EditMode)
            {
                EditMode = false;
                // 设置按钮内容颜色为黑色
                Edit_TaskPanelButton.Foreground = new SolidColorBrush(Colors.Black);
                TaskPanel.BorderBrush = new SolidColorBrush(Colors.LightGray);

                TaskPanelTitle.IsVisible = true; // 显示标题
                TaskPanelTitle_Edit.IsVisible = false;

                TaskTargetPanel.Children.Clear(); // 清除任务条目
                // 重新加载
                for (int i = 0; i < jsonData.Tasks[OpenTaskSerial].TaskTarget.Count; i++)
                {
                    TaskTargetPanel.Children.Add(new TaskTarget()
                    {
                        Tag = i
                    });
                    //Controls_TaskTarget(jsonData.Data[OpenTaskSerial].TaskTarget[i].Target,
                    //    jsonData.Data[OpenTaskSerial].TaskTarget[i].IsComplete,
                    //    i);
                }

                // 隐藏添加新任务的按钮
                New_TaskTarget.IsVisible = false;
            }
            else
            {
                EditMode = true;
                // 设置按钮内容颜色为绿色
                Edit_TaskPanelButton.Foreground = new SolidColorBrush(Colors.Green);
                TaskPanel.BorderBrush = new SolidColorBrush(Colors.Green);

                TaskPanelTitle.IsVisible = false; // 隐藏标题
                TaskPanelTitle_Edit.Content = TaskPanelTitle.Text; // 赋予编辑按钮的内容为
                TaskPanelTitle_Edit.IsVisible = true;

                // 修改任务条目后面的按钮为修改按钮
                TaskTargetPanel.Children.Clear(); // 清除任务条目
                // 重新加载
                for (int i = 0; i < jsonData.Tasks[OpenTaskSerial].TaskTarget.Count; i++)
                {
                    TaskTargetPanel.Children.Add(new TaskTarget()
                    {
                        Tag = i
                    });
                    //Controls_TaskTarget(jsonData.Data[OpenTaskSerial].TaskTarget[i].Target,
                    //    jsonData.Data[OpenTaskSerial].TaskTarget[i].IsComplete,
                    //    i);
                }

                // 显示添加新任务的按钮
                New_TaskTarget.IsVisible = true;
            }
        }

        // 确认完成 : 取消按钮
        private void Click_TaskSubmitConfirmationA(object? sender, RoutedEventArgs e)
        {
            SubmitConfirmation.IsVisible = false;
            TaskPanel.IsVisible = true;
            SubmitTaskListSerial = -1;
        }
        // 确认完成 : 确定按钮
        private void Click_TaskSubmitConfirmationB(object? sender, RoutedEventArgs e)
        {
            if (SubmitTaskListSerial >= 0)
            {
                if (!EditMode)
                {
                    if (jsonData.Tasks[OpenTaskSerial].TaskTarget[SubmitTaskListSerial].IsComplete)
                    {
                        jsonData.Tasks[OpenTaskSerial].TaskTarget[SubmitTaskListSerial].IsComplete = false;
                    }
                    else
                    {
                        jsonData.Tasks[OpenTaskSerial].TaskTarget[SubmitTaskListSerial].IsComplete = true;
                    }
                }
                else
                {
                    // 移除当前元素
                    jsonData.Tasks[OpenTaskSerial].TaskTarget.RemoveAt(SubmitTaskListSerial);
                }
            }

            SubmitConfirmation.IsVisible = false;
            TaskPanel.IsVisible = true;

            TaskTargetPanel.Children.Clear(); // 清除任务条目
            // 重新加载
            for (int i = 0; i < jsonData.Tasks[OpenTaskSerial].TaskTarget.Count; i++)
            {
                TaskTargetPanel.Children.Add(new TaskTarget()
                {
                    Tag = i
                });
                //Controls_TaskTarget(jsonData.Data[OpenTaskSerial].TaskTarget[i].Target,
                //    jsonData.Data[OpenTaskSerial].TaskTarget[i].IsComplete,
                //    i);
            }
            SubmitTaskListSerial = -1;
        }

        // 打开确认完成/任务修改的界面
        public void OpenTargetPlane(int ListSerial)
        {
            SubmitTaskListSerial = ListSerial;
            if (EditMode)
            {
                Edit_TaskTargetPanel.IsVisible = true;
                TaskPanel.IsVisible = false;
                Edit_TaskTargetTextBox.Watermark = jsonData.Tasks[OpenTaskSerial].TaskTarget[ListSerial].Target;
            }
            else
            {
                TaskPanel.IsVisible = false;
                SubmitConfirmation.IsVisible = true;
            }
        }

        // 打开删除确认的界面
        public void OpenTargetDelPlane(int ListSerial)
        {
            SubmitTaskListSerial = ListSerial;
            TaskPanel.IsVisible = false;
            SubmitConfirmation.IsVisible = true;
        }

        /// <summary>
        /// 加载任务树图,注意:此方法会清空所有内容后重新生成
        /// </summary>
        public void TaskTreePanelUpdate()
        {
            // 清除任务树/指向线面板里的所有内容
            TaskTreePanel.Children.Clear();
            DirectionLinePanel.Children.Clear();
            // 读取基本信息
            if (jsonData.Panel != null)
            {
                TaskTreePanel.Height = jsonData.Panel.Height;
                TaskTreePanel.Width = jsonData.Panel.Width;
            }

            // 读取任务，生成图标
            if (jsonData.Tasks != null)
            {
                //Debug.WriteLine("任务列表:");
                foreach (var task in jsonData.Tasks)
                {
                    ////Debug.WriteLine($"TaskSerial: {task.TaskSerial}, Width: {task.Width}, Height: {task.Height}");
                    task.Value.taskIcon = new TaskIcon(new Thickness(task.Value.Width, task.Value.Height, 0, 0), task.Value.TaskSerial);
                }
            }
            // 生成指向线
            foreach (var a in jsonData.Tasks)
            {
                foreach (var b in jsonData.Tasks[a.Key].TaskTargetLine)
                {
                    DirectionLine line = new DirectionLine(jsonData.Tasks[b], jsonData.Tasks[a.Key]);
                    jsonData.Tasks[a.Key].taskIcon.directionLines.Add(line);
                    jsonData.Tasks[b].taskIcon.directionLines.Add(line);
                }
            }

        }
    }
}
