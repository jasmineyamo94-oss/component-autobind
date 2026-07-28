using UnityEngine;
using UnityEngine.UI;

namespace JasmineYamo.ComponentAutoBind.Samples.BasicUGUI
{
    public partial class BasicAutoBindTarget : MonoBehaviour
    {
        public Button SubmitButton => m_AutoBindComponents?.submitButton;
        public Text StatusText => m_AutoBindComponents?.statusText;

        public void Initialize()
        {
            EnsureAutoBind(gameObject);
        }
    }
}
