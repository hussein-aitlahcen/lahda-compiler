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
        }

        public void PushScope() => m_scopes.Push(new SymbolScope());

        public SymbolScope PopScope() => m_scopes.Pop();

        public SymbolScope CurrentScope() => m_scopes.Peek();

        public SymbolScope GetScope(int index) => m_scopes.ElementAt(index);

        public void DefineSymbol(Symbol symbol)
        {
            if(CurrentScope().ContainsKey(symbol.Identifier))
                throw new InvalidOperationException($"identifier already defined for {symbol.Identifier}");
            CurrentScope().Add(symbol.Identifier, symbol);
        }

        public Symbol Search(string identifier)
        {
            var symbol = Symbol.Unknow;
            var i = m_scopes.Count - 1;
            while(i >= 0 && symbol.IsUnknow)
            {
                var scope = GetScope(i);
                if(scope.ContainsKey(identifier))
                    symbol = scope[identifier];
                i--;
            }
            return symbol;
        }
    }
}