//TODO: Артём напиши пожалуйста тест
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using DropSpace.Controllers;

public class HomeControllerTests
{
    [Fact]
    public void Index_ReturnsAViewResult()
    {
        // Arrange
        var controller = new HomeController();

        // Act
        var result = controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
    }

    [Fact]
    public void About_ReturnsViewWithMessage()
    {
        // Arrange
        var controller = new HomeController();

        // Act
        var result = controller.About();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Your application description page.", viewResult.ViewData["Message"]);
    }
}