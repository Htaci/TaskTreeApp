using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Remote.Protocol.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using MouseButton = Avalonia.Input.MouseButton;
using Path = System.IO.Path;
using Point = Avalonia.Point;

namespace TaskTree
{
    public partial class MainWindow : Window
    {
        public Point _startPoint;
        public bool move = false;

        public Double TaskTreePanelX = 0; // 初始 X 偏移量
        public Double TaskTreePanelY = 0; // 初始 Y 偏移量

        public ContextMenu _contextMenu;
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

        public static MainWindow? instance;


        public MainWindow()
        {
            InitializeComponent();
            instance = this;
            // 最小化，最大化，关闭窗口
            this.FindControl<Button>("Minimize").Click += (s, e) => this.WindowState = WindowState.Minimized;
            this.FindControl<Button>("Maximize").Click += (s, e) =>
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                    Maximize.Content = "\ue922";
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                    Maximize.Content = "\ue923";
                }
            };
            this.FindControl<Button>("Close").Click += (s, e) =>
            {
                Save_TackData(); // 保存
                this.Close(); // 退出程序
            };
            this.FindControl<Button>("Refresh").Click += (s, e) =>
            {
                // 调用刷新程序
                TaskTreePanelUpdate();
            };
            initialization();
            Debug.WriteLine(""); 
            Line1.EndPoint = new Point(300, 30);

            // 测试<
            //new TaskIconPreview(1);
            // 测试>
            DebugTask();
        }

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
                
                move = true;
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
            if (move)
            {
                var currentPosition = e.GetPosition(this);
                var deltaX = currentPosition.X - _startPoint.X;
                var deltaY = currentPosition.Y - _startPoint.Y;

                TaskTreePanelX = TaskTreePanelX + deltaX;
                TaskTreePanelY = TaskTreePanelY + deltaY;

                TaskTreePanel.RenderTransform = new TranslateTransform
                {
                    X = TaskTreePanelX, // X 偏移量
                    Y = TaskTreePanelY  // Y 偏移量
                };
                DirectionLinePanel.RenderTransform = new TranslateTransform
                {
                    X = TaskTreePanelX, // X 偏移量
                    Y = TaskTreePanelY  // Y 偏移量
                };

                _startPoint = currentPosition;
            }
        }
        private void Form_OnDrag(object? sender, PointerPressedEventArgs e)
        {
            if (e.Pointer.Type == PointerType.Mouse) this.BeginMoveDrag(e);
        }
        // 当鼠标松开时
        private void Form_OnDragEnd(object? sender, PointerReleasedEventArgs e)
        {
            move = false;
        }

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
                Margin = new Thickness(point.X, point.Y - 50,0,0),
                Width = 150,
                Height = 55
            };
            右键菜单.Items.Add(右键菜单选项);

            MainPanel.Children.Add(右键菜单);
            _contextMenu = 右键菜单;

            // 阻止事件冒泡
            右键菜单.PointerPressed += (s, e) => e.Handled = true;
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
                jsonData.Tasks[newTaskId].taskIcon = new TaskIcon(new Thickness(x,y,0,0), newTaskId);
                Debug.WriteLine($"创建任务触发:创建了新任务,X{x},Y{y}");
                // TaskTreePanelUpdate();
                // 删除右键菜单
                MainPanel.Children.Remove(_contextMenu);
                _contextMenu = null;
            }
        }
    }
}