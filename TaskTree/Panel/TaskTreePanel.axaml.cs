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
    public Point _startPoint;
    public bool move = false;

    public Double TaskTreePanelX = 0; // ��ʼ X ƫ����
    public Double TaskTreePanelY = 0; // ��ʼ Y ƫ����

    public ContextMenu _contextMenu;
    /// <summary>
    /// �Ƿ�������״̬
    /// </summary>
    public bool isConnectionStatus = false;
    /// <summary>
    /// �ĸ������������
    /// </summary>
    public TaskIcon connectionTaskSerial;

    /// <summary>
    /// ��ʱָ����,����Ԥ��ָ���ߴ���Ч��
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

        // ����<
        //new TaskIconPreview(1);
        // ����>
        DebugTask();
    }

    #region ������
    // ����갴��ʱ
    private void Form_OnDragStart(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(sender as Control);
        _startPoint = e.GetPosition(this);
        if (point.Properties.IsLeftButtonPressed)
        {
            Debug.WriteLine("���������");
            // ����Ƿ��Ѿ������Ҽ��˵�
            if (_contextMenu != null)
            {
                MainPanel.Children.Remove(_contextMenu);
                _contextMenu = null;
            }

            move = true;
        }
        if (point.Properties.IsRightButtonPressed)
        {
            Debug.WriteLine("�Ҽ�������");

            �����Ҽ��˵�(e.GetPosition(this));
        }

    }
    // ������ƶ�ʱ
    private void Form_OnDragMove(object? sender, PointerEventArgs e)
    {
        if (move)
        {
            var currentPosition = e.GetPosition(this);
            var deltaX = currentPosition.X - _startPoint.X;
            var deltaY = currentPosition.Y - _startPoint.Y;

            TaskTreePanelX = TaskTreePanelX + deltaX;
            TaskTreePanelY = TaskTreePanelY + deltaY;

            TaskTreeGrid.RenderTransform = new TranslateTransform
            {
                X = TaskTreePanelX, // X ƫ����
                Y = TaskTreePanelY  // Y ƫ����
            };
            DirectionLineGrid.RenderTransform = new TranslateTransform
            {
                X = TaskTreePanelX, // X ƫ����
                Y = TaskTreePanelY  // Y ƫ����
            };

            _startPoint = currentPosition;
        }
    }

    // ������ɿ�ʱ
    private void Form_OnDragEnd(object? sender, PointerReleasedEventArgs e)
    {
        move = false;
    }

    #endregion

    /// <summary>
    /// ��ʼ��
    /// </summary>
    public void initialization()
    {
        // �����ļ�
        bool isFile = Load_JsonFile();


        if (isFile)
        {
            Debug.WriteLine("�ļ� MainPanel.json ���ڣ�");
            // �����л��ļ�����
            Load_TackData();
            // ��������������
            TaskTreePanelUpdate();
        }
        else
        {
            Debug.WriteLine("�ļ� MainPanel.json �����ڡ�");
            // ������ť
            var button = new Button
            {
                Content = "��û��������������һ����!",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10)
            };

            // �󶨵���¼�
            button.Click += Click_NewMainTree;

            // ����ť��ӵ� TaskTreePanel
            MainPanel.Children.Add(button);
        }

    }

    // �½��������İ�ť
    private void Click_NewMainTree(object? sender, RoutedEventArgs e)
    {
        NewTaskFile("MainPanel");
        Load_TackData();

        TaskTreePanelUpdate();
        // ��� sender �Ƿ�Ϊ Control ����
        if (sender is Control control)
        {
            // �� MainPanel ���Ƴ��ÿؼ�
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

    public void �����Ҽ��˵�(Point point)
    {
        // ����Ƿ��Ѿ������Ҽ��˵�
        if (_contextMenu != null)
        {
            MainPanel.Children.Remove(_contextMenu);
            _contextMenu = null;
        }
        var �Ҽ��˵�ѡ�� = new MenuItem
        {
            Header = "�½�����"
        };
        // �󶨵���¼�
        �Ҽ��˵�ѡ��.Click += On��������Clicked;
        var �Ҽ��˵� = new ContextMenu
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(point.X, point.Y - 50, 0, 0),
            Width = 150,
            Height = 55
        };
        �Ҽ��˵�.Items.Add(�Ҽ��˵�ѡ��);

        MainPanel.Children.Add(�Ҽ��˵�);
        _contextMenu = �Ҽ��˵�;

        // ��ֹ�¼�ð��
        �Ҽ��˵�.PointerPressed += (s, e) => e.Handled = true;
    }


    private void On��������Clicked(object? sender, RoutedEventArgs e)
    {
        // ��ȡContextMenu��Marginλ��
        if (_contextMenu != null)
        {
            var margin = _contextMenu.Margin;
            //Debug.WriteLine($"�Ҽ����� Margin: Left={margin.Left}, Top={margin.Top}, Right={margin.Right}, Bottom={margin.Bottom}");
            //Debug.WriteLine( $"TaskTreePanelX = {TaskTreePanelX},TaskTreePanelY = {TaskTreePanelY}" );
            int x = (int)(margin.Left - TaskTreePanelX);
            int y = (int)(margin.Top - TaskTreePanelY);

            // �������ˢ��
            int newTaskId = AddTask(x, y);
            jsonData.Tasks[newTaskId].taskIcon = new TaskIcon 
            {
                TaskSerial = newTaskId,
                thickness = new Thickness(x, y, 0, 0)
            };//new TaskIcon(new Thickness(x, y, 0, 0), newTaskId);
            Debug.WriteLine($"�������񴥷�:������������,X{x},Y{y}");
            // TaskTreePanelUpdate();
            // ɾ���Ҽ��˵�
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
    /// ����������������
    /// <para>�����ˣ�</para>
    /// <para>1.����ͼ�꣨λ����Ϣ����ǩ��Ϣ���Ƿ���ɣ�</para>
    /// <para>2.����ͼ���ָ����</para>
    /// <para>3.�����������ϸ���ݣ�������Ŀ�Լ��Ƿ���ɣ�</para>
    /// </summary>
    public RootData jsonData = new RootData();
    public string filePath;

    /// <summary>
    /// ����json�ļ�·��
    /// </summary>
    public bool Load_JsonFile()
    {
        // ��ȡ����ĸ�Ŀ¼
        string rootDirectory = AppContext.BaseDirectory;
        // ����Ŀ���ļ�������·��
        string assetsFolder = Path.Combine(rootDirectory, "Assets");
        filePath = Path.Combine(assetsFolder, $"MainPanel.json");
        // ����ļ��Ƿ����
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
    /// ��ȡ Json �ļ��������л��� jsonData
    /// </summary>
    public void Load_TackData()
    {
        // ����ļ��Ƿ����
        if (!File.Exists(filePath))
        {
            Debug.WriteLine("�ļ������ڡ�");
            return;
        }

        // ��ȡ�ļ�����
        string content = File.ReadAllText(filePath);

        // �����л� JSON ����
        jsonData = JsonSerializer.Deserialize<RootData>(content, new JsonSerializerOptions { WriteIndented = true });

    }

    /// <summary>
    /// ���л������浽�ļ�
    /// </summary>
    public void Save_TackData()
    {
        // ���л� JSON ����
        string updatedContent = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true });
        Debug.WriteLine($"{updatedContent}");
        DebugTask();
        // д���ļ�
        File.WriteAllText(filePath, updatedContent);

    }

    // �½��µ��������ļ�
    public void NewTaskFile(string name)
    {
        // ��ȡ����ĸ�Ŀ¼
        string rootDirectory = AppContext.BaseDirectory;

        // ����Ŀ���ļ�������·��
        string assetsFolder = Path.Combine(rootDirectory, "Assets");
        string filePath = Path.Combine(assetsFolder, $"{name}.json");

        // ȷ��Ŀ���ļ��д���
        string directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        AddTask(200, 200);

        Save_TackData();
    }

    // ���������
    public int AddTask(int width, int height)
    {
        int newTaskSerial;
        // ���һ�� ����id
        for (; ; )
        {
            uint raw = (uint)Random.Shared.Next(int.MinValue, int.MaxValue); // 0�E0x7FFFFFFE
            raw |= (uint)(Random.Shared.Next(0, 2) << 31);                   // �������λ
            newTaskSerial = unchecked((int)raw);

            if (!jsonData.Tasks.ContainsKey(newTaskSerial))
            {
                // ����ֵ���û������������������ɵ����id���ã��˳�ѭ��
                break;
            }
        }

        Debug.WriteLine($"������id��{newTaskSerial}");

        jsonData.Tasks[newTaskSerial] = new TaskData
        {
            TaskSerial = newTaskSerial,
            Width = width,
            Height = height,
            TaskTitle = "�հױ���",
            TaskTarget = new List<TaskTargetDate> { new TaskTargetDate { Target = "�հ�����", IsComplete = false } },
            TaskDetails = new List<string> { "�հ׽���" }, // ��Ϊ�ַ�������
        };
        Debug.WriteLine($"�����������{newTaskSerial}");
        return newTaskSerial;
    }

    // ɾ������
    public void DeleteTask(int taskSerial)
    {
        if (jsonData.Tasks.ContainsKey(taskSerial))
        {
            jsonData.Tasks[taskSerial].taskIcon.ɾ������ͼ��();
            jsonData.Tasks.Remove(taskSerial);
        }

    }

    // ��ӡ�����������
    public void DebugTask()
    {
        if (jsonData != null)
        {
            if (jsonData.Tasks != null)
            {
                foreach (var item in jsonData.Tasks)
                {
                    Debug.WriteLine("�����ţ�" + item.Value.TaskSerial);
                }
            }
            else
            {
                Debug.WriteLine("jsonData.TasksΪ��");
            }
        }
        else
        {
            Debug.WriteLine("jsonDataΪ��");
        }

    }

    public bool EditMode = false; // �༭ģʽ

    public bool pointingLine = false; // ����ָ����

    // �������༭
    public void Edit_TaskTitle(object? sender, RoutedEventArgs e)
    {
        Edit_TaskTitlePanel.IsVisible = true;
        TaskPanel.IsVisible = false;
        Edit_TaskTitleTextBox.Watermark = TaskPanelTitle.Text;
    }
    public void Edit_TaskTitle1(object? sender, RoutedEventArgs e)
    {
        // ȡ��
        Edit_TaskTitlePanel.IsVisible = false;
        Edit_TaskTitleTextBox.Text = string.Empty;
        TaskPanel.IsVisible = true;
    }
    public void Edit_TaskTitle2(object? sender, RoutedEventArgs e)
    {
        // ȷ��
        Edit_TaskTitlePanel.IsVisible = false;
        Debug.WriteLine(Edit_TaskTitleTextBox.Text);
        if (Edit_TaskTitleTextBox.Text == string.Empty || Edit_TaskTitleTextBox.Text == null)
        {
            Edit_TaskTitleTextBox.Text = "�հױ���";
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
        // ��������Ŀ�༭����
        Edit_TaskTargetPanel.IsVisible = true;
        TaskPanel.IsVisible = false;
        Edit_TaskTargetTextBox.Watermark = "�½�������Ŀ";
    }
    public void Edit_TaskTarget1(object? sender, RoutedEventArgs e)
    {
        // ȡ��
        Edit_TaskTargetPanel.IsVisible = false;
        Edit_TaskTargetTextBox.Text = string.Empty;
        TaskPanel.IsVisible = true;
        SubmitTaskListSerial = -1;
    }
    public void Edit_TaskTarget2(object? sender, RoutedEventArgs e)
    {
        // ȷ��
        Edit_TaskTargetPanel.IsVisible = false;
        if (Edit_TaskTargetTextBox.Text == null || Edit_TaskTargetTextBox.Text == string.Empty)
        {
            Edit_TaskTargetTextBox.Text = "�հ�������Ŀ";
        }
        if (SubmitTaskListSerial < 0)
        {
            Debug.WriteLine("�½�����");
            jsonData.Tasks[OpenTaskSerial].TaskTarget.Add(new TaskTargetDate { Target = Edit_TaskTargetTextBox.Text, IsComplete = false });
        }
        else
        {
            Debug.WriteLine("�޸�����");
            jsonData.Tasks[OpenTaskSerial].TaskTarget[SubmitTaskListSerial].Target = Edit_TaskTargetTextBox.Text;
        }

        Edit_TaskTargetTextBox.Text = string.Empty;
        TaskPanel.IsVisible = true;

        TaskTargetPanel.Children.Clear(); // ���������Ŀ
                                          // ���¼���
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
        // �Ƴ���ǰԪ��
        jsonData.Tasks[OpenTaskSerial].TaskTarget.RemoveAt(ListSerial);

        TaskTargetPanel.Children.Clear(); // ���������Ŀ
                                          // ���¼���
        for (int i = 0; i < jsonData.Tasks[OpenTaskSerial].TaskTarget.Count; i++)
        {
            TaskTargetPanel.Children.Add(new TaskTarget()
            {
                Tag = i
            });
        }
    }




    public int SubmitTaskListSerial; // ѡ�е�������Ŀ
    public int OpenTaskSerial; // ��ǰ�򿪵�����

    // ������Ƭ
    public void OpenTaskPlanel(int TaskSerial)
    {
        // Debug.WriteLine("���óɹ�");
        OpenTaskSerial = TaskSerial;
        TaskPanelTitle.Text = jsonData.Tasks[TaskSerial].TaskTitle;

        Debug.WriteLine("����Ƭ:��ǰ����:" + TaskSerial);
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
            //Debug.WriteLine("����Ƭ:��ǰ���������Ŀ:" + i);
            //Debug.WriteLine("����Ƭ:���������Ŀ������:" + MainWindow.instance.jsonData.Data[taskData.TaskSerial].TaskTarget[i].Target);
        }

        TaskPanel.IsVisible = true;
        TaskPanelBackground.IsVisible = true;
    }

    // ����Ƭ : �رհ�ť
    private void Click_TaskPanelClose(object? sender, RoutedEventArgs e)
    {
        TaskPanel.IsVisible = false;
        TaskPanelBackground.IsVisible = false;
        TaskTargetPanel.Children.Clear(); // ���������Ŀ

        // �رտ�Ƭ�༭ģʽ
        EditMode = false;
        Edit_TaskPanelButton.Foreground = new SolidColorBrush(Colors.Black);
        TaskPanel.BorderBrush = new SolidColorBrush(Colors.LightGray);

        TaskPanelTitle.IsVisible = true; // ��ʾ����
        TaskPanelTitle_Edit.IsVisible = false;

        New_TaskTarget.IsVisible = false; // �������������İ�ť
    }

    // ����Ƭ : �༭��ť
    private void Click_TaskPanelEdit(object? sender, RoutedEventArgs e)
    {
        if (EditMode)
        {
            EditMode = false;
            // ���ð�ť������ɫΪ��ɫ
            Edit_TaskPanelButton.Foreground = new SolidColorBrush(Colors.Black);
            TaskPanel.BorderBrush = new SolidColorBrush(Colors.LightGray);

            TaskPanelTitle.IsVisible = true; // ��ʾ����
            TaskPanelTitle_Edit.IsVisible = false;

            TaskTargetPanel.Children.Clear(); // ���������Ŀ
                                              // ���¼���
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

            // �������������İ�ť
            New_TaskTarget.IsVisible = false;
        }
        else
        {
            EditMode = true;
            // ���ð�ť������ɫΪ��ɫ
            Edit_TaskPanelButton.Foreground = new SolidColorBrush(Colors.Green);
            TaskPanel.BorderBrush = new SolidColorBrush(Colors.Green);

            TaskPanelTitle.IsVisible = false; // ���ر���
            TaskPanelTitle_Edit.Content = TaskPanelTitle.Text; // ����༭��ť������Ϊ
            TaskPanelTitle_Edit.IsVisible = true;

            // �޸�������Ŀ����İ�ťΪ�޸İ�ť
            TaskTargetPanel.Children.Clear(); // ���������Ŀ
                                              // ���¼���
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

            // ��ʾ���������İ�ť
            New_TaskTarget.IsVisible = true;
        }
    }

    // ȷ����� : ȡ����ť
    private void Click_TaskSubmitConfirmationA(object? sender, RoutedEventArgs e)
    {
        SubmitConfirmation.IsVisible = false;
        TaskPanel.IsVisible = true;
        SubmitTaskListSerial = -1;
    }
    // ȷ����� : ȷ����ť
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
                // �Ƴ���ǰԪ��
                jsonData.Tasks[OpenTaskSerial].TaskTarget.RemoveAt(SubmitTaskListSerial);
            }
        }

        SubmitConfirmation.IsVisible = false;
        TaskPanel.IsVisible = true;

        TaskTargetPanel.Children.Clear(); // ���������Ŀ
                                          // ���¼���
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

    // ��ȷ�����/�����޸ĵĽ���
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

    // ��ɾ��ȷ�ϵĽ���
    public void OpenTargetDelPlane(int ListSerial)
    {
        SubmitTaskListSerial = ListSerial;
        TaskPanel.IsVisible = false;
        SubmitConfirmation.IsVisible = true;
    }

    /// <summary>
    /// ����������ͼ,ע��:�˷���������������ݺ���������
    /// </summary>
    public void TaskTreePanelUpdate()
    {
        // ���������/ָ������������������
        TaskTreeGrid.Children.Clear();
        DirectionLineGrid.Children.Clear();
        // ��ȡ������Ϣ
        if (jsonData.Panel != null)
        {
            TaskTreeGrid.Height = jsonData.Panel.Height;
            TaskTreeGrid.Width = jsonData.Panel.Width;
        }

        // ��ȡ��������ͼ��
        if (jsonData.Tasks != null)
        {
            //Debug.WriteLine("�����б�:");
            foreach (var task in jsonData.Tasks)
            {
                ////Debug.WriteLine($"TaskSerial: {task.TaskSerial}, Width: {task.Width}, Height: {task.Height}");
                //task.Value.taskIcon = new TaskIcon(new Thickness(task.Value.Width, task.Value.Height, 0, 0), task.Value.TaskSerial);
                task.Value.taskIcon = new TaskIcon
                {
                    TaskSerial = task.Value.TaskSerial,
                    thickness = new Thickness(task.Value.Width, task.Value.Height, 0, 0)
                };
                // ��ӵ������ڲ���ʼ��
                TaskTreeGrid.Children.Add(task.Value.taskIcon);
                task.Value.taskIcon.Init();
                
            }
        }
        // ����ָ����
        foreach (var a in jsonData.Tasks)
        {
            foreach (var b in jsonData.Tasks[a.Key].TaskTargetLine)
            {
                DirectionLine line = new DirectionLine(jsonData.Tasks[a.Key], jsonData.Tasks[b]);
                jsonData.Tasks[a.Key].taskIcon.directionLines.Add(line);
                jsonData.Tasks[b].taskIcon.directionLines.Add(line);
            }
        }

    }
}