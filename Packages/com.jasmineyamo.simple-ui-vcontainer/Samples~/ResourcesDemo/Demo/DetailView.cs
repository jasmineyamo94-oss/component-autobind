using JasmineYamo.SimpleUI.VContainer;
using VContainer;

namespace JasmineYamo.SimpleUI.VContainer.Samples.ResourcesDemo.Demo
{
    public partial class DetailView : ViewLifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            RegisterCommon<DetailPresenter>(builder, view);
        }
    }
}
