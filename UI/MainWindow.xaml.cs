﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Panuon.UI.Silver;
using LiveCharts;
using LiveCharts.Wpf;
using UI.Process;
using System.ComponentModel;

namespace UI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public SeriesCollection SeriesCollection { get; set; }

        // 关于番茄时钟倒计时
        private TimeCount timeCount;

        private DispatcherTimer timer;
        //

        public MainWindow()
        {
            InitializeComponent();
            
            this.DataContext = new ViewModel();

            this.Loaded += new RoutedEventHandler(TomatoClock_OnLoaded); //***加载倒计时

            PointLabel = chartPoint =>
             string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);

            SeriesCollection = new SeriesCollection
            {
                new ColumnSeries
                {
                    Values = new ChartValues<decimal> {5, 6, 2, 7}
                }
            };
            DataContext = this;
        }
/// <summary>
/// Button点击更改按钮背景
/// </summary>
        public class ViewModel : INotifyPropertyChanged
        {
            private bool _isPlaying = true;
            private RelayCommand _playCommand;

            public ViewModel()
            {
                isPlaying = true;
            }

            public bool isPlaying
            {
                get { return _isPlaying; }
                set
                {
                    _isPlaying = value;
                    OnPropertyChanged("isPlaying");
                }
            }

            public ICommand PlayCommand
            {
                get
                {
                    return _playCommand ?? new RelayCommand((x) =>
                    {
                        var buttonType = x.ToString();

                        if (null != buttonType)
                        {
                            if (buttonType.Contains("Start"))
                            {
                                isPlaying = true;
                            }
                            else if (buttonType.Contains("Pause"))
                            {
                                isPlaying = false;
                            }
                        }
                    });
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            public void OnPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }

        public class RelayCommand : ICommand
        {
            private readonly Predicate<object> _canExecute;
            private readonly Action<object> _execute;

            public event EventHandler CanExecuteChanged;

            public RelayCommand(Action<object> execute) : this(execute, null) { }

            public RelayCommand(Action<object> execute, Predicate<object> canExecute)
            {
                _execute = execute;
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter)
            {

                if (_canExecute == null)
                {
                    return true;
                }

                return _canExecute(parameter);
            }

            public void Execute(object parameter)
            {
                _execute(parameter);
            }

            public void RaiseCanExecuteChanged()
            {
                if (CanExecuteChanged != null)
                {
                    CanExecuteChanged(this, EventArgs.Empty);
                }
            }
        }

        //***********************

        public Func<ChartPoint, string> PointLabel { get; set; }

        private void Chart_OnDataClick(object sender, ChartPoint chartpoint)
        {
            var chart = (LiveCharts.Wpf.PieChart) chartpoint.ChartView;

            //clear selected slice.
            foreach (PieSeries series in chart.Series)
                series.PushOut = 0;

            var selectedSeries = (PieSeries) chartpoint.SeriesView;
            selectedSeries.PushOut = 8;
        }


        private void TomatoClock_OnLoaded(object sender, RoutedEventArgs e)
        {
            //设置定时器

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(10000000); //时间间隔为一秒
            timer.Tick += new EventHandler(timer_Tick);
            //转换成秒数
            Int32 hour = Convert.ToInt32(HourArea.Text);
            Int32 minute = Convert.ToInt32(MinuteArea.Text);
            Int32 second = Convert.ToInt32(SecondArea.Text);

            //处理倒计时的类
            timeCount = new TimeCount(hour * 3600 + minute * 60 + second);
            CountDown += new CountDownHandler(timeCount.TimeCountDown);
           //  timer.Start();
        }

        /// <summary>
        /// 处理倒计时的委托
        /// </summary>

        public delegate bool CountDownHandler();

        /// <summary>
        /// 处理事件
        /// </summary>
        public event CountDownHandler CountDown;

        public bool OnCountDown()
        {
            if (CountDown != null)
                return CountDown();

            return false;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (OnCountDown())
            {
                HourArea.Text = timeCount.GetHour();
                MinuteArea.Text = timeCount.GetMinute();
                SecondArea.Text = timeCount.GetSecond();
            }
            else
                timer.Stop();
        }

        private void File_DragEnter(object sender, DragEventArgs e)
        {
            //MessageBox.Show("File Drop Enter");
            Debug.WriteLine("drag in");
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Link;
            else
                e.Effects = DragDropEffects.None;
        }

        private void File_Drop(object sender, DragEventArgs e)
        {
            MessageBox.Show("File Drop");
            Debug.WriteLine("drop");
            TagWindow tagWindow=new TagWindow();
            tagWindow.Show();
        }

      

        private void TimeCountStart_OnClick(object sender, RoutedEventArgs e)
        {

 
            timer.Start();
           ImageSource pause = new BitmapImage(new Uri("Icon/Pause.jpg", UriKind.Relative));

           this.ButtonImage.Source = pause;


        }

        private void TimeCountPause_Click(object sender, MouseButtonEventArgs e)
        {
            timer.Stop();
        }
    }
}


