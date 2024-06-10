using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AnimatedTimer
{
    public class AnimatedNumber : Control
    {
        private Storyboard? animation;
        private TextBlock textBlock1;
        private TextBlock textBlock2;
        private ScaleTransform textBlock1ScaleTrans;
        private ScaleTransform textBlock2ScaleTrans;
        private TranslateTransform textBlock1TranslateTrans;
        private TranslateTransform textBlock2TranslateTrans;
        private TransformGroup textBlock1TransGroup;
        private TransformGroup textBlock2TransGroup;
        private IEnumerable logicChildrenFactory;

        public AnimatedNumber()
        {
            textBlock1 = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = (textBlock1TransGroup = new TransformGroup()
                {
                    Children =
                    {
                        (textBlock1ScaleTrans = new ScaleTransform()),
                        (textBlock1TranslateTrans = new TranslateTransform())
                    }
                }),
            };
            textBlock2 = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = (textBlock2TransGroup = new TransformGroup()
                {
                    Children =
                    {
                        (textBlock2ScaleTrans = new ScaleTransform()),
                        (textBlock2TranslateTrans = new TranslateTransform())
                    }
                })
            };
            textBlock1TransGroup.Freeze();
            textBlock2TransGroup.Freeze();


            logicChildrenFactory = new[] { textBlock1, textBlock2 };

            this.AddLogicalChild(textBlock1);
            this.AddLogicalChild(textBlock2);
            this.AddVisualChild(textBlock1);
            this.AddVisualChild(textBlock2);

            UpdateAnimation(null);
        }

        protected override IEnumerator LogicalChildren => logicChildrenFactory.GetEnumerator();

        protected override int VisualChildrenCount => 2;

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0 && index != 1) throw new ArgumentOutOfRangeException();

            return index == 0 ? textBlock1 : textBlock2;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            textBlock1.Measure(constraint);
            textBlock2.Measure(constraint);
            return textBlock2.DesiredSize;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            textBlock1.Arrange(new Rect(default, arrangeBounds));
            textBlock2.Arrange(new Rect(default, arrangeBounds));
            return textBlock2.RenderSize;
        }

        public int Number
        {
            get { return (int)GetValue(NumberProperty); }
            set { SetValue(NumberProperty, value); }
        }

        public static readonly DependencyProperty NumberProperty =
            DependencyProperty.Register("Number", typeof(int), typeof(AnimatedNumber), new PropertyMetadata(0, (s, a) =>
            {
                if (s is AnimatedNumber sender && !Equals(a.NewValue, a.OldValue))
                {
                    var animation = sender.CreateStoryboard(
                        (int)a.OldValue,
                        (int)a.NewValue);

                    sender.UpdateAnimation(animation);
                }
            }));



        public Duration Duration
        {
            get { return (Duration)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(Duration), typeof(AnimatedNumber), new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(600)), (s, a) =>
            {
                if (s is AnimatedNumber sender && !Equals(a.NewValue, a.OldValue))
                {
                    sender.UpdateAnimation(null);
                }
            }));



        public TimeSpan BeginTime
        {
            get { return (TimeSpan)GetValue(BeginTimeProperty); }
            set { SetValue(BeginTimeProperty, value); }
        }

        public static readonly DependencyProperty BeginTimeProperty =
            DependencyProperty.Register("BeginTime", typeof(TimeSpan), typeof(AnimatedNumber), new PropertyMetadata(TimeSpan.Zero, (s, a) =>
            {
                if (s is AnimatedNumber sender && !Equals(a.NewValue, a.OldValue))
                {
                    sender.UpdateAnimation(null);
                }
            }));



        private void UpdateAnimation(Storyboard? newAnimation)
        {
            if (animation != null)
            {
                animation.Completed -= Animation_Completed;
                animation.Stop();
                animation = null;
            }

            if (newAnimation != null)
            {
                animation = newAnimation;
                animation.Completed += Animation_Completed;
                newAnimation.Begin();
            }
            else
            {
                textBlock1.Opacity = 0;
                textBlock2.Opacity = 1;
                textBlock1.Text = textBlock2.Text = $"{Number}";
            }
        }

        private void Animation_Completed(object? sender, EventArgs e)
        {
            if (animation != null)
            {
                animation.Completed -= Animation_Completed;
                animation = null;
            }
        }

        private Storyboard? CreateStoryboard(
            int oldNumber,
            int newNumber)
        {
            var duration = Duration;
            var beginTime = BeginTime;

            if (oldNumber != newNumber
                && newNumber <= 9 && newNumber >= 0
                && oldNumber <= 9 && oldNumber >= 0
                && duration.HasTimeSpan && duration.TimeSpan.TotalMilliseconds > 0.001
                && textBlock2.ActualHeight > 0)
            {
                textBlock1.Text = $"{oldNumber}";
                textBlock1.Opacity = 1;
                textBlock2.Text = $"{newNumber}";
                textBlock2.Opacity = 0;

                var height = textBlock2.ActualHeight;

                var storyboard = new Storyboard()
                {
                    Duration = duration + beginTime,
                };

                var scaleEasingFunc = new ExponentialEase()
                {
                    Exponent = 4.5,
                    EasingMode = EasingMode.EaseOut
                };

                var oldTextBlockOpacityAnimation = new DoubleAnimationUsingKeyFrames()
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(1, TimeSpan.Zero),
                        new DiscreteDoubleKeyFrame(1, GetTimespan(duration, beginTime, 0)),
                        new LinearDoubleKeyFrame(0, GetTimespan(duration, beginTime, 0.2)),
                    },
                    Duration = duration + beginTime,
                };

                var oldTextBlockScaleXAnimation = new DoubleAnimationUsingKeyFrames()
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(1, TimeSpan.Zero),
                        new DiscreteDoubleKeyFrame(1, GetTimespan(duration, beginTime, 0)),
                        new LinearDoubleKeyFrame(0.5, GetTimespan(duration, beginTime, 0.35)),
                    },
                    Duration = duration + beginTime,
                };

                var oldTextBlockScaleYAnimation = new DoubleAnimationUsingKeyFrames()
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(1, TimeSpan.Zero),
                        new DiscreteDoubleKeyFrame(1, GetTimespan(duration, beginTime, 0)),
                        new LinearDoubleKeyFrame(0.5, GetTimespan(duration, beginTime, 0.4)),
                    },
                    Duration = duration + beginTime,
                };

                var oldTextBlockTranslateYAnimation = new DoubleAnimationUsingKeyFrames()
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(0, TimeSpan.Zero),
                        new DiscreteDoubleKeyFrame(0, GetTimespan(duration, beginTime, 0)),
                        new LinearDoubleKeyFrame(-height / 2, GetTimespan(duration, beginTime, 0.4)),
                    },
                    Duration = duration + beginTime,
                };


                var newTextBlockOpacityAnimation = new DoubleAnimationUsingKeyFrames()
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(0, TimeSpan.Zero),
                        new DiscreteDoubleKeyFrame(0, GetTimespan(duration, beginTime, 0)),
                        new DiscreteDoubleKeyFrame(0, GetTimespan(duration, beginTime, 0.15)),
                        new EasingDoubleKeyFrame(1, GetTimespan(duration, beginTime, 1), scaleEasingFunc),
                    },
                    Duration = duration + beginTime,
                };

                var newTextBlockScaleXAnimation = new DoubleAnimationUsingKeyFrames()
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(0.5, TimeSpan.Zero),
                        new DiscreteDoubleKeyFrame(0.5, GetTimespan(duration, beginTime, 0)),
                        new EasingDoubleKeyFrame(1, GetTimespan(duration, beginTime, 1), scaleEasingFunc),
                    },
                    Duration = duration + beginTime,
                };

                var newTextBlockScaleYAnimation = new DoubleAnimationUsingKeyFrames()
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(0.5, TimeSpan.Zero),
                        new DiscreteDoubleKeyFrame(0.5, GetTimespan(duration, beginTime, 0)),
                        new EasingDoubleKeyFrame(1, GetTimespan(duration, beginTime, 1), scaleEasingFunc),
                    },
                    Duration = duration + beginTime,
                };

                var newTextBlockTranslateYAnimation = new DoubleAnimationUsingKeyFrames()
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(height / 2, TimeSpan.Zero),
                        new DiscreteDoubleKeyFrame(height / 2, GetTimespan(duration, beginTime, 0)),
                        new DiscreteDoubleKeyFrame(height / 2, GetTimespan(duration, beginTime, 0.05)),
                        new EasingDoubleKeyFrame(0, GetTimespan(duration, beginTime, 1), new ElasticEase()
                        {
                            EasingMode = EasingMode.EaseOut,
                            Oscillations = 1,
                            Springiness = 4.5,
                        }),
                    },
                    Duration = duration + beginTime,
                };

                AddToStoryboard(storyboard, oldTextBlockOpacityAnimation, textBlock1, UIElement.OpacityProperty);
                AddToStoryboard(storyboard, oldTextBlockScaleXAnimation, textBlock1, "RenderTransform.(TransformGroup.Children)[0].(ScaleTransform.ScaleX)");
                AddToStoryboard(storyboard, oldTextBlockScaleYAnimation, textBlock1, "RenderTransform.(TransformGroup.Children)[0].(ScaleTransform.ScaleY)");
                AddToStoryboard(storyboard, oldTextBlockTranslateYAnimation, textBlock1, "RenderTransform.(TransformGroup.Children)[1].(TranslateTransform.Y)");

                AddToStoryboard(storyboard, newTextBlockOpacityAnimation, textBlock2, OpacityProperty);
                AddToStoryboard(storyboard, newTextBlockScaleXAnimation, textBlock2, "RenderTransform.(TransformGroup.Children)[0].(ScaleTransform.ScaleX)");
                AddToStoryboard(storyboard, newTextBlockScaleYAnimation, textBlock2, "RenderTransform.(TransformGroup.Children)[0].(ScaleTransform.ScaleY)");
                AddToStoryboard(storyboard, newTextBlockTranslateYAnimation, textBlock2, "RenderTransform.(TransformGroup.Children)[1].(TranslateTransform.Y)");

                return storyboard;
            }
            else
            {
                textBlock1.Text = $"{newNumber}";
                textBlock1.Opacity = 0;
                textBlock2.Text = $"{newNumber}";
                textBlock2.Opacity = 1;
            }

            return null;
        }

        private static void AddToStoryboard(Storyboard storyboard, Timeline timeline, DependencyObject targetObject, object targetProperty)
        {
            Storyboard.SetTarget(timeline, targetObject);
            Storyboard.SetTargetProperty(timeline, targetProperty switch
            {
                string pathString => new PropertyPath(pathString),
                _ => new PropertyPath(targetProperty)
            });
            storyboard.Children.Add(timeline);
        }

        private static TimeSpan GetTimespan(Duration duration, TimeSpan beginTime, double progress)
        {
            if (!duration.HasTimeSpan) return TimeSpan.Zero;

            return beginTime + duration.TimeSpan * progress;
        }
    }
}
