using System.Collections.Generic;

namespace Lahda.Codegen
{
    public sealed class ScopeLabels : Dictionary<ScopeType, ScopeLabel>
    {
        public ScopeLabels()
        {
            Add(ScopeType.Conditional, new ScopeLabel());
            Add(ScopeType.Loop, new ScopeLabel());
        }
    }
}