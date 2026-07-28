using UnityEngine;

namespace JasmineYamo.ComponentAutoBind
{
    /// <summary>
    /// Implemented by a target type that owns generated component bindings.
    /// </summary>
    public interface IAutoBindTarget
    {
        void EnsureAutoBind(GameObject go);
    }
}
