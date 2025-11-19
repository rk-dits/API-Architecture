using BuildingBlocks.Common.Abstractions;
using Xunit;

namespace UnitTests;

public class ResultTests
{
    [Fact]
    public void Success_ShouldHaveValue()
    {
        var r = Result.Success("ok");
        Assert.True(r.IsSuccess);
        var typed = (Result<string>)r;
        Assert.Equal("ok", typed.Value);
    }

    [Fact]
    public void Failure_ShouldExposeError()
    {
        var err = Error.Validation("bad");
        var r = Result.Failure<string>(err);
        Assert.True(r.IsFailure);
        Assert.Equal(err, r.Error);
    }
}
