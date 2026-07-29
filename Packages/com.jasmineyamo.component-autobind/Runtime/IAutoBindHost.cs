using UnityEngine;

namespace JasmineYamo.ComponentAutoBind
{
    /// <summary>
    /// Implemented by a target type that owns generated component bindings.
    /// </summary>
    public interface IAutoBindHost
    {
        void EnsureAutoBind(GameObject go);
    }
}
