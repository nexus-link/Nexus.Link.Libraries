using System;
using System.Threading.Tasks;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services.State;
using Nexus.Link.Libraries.Core.Error.Logic;
using Xunit;

namespace Nexus.Link.Capabilities.WorkflowMgmt.UnitTests.Services
{
    public abstract class ActivityInstanceServiceTestsBase
    {
        private readonly IActivityInstanceService _service;

        protected ActivityInstanceServiceTestsBase(IActivityInstanceService service)
        {
            _service = service;
        }

        [Fact]
        public async Task CreateAndReadAsync()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var workflowInstanceId = Guid.NewGuid().ToString();
            var activityVersionId = Guid.NewGuid().ToString();
            var itemToCreate = new ActivityInstanceCreate
            {
                WorkflowInstanceId = workflowInstanceId,
                ActivityVersionId = activityVersionId,
                ParentActivityInstanceId = Guid.NewGuid().ToString(),
                ParentIteration = 1, 
            };

            // Act
            var created = await _service.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
            var findUnique = new ActivityInstanceUnique
            {
                WorkflowInstanceId = itemToCreate.WorkflowInstanceId,
                ActivityVersionId = itemToCreate.ActivityVersionId,
                ParentActivityInstanceId = itemToCreate.ParentActivityInstanceId,
                ParentIteration = itemToCreate.ParentIteration
            };
            var readItem = await _service.ReadAsync(id);

            // Assert
            Assert.NotNull(readItem);
            Assert.Equal(created.Id, readItem.Id);
            Assert.Equal(workflowInstanceId, readItem.WorkflowInstanceId);
            Assert.Equal(itemToCreate.ActivityVersionId, readItem.ActivityVersionId);
            Assert.Equal(itemToCreate.ParentActivityInstanceId, readItem.ParentActivityInstanceId);
            Assert.Equal(itemToCreate.ParentIteration, readItem.ParentIteration);
        }

        [Fact]
        public async Task UpdateAsync()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var workflowInstanceId = Guid.NewGuid().ToString();
            var activityVersionId = Guid.NewGuid().ToString();
            var itemToCreate = new ActivityInstanceCreate
            {
                WorkflowInstanceId = workflowInstanceId,
                ActivityVersionId = activityVersionId,
                ParentActivityInstanceId = Guid.NewGuid().ToString(),
                ParentIteration = 1, 
            };
            var created = await _service.CreateWithSpecifiedIdAndReturnAsync(id, itemToCreate);
            var findUnique = new ActivityInstanceUnique
            {
                WorkflowInstanceId = itemToCreate.WorkflowInstanceId,
                ActivityVersionId = itemToCreate.ActivityVersionId,
                ParentActivityInstanceId = itemToCreate.ParentActivityInstanceId,
                ParentIteration = itemToCreate.ParentIteration
            };
            var itemToUpdate = await _service.ReadAsync(id);

            // Act
            itemToUpdate.FinishedAt = DateTimeOffset.Now;
            itemToUpdate.ExceptionCategory = ActivityExceptionCategoryEnum.Technical;
            itemToUpdate.ExceptionFriendlyMessage =  Guid.NewGuid().ToString();
            itemToUpdate.ExceptionTechnicalMessage = Guid.NewGuid().ToString();
            var updatedItem = await _service.UpdateAndReturnAsync(created.Id, itemToUpdate);

            // Assert
            Assert.Equal(created.Id, updatedItem.Id);
            Assert.Equal(workflowInstanceId, updatedItem.WorkflowInstanceId);
            Assert.Equal(itemToUpdate.ActivityVersionId, updatedItem.ActivityVersionId);
            Assert.Equal(itemToUpdate.ParentActivityInstanceId, updatedItem.ParentActivityInstanceId);
            Assert.Equal(itemToUpdate.FinishedAt, updatedItem.FinishedAt);
            Assert.Equal(itemToUpdate.ParentIteration, updatedItem.ParentIteration);
            Assert.Equal(itemToUpdate.State, updatedItem.State);
            Assert.Equal(itemToUpdate.ExceptionCategory, updatedItem.ExceptionCategory);
            Assert.Equal(itemToUpdate.ExceptionFriendlyMessage, updatedItem.ExceptionFriendlyMessage);
            Assert.Equal(itemToUpdate.ExceptionTechnicalMessage, updatedItem.ExceptionTechnicalMessage);
        }
    }
}
