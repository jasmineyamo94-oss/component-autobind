using JasmineYamo.SimpleUI.VContainer;
using VContainer;

namespace JasmineYamo.SimpleUI.VContainer.Samples.ResourcesDemo.Demo
{
    public partial class HomeView : ViewLifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            RegisterCommon<HomePresenter>(builder, view);
        }
    }
}
