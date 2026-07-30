using JasmineYamo.SimpleUI.VContainer;
using VContainer.Unity;

namespace JasmineYamo.SimpleUI.VContainer.Samples.ResourcesDemo.Demo
{
    public sealed class DetailPresenter : IStartable
    {
        private readonly DetailView.UIView m_View;
        private readonly ViewBundle m_ViewBundle;
        private readonly IViewManager m_ViewManager;

        public DetailPresenter(
            DetailView.UIView view,
            ViewBundle viewBundle,
            IViewManager viewManager)
        {
            m_View = view;
            m_ViewBundle = viewBundle;
            m_ViewManager = viewManager;
        }

        public void Start()
        {
            object payload = m_ViewBundle.DataBundle != null
                && m_ViewBundle.DataBundle.Length > 0
                    ? m_ViewBundle.DataBundle[0]
                    : "No payload";
            m_View.statusText.text = $"DetailView - {payload}";
            m_View.showHomeButton.onClick.AddListener(
                () => m_ViewManager.ShowViewAtHidePeekView(
                    "HomeView",
                    "Returned from DetailView"));
            m_View.closeButton.onClick.AddListener(m_ViewManager.HideView);
        }
    }
}
