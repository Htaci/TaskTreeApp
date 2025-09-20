using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Avalonia.Controls.Shapes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Layout;
using SkiaSharp;
using System.Drawing;
using Point = Avalonia.Point;
using Path = System.IO.Path;

namespace TaskTree;

public partial class TaskTreePanel : UserControl
{
    private Point _startPoint;
    private bool _move = false;

    public Double TaskTreePanelX = 0; // 初始 X 偏移量
    public Double TaskTreePanelY = 0; // 初始 Y 偏移量

    public ContextMenu _contextMenu;

    /// <summary>
    /// 在任务背景图上右键时打开的菜单是否存在
    /// </summary>
    private bool _panelContentMenu;
    /// <summary>
    /// 是否处于连接状态
    /// </summary>
    public bool isConnectionStatus = false;
    /// <summary>
    /// 哪个任务发起的连接
    /// </summary>
    public TaskIcon connectionTaskSerial;

    /// <summary>
    /// 临时指向线,用于预览指向线创建效果
    /// </summary>
    public DirectionLine previewDirectionLine;

    public static TaskTreePanel? instance;

    public TaskTreePanel()
    {
        InitializeComponent();

        instance = this;

        initialization();
        //Debug.WriteLine("");
        //Line1.EndPoint = new Point(300, 30);

        // 测试<
        //new TaskIconPreview(1);
        // 测试>
        DebugTask();
    }

    #region 鼠标操作
    // 当鼠标按下时
    private void Form_OnDragStart(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(sender as Control);
        _startPoint = e.GetPosition(this);
        if (point.Properties.IsLeftButtonPressed)
        {
            Debug.WriteLine("左键被按下");
            // 检查是否已经有了右键菜单
            if (_contextMenu != null)
            {
                MainPanel.Children.Remove(_contextMenu);
                _contextMenu = null;
            }

            _move = true;
        }
        if (point.Properties.IsRightButtonPressed)
        {
            Debug.WriteLine("右键被按下");

            创建右键菜单(e.GetPosition(this));
        }

    }
    // 当鼠标移动时
    private void Form_OnDragMove(object? sender, PointerEventArgs e)
    {
        if (_move)
        {
            var currentPosition = e.GetPosition(this);
            var deltaX = currentPosition.X - _startPoint.X;
            var deltaY = currentPosition.Y - _startPoint.Y;

            TaskTreePanelX = TaskTreePanelX + deltaX;
            TaskTreePanelY = TaskTreePanelY + deltaY;

            TaskTreeGrid.RenderTransform = new TranslateTransform
            {
                X = TaskTreePanelX, // X 偏移量
                Y = TaskTreePanelY  // Y 偏移量
            };
            DirectionLineGrid.RenderTransform = new TranslateTransform
            {
                X = TaskTreePanelX, // X 偏移量
                Y = TaskTreePanelY  // Y 偏移量
            };

            _startPoint = currentPosition;
        }
    }

    // 当鼠标松开时
    private void Form_OnDragEnd(object? sender, PointerReleasedEventArgs e)
    {
        _move = false;
    }

    #endregion

