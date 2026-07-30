using UnityEditor;
using UnityEngine;

namespace JasmineYamo.SimpleUI.VContainer.Editor
{
#if !JASMINEYAMO_SIMPLE_UI_VCONTAINER
    [InitializeOnLoad]
    internal static class VContainerDependencyNotifier
    {
        private const string SessionKey =
            "JasmineYamo.SimpleUI.VContainer.MissingDependencyWarning";

        static VContainerDependencyNotifier()
        {
            if (SessionState.GetBool(SessionKey, false))
            {
                return;
            }

            SessionState.SetBool(SessionKey, true);
            Debug.LogWarning(
                "[Simple UI] com.jasmineyamo.simple-ui-vcontainer is installed, "
                + "but jp.hadashikick.vcontainer is missing. Install a compatible "
                + "VContainer version to enable its Runtime.");
        }
    }
#endif
}
