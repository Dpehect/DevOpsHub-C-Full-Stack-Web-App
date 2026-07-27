using DevOpsHub.Domain;

namespace DevOpsHub.Tests;

public sealed class EntityTests
{
    private sealed class TestEntity : Entity { }

    [Fact]
    public void Entity_should_generate_id_and_created_date()
    {
        var entity = new TestEntity();
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.True(entity.CreatedAtUtc <= DateTime.UtcNow);
    }
}
