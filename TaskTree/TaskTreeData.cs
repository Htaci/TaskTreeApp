using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TaskTree
{
    /// <summary>
    /// 数据的保存/加载/修改
    /// </summary>
    public static class TaskTreeData
    {
        /// <summary>
        /// 单个任务树的数据
        /// </summary>
        public static RootData jsonData = new RootData();

        /// <summary>
        /// 设置参数和已记录的任务树
        /// </summary>
        public static TaskSettings taskSettings = new TaskSettings();

        private static string _path;

        // 初始化，在Program中调用，会在程序启动时最开始执行
        public static void TaskTreeData_initialization()
        {
            // 获取程序的根目录
            string rootDirectory = AppContext.BaseDirectory;
            // 构建目标文件的完整路径
            string assetsFolder = Path.Combine(rootDirectory, "Assets");
            //string path = Path.Combine(assetsFolder, $"TaskTreeConfig.json");
            _path = Path.Combine(assetsFolder, $"MainPanel.json");
            
            // 检查文件是否存在
            if (!File.Exists(_path))
            {
                // 如果不存在，创建一个

                File.WriteAllText(_path, "{}");
            }

            // 读取文件内容
            string content = File.ReadAllText(_path);
            // 将 JSON 字符串反序列化为对象
            taskSettings = JsonSerializer.Deserialize<TaskSettings>(content, new JsonSerializerOptions { WriteIndented = true });
            Debug.WriteLine("");
        }
        
        // 添加新任务
        public static int AddTask(int width, int height)
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
        
        
        /// <summary>
        /// 序列化并保存到文件
        /// </summary>
        public static void Save_TackData()
        {
            // 序列化 JSON 数据
            string updatedContent = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true });
            Debug.WriteLine($"{updatedContent}");
            DebugTask();
            // 写入文件
            File.WriteAllText(_path, updatedContent);

        }
        
        // 打印所有任务序号
        public static void DebugTask()
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
        
        // 删除任务
        public static void DeleteTask(int taskSerial)
        {
            if (jsonData.Tasks.ContainsKey(taskSerial))
            {
                jsonData.Tasks[taskSerial].taskIcon.删除任务图标();
                jsonData.Tasks.Remove(taskSerial);
            }

        }
        
        
    }
    
    
    


    #region 数据结构


    /// <summary>
    /// 放置任务图标的面板属性
    /// </summary>
    public class PanelData
    {
        public int Width { get; set; } // 感觉这俩个很没用，回头换成记录上次停留的位置，把偏移保存下来
        public int Height { get; set; }
    }
    /// <summary>
    /// 单个任务的数据
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
        public PanelData Panel { get; set; } // 面板数据
        public Dictionary<int, TaskData> Tasks { get; set; } = new Dictionary<int, TaskData>(); // 任务数据
    }

    /// <summary>
    /// 每个任务中的单个任务条目，也可以理解为一个任务下的子任务，所有子任务完成时才视为任务完成。
    /// </summary>
    public class TaskTargetDate
    {
        public string Target { get; set; } // 任务名称
        public bool IsComplete { get; set; } // 是否完成
    }

    /// <summary>
    /// App 设置，记录了设置，和已添加的任务树文件，
    /// </summary>
    public class TaskSettings
    {

    }


    #endregion
}
