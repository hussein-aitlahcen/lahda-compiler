using System.Collections.Generic;
using System.Linq;
using System;

namespace Lahda.Common
{
    public sealed class SymbolTable
    {
        private Stack<SymbolScope> m_scopes;

        public SymbolTable()
        {
            m_scopes = new Stack<SymbolScope>();
            PushScope(); // rootScope
        }

        public int NextStackPointer => m_scopes.Sum(scope => scope.VarNumber);

        public void PushScope() => m_scopes.Push(new SymbolScope());

        public SymbolScope PopScope() => m_scopes.Pop();

        public SymbolScope CurrentScope => m_scopes.Peek();

        public SymbolScope GetScope(int index) => m_scopes.ElementAt(index);

        public T DefineSymbol<T>(T symbol)
            where T : AbstractSymbol
        {
            if (CurrentScope.ContainsKey(symbol.Name))
            {
                throw new InvalidOperationException($"identifier already defined for {symbol.Name}");
            }
            var primitiveSymbol = symbol as AbstractAddressableSymbol;
            if (primitiveSymbol != null)
            {
                primitiveSymbol.Pointer = NextStackPointer;
            }
            CurrentScope.Add(symbol.Name, symbol);
            return symbol;
        }

        public T Search<T>(string identifier)
            where T : AbstractSymbol
        {
            var symbol = AbstractSymbol.Unknow;
            var i = m_scopes.Count - 1;
            while (i >= 0 && symbol.IsUnknow)
            {
                var scope = GetScope(i);
                if (scope.ContainsKey(identifier))
                {
                    symbol = scope[identifier];
                }
                i--;
            }
            if (symbol.IsUnknow)
            {
                throw new InvalidOperationException($"unknow symbol {identifier}");
            }
            else if (!(symbol is T))
            {
                throw new InvalidOperationException($"wrong symbol type {identifier}");
            }
            return (T)symbol;
        }
    }
}