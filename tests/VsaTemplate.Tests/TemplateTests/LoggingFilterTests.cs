using VsaTemplate.Common.Pipeline;
using VsaTemplate.Tests.Infrastructure.TemplateTests;

namespace VsaTemplate.Tests.TemplateTests;

public sealed class LoggingFilterTests : TemplateTestBase
{
    [Test]
    public void LoggingFilterShouldLogRequestAndReturnNext()
    {
        var logger = new TemplateTestLogger<LoggingFilter>();
    }
}
