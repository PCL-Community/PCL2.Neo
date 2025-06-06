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
        private bool _hasErrorOccurred = false;
        private AnimationChain[]? _animation;

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
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var currentState = State;
                switch (currentState)
                {
                    case LoadingState.Loading:
                        if (_hasErrorOccurred)
                        {
                            await AnimationErrorToLoadingAsync();
                        }

                        _hasErrorOccurred = false;
                        await AnimationLoadingAsync();
                        break;

                    case LoadingState.Error:
                        if (!_hasErrorOccurred)
                        {
                            _hasErrorOccurred = true;
                            await AnimationLoadingToErrorAsync();
                        }

                        break;
                }
            });
        }

        private async Task AnimationErrorToLoadingAsync()
        {
            await _pathPickaxe!.Animate()
                .RotateFromTo(55d, -20d, duration: 350, easing: new MyBackEaseIn(EasePower.Weak))
                .RunAsync();

            await _pathError!.Animate()
                .FadeTo(1d, 100)
                .ScaleFromTo(1d, 0.5d, 100)
                .ScaleFromTo(1d, 0.5d, 400)
                .RunAsync();
            //_animation.CancelAndClear();
            //_animation.Animations.AddRange(
            //[
            //    new RotateTransformAngleAnimation(this._pathPickaxe!, duration: TimeSpan.FromMilliseconds(350),
            //        before: 55d, after: -20d, easing: new MyBackEaseIn(EasePower.Weak)),

            //    new OpacityAnimation(this._pathError!, duration: TimeSpan.FromMilliseconds(100), before: 0d),
            //    new ScaleTransformScaleXAnimation(this._pathError!, duration: TimeSpan.FromMilliseconds(100),
            //        before: 1d, after: 0.5d),
            //    new ScaleTransformScaleYAnimation(this._pathError!, duration: TimeSpan.FromMilliseconds(400),
            //        before: 1d, after: 0.5d)
            //]);
            //await _animation.RunAsync();
        }

        private async Task AnimationLoadingToErrorAsync()
        {
            await _pathPickaxe!.Animate()
                .RotateTo(55d, duration: 900, easing: new CubicEaseOut())
                .RunAsync();

            await _pathError!.Animate()
                .FadeTo(1d, 300)
                .ScaleFromTo(0.5d, 1d, 400, easing: new MyBackEaseOut())
                .ScaleFromTo(0.5d, 1d, 400, easing: new MyBackEaseOut())
                .RunAsync();
            //_animation.CancelAndClear();
            //_animation.Animations.AddRange(
            //[
            //    new RotateTransformAngleAnimation(this._pathPickaxe!, duration: TimeSpan.FromMilliseconds(900),
            //        after: 55d, easing: new CubicEaseOut()),

            //    new OpacityAnimation(this._pathError!, duration: TimeSpan.FromMilliseconds(300), after: 1d),
            //    new ScaleTransformScaleXAnimation(this._pathError!, duration: TimeSpan.FromMilliseconds(400),
            //        before: 0.5d, after: 1d,
            //        easing: new MyBackEaseOut()),
            //    new ScaleTransformScaleYAnimation(this._pathError!, duration: TimeSpan.FromMilliseconds(400),
            //        before: 0.5d, after: 1d, easing: new MyBackEaseOut())
            //]);
            //await _animation.RunAsync();
        }

        private async Task AnimationLoadingAsync()
        {
            // 循环动画，听说这里折磨龙猫很久(doge)

            _ = _pathPickaxe!.LoopAnimate()
                .RotateFromTo(55d, -20d, duration: 350, easing: new MyBackEaseIn(EasePower.Weak))
                .RotateFromTo(30d, 55d, duration: 900, easing: new ElasticEaseOut())
                .RotateFromTo(-20d, 30d, duration: 180, wait: true)
                .RunAsync();

            _ = _pathLeft!.LoopAnimate()
                .FadeFromTo(1d, 0d, duration: 100, easing: new LinearEasing())
                .MarginXTo(-5d, 180, easing: new CubicEaseOut())
                .MarginYTo(-6d, 180, easing: new CubicEaseOut(), wait: true)
                .RunAsync();

            _ = _pathRight!.LoopAnimate()
                .FadeFromTo(1d, 0d, duration: 100, easing: new LinearEasing())
                .MarginXTo(5d, 180, easing: new CubicEaseOut())
                .MarginYTo(-6d, 180, easing: new CubicEaseOut(), wait: true)
                .RunAsync();


            //_animation.CancelAndClear();

            //_animation.Animations.AddRange(
            //[
            //    new RotateTransformAngleAnimation(this._pathPickaxe!, duration: TimeSpan.FromMilliseconds(350),
            //        before: 55d, after: -20d, easing: new MyBackEaseIn(EasePower.Weak)),
            //    new RotateTransformAngleAnimation(this._pathPickaxe!, duration: TimeSpan.FromMilliseconds(900),
            //        before: 30d, after: 55d, easing: new ElasticEaseOut()),
            //    new RotateTransformAngleAnimation(this._pathPickaxe!, duration: TimeSpan.FromMilliseconds(180),
            //        before: -20d, after: 30d),

            //    new OpacityAnimation(new WeakReference<Animatable>(_pathLeft!),
            //        duration: TimeSpan.FromMilliseconds(100),
            //        delay: TimeSpan.FromMilliseconds(50), before: 1d,
            //        after: 0d),
            //    new XAnimation(new WeakReference<Animatable>(this._pathLeft!), duration: TimeSpan.FromMilliseconds(180),
            //        value: -5d,
            //        easing: new CubicEaseOut()),
            //    new YAnimation(new WeakReference<Animatable>(this._pathLeft!), duration: TimeSpan.FromMilliseconds(180),
            //        value: -6d,
            //        easing: new CubicEaseOut()),

            //    new OpacityAnimation(new WeakReference<Animatable>(this._pathRight!),
            //        duration: TimeSpan.FromMilliseconds(100),
            //        delay: TimeSpan.FromMilliseconds(50),
            //        before: 1d, after: 0d),
            //    new XAnimation(new WeakReference<Animatable>(this._pathRight !),
            //        duration: TimeSpan.FromMilliseconds(180), value: 5d,
            //        easing: new CubicEaseOut()),
            //    new YAnimation(new WeakReference<Animatable>(this._pathRight!),
            //        duration: TimeSpan.FromMilliseconds(180), value: -6d,
            //        easing: new CubicEaseOut())
            //]);

            //await _animation.RunAsync();
            this._pathLeft!.Margin = new Thickness(7, 41, 0, 0);
            this._pathRight!.Margin = new Thickness(14, 41, 0, 0);
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