using System;
using DominoTrainGame.Server.Persistence.Email;
using NUnit.Framework;

namespace DominoTrainGame.Server.Persistence.Tests.Unit.Email;

[TestFixture]
public sealed class SmtpEmailSenderTests
{
    private const string RecipientEmail = "samuel@uv.mx";
    private const string VerificationCode = "482915";
    private const string SpanishLanguage = "es";
    private const string RecipientParameterName = "recipientEmail";
    private const string CodeParameterName = "verificationCode";
    private const string ParameterNamePropertyName = nameof(ArgumentException.ParamName);
    private const string EmptyText = "";
    private const string WhitespaceText = "   ";

    private SmtpEmailSender _emailSender;
    private string _recipientEmail;
    private string _verificationCode;

    [SetUp]
    public void SetUp()
    {
        _emailSender = new SmtpEmailSender();
        _recipientEmail = RecipientEmail;
        _verificationCode = VerificationCode;
    }

    [TestCase(EmptyText)]
    [TestCase(WhitespaceText)]
    [TestCase(null)]
    public void Test_SendVerificationCode_BlankRecipient_ThrowsArgumentException(string blankRecipient)
    {
        _recipientEmail = blankRecipient;

        Assert.Throws<ArgumentException>(SendArrangedVerificationCode);
    }

    [Test]
    public void Test_SendVerificationCode_BlankRecipient_NamesRecipientParameter()
    {
        _recipientEmail = WhitespaceText;

        Assert.That(
            SendArrangedVerificationCode,
            Throws.ArgumentException.With.Property(ParameterNamePropertyName).EqualTo(RecipientParameterName));
    }

    [TestCase(EmptyText)]
    [TestCase(WhitespaceText)]
    [TestCase(null)]
    public void Test_SendVerificationCode_BlankCode_ThrowsArgumentException(string blankCode)
    {
        _verificationCode = blankCode;

        Assert.Throws<ArgumentException>(SendArrangedVerificationCode);
    }

    [Test]
    public void Test_SendVerificationCode_BlankCode_NamesCodeParameter()
    {
        _verificationCode = WhitespaceText;

        Assert.That(
            SendArrangedVerificationCode,
            Throws.ArgumentException.With.Property(ParameterNamePropertyName).EqualTo(CodeParameterName));
    }

    private void SendArrangedVerificationCode()
    {
        _emailSender.SendVerificationCode(_recipientEmail, _verificationCode, SpanishLanguage);
    }
}
