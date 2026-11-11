using MegaMarketLibnk.Helpers;

namespace MegaMarketLibnk.Tests;

public class ValidationTests
{
    [Fact]
    public void IsValidEmail_Valid_ReturnsTrue()
    {
        //arrange
        string email = "user@example.com";
        bool expected = true;
        //act
        bool act = Validation.IsValidEmail(email);
        //assert
        Assert.Equal(expected, act);
    }

    [Fact]
    public void IsValidEmail_NoAt_ReturnsFalse()
    {
        //arrange
        string email = "userexample.com";
        bool expected = false;
        //act
        bool act = Validation.IsValidEmail(email);
        //assert
        Assert.Equal(expected, act);
    }

    [Fact]
    public void IsValidEmail_WithSpace_ReturnsFalse()
    {
        //arrange
        string email = "user @example.com";
        bool expected = false;
        //act
        bool act = Validation.IsValidEmail(email);
        //assert
        Assert.Equal(expected, act);
    }

    [Fact]
    public void IsValidPassword_Valid_ReturnsTrue()
    {
        //arrange
        string password = "abc123";
        bool expected = true;
        //act
        bool act = Validation.IsValidPassword(password);
        //assert
        Assert.Equal(expected, act);
    }

    [Fact]
    public void IsValidPassword_TooShort_ReturnsFalse()
    {
        //arrange
        string password = "ab1";
        bool expected = false;
        //act
        bool act = Validation.IsValidPassword(password);
        //assert
        Assert.Equal(expected, act);
    }

    [Fact]
    public void IsValidPassword_NoDigit_ReturnsFalse()
    {
        //arrange
        string password = "abcdef";
        bool expected = false;
        //act
        bool act = Validation.IsValidPassword(password);
        //assert
        Assert.Equal(expected, act);
    }

    [Fact]
    public void IsValidPhone_ElevenDigits_ReturnsTrue()
    {
        //arrange
        string phone = "+7 (999) 123-45-67";
        bool expected = true;
        //act
        bool act = Validation.IsValidPhone(phone);
        //assert
        Assert.Equal(expected, act);
    }

    [Fact]
    public void IsValidPhone_TenDigits_ReturnsFalse()
    {
        //arrange
        string phone = "9991234567";
        bool expected = false;
        //act
        bool act = Validation.IsValidPhone(phone);
        //assert
        Assert.Equal(expected, act);
    }

    [Fact]
    public void IsValidInn_TenDigits_ReturnsTrue()
    {
        //arrange
        string inn = "1234567890";
        bool expected = true;
        //act
        bool act = Validation.IsValidInn(inn);
        //assert
        Assert.Equal(expected, act);
    }

    [Fact]
    public void GenerateCaptchaCode_LengthFour_ReturnsTrue()
    {
        //arrange
        int expected = 4;
        //act
        string code = Validation.GenerateCaptchaCode();
        //assert
        Assert.Equal(expected, code.Length);
    }
}