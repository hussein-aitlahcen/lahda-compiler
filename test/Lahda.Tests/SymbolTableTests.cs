using System;
using Lahda.Common;
using Xunit;

namespace Lahda.Tests
{
    public sealed class SymbolTableTest
    {
        [Theory]
        [InlineData("a")]
        [InlineData("m_b")]
        public void SymbolTable_should_find_defined_symbol(string ident)
        {
            var table = new SymbolTable();
            table.DefineSymbol(new Symbol(ident));
            var symbol = table.Search(ident);
            Assert.False(symbol.IsUnknow);
            Assert.Equal(symbol.Name, ident);
        }
    }
}