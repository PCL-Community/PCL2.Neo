using Avalonia.Rendering.Composition;
using CompositionScroll.Interactions.Server;

namespace CompositionScroll.Interactions
{
    public sealed class ScrollPropertiesSource : CompositionObject
    {
        private readonly CompositionScrollContentPresenter _presenter;
        private readonly InteractionTracker _tracker;

        internal ScrollPropertiesSource(Compositor compositor, CompositionScrollContentPresenter presenter, InteractionTracker tracker)
            : base(compositor, new ServerScrollPropertiesSource(compositor.Server, tracker.Server as ServerInteractionTracker))
        {
            _presenter = presenter;
            _tracker = tracker;
            RegisterForSerialization();
        }

        internal static ScrollPropertiesSource Create(CompositionScrollContentPresenter presenter, InteractionTracker tracker)
        {
            var compositor = ElementComposition.GetElementVisual(presenter).Compositor;
            return new ScrollPropertiesSource(compositor, presenter, tracker);
        }
    }
}