using System.ComponentModel.DataAnnotations;
using TaskManagerApi.DTOs;
using TaskManagerApi.Models;

namespace TaskManagerApi.Tests.Models;

public class TaskValidationTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }

    [Fact]
    public void TaskItem_ValidTask_PassesValidation()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = 1,
            Title = "Valid Task",
            Description = "Valid Description",
            IsCompleted = false,
            Priority = Priority.Medium,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var validationResults = ValidateModel(task);

        // Assert
        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void TaskItem_InvalidTitle_FailsValidation(string? title)
    {
        // Arrange
        var task = new TaskItem
        {
            Id = 1,
            Title = title!,
            Description = "Valid Description",
            IsCompleted = false,
            Priority = Priority.Medium,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var validationResults = ValidateModel(task);

        // Assert
        Assert.Single(validationResults);
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Title"));
    }

    [Fact]
    public void TaskItem_TitleTooLong_FailsValidation()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = 1,
            Title = new string('A', 201), // 201 characters, exceeds 200 limit
            Description = "Valid Description",
            IsCompleted = false,
            Priority = Priority.Medium,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var validationResults = ValidateModel(task);

        // Assert
        Assert.Single(validationResults);
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Title"));
    }

    [Fact]
    public void TaskItem_DescriptionTooLong_FailsValidation()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = 1,
            Title = "Valid Title",
            Description = new string('A', 1001), // 1001 characters, exceeds 1000 limit
            IsCompleted = false,
            Priority = Priority.Medium,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var validationResults = ValidateModel(task);

        // Assert
        Assert.Single(validationResults);
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Description"));
    }

    [Theory]
    [InlineData(Priority.Low)]
    [InlineData(Priority.Medium)]
    [InlineData(Priority.High)]
    public void TaskItem_ValidPriority_PassesValidation(Priority priority)
    {
        // Arrange
        var task = new TaskItem
        {
            Id = 1,
            Title = "Valid Task",
            Description = "Valid Description",
            IsCompleted = false,
            Priority = priority,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var validationResults = ValidateModel(task);

        // Assert
        Assert.Empty(validationResults);
    }
}

public class TaskCreateDtoValidationTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }

    [Fact]
    public void TaskCreateDto_ValidData_PassesValidation()
    {
        // Arrange
        var dto = new TaskCreateDto
        {
            Title = "Valid Task",
            Description = "Valid Description",
            Priority = Priority.Medium
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void TaskCreateDto_InvalidTitle_FailsValidation(string? title)
    {
        // Arrange
        var dto = new TaskCreateDto
        {
            Title = title!,
            Description = "Valid Description",
            Priority = Priority.Medium
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Single(validationResults);
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Title"));
    }

    [Fact]
    public void TaskCreateDto_TitleTooLong_FailsValidation()
    {
        // Arrange
        var dto = new TaskCreateDto
        {
            Title = new string('A', 201), // 201 characters, exceeds 200 limit
            Description = "Valid Description",
            Priority = Priority.Medium
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Single(validationResults);
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Title"));
    }

    [Fact]
    public void TaskCreateDto_DescriptionTooLong_FailsValidation()
    {
        // Arrange
        var dto = new TaskCreateDto
        {
            Title = "Valid Title",
            Description = new string('A', 1001), // 1001 characters, exceeds 1000 limit
            Priority = Priority.Medium
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Single(validationResults);
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Description"));
    }

    [Fact]
    public void TaskCreateDto_NullDescription_PassesValidation()
    {
        // Arrange
        var dto = new TaskCreateDto
        {
            Title = "Valid Title",
            Description = null,
            Priority = Priority.Medium
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Empty(validationResults);
    }
}

public class TaskUpdateDtoValidationTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }

    [Fact]
    public void TaskUpdateDto_ValidData_PassesValidation()
    {
        // Arrange
        var dto = new TaskUpdateDto
        {
            Title = "Valid Task",
            Description = "Valid Description",
            Priority = Priority.High,
            IsCompleted = true
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData("")]
    public void TaskUpdateDto_InvalidTitle_FailsValidation(string? title)
    {
        // Arrange
        var dto = new TaskUpdateDto
        {
            Title = title!,
            Description = "Valid Description",
            Priority = Priority.Medium,
            IsCompleted = false
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Single(validationResults);
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Title"));
    }

    [Fact]
    public void TaskUpdateDto_NullTitle_IsValid()
    {
        // Arrange
        var dto = new TaskUpdateDto
        {
            Title = null,
            Description = "Valid Description",
            Priority = Priority.Medium,
            IsCompleted = false
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void TaskUpdateDto_TitleTooLong_FailsValidation()
    {
        // Arrange
        var dto = new TaskUpdateDto
        {
            Title = new string('A', 201), // 201 characters, exceeds 200 limit
            Description = "Valid Description",
            Priority = Priority.Medium,
            IsCompleted = false
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Single(validationResults);
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Title"));
    }

    [Fact]
    public void TaskUpdateDto_DescriptionTooLong_FailsValidation()
    {
        // Arrange
        var dto = new TaskUpdateDto
        {
            Title = "Valid Title",
            Description = new string('A', 1001), // 1001 characters, exceeds 1000 limit
            Priority = Priority.Medium,
            IsCompleted = false
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Single(validationResults);
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Description"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TaskUpdateDto_ValidIsCompleted_PassesValidation(bool isCompleted)
    {
        // Arrange
        var dto = new TaskUpdateDto
        {
            Title = "Valid Task",
            Description = "Valid Description",
            Priority = Priority.Low,
            IsCompleted = isCompleted
        };

        // Act
        var validationResults = ValidateModel(dto);

        // Assert
        Assert.Empty(validationResults);
    }
}

public class PriorityEnumTests
{
    [Fact]
    public void Priority_HasExpectedValues()
    {
        // Assert
        Assert.Equal(1, (int)Priority.Low);
        Assert.Equal(2, (int)Priority.Medium);
        Assert.Equal(3, (int)Priority.High);
    }

    [Theory]
    [InlineData(Priority.Low, "Low")]
    [InlineData(Priority.Medium, "Medium")]
    [InlineData(Priority.High, "High")]
    public void Priority_ToString_ReturnsExpectedString(Priority priority, string expected)
    {
        // Act & Assert
        Assert.Equal(expected, priority.ToString());
    }

    [Theory]
    [InlineData("Low", Priority.Low)]
    [InlineData("Medium", Priority.Medium)]
    [InlineData("High", Priority.High)]
    public void Priority_Parse_ReturnsExpectedEnum(string value, Priority expected)
    {
        // Act
        var result = Enum.Parse<Priority>(value);

        // Assert
        Assert.Equal(expected, result);
    }
}

