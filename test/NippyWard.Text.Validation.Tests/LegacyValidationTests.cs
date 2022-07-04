using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NippyWard.Text.Validation.Tests
{
    public class LegacyValidationTests : BaseValidationTests
    {
        protected override IUtf8Validator CreateValidator()
            => new LegacyUtf8Validator();
    }
}
