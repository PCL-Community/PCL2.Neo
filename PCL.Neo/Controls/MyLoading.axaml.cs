using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Threading;
using PCL.Neo.Animations.Easings;
using PCL.Neo.Helpers.Animation;
using System.Threading.Tasks;

namespace PCL.Neo.Controls
{
    [PseudoClasses(":loading", ":error")]
    public class MyLoading : TemplatedControl
    {
        private Path? _pathPickaxe;
        private Path? _pathError;
        private Path? _pathLeft;
        private Path? _pathRight;
        private bool _hasErrorOccurred;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _pathPickaxe = e.NameScope.Find<Path>("PathPickaxe");
            _pathError = e.NameScope.Find<Path>("PathError");
            _pathLeft = e.NameScope.Find<Path>("PathLeft");
            _pathRight = e.NameScope.Find<Path>("PathRight");

            SetPseudoClasses();
            RefreshText();
            RefreshState();
        }

        public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<MyLoading, string>(
            nameof(Text));

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly StyledProperty<string> TextErrorProperty = AvaloniaProperty.Register<MyLoading, string>(
            nameof(TextError),
            "加载失败");

        public string TextError
        {
            get => GetValue(TextErrorProperty);
            set
            {
                SetValue(TextErrorProperty, value);
                RefreshText();
            }
        }

        public static readonly StyledProperty<string> TextLoadingProperty = AvaloniaProperty.Register<MyLoading, string>(
            nameof(TextLoading),
            "加载中");

        public string TextLoading
        {
            get => GetValue(TextLoadingProperty);
            set
            {
                SetValue(TextLoadingProperty, value);
                RefreshText();
            }
        }

        public enum LoadingState
        {
            Loading,
            Error
        }

        public static readonly StyledProperty<LoadingState> StateProperty =
            AvaloniaProperty.Register<MyLoading, LoadingState>(
                nameof(State));

        public LoadingState State
        {
            get => GetValue(StateProperty);
            set
            {
                SetValue(StateProperty, value);
                SetPseudoClasses();
                RefreshText();
                RefreshState();
            }
        }

        private void RefreshState()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var currentState = State;
                switch (currentState)
                {
                    case LoadingState.Loading:
                        if (_hasErrorOccurred)
                        {
                            AnimationErrorToLoading();
                        }

                        _hasErrorOccurred = false;
                        AnimationLoading();
                        break;

                    case LoadingState.Error:
                        if (!_hasErrorOccurred)
                        {
                            _hasErrorOccurred = true;
                            AnimationLoadingToError();
                        }

                        break;
                }
            });
        }

        private void AnimationErrorToLoading()
        {
            _ = _pathPickaxe!.Animate()
                .RotateFromTo(55d, -20d, duration: 350, easing: new MyBackEaseIn(EasePower.Weak))
                .RunAsync();

            _ = _pathError!.Animate()
                .FadeTo(1d, 100)
                .ScaleFromTo(1d, 1.2d, 100, wait: true)
                .ScaleTo(0.0d, 400, wait: true)
                .RunAsync();
        }

        private void AnimationLoadingToError()
        {
            _ = _pathPickaxe!.Animate()
                .RotateTo(55d, duration: 900, easing: new CubicEaseOut())
                .RunAsync();

            _ = _pathError!.Animate()
                .FadeTo(1d, 300)
                .ScaleTo(1.05d, 400, easing: new MyBackEaseOut(), wait: true)
                .ScaleTo(1d, 400, easing: new MyBackEaseOut(), wait: true)
                .RunAsync();
        }

        private void AnimationLoading()
        {
            // 循环动画，听说这里折磨龙猫很久(doge)
            // From Whitecat346: same, really torture for me too
            _ = _pathPickaxe!.LoopAnimate()
                .RotateFromTo(55d, -20d, duration: 350, easing: new MyBackEaseIn(EasePower.Weak))
                .RotateFromTo(30d, 55d, duration: 900, easing: new ElasticEaseOut())
                .RotateFromTo(-20d, 30d, duration: 180, wait: true)
                .RunAsync();


            _ = _pathLeft!.LoopAnimate()
                .FadeFromTo(1d, 0d, duration: 100, delay: 280, easing: new LinearEasing())
                .MarginXTo(-5d, 180, easing: new CubicEaseOut())
                .MarginYTo(-6d, 180, easing: new CubicEaseOut(), wait: true)
                .Wait(1050)
                .RunAsync();

            _ = _pathRight!.LoopAnimate()
                .FadeFromTo(1d, 0d, duration: 100, delay: 280, easing: new LinearEasing())
                .MarginXTo(5d, 180, easing: new CubicEaseOut())
                .MarginYTo(-6d, 180, easing: new CubicEaseOut(), wait: true)
                .Wait(1050)
                .RunAsync();

            _pathLeft!.Margin = new Thickness(7, 41, 0, 0);
            _pathRight!.Margin = new Thickness(14, 41, 0, 0);
        }

        private void SetPseudoClasses()
        {
            PseudoClasses.Remove(":loading");
            PseudoClasses.Remove(":error");
            PseudoClasses.Set(State == LoadingState.Loading ? ":loading" : ":error", true);
        }

        private void RefreshText()
        {
            this.Text = State == LoadingState.Loading ? TextLoading : TextError;
        }
    }
}