﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using AgileDesignThemes.Wpf.Controls;
using AgileDesignThemes.Wpf.Enums;
using AgileDesignThemes.Wpf.Helper;
using AgileDesignThemes.Wpf.Panels;

namespace AgileDesignThemes.Wpf
{
    /// <summary>
    /// 全局消息通知
    /// </summary>
    public class Message : Control
    {


        #region Message Control
        private static readonly int DefaultSeconds = 3;

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(Message), new PropertyMetadata(default(string)));
        /// <summary>
        /// 显示的文本
        /// </summary>
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty MessageTypeProperty = DependencyProperty.Register(
            "MessageType", typeof(MessageType), typeof(Message), new PropertyMetadata(default(MessageType)));
        /// <summary>
        /// 消息类型
        /// </summary>
        public MessageType MessageType
        {
            get => (MessageType)GetValue(MessageTypeProperty);
            set => SetValue(MessageTypeProperty, value);
        }

        public static readonly DependencyProperty SecondsProperty = DependencyProperty.Register(
            "Seconds", typeof(int), typeof(Message), new PropertyMetadata(DefaultSeconds));
        /// <summary>
        /// 持续时间(ms)
        /// </summary>
        public int Seconds
        {
            get => (int)GetValue(SecondsProperty);
            set => SetValue(SecondsProperty, value);
        }

        private static ResourceDictionary resourceDictionary = new() { Source = new Uri("pack://application:,,,/AgileDesignThemes.Wpf;component/Themes/ControlThemes/AgileDesignTheme.Message.xaml", UriKind.Absolute) };
        public async void Close()
        {
            await Task.Delay(new TimeSpan(0, 0, 0, Seconds));

            //var opacityAnimation = new DoubleAnimation(1, 0.5, new Duration(new TimeSpan(3000)));
            //var scaleAnimation = new DoubleAnimation(1, 0.5, new Duration(new TimeSpan(3000)));

            //ScaleTransform scale = new ScaleTransform();
            //this.RenderTransform = scale;
            //var clock = scaleAnimation.CreateClock();
            //clock.Completed += (obj, args) =>
            //{
            //    //MessagePanel?.Children.Remove(this);

            //};
            //this.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            //scale.ApplyAnimationClock(ScaleTransform.ScaleYProperty, clock);
            var storyboard = (Storyboard)resourceDictionary["MsgClose"];
            storyboard?.Begin(this);
            await Task.Delay(300);
            MessagePanel?.Children.Remove(this);
        }


        #endregion

        #region Message Controller

        private static readonly Dictionary<string, Panel> PanelDic = new();
        /// <summary>
        /// 消息容器
        /// </summary>
        private static Panel MessagePanel { get; set; }

        public static readonly DependencyProperty ControllerTokenProperty = DependencyProperty.RegisterAttached(
          "ControllerToken", typeof(string), typeof(Message), new PropertyMetadata(default(string), OnTokenChanged));

        public static string GetControllerToken(DependencyObject element) => (string)element.GetValue(ControllerTokenProperty);
        /// <summary>
        /// 设置唯一标识
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetControllerToken(DependencyObject element, string value) => element.SetValue(ControllerTokenProperty, value);
        /// <summary>
        /// Token property value changed.
        /// </summary>
        /// <param name="sen"></param>
        /// <param name="args"></param>
        private static void OnTokenChanged(DependencyObject sen, DependencyPropertyChangedEventArgs args)
        {
            if (sen is not Panel panel) return;

            if (args.NewValue == null)
                UnRegister(panel);
            else
                Register(panel, args.NewValue.ToString());
        }
        /// <summary>
        /// 注销容器
        /// </summary>
        /// <param name="panel"></param>
        private static void UnRegister(Panel panel)
        {
            var firstDic = PanelDic.FirstOrDefault(a => ReferenceEquals(panel, a.Value));
            if (!string.IsNullOrEmpty(firstDic.Key))
            {
                PanelDic.Remove(firstDic.Key);
                panel.ContextMenu = null;
            }
        }
        /// <summary>
        /// 注册容器
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="token"></param>
        private static void Register(Panel panel, string token)
        {
            if (!PanelDic.ContainsKey(token))
                PanelDic.Add(token, panel);
        }


        public static void Show(MessageInfo msgInfo)
        {

            Application.Current.Dispatcher.Invoke(() =>
            {
                var msgCtl = new Message()
                {
                    Text = msgInfo.Message,
                    MessageType = msgInfo.MessageType,
                    Seconds = msgInfo.Time
                };
                if (!string.IsNullOrEmpty(msgInfo.Token))
                {
                    if (PanelDic.TryGetValue(msgInfo.Token, out var panel))
                    {
                        panel?.Children.Add(msgCtl);
                    }
                    else
                    {
                        MessagePanel ??= CreateDefaultPanel();
                        MessagePanel?.Children.Add(msgCtl);
                        var scrollViewer = VisualHelper.GetParent<ScrollViewer>(MessagePanel);
                        scrollViewer?.ScrollToEnd();
                        msgCtl.Close();
                    }
                }
            });
        }

        private static Panel CreateDefaultPanel()
        {
            // 查找当前激活的窗口
            var element = WindowHelper.GetActiveWindow();
            // 查找当前窗口装饰器的容器
            var decorator = VisualHelper.GetChild<AdornerDecorator>(element);
            var layer = decorator?.AdornerLayer;
            if (layer == null) return null;

            // 创建消息容器
            var panel = new StackPanel()
            {
                VerticalAlignment = VerticalAlignment.Top,
            };
            var scrollViewer = new ScrollViewer()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                Padding = new Thickness(20),
                Content = panel
            };

            var container = new AdornerContainer(layer)
            {
                Child = scrollViewer
            };
            layer.Add(container);

            return panel;
        }
        #endregion

    }



    public class MessageInfo
    {
        public string Message { get; set; }
        public MessageType MessageType { get; set; }
        public int Time { get; set; }
        public string Token { get; set; }
    }

}
