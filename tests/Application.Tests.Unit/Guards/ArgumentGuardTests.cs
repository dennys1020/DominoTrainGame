using System;
using DominoTrainGame.Server.Application.Guards;
using log4net;
using NSubstitute;
using NUnit.Framework;

namespace DominoTrainGame.Server.Application.Tests.Unit.Guards;

[TestFixture]
public sealed class ArgumentGuardTests
{
    private const int ExpectedCallCount = 1;
    private const string ParameterName = "playerProfile";
    private const string TextParameterName = "matchCode";
    private const string ParameterNamePropertyName = nameof(ArgumentException.ParamName);
    private const string ValidText = "ABC123";
    private const string EmptyText = "";
    private const string WhitespaceText = "   ";
    private const string MissingArgumentFormat = "A required argument is missing. Parameter: {0}";
    private const string EmptyArgumentFormat = "A required text argument is empty. Parameter: {0}";

    private ILog _logger;
    private object _argument;
    private string _text;

    [SetUp]
    public void SetUp()
    {
        _logger = Substitute.For<ILog>();
        _argument = new object();
        _text = ValidText;
    }

    [Test]
    public void Test_NotNull_PresentArgument_ReturnsSameArgument()
    {
        object guardedArgument = ArgumentGuard.NotNull(_argument, ParameterName, _logger);

        Assert.That(guardedArgument, Is.SameAs(_argument));
    }

    [Test]
    public void Test_NotNull_NullArgument_ThrowsArgumentNullException()
    {
        _argument = null;

        Assert.Throws<ArgumentNullException>(GuardNotNullArgument);
    }

    [Test]
    public void Test_ThrowIfNull_PresentArgument_DoesNotThrow()
    {
        Assert.DoesNotThrow(GuardArrangedArgument);
    }

    [Test]
    public void Test_ThrowIfNull_PresentArgument_DoesNotLogWarning()
    {
        GuardArrangedArgument();

        _logger.DidNotReceive().WarnFormat(Arg.Any<string>(), Arg.Any<object>());
    }

    [Test]
    public void Test_ThrowIfNull_NullArgument_ThrowsArgumentNullException()
    {
        _argument = null;

        Assert.Throws<ArgumentNullException>(GuardArrangedArgument);
    }

    [Test]
    public void Test_ThrowIfNull_NullArgument_NamesRejectedParameter()
    {
        _argument = null;

        Assert.That(
            GuardArrangedArgument,
            Throws.ArgumentNullException.With.Property(ParameterNamePropertyName).EqualTo(ParameterName));
    }

    [Test]
    public void Test_ThrowIfNull_NullArgument_LogsMissingArgumentWarningOnce()
    {
        _argument = null;

        IsRejected(GuardArrangedArgument);

        _logger.Received(ExpectedCallCount).WarnFormat(MissingArgumentFormat, ParameterName);
    }

    [Test]
    public void Test_ThrowIfEmpty_ValidText_DoesNotThrow()
    {
        Assert.DoesNotThrow(GuardArrangedText);
    }

    [Test]
    public void Test_ThrowIfEmpty_ValidText_DoesNotLogWarning()
    {
        GuardArrangedText();

        _logger.DidNotReceive().WarnFormat(Arg.Any<string>(), Arg.Any<object>());
    }

    [TestCase(EmptyText)]
    [TestCase(WhitespaceText)]
    [TestCase(null)]
    public void Test_ThrowIfEmpty_BlankText_ThrowsArgumentException(string blankText)
    {
        _text = blankText;

        Assert.Throws<ArgumentException>(GuardArrangedText);
    }

    [Test]
    public void Test_ThrowIfEmpty_BlankText_NamesRejectedParameter()
    {
        _text = WhitespaceText;

        Assert.That(
            GuardArrangedText,
            Throws.ArgumentException.With.Property(ParameterNamePropertyName).EqualTo(TextParameterName));
    }

    [Test]
    public void Test_ThrowIfEmpty_BlankText_LogsEmptyArgumentWarningOnce()
    {
        _text = WhitespaceText;

        IsRejected(GuardArrangedText);

        _logger.Received(ExpectedCallCount).WarnFormat(EmptyArgumentFormat, TextParameterName);
    }

    private static bool IsRejected(Action guard)
    {
        bool isRejected = false;

        try
        {
            guard();
        }
        catch (ArgumentException)
        {
            isRejected = true;
        }

        return isRejected;
    }

    private void GuardNotNullArgument()
    {
        ArgumentGuard.NotNull(_argument, ParameterName, _logger);
    }

    private void GuardArrangedArgument()
    {
        ArgumentGuard.ThrowIfFull(_argument, ParameterName, _logger);
    }

    private void GuardArrangedText()
    {
        ArgumentGuard.ThrowIfEmpty(_text, TextParameterName, _logger);
    }
}
