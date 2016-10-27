using ExperimentalTools.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ExperimentalTools.Tests.Features.AccessModifier
{
    public class NestedInClassTests
    {
        [Theory, MemberData("TestData")]
        public async Task Test(string test, string actionTitle, string input, string expectedResult)
        {
            await TestRunner.RunAsync(input, async (acceptor, context) =>
            {
                Assert.True(acceptor.HasAction(actionTitle));

                var result = await acceptor.GetResultAsync(actionTitle, context);
                Assert.Equal(expectedResult.HomogenizeLineEndings(), result.HomogenizeLineEndings());
            });
        }

        public static IEnumerable<object[]> TestData =>
            new[]
            {
                new object[]
                {
                    "Implicit class to PrivateExplicit",
                    Resources.ToPrivateExplicit,
                    @"
namespace HelloWorld
{
    class Test
    {
        @::@class Nested
        {
        }
    }
}",
                    @"
namespace HelloWorld
{
    class Test
    {
        private class Nested
        {
        }
    }
}"
                }
            };
    }
}
