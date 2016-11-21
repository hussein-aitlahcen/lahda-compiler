using System.Collections.Generic;

namespace Lahda.Codegen
{
    public sealed class ScopeLabel : Stack<int>
    {
        public ScopeLabel()
        {
            Push(0);
        }

        public string CurrentLabel
        {
            get
            {
                return string.Join("_", this);
            }
        }

        public void Increment() => Push(Pop() + 1);
    }
}