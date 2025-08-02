using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Remote.Protocol.Input;
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
    //public class TaskData
    //{

    //}
    public partial class MainWindow : Window
    {
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

            AddTask(200,200);

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

        // 删除任务
        public void DeleteTask(int taskSerial)
        {
            if (jsonData.Tasks.ContainsKey(taskSerial))
            {
                jsonData.Tasks[taskSerial].taskIcon.删除任务图标();
                jsonData.Tasks.Remove(taskSerial);
            }

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
    }

    // JSON 数据结构

    // 放置图标的面板属性
    public class PanelData
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
    /// <summary>
    /// 单个图标任务的数据
    /// </summary>
    public class TaskData
    {
        public int TaskSerial { get; set; } // 任务编号
        // 任务图标坐标
        public int Width { get; set; } // x坐标
        public int Height { get; set; }// y坐标
        // 任务信息
        public string TaskTitle { get; set; } // 任务标题
        public List<TaskTargetDate> TaskTarget { get; set; } = new List<TaskTargetDate>(); // 子任务条目
        public List<string> TaskDetails { get; set; } = new List<string>(); // 任务细节

        // 连接属性（该任务图标与哪些图标建立了指向性连接）
        public List<int> TaskTargetLine { get; set; } = new List<int>();

        public TaskIcon? taskIcon;
    }
    public class RootData
    {
        public PanelData Panel { get; set; } // 任务树呈现面板数据
        public Dictionary<int, TaskData> Tasks { get; set; }= new Dictionary<int, TaskData>(); // 任务数据
    }

    // 任务条目
    public class TaskTargetDate
    {
        public string Target {  get; set; } // 任务名称
        public bool IsComplete { get; set; } // 是否完成
    }



}
