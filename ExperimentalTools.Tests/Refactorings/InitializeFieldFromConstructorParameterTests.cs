using ExperimentalTools.Refactorings;
using ExperimentalTools.Tests.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ExperimentalTools.Tests.Refactorings
{
    public class InitializeFieldFromConstructorParameterTests
    {
        [Theory, MemberData("HasActionTestData")]
        public async Task HasActionTest(string test, string input, string expectedOutput)
        {
            var acceptor = new CodeRefactoringActionAcceptor();
            var context = CodeRefactoringContextBuilder.Build(input, acceptor);

            var provider = new InitializeFieldFromConstructorParameter();
            await provider.ComputeRefactoringsAsync(context);

            Assert.True(acceptor.HasAction);

            var result = await acceptor.GetResultAsync(context);
            Assert.Equal(expectedOutput, result);
        }

        public static IEnumerable<object[]> HasActionTestData =>
            new[]
            {
                new object[]
                {
                    "Inside parameter name",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public TestService(int @::@index)
        {
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private readonly int index;

        public TestService(int index)
        {
            this.index = index;
        }
    }
}"
                }
            };

        [Theory, MemberData("NoActionTestData")]
        public async Task NoActionTest(string test, string input)
        {
            var acceptor = new CodeRefactoringActionAcceptor();
            var context = CodeRefactoringContextBuilder.Build(input, acceptor);

            var provider = new InitializeFieldFromConstructorParameter();
            await provider.ComputeRefactoringsAsync(context);

            Assert.False(acceptor.HasAction);
        }

        public static IEnumerable<object[]> NoActionTestData =>
            new[]
            {
                new object[]
                {
                    "Inside constructor name",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public Test@::@Service(int index)
        {
        }
    }
}"
                },
                new object[]
                {
                    "Inside parameter type",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public TestService(i@::@nt index)
        {
        }
    }
}"
                },
                new object[]
                {
                    "Method parameter",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        public void Test(int @::@index)
        {
        }
    }
}"
                }
            };
    }
}
