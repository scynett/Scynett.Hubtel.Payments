using AutoFixture.Xunit2;

using System;
using System.Collections.Generic;
using System.Text;

namespace Scynett.Hubtel.Payments.Tests.Testing;

internal sealed class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute()
        : base(FixtureFactory.Create)
    {
    }
}
