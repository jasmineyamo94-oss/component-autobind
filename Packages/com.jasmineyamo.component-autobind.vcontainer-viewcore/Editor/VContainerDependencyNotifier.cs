using UnityEditor;
using UnityEngine;

namespace JasmineYamo.ComponentAutoBind.ViewCore.Editor
{
#if !JASMINEYAMO_VCONTAINER
    [InitializeOnLoad]
    internal static class VContainerDependencyNotifier
    {
        private const string SessionKey =
            "JasmineYamo.ComponentAutoBind.VContainerViewCore.MissingDependencyWarning";

        static VContainerDependencyNotifier()
        {
            if (SessionState.GetBool(SessionKey, false))
            {
                return;
            }

            SessionState.SetBool(SessionKey, true);
            Debug.LogWarning(
                "[Component Auto Bind] The optional VContainer ViewCore package is installed, "
                + "but jp.hadashikick.vcontainer is missing. Install a compatible VContainer "
                + "version to enable ViewLifetimeScope.");
        }
    }
#endif
}
