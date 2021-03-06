﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DesktopLearningAssistant.Configuration.Config;
using Newtonsoft.Json;

namespace DesktopLearningAssistant.Configuration
{
    /// <summary>
    /// 配置类，负责读入和管理配置
    /// </summary>
    public class ConfigService
    {
        #region 托管配置

        /// <summary>
        /// 屏幕使用时间统计模块的配置项
        /// </summary>
        public TimeStatisticConfig TSConfig { get; set; }

        #endregion

        /// <summary>
        /// 单例变量
        /// </summary>
        private static ConfigService uniqueConfigService;

        /// <summary>
        /// 确保线程同步的锁标识
        /// </summary>
        private static readonly object locker = new object();

        /// <summary>
        /// 配置文件路径
        /// </summary>
        private static string configPath = ".\\Config.json";

        /// <summary>
        /// 构造函数
        /// </summary>
        public ConfigService()
        {
            TSConfig = new TimeStatisticConfig();
        }

        /// <summary>
        /// 获取单例对象
        /// </summary>
        /// <returns></returns>
        public static ConfigService GetConfigService()
        {
            if (uniqueConfigService != null) return uniqueConfigService;

            lock (locker)
            {
                if (File.Exists(configPath))
                {
                    LoadFromJson();     // 配置文件存在时，从配置文件中加载配置信息
                }
                else
                {
                    uniqueConfigService = new ConfigService();      // 否则生成一个新的配置类
                }
            }
            return uniqueConfigService;
        }

        /// <summary>
        /// 从JSON文件中加载配置信息
        /// </summary>
        private static void LoadFromJson()
        {
            try
            {
                Object obj = JsonConvert.DeserializeObject(File.ReadAllText(configPath), typeof(ConfigService));
                if (obj != null) uniqueConfigService = obj as ConfigService;
            }
            catch (Exception)
            {
                throw;      // TODO: 文件异常处理和写入Log日志
            }
            
        }

        /// <summary>
        /// 将配置类写入JSON文件
        /// </summary>
        private static void SaveAsJson()
        {
            string jsonStr = JsonConvert.SerializeObject(uniqueConfigService);
            try
            {
                File.WriteAllText(configPath, jsonStr);
            }
            catch (Exception)
            {
                throw;          // TODO: 写入Log日志
            }
        }
    }
}
