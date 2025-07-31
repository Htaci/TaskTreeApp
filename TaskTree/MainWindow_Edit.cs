using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTree
{
    public partial class MainWindow : Window
    {
        public bool EditMode = false; // 编辑模式

        public bool pointingLine = false; // 连接指向线

        // 任务标题编辑
        public void Edit_TaskTitle(object? sender, RoutedEventArgs e)
        {
            Edit_TaskTitlePanel.IsVisible = true;
            TaskPanel.IsVisible = false;
            Edit_TaskTitleTextBox.Watermark = TaskPanelTitle.Text;
        }
        public void Edit_TaskTitle1(object? sender, RoutedEventArgs e)
        {
            // 取消
            Edit_TaskTitlePanel.IsVisible = false;
            Edit_TaskTitleTextBox.Text = string.Empty;
            TaskPanel.IsVisible = true;
        }
        public void Edit_TaskTitle2(object? sender, RoutedEventArgs e)
        {
            // 确定
            Edit_TaskTitlePanel.IsVisible = false;
            Debug.WriteLine(Edit_TaskTitleTextBox.Text);
            if (Edit_TaskTitleTextBox.Text == string.Empty || Edit_TaskTitleTextBox.Text == null)
            {
                Edit_TaskTitleTextBox.Text = "空白标题";
            }
            jsonData.Tasks[OpenTaskSerial].TaskTitle = Edit_TaskTitleTextBox.Text;
            TaskPanelTitle.Text = Edit_TaskTitleTextBox.Text;
            TaskPanelTitle_Edit.Content = Edit_TaskTitleTextBox.Text;

            Edit_TaskTitleTextBox.Text = string.Empty;
            Edit_TaskTitleTextBox.Watermark = string.Empty;
            TaskPanel.IsVisible = true;
        }
        public void Edit_NewTaskTarget(object? sender, RoutedEventArgs e)
        {
            // 打开任务条目编辑界面
            Edit_TaskTargetPanel.IsVisible = true;
            TaskPanel.IsVisible = false;
            Edit_TaskTargetTextBox.Watermark = "新建任务条目";
        }
        public void Edit_TaskTarget1(object? sender, RoutedEventArgs e)
        {
            // 取消
            Edit_TaskTargetPanel.IsVisible = false;
            Edit_TaskTargetTextBox.Text = string.Empty;
            TaskPanel.IsVisible = true;
            SubmitTaskListSerial = -1;
        }
        public void Edit_TaskTarget2(object? sender, RoutedEventArgs e)
        {
            // 确认
            Edit_TaskTargetPanel.IsVisible = false;
            if (Edit_TaskTargetTextBox.Text == null || Edit_TaskTargetTextBox.Text == string.Empty)
            {
                Edit_TaskTargetTextBox.Text = "空白任务条目";
            }
            if ( SubmitTaskListSerial < 0)
            {
                Debug.WriteLine("新建任务");
                jsonData.Tasks[OpenTaskSerial].TaskTarget.Add(new TaskTargetDate { Target = Edit_TaskTargetTextBox.Text, IsComplete = false });
            }
            else
            {
                Debug.WriteLine("修改任务");
                jsonData.Tasks[OpenTaskSerial].TaskTarget[SubmitTaskListSerial].Target = Edit_TaskTargetTextBox.Text;
            }

            Edit_TaskTargetTextBox.Text = string.Empty;
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

        public void TaskTarget_Del(int ListSerial)
        {
            // 移除当前元素
            jsonData.Tasks[OpenTaskSerial].TaskTarget.RemoveAt(ListSerial);

            TaskTargetPanel.Children.Clear(); // 清除任务条目
            // 重新加载
            for (int i = 0; i < jsonData.Tasks[OpenTaskSerial].TaskTarget.Count; i++)
            {
                TaskTargetPanel.Children.Add(new TaskTarget()
                {
                    Tag = i
                });
            }
        }
    }
}
