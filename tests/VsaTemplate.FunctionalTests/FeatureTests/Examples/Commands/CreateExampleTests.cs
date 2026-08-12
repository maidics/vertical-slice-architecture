using Shouldly;
using VsaTemplate.Common.Models;
using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.Commands;
using VsaTemplate.FunctionalTests.Infrastructure;
using VsaTemplate.FunctionalTests.Infrastructure.Common;

namespace VsaTemplate.FunctionalTests.FeatureTests.Examples.Commands;

public sealed class CreateExampleTests : ApplicationTestBase
{
    [Test]
    public async Task ShouldReturnConflictIfExampleWithContentExists()
    {
        var example = new Example { Content = "test" };

        await Testing.AddAsync(example);

        var command = new CreateExampleCommand(example.Content);

        var handler = GetService<CreateExampleCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeFailed(
            ResultType.Conflict,
            [$"Example already exists with content: {command.Content}"]
        );
    }

    [Test]
    public async Task ShouldCreateExample()
    {
        var command = new CreateExampleCommand("test");

        var handler = GetService<CreateExampleCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeSuccessful();

        var example = await Testing.FirstOrDefaultAsync<Example>(x => x.Id == result.Value);
        example.ShouldNotBeNull();
        example.Content.ShouldBe(command.Content);
    }
}
