using AutoFixture.Xunit2;

using Scynett.Hubtel.Payments.Tests.Testing.Fixtures;

using System;
using System.Collections.Generic;
using System.Text;

namespace Scynett.Hubtel.Payments.Tests.Testing.Attributes;

internal sealed class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute()
        : base(FixtureFactory.Create)
    {
    }
}
