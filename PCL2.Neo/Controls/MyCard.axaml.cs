using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using PCL2.Neo.Helpers;
using System;

namespace PCL2.Neo.Controls
{
    public class MyCard : ContentControl
    {
        private Border? _borderMain;
        private AnimationHelper _animation;

        public MyCard()
        {
            _animation = new();
        }
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _borderMain = e.NameScope.Find<Border>("BorderMain")!;
        }

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);

        }

        public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<MyCard, string>(
            nameof(Title));

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly StyledProperty<Geometry> IconProperty = AvaloniaProperty.Register<MyCard, Geometry>(
            nameof(Icon));

        public Geometry Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        private void HeightAnimation()
        {

        }
    }
}