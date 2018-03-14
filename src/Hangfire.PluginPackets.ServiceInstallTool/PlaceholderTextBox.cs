﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Hangfire.PluginPackets.ServiceInstallTool
{
    /// <summary>
    /// 带点位符的文本输入控件
    /// </summary>
    public class PlaceholderTextBox : TextBox
    {
        #region Fields

        /// <summary>
        /// 占位符的文本框
        /// </summary>
        private readonly TextBlock _placeholderTextBlock = new TextBlock();

        /// <summary>
        /// 占位符的画刷
        /// </summary>
        private readonly VisualBrush _placeholderVisualBrush = new VisualBrush();

        #endregion Fields

        #region Properties

        /// <summary>
        /// 占位符的依赖属性
        /// </summary>
        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            "Placeholder", typeof(string), typeof(PlaceholderTextBox),
            new FrameworkPropertyMetadata("请在此输入", FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// 占位符
        /// </summary>
        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        #endregion Properties

        #region Public Methods

        public PlaceholderTextBox()
        {
            var binding = new Binding
            {
                Source = this,
                Path = new PropertyPath("Placeholder")
            };
            _placeholderTextBlock.SetBinding(TextBlock.TextProperty, binding);
            _placeholderTextBlock.FontStyle = FontStyles.Italic;

            _placeholderVisualBrush.AlignmentX = AlignmentX.Left;
            _placeholderVisualBrush.Stretch = Stretch.None;
            _placeholderVisualBrush.Visual = _placeholderTextBlock;

            Background = _placeholderVisualBrush;
            TextChanged += PlaceholderTextBox_TextChanged;
        }

        #endregion Public Methods

        #region Events Handling

        /// <summary>
        /// 文本变化的响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlaceholderTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Background = string.IsNullOrEmpty(Text) ? _placeholderVisualBrush : null;
        }

        #endregion Events Handling

    }
}
