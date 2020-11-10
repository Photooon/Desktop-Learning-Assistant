﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DesktopLearningAssistant.TomatoClock.Model;
using DesktopLearningAssistant.TomatoClock;
using DesktopLearningAssistant.TomatoClock.SQLite;

namespace UI.Tomato
{
    /// <summary>
    /// AllTasksWindow.xaml 的交互逻辑
    /// </summary>
    ///
 
    public partial class AllTasksWindow : Window
    {
        public TaskInfo taskinfo;
        public TaskService tasksercive;

        public AllTasksWindow()
        {
           InitializeComponent(); 

           taskinfo=new TaskInfo();
           int taskid = taskinfo.TaskID;
           string name = taskinfo.Name;
           DateTime startTime = taskinfo.StartTime;
           DateTime deadLine = taskinfo.Deadline;
           int tomatoNum = taskinfo.TomatoNum;
           int tomatoCount = taskinfo.TomatoCount;
           int taskState = taskinfo.TaskState;
           string notes = taskinfo.Notes;

           AllTasksDataGrid.Items.Add(new {taskid, name, startTime, deadLine, tomatoNum, tomatoCount, taskState,notes});

        }


        private void Modify_OnClick(object sender, RoutedEventArgs e)
        {
            if (AllTasksDataGrid.SelectedItem != null)
            {
                tasksercive.ModifyTask(taskinfo);
                MessageBox.Show("修改成功", "提示");
            }
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            if (AllTasksDataGrid.SelectedItem != null)
            {
                taskinfo.TaskID = int.Parse(AllTasksDataGrid.SelectedValue.ToString());
                tasksercive.DeletTask(taskinfo.TaskID);
                MessageBox.Show("删除成功", "提示");
            }
        }

        private void Add_OnClick(object sender, RoutedEventArgs e)
        {
            tasksercive.AddTask(taskinfo);
            MessageBox.Show("添加成功", "提示");

        }
    }

  

}
