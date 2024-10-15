using Xunit.Abstractions;

namespace Common.UnitTests;

public class OutputConsoleHelper(ITestOutputHelper tempOutput)
{
    protected readonly ITestOutputHelper Output = tempOutput;
}