using AutoFixture.Xunit2;

using System;
using System.Collections.Generic;
using System.Text;

namespace Scynett.Hubtel.Payments.Tests.Testing;

internal sealed class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
{
    public InlineAutoMoqDataAttribute(params object[] values)
        : base(new AutoMoqDataAttribute(), values)
    {
    }
}