    /// <summary>
    /// 初始化
    /// </summary>
    public void initialization()
    {
        // 加载文件
        bool isFile = Load_JsonFile();


        if (isFile)
        {
            Debug.WriteLine("文件 MainPanel.json 存在！");
            // 反序列化文件内容
            Load_TackData();
            // 更新整个任务树
            TaskTreePanelUpdate();
        }
        else
        {
            Debug.WriteLine("文件 MainPanel.json 不存在。");
            // 创建按钮
            var button = new Button
            {
                Content = "还没有任务树，创建一个吧!",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10)
            };

            // 绑定点击事件
            button.Click += Click_NewMainTree;

            // 将按钮添加到 TaskTreePanel
            MainPanel.Children.Add(button);
        }

    }

    // 新建任务树的按钮
    private void Click_NewMainTree(object? sender, RoutedEventArgs e)
    {
        NewTaskFile("MainPanel");
        Load_TackData();

        TaskTreePanelUpdate();
        // 检查 sender 是否为 Control 类型
        if (sender is Control control)
        {
            // 从 MainPanel 中移除该控件
            MainPanel.Children.Remove(control);
        }
    }

    private void Grid_Loaded(object sender, RoutedEventArgs e)
    {
        var grid = sender as Grid;
        if (grid != null)
        {
            //Debug.WriteLine($"Grid: {grid.Width},{grid.Height}");
        }
    }

    public void 创建右键菜单(Point point)
    {
        // 检查是否已经有了右键菜单
        if (_contextMenu != null)
        {
            MainPanel.Children.Remove(_contextMenu);
            _contextMenu = null;
        }
        var 右键菜单选项 = new MenuItem
        {
            Header = "新建任务"
        };
        // 绑定点击事件
        右键菜单选项.Click += On创建任务Clicked;
        var 右键菜单 = new ContextMenu
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(point.X, point.Y - 50, 0, 0),
            Width = 150,
            Height = 55
        };
        //右键菜单.Items.Add(右键菜单选项);

        //MainPanel.Children.Add(右键菜单);
        //_contextMenu = 右键菜单;

        // 阻止事件冒泡
        右键菜单.PointerPressed += (s, e) => e.Handled = true;
        
        // 清空右键菜单容器
        MenuLtemGrid.Children.Clear();

        MenuLtemGrid.Children.Add(new TaskIconMenuItem()
        {
            Margin = new Thickness(point.X, point.Y - 50, 0, 0),
            taskTreePanel = this
        });
    }


    private void On创建任务Clicked(object? sender, RoutedEventArgs e)
    {
        // 获取ContextMenu的Margin位置
        if (_contextMenu != null)
        {
            var margin = _contextMenu.Margin;
            //Debug.WriteLine($"右键窗口 Margin: Left={margin.Left}, Top={margin.Top}, Right={margin.Right}, Bottom={margin.Bottom}");
            //Debug.WriteLine( $"TaskTreePanelX = {TaskTreePanelX},TaskTreePanelY = {TaskTreePanelY}" );
            int x = (int)(margin.Left - TaskTreePanelX);
            int y = (int)(margin.Top - TaskTreePanelY);

            // 添加任务并刷新
            int newTaskId = AddTask(x, y);
            jsonData.Tasks[newTaskId].taskIcon = new TaskIcon 
            {
                TaskSerial = newTaskId,
                thickness = new Thickness(x, y, 0, 0)
            };//new TaskIcon(new Thickness(x, y, 0, 0), newTaskId);
            Debug.WriteLine($"创建任务触发:创建了新任务,X{x},Y{y}");
            TaskTreePanelUpdate();
            // 删除右键菜单
            MainPanel.Children.Remove(_contextMenu);
            _contextMenu = null;
        }
    }

    private void OpenSettings_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow();
        settingsWindow.Show();
    }


    /// <summary>
    /// 单个任务树的数据
    /// <para>包含了：</para>
    /// <para>1.任务图标（位置信息，标签信息，是否完成）</para>
    /// <para>2.任务图标的指向线</para>
    /// <para>3.单个任务的详细内容（任务条目以及是否完成）</para>
    /// </summary>
    public RootData jsonData = new RootData();
    public string filePath;

    /// <summary>
    /// 加载json文件路径
    /// </summary>
    public bool Load_JsonFile()
    {
        // 获取程序的根目录
        string rootDirectory = AppContext.BaseDirectory;
        // 构建目标文件的完整路径
        string assetsFolder = Path.Combine(rootDirectory, "Assets");
        filePath = Path.Combine(assetsFolder, $"MainPanel.json");
        // 检查文件是否存在
        if (File.Exists(filePath))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// 读取 Json 文件并反序列化到 jsonData
    /// </summary>
    public void Load_TackData()
    {
        // 检查文件是否存在
        if (!File.Exists(filePath))
        {
            Debug.WriteLine("文件不存在。");
            return;
        }

        // 读取文件内容
        string content = File.ReadAllText(filePath);

        // 反序列化 JSON 数据
        jsonData = JsonSerializer.Deserialize<RootData>(content, new JsonSerializerOptions { WriteIndented = true });

    }

    /// <summary>
    /// 序列化并保存到文件
    /// </summary>
    public void Save_TackData()
    {
        // 序列化 JSON 数据
        string updatedContent = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true });
        Debug.WriteLine($"{updatedContent}");
        DebugTask();
        // 写入文件
        File.WriteAllText(filePath, updatedContent);

    }

    // 新建新的任务树文件
    public void NewTaskFile(string name)
    {
        // 获取程序的根目录
        string rootDirectory = AppContext.BaseDirectory;

        // 构建目标文件的完整路径
        string assetsFolder = Path.Combine(rootDirectory, "Assets");
        string filePath = Path.Combine(assetsFolder, $"{name}.json");

        // 确保目标文件夹存在
        string directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        AddTask(200, 200);

        Save_TackData();
    }

    // 添加新任务
    public int AddTask(int width, int height)
    {
        int newTaskSerial;
        // 随机一个 任务id
        for (; ; )
        {
            uint raw = (uint)Random.Shared.Next(int.MinValue, int.MaxValue); // 0‥0x7FFFFFFE
            raw |= (uint)(Random.Shared.Next(0, 2) << 31);                   // 决定最高位
            newTaskSerial = unchecked((int)raw);

            if (!jsonData.Tasks.ContainsKey(newTaskSerial))
            {
                // 如果字典内没有这个键则代表随机生成的这个id可用，退出循环
                break;
            }
        }

        Debug.WriteLine($"新任务id：{newTaskSerial}");

        jsonData.Tasks[newTaskSerial] = new TaskData
        {
            TaskSerial = newTaskSerial,
            Width = width,
            Height = height,
            TaskTitle = "空白标题",
            TaskTarget = new List<TaskTargetDate> { new TaskTargetDate { Target = "空白任务", IsComplete = false } },
            TaskDetails = new List<string> { "空白介绍" }, // 改为字符串数组
        };
        Debug.WriteLine($"添加了新任务：{newTaskSerial}");
        return newTaskSerial;
    }
    

    // 打印所有任务序号
    public void DebugTask()
    {
        if (jsonData != null)
        {
            if (jsonData.Tasks != null)
            {
                foreach (var item in jsonData.Tasks)
                {
                    Debug.WriteLine("任务编号：" + item.Value.TaskSerial);
                }
            }
            else
            {
                Debug.WriteLine("jsonData.Tasks为空");
            }
        }
        else
        {
            Debug.WriteLine("jsonData为空");
        }

    }

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
        if (SubmitTaskListSerial < 0)
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
        // 清除 任务树/指向线/右键菜单 容器里的所有内容
        TaskTreeGrid.Children.Clear();
        DirectionLineGrid.Children.Clear();
        MenuLtemGrid.Children.Clear();
        // 读取基本信息
        TaskTreeGrid.Height = TaskTreeData.jsonData.Panel.Height;
        TaskTreeGrid.Width = TaskTreeData.jsonData.Panel.Width;


        // 读取任务，生成图标
        //Debug.WriteLine("任务列表:");
        foreach (var task in TaskTreeData.jsonData.Tasks)
        {
            ////Debug.WriteLine($"TaskSerial: {task.TaskSerial}, Width: {task.Width}, Height: {task.Height}");
            //task.Value.taskIcon = new TaskIcon(new Thickness(task.Value.Width, task.Value.Height, 0, 0), task.Value.TaskSerial);
            task.Value.taskIcon = new TaskIcon
            {
                TaskSerial = task.Value.TaskSerial,
                thickness = new Thickness(task.Value.Width, task.Value.Height, 0, 0)
            };
            // 添加到容器内并初始化
            TaskTreeGrid.Children.Add(task.Value.taskIcon);
            task.Value.taskIcon.Init();
                
        }
        // 生成指向线
        foreach (var a in TaskTreeData.jsonData.Tasks)
        {
            foreach (var b in TaskTreeData.jsonData.Tasks[a.Key].TaskTargetLine)
            {
                DirectionLine line = new DirectionLine(TaskTreeData.jsonData.Tasks[a.Key], TaskTreeData.jsonData.Tasks[b]);
                TaskTreeData.jsonData.Tasks[a.Key].taskIcon.directionLines.Add(line);
                TaskTreeData.jsonData.Tasks[b].taskIcon.directionLines.Add(line);
            }
        }

    }
}