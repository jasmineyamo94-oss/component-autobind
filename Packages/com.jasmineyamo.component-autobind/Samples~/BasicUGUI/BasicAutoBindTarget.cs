using UnityEngine;
using UnityEngine.UI;

namespace JasmineYamo.ComponentAutoBind.Samples.BasicUGUI
{
    public partial class BasicAutoBindTarget : MonoBehaviour
    {
        public Button SubmitButton => view?.submitButton;
        public Text StatusText => view?.statusText;

        public void Initialize()
        {
            EnsureAutoBind(gameObject);
        }
    }
}
