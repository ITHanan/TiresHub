using ApplicationLayer.Common.Mappings;

namespace Tests.Fakes;

public class FakeCurrentUser : ICurrentUser
{
    public Guid UserId { get; }

    public FakeCurrentUser(Guid userId)
    {
        UserId = userId;
    }
}
