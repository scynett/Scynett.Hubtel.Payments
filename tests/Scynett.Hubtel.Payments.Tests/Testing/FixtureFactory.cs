using AutoFixture;
using AutoFixture.AutoMoq;

namespace Scynett.Hubtel.Payments.Tests.Testing;

internal static class FixtureFactory
{
    public static IFixture Create()
    {
        var fixture = new Fixture();

        fixture.Customize(new AutoMoqCustomization
        {
            ConfigureMembers = true
        });

        // Helps avoid recursion issues when object graphs loop
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        return fixture;
    }
}