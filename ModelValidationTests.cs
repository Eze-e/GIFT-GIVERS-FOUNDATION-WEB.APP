using System.ComponentModel.DataAnnotations;
using ST10371895_Part3.Models;
using Xunit;

namespace ST10371895_Part3_Test.UnitTests.Models
{
    public class ModelValidationTests
    {
        [Fact]
        public void RegisterViewModel_ValidData_PassesValidation()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "1234567890"
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void LoginViewModel_ValidData_PassesValidation()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Username = "testuser",
                Password = "password123"
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void DonationViewModel_ValidData_PassesValidation()
        {
            // Arrange
            var model = new DonationViewModel
            {
                DonationType = "Food",
                ItemDescription = "Canned goods for disaster relief",
                Quantity = 10,
                Unit = "kg",
                TargetArea = "Flood Area"
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void IncidentReportViewModel_ValidData_PassesValidation()
        {
            // Arrange
            var model = new IncidentReportViewModel
            {
                Title = "Test Incident",
                Description = "Detailed description of the incident",
                Location = "Test Location",
                IncidentDate = DateTime.UtcNow,
                DisasterType = "Flood",
                AffectedAreas = "Test Area",
                UrgencyLevel = "High"
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void VolunteerViewModel_ValidData_PassesValidation()
        {
            // Arrange
            var model = new VolunteerViewModel
            {
                Skills = "First Aid, Emergency Response",
                Availability = "Weekends",
                PreferredLocation = "Local Area",
                EmergencyContact = "Contact Person - 1234567890",
                HasTransportation = true
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Empty(results);
        }

        private List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, context, validationResults, true);
            return validationResults;
        }
    }
}