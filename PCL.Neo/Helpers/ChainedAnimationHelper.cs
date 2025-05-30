// In PCL.Neo.Helpers/ChainedAnimationHelper.cs

using Avalonia;
using Avalonia.Animation; // For FillMode
using Avalonia.Animation.Easings;
using Avalonia.Media; // For Transform, ScaleTransform, TransformGroup
using Avalonia.Layout; // For Layoutable
using PCL.Neo.Animations;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic; // Required for List<IAnimation>

namespace PCL.Neo.Helpers
{
    public static class ChainedAnimationHelper
    {
        /// <summary>
        /// Starts an animation chain for the specified control.
        /// </summary>
        public static AnimationChain Animate(this Animatable control)
        {
            return new AnimationChain(control);
        }

        /// <summary>
        /// Adds a fade animation to the chain.
        /// </summary>
        /// <param name="chain">The animation chain.</param>
        /// <param name="targetOpacity">The target opacity (0.0 to 1.0).</param>
        /// <param name="durationMs">Duration of the animation in milliseconds.</param>
        /// <param name="easing">The easing function to use.</param>
        /// <param name="delayMs">Delay before the animation starts in milliseconds.</param>
        /// <param name="wait">If true, this animation will wait for previous animations in the chain (that were not themselves set to wait) to complete.</param>
        public static AnimationChain FadeTo(
            this AnimationChain chain,
            double targetOpacity,
            uint durationMs = 250,
            Easing? easing = null,
            uint? delayMs = null,
            bool wait = false)
        {
            if (chain.TargetControl is not Visual visualControl)
            {
                // Or handle gracefully, e.g., by not adding the animation or logging a warning
                throw new InvalidOperationException("FadeTo can only be applied to Visual controls.");
            }

            var animation = new OpacityAnimation(
                visualControl,
                before: null, // OpacityAnimation will try to get current opacity
                after: targetOpacity,
                duration: TimeSpan.FromMilliseconds(durationMs),
                easing: easing, // Default handled by OpacityAnimation or BaseAnimation
                delay: delayMs.HasValue ? TimeSpan.FromMilliseconds(delayMs.Value) : null,
                wait: wait
            );
            chain.AddAnimation(animation);
            return chain;
        }

        /// <summary>
        /// Adds a scale animation to the chain.
        /// </summary>
        /// <param name="chain">The animation chain.</param>
        /// <param name="beforeScale">The target uniform before scale factor.</param>
        /// <param name="afterScale">The target uniform after scale factor.</param>
        /// <param name="durationMs">Duration of the animation in milliseconds.</param>
        /// <param name="easing">The easing function to use.</param>
        /// <param name="delayMs">Delay before the animation starts in milliseconds.</param>
        /// <param name="wait">If true, this animation will wait for previous animations in the chain to complete.</param>
        public static AnimationChain ScaleTo(
            this AnimationChain chain,
            double beforeScale,
            double afterScale,
            uint durationMs = 250,
            Easing? easing = null,
            uint? delayMs = null,
            bool wait = false)
        {
            return ScaleTo(chain, beforeScale, afterScale, durationMs, easing, delayMs, wait);
        }

        /// <summary>
        /// Adds a scale animation to the chain with separate X and Y scale factors.
        /// </summary>
        public static AnimationChain ScaleTo(
            this AnimationChain chain,
            double beforeScaleX,
            double beforeScaleY,
            double afterScalueX,
            double afterScaleY,
            uint durationMs = 250,
            Easing? easing = null,
            uint? delayMs = null,
            bool wait = false)
        {
            var scaleTransform = EnsureTransform<ScaleTransform>(chain.TargetControl);
            if (scaleTransform == null)
            {
                // Could log a warning: "Cannot apply ScaleTo to a control that is not Layoutable or cannot have a ScaleTransform."
                return chain; // or throw
            }

            var animation = new ScaleTransformScaleAnimation(
                chain.TargetControl,
                before: beforeScaleX,
                after: afterScalueX,
                duration: TimeSpan.FromMilliseconds(durationMs),
                easing: easing,
                delay: delayMs.HasValue ? TimeSpan.FromMilliseconds(delayMs.Value) : null,
                wait: wait
            );
            chain.AddAnimation(animation);
            return chain;
        }

        // You can add more animation types here: TranslateTo, RotateTo, etc.
        // e.g. public static AnimationChain RotateTo(this AnimationChain chain, double angleDegrees, ...)
        // e.g. public static AnimationChain TranslateTo(this AnimationChain chain, double x, double y, ...)

        /// <summary>
        /// Runs all animations in the chain.
        /// </summary>
        public static async Task RunAsync(this AnimationChain chain)
        {
            if (!chain.Animations.Any())
                return;

            var animationHelper = new AnimationHelper(chain.Animations)
            {
                Loop = chain.LoopChain // Set the loop property for the entire sequence
            };
            await animationHelper.RunAsync();
        }

        /// <summary>
        /// Cancels all animations in the chain.
        /// </summary>
        public static void Cancel(this AnimationChain chain)
        {
            if (!chain.Animations.Any())
                return;

            // Use a temporary AnimationHelper to leverage its Cancel logic
            // Or iterate through chain.Animations and call Cancel() on each
            var animationHelper = new AnimationHelper(chain.Animations);
            animationHelper.Cancel();
        }

        /// <summary>
        /// Helper to get or create a specific transform type on a control.
        /// </summary>
        private static T? EnsureTransform<T>(Animatable control) where T : Transform, new()
        {
            if (control is not Layoutable layoutable)
            {
                // Cannot apply RenderTransform to non-Layoutable controls
                return null;
            }

            T? foundTransform = null;

            switch (layoutable.RenderTransform)
            {
                case T specificTransform:
                    foundTransform = specificTransform;
                    break;
                case TransformGroup group:
                    {
                        foundTransform = group.Children.OfType<T>().FirstOrDefault();
                        if (foundTransform == null)
                        {
                            foundTransform = new T();
                            group.Children.Add(foundTransform);
                        }

                        break;
                    }
                // RenderTransform is null or some other single transform
                default:
                    {
                        var newTransform = new T();
                        if (layoutable.RenderTransform == null)
                        {
                            layoutable.RenderTransform = newTransform;
                        }
                        else // It's a different single transform, create a group
                        {
                            var existingTransform = layoutable.RenderTransform;
                            var newGroup = new TransformGroup();
                            newGroup.Children.Add((Transform)existingTransform);
                            newGroup.Children.Add(newTransform);
                            layoutable.RenderTransform = newGroup;
                        }

                        foundTransform = newTransform;
                        break;
                    }
            }

            return foundTransform;
        }
    }
}