using ExperimentalTools.Refactorings;
using ExperimentalTools.Tests.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ExperimentalTools.Tests.Refactorings
{
    public class AddConstructorParameterRefactoringTests
    {
//        [Theory, MemberData("HasActionTestData")]
//        public async Task HasActionTest(string test, string input, string expectedOutput)
//        {
//            var acceptor = new CodeRefactoringActionAcceptor();
//            var context = CodeRefactoringContextBuilder.Build(input, acceptor);

//            var provider = new AddConstructorParameterRefactoring();
//            await provider.ComputeRefactoringsAsync(context);

//            Assert.True(acceptor.HasAction);

//            var result = await acceptor.GetResultAsync(context);
//            Assert.Equal(expectedOutput.HomogenizeLineEndings(), result.HomogenizeLineEndings());
//        }

//        public static IEnumerable<object[]> HasActionTestData =>
//            new[]
//            {
//                new object[]
//                {
//                    "Constructor exists",
//                    @"
//using System;

//namespace HelloWorld
//{
//    class TestService
//    {
//        private readonly int @::@index;

//        public TestService()
//        {
//        }
//    }
//}",
//                    @"
//using System;

//namespace HelloWorld
//{
//    class TestService
//    {
//        private readonly int index;

//        public TestService(int index)
//        {
//            this.index = index;
//        }
//    }
//}"
//                }
//            };

        [Theory, MemberData("NoActionTestData")]
        public async Task NoActionTest(string test, string input)
        {
            var acceptor = new CodeRefactoringActionAcceptor();
            var context = CodeRefactoringContextBuilder.Build(input, acceptor);

            var provider = new AddConstructorParameterRefactoring();
            await provider.ComputeRefactoringsAsync(context);

            Assert.False(acceptor.HasAction);
        }

        public static IEnumerable<object[]> NoActionTestData =>
            new[]
            {
                new object[]
                {
                    "Outside field name (1)",
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
                    "Outside field name (2)",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private readonly int index;@::@
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
                    "Field already initialized",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int m_index;
        public TestService(int @::@index)
        {
            m_index = index;
        }
    }
}"
                },
                new object[]
                {
                    "Field already initialized (complex right expression)",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int index;
        public TestService(int @::@index)
        {
            this.index = index * 2;
        }
    }
}"
                }
            };
    }
}
