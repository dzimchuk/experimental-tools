using ExperimentalTools.Refactorings;
using ExperimentalTools.Tests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ExperimentalTools.Tests.Refactorings
{
    public class InitializeFieldFromConstructorParameterTests
    {
        [Fact]
        public async Task FirstTest()
        {
            var input = @"
using System;
using System.Collections.Generic;
using System.Text;

namespace HelloWorld
{
    class TestService
    {
        public TestService(int @:index:@)
        {
        }
    }
}";
            var acceptor = new CodeRefactoringActionAcceptor();
            var context = CodeRefactoringContextBuilder.Build(input, acceptor);

            var provider = new InitializeFieldFromConstructorParameter();
            await provider.ComputeRefactoringsAsync(context);
        }
    }
}
