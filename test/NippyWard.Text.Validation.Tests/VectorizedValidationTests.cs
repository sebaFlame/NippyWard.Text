using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NippyWard.Text.Validation.Tests
{
    public class VectorizedValidationTests : BaseValidationTests
    {
        protected override IUtf8Validator CreateValidator()
            => new VectorizedUtf8Validator();
    }
}
