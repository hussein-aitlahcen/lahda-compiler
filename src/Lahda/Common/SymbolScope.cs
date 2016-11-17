using System.Collections.Generic;

namespace Lahda.Common
{
    public sealed class SymbolScope : Dictionary<string, Symbol>
    {
        public int VarNumber => Values.Count;
    }
}