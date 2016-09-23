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
    public class AddNewConstructorWithParameterRefactoringTests
    {
        [Theory, MemberData("HasActionTestData")]
        public async Task HasActionTest(string test, string input, string expectedOutput)
        {
            var acceptor = new CodeRefactoringActionAcceptor();
            var context = CodeRefactoringContextBuilder.Build(input, acceptor);

            var provider = new AddNewConstructorWithParameterRefactoring();
            await provider.ComputeRefactoringsAsync(context);

            Assert.True(acceptor.HasAction);

            var result = await acceptor.GetResultAsync(context);
            Assert.Equal(expectedOutput.HomogenizeLineEndings(), result.HomogenizeLineEndings());
        }

        public static IEnumerable<object[]> HasActionTestData =>
            new[]
            {
                new object[]
                {
                    "Constructor does not exist",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private readonly int @::@index;
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
                },
                new object[]
                {
                    "Constructor does not exist (at the end of declarator)",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private readonly int index@::@;
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

            var provider = new AddNewConstructorWithParameterRefactoring();
            await provider.ComputeRefactoringsAsync(context);

            Assert.False(acceptor.HasAction);
        }

        public static IEnumerable<object[]> NoActionTestData =>
            new[]
            {
                new object[]
                {
                    "Outside field name",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private readonly i@::@nt index;
    }
}"
                },
                new object[]
                {
                    "Non-empty span",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private readonly int in@:d:@ex;
    }
}"
                },
                new object[]
                {
                    "Field already initialized (single constructor)",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int @::@m_index;
        public TestService(int index)
        {
            m_index = index;
        }
    }
}"
                },
                new object[]
                {
                    "Field already initialized (multiple constructors)",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int @::@m_index;
        private readonly string name;

        public TestService(string name)
        {
            this.name = name;
        }

        public TestService(string name, int index)
        {
            this.name = name;
            m_index = index;
        }
    }
}"
                }
            };
    }
}
