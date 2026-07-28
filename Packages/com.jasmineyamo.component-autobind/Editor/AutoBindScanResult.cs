using System.Collections.Generic;
using UnityEngine;
using BindData = JasmineYamo.ComponentAutoBind.ComponentAutoBindTool.BindData;

namespace JasmineYamo.ComponentAutoBind.Editor
{
    internal sealed class AutoBindScanResult
    {
        private readonly List<BindData> m_BindDatas = new List<BindData>();
        private readonly List<string> m_Errors = new List<string>();

        public IReadOnlyList<BindData> BindDatas => m_BindDatas;
        public IReadOnlyList<string> Errors => m_Errors;
        public bool HasErrors => m_Errors.Count > 0;

        public void AddBindData(string name, Component bindCom)
        {
            m_BindDatas.Add(new BindData(name, bindCom));
        }

        public void AddError(string message)
        {
            m_Errors.Add(message);
        }
    }
}
