using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AnimatedTimer
{
    public class AnimatedTimerView : Control
    {
        private StackPanel layoutRoot;
        private AnimatedNumber minuteNum1;
        private AnimatedNumber minuteNum2;
        private AnimatedNumber secondsNum1;
        private AnimatedNumber secondsNum2;

        private IEnumerable logicalChildrenFactory;

        public AnimatedTimerView()
        {
            layoutRoot = new StackPanel()
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                Orientation = Orientation.Horizontal,
            };

            minuteNum2 = new AnimatedNumber();
            minuteNum1 = new AnimatedNumber()
            {
                BeginTime = TimeSpan.FromMilliseconds(80),
            };
            secondsNum2 = new AnimatedNumber()
            {
                BeginTime = TimeSpan.FromMilliseconds(160),
            };
            secondsNum1 = new AnimatedNumber()
            {
                BeginTime = TimeSpan.FromMilliseconds(240),
            };

            var label = new TextBlock()
            {
                Text = ":"
            };

            layoutRoot.Children.Add(minuteNum2);
            layoutRoot.Children.Add(minuteNum1);
            layoutRoot.Children.Add(label);
            layoutRoot.Children.Add(secondsNum2);
            layoutRoot.Children.Add(secondsNum1);

            logicalChildrenFactory = new[] { layoutRoot };

            this.AddVisualChild(layoutRoot);
            this.AddLogicalChild(layoutRoot);
        }

        protected override IEnumerator LogicalChildren => logicalChildrenFactory.GetEnumerator();

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0) throw new ArgumentOutOfRangeException();
            return layoutRoot;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            layoutRoot.Measure(constraint);
            return layoutRoot.DesiredSize;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            layoutRoot.Arrange(new Rect(default, arrangeBounds));
            return layoutRoot.RenderSize;
        }

        public TimeSpan Time
        {
            get { return (TimeSpan)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(TimeSpan), typeof(AnimatedTimerView), new PropertyMetadata(TimeSpan.Zero, (s, a) =>
            {
                if (s is AnimatedTimerView sender && !Equals(a.NewValue, a.OldValue))
                {
                    sender.UpdateTime();
                }
            }));

        private void UpdateTime()
        {
            if (Time > TimeSpan.FromMinutes(60) || Time < TimeSpan.Zero)
                throw new ArgumentException(nameof(Time));

            var minute = (int)Time.TotalMinutes;
            var seconds = Time.Seconds;

            minuteNum2.Number = minute / 10;
            minuteNum1.Number = minute % 10;

            secondsNum2.Number = seconds / 10;
            secondsNum1.Number = seconds % 10;
        }
    }
}
