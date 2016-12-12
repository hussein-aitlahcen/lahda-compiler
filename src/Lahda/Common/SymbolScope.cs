using System.Linq;
using System.Collections.Generic;

namespace Lahda.Common
{
    public sealed class SymbolScope : Dictionary<string, AbstractSymbol>
    {
        public int VarNumber => Values.Count(symb => symb is PrimitiveVariableSymbol);
    }
}