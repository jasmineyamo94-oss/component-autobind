namespace JasmineYamo.ComponentAutoBind.ViewCore
{
    public class ViewBundle
    {
        public object[] DataBundle { get; private set; }

        public void SetViewBundle(object[] args)
        {
            DataBundle = args;
        }
    }
}
