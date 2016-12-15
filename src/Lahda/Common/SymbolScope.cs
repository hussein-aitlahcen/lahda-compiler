using System.Linq;
using System.Collections.Generic;
using Lahda.Parser;
using Lahda.Parser.Impl;

namespace Lahda.Common
{
    public sealed class SymbolScope : Dictionary<string, AbstractSymbol>
    {
        public int VarNumber => Values.Count(symb => symb is AbstractAddressableSymbol);
        public AbstractStatementNode ReleaseStatements => new BlockNode(Values.OfType<AbstractAddressableSymbol>().Select(symb => symb.ReleaseInstruction()));
    }
}