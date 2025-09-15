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
    /// ������
    /// </summary>
    public int TaskSerial { get; set; }
    /// <summary>
    /// ָ�����б�
    /// </summary>
    public List<DirectionLine> directionLines = new List<DirectionLine>();

    /// <summary>
    /// �Ҽ��˵�
    /// </summary>
    public ContextMenu contextMenu;

    //public Border Taskborder;
    /// <summary>
    /// ����ͼ�������
    /// </summary>
    public Thickness thickness { get; set; }

    public TaskIcon()
    {
        InitializeComponent();
        // ��ֹ�¼�ð��
        Taskborder.PointerPressed += (s, e) => e.Handled = true;
    }
    /// <summary>
    /// ��ʼ��
    /// </summary>
    public void Init()
    {
        // ����UserControl��Margin
        this.Margin = thickness;
        //Debug.WriteLine($"����{TaskSerial}�����꣺{Margin}");

        bool IsTaskComplete = true;
        // �жϴ������Ƿ���ɣ����û��������Ŀ��Ĭ��û�����
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
        // ���������Ƿ����������ɫ
        if (IsTaskComplete)
            Taskborder.BorderBrush = new SolidColorBrush(Colors.Green);
        else
            Taskborder.BorderBrush = new SolidColorBrush(Colors.White);


        textBlock.Text = "\ue922";
    }


    private void Button_Icon(object? sender, PointerPressedEventArgs e)
    {
        Debug.WriteLine($"����ID��{TaskSerial}�������");

        // ����Ƿ��Ѿ������Ҽ��˵�
        if (TaskTreePanel.instance._contextMenu != null)
        {
            TaskTreePanel.instance.MainPanel.Children.Remove(TaskTreePanel.instance._contextMenu);
            TaskTreePanel.instance._contextMenu = null;
        }

        var point = e.GetCurrentPoint(sender as Control);
        if (point.Properties.IsLeftButtonPressed)   // ������
        {
            Debug.WriteLine("���������");



            if (!TaskTreePanel.instance.isConnectionStatus)
            {
                TaskData taskData = TaskTreePanel.instance.jsonData.Tasks[TaskSerial];
                //MainWindow.instance.OpenTaskPlanel(taskData);
                Debug.WriteLine($"����ID��{TaskSerial} �������񣺴��������");
                TaskTreePanel.instance.OpenTaskPlanel(TaskSerial);
            }

            // �������������״̬ , �Ҹ�ͼ�겻�Ƿ��������ͼ��ʱ, ��������
            if (TaskTreePanel.instance.isConnectionStatus && TaskTreePanel.instance.connectionTaskSerial.TaskSerial != TaskSerial)
            {
                // ���Ԥ��
                TaskTreePanel.instance.previewDirectionLine.RemoveDirectionLine();
                TaskTreePanel.instance.previewDirectionLine = null;
                TaskTreePanel.instance.isConnectionStatus = false;
                // ����һ���µ�ָ����
                DirectionLine line = new DirectionLine(TaskTreePanel.instance.jsonData.Tasks[TaskTreePanel.instance.connectionTaskSerial.TaskSerial], TaskTreePanel.instance.jsonData.Tasks[TaskSerial]);
                directionLines.Add(line);
                TaskTreePanel.instance.connectionTaskSerial.directionLines.Add(line);
                // ��ӵ�json
                TaskTreePanel.instance.jsonData.Tasks[TaskTreePanel.instance.connectionTaskSerial.TaskSerial].TaskTargetLine.Add(TaskSerial);
                Debug.WriteLine($"��ʼ����{TaskTreePanel.instance.connectionTaskSerial.TaskSerial}��ָ�����б���{TaskTreePanel.instance.connectionTaskSerial.directionLines.Count}��");
                Debug.WriteLine($"��ָ������{TaskSerial}��ָ�����б���{directionLines.Count}��");
                // ��������
                TaskTreePanel.instance.connectionTaskSerial = null;
            }


        }
        if (point.Properties.IsRightButtonPressed)  // �Ҽ����
        {
            Debug.WriteLine("�Ҽ������£�ͼ�꣩");



            �����Ҽ��˵�(e.GetPosition(TaskTreePanel.instance));
        }

        
    }

    public void �����Ҽ��˵�(Point point)
    {
        // ����Ƿ��Ѿ������Ҽ��˵�
        if (TaskTreePanel.instance?._contextMenu != null)
        {
            TaskTreePanel.instance.MainPanel.Children.Remove(TaskTreePanel.instance._contextMenu);
            TaskTreePanel.instance._contextMenu = null;
        }
        var �Ҽ��˵�ѡ�� = new MenuItem
        {
            Header = "��������",
        };
        var �Ҽ��˵�ѡ��2 = new MenuItem
        {
            Header = "�ƶ�λ��",
        };
        var �Ҽ��˵�ѡ��3 = new MenuItem
        {
            Header = "ɾ������",
        };
        // �󶨵���¼�
        �Ҽ��˵�ѡ��.Click += On��������Clicked;
        �Ҽ��˵�ѡ��2.Click += On�ƶ�����Clicked;
        �Ҽ��˵�ѡ��3.Click += Onɾ������Clicked;
        var �Ҽ��˵� = new ContextMenu
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(point.X, point.Y - 40, 0, 0),
            Width = 150,

        };
        �Ҽ��˵�.Items.Add(�Ҽ��˵�ѡ��);
        �Ҽ��˵�.Items.Add(�Ҽ��˵�ѡ��2);
        �Ҽ��˵�.Items.Add(�Ҽ��˵�ѡ��3);


        TaskTreePanel.instance.MainPanel.Children.Add(�Ҽ��˵�);
        TaskTreePanel.instance._contextMenu = �Ҽ��˵�;

        // ��ֹ�¼�ð��
        �Ҽ��˵�.PointerPressed += (s, e) => e.Handled = true;
    }

    private void On��������Clicked(object? sender, RoutedEventArgs e)
    {
        // �����ﴦ�����¼�
        Debug.WriteLine("�������񴥷�");
        TaskTreePanel.instance.connectionTaskSerial = this;
        TaskTreePanel.instance.isConnectionStatus = true;

        // ����Ƿ��Ѿ������Ҽ��˵�
        if (TaskTreePanel.instance?._contextMenu != null)
        {
            TaskTreePanel.instance.MainPanel.Children.Remove(TaskTreePanel.instance._contextMenu);
            TaskTreePanel.instance._contextMenu = null;
        }
    }

    private void On�ƶ�����Clicked(object? sender, RoutedEventArgs e)
    {
        // �����ﴦ�����¼�
        // ���ظÿؼ�
        Taskborder.IsVisible = false;
        // ����һ���µ�Ԥ��ͼ��
        new TaskIconPreview(TaskTreePanel.instance.jsonData.Tasks[TaskSerial]);

        // ����Ƿ��Ѿ������Ҽ��˵�
        if (TaskTreePanel.instance?._contextMenu != null)
        {
            TaskTreePanel.instance.MainPanel.Children.Remove(TaskTreePanel.instance._contextMenu);
            TaskTreePanel.instance._contextMenu = null;
        }
    }

    private void Onɾ������Clicked(object? sender, RoutedEventArgs e)
    {
        TaskTreePanel.instance.DeleteTask(TaskSerial);

        // ����Ƿ��Ѿ������Ҽ��˵�
        if (TaskTreePanel.instance?._contextMenu != null)
        {
            TaskTreePanel.instance.MainPanel.Children.Remove(TaskTreePanel.instance._contextMenu);
            TaskTreePanel.instance._contextMenu = null;
        }
    }
    // ������ʱ
    private void Border_PointerEnter(object? sender, PointerEventArgs e)
    {
        // �������ָ��Ϊ����ͼ��
        TaskTreePanel.instance.Cursor = new Cursor(StandardCursorType.Hand);

        // �������������״̬ , �Ҹ�ͼ�겻�Ƿ��������ͼ��
        if (TaskTreePanel.instance.isConnectionStatus && TaskTreePanel.instance.connectionTaskSerial.TaskSerial != TaskSerial)
        {
            DirectionLine directionLine = new DirectionLine(TaskTreePanel.instance.jsonData.Tasks[TaskTreePanel.instance.connectionTaskSerial.TaskSerial], TaskTreePanel.instance.jsonData.Tasks[TaskSerial]);
            directionLine.���� = true;
            directionLine.StartAsyncTask();

            TaskTreePanel.instance.previewDirectionLine = directionLine;
        }

        foreach (var s in directionLines)
        {
            s.���� = true;
            s.StartAsyncTask();
        }
    }
    // ����뿪ʱ
    private void Border_PointerLeave(object? sender, PointerEventArgs e)
    {
        // �ָ�Ĭ�����ָ��
        TaskTreePanel.instance.Cursor = new Cursor(StandardCursorType.Arrow);

        // �������������״̬ , �Ҹ�ͼ�겻�Ƿ��������ͼ��
        if (TaskTreePanel.instance.isConnectionStatus && TaskTreePanel.instance.connectionTaskSerial.TaskSerial != TaskSerial)
        {
            //MainWindow.instance.previewDirectionLine.���� = false;
            TaskTreePanel.instance.previewDirectionLine.RemoveDirectionLine();
            TaskTreePanel.instance.previewDirectionLine = null;
        }

        foreach (var s in directionLines)
        {
            s.���� = false;
        }
    }
    // ˢ������ָ����
    public void ˢ������ָ����()
    {
        foreach (var s in directionLines)
        {
            // ɾ��ָ��������
            s.RemoveDirectionLine();

            //Debug.WriteLine($"ָ���߲��ԣ���ʼ�㣺{s.start.Width}/{s.start.Height}/������{s.start.TaskSerial}��" +
            //    $"�����㣺{s.end.Width}/{s.end.Height}/������{s.end.TaskSerial}");

            // ��������ָ����
            s.NewDirectionLine();
        }
    }

    public void ˢ������ͼ��()
    {
        Taskborder.Margin = new Thickness(TaskTreePanel.instance.jsonData.Tasks[TaskSerial].Width,
            TaskTreePanel.instance.jsonData.Tasks[TaskSerial].Height, 0, 0);


        bool IsTaskComplete = true;
        // �жϴ������Ƿ���ɣ����û��������Ŀ��Ĭ��û�����
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

    public void ɾ������ͼ��()
    {
        Debug.WriteLine($"����{TaskSerial} ��ɾ��");

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
