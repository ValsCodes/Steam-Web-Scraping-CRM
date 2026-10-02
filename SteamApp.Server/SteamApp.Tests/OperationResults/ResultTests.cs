using SteamApp.Application.OperationResults;

namespace SteamApp.Tests.OperationResults;

[TestFixture]
public sealed class ResultTests
{
    [Test]
    public void Success_ValueProvided_CreatesSuccessfulResult()
    {
        var result = Result<string>.Success("value");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Value, Is.EqualTo("value"));
            Assert.That(result.Error, Is.Null);
        });
    }

    [Test]
    public void Failure_ErrorProvided_CreatesFailedResult()
    {
        var error = new Error("products.not-found", "The product was not found.", ErrorType.NotFound);

        var result = Result<string>.Failure(error);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.SameAs(error));
        });
    }

    [Test]
    public void Success_NullValue_ThrowsArgumentNullException()
    {
        Assert.That(
            () => Result<string>.Success(null!),
            Throws.ArgumentNullException);
    }

    [Test]
    public void Failure_NullError_ThrowsArgumentNullException()
    {
        Assert.That(
            () => Result<string>.Failure(null!),
            Throws.ArgumentNullException);
    }

    [Test]
    public void Match_SuccessfulResult_InvokesSuccessBranch()
    {
        var result = Result<string>.Success("value");

        var matched = result.Match(
            value => $"success:{value}",
            error => $"failure:{error.Code}");

        Assert.That(matched, Is.EqualTo("success:value"));
    }

    [Test]
    public void Match_FailedResult_InvokesFailureBranch()
    {
        var result = Result<string>.Failure(
            new Error("products.not-found", "The product was not found.", ErrorType.NotFound));

        var matched = result.Match(
            value => $"success:{value}",
            error => $"failure:{error.Code}");

        Assert.That(matched, Is.EqualTo("failure:products.not-found"));
    }
}
