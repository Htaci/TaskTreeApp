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

        // 初始化，在Program中调用，会在程序启动时最开始执行
        public static void TaskTreeData_initialization()
        {
            // 获取程序的根目录
            string rootDirectory = AppContext.BaseDirectory;
            // 构建目标文件的完整路径
            string assetsFolder = Path.Combine(rootDirectory, "Assets");
            string path = Path.Combine(assetsFolder, $"TaskTreeConfig.json");
            // 检查文件是否存在
            if (!File.Exists(path))
            {
                // 如果不存在，创建一个

                File.WriteAllText(path, "{}");
            }

            // 读取文件内容
            string content = File.ReadAllText(path);
            // 将 JSON 字符串反序列化为对象
            taskSettings = JsonSerializer.Deserialize<TaskSettings>(content, new JsonSerializerOptions { WriteIndented = true });
            Debug.WriteLine("");
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
