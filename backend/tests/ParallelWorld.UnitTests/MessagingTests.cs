using Microsoft.Extensions.Options;
using ParallelWorld.AI;
using ParallelWorld.Application.Messaging;
using ParallelWorld.Domain.Messaging;
using ParallelWorld.Domain.Relationships;

namespace ParallelWorld.UnitTests;

public sealed class MessagingTests
{
    [Fact]
    public void Msg02_UsesAcceptedFixedInputsAndExactFormula()
    {
        Assert.Equal(50, MessageReplyMechanics.Urgency);
        Assert.Equal(0, MessageReplyMechanics.ConflictAvoidancePenalty);
        Assert.Equal(62, MessageReplyMechanics.Score(RelationshipValues.Initial, 55));
    }

    [Fact]
    public void Msg02_RollIsRepeatableScopedAndHasNoTextInput()
    {
        var world = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var message = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var character = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var player = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var first = MessageReplyMechanics.Roll(world, 90210, 1, message, character, player);
        Assert.Equal(first, MessageReplyMechanics.Roll(world, 90210, 1, message, character, player));
        Assert.NotEqual(first, MessageReplyMechanics.Roll(world, 90211, 1, message, character, player));
        Assert.InRange(first, 0, 99);
    }

    [Fact]
    public void Msg02_RivalryAloneDoesNotCreateAConflictPenalty()
    {
        var baseline = RelationshipValues.Initial;
        var rivalryOnly = baseline with { Rivalry = 100 };

        Assert.Equal(MessageReplyMechanics.Score(baseline, 55),
            MessageReplyMechanics.Score(rivalryOnly, 55));
        Assert.Equal(0, MessageReplyMechanics.ConflictAvoidancePenalty);
    }

    [Fact]
    public async Task MessageWording_DisabledProviderUsesDeterministicFallbackAfterDecision()
    {
        var provider = new RejectingProvider();
        var generator = new MessageWordingGenerator(provider, new AiPromptBuilder(),
            new DeterministicAiFallbackRenderer(), new AiOutputValidator(), Options.Create(new AiGenerationOptions
            {
                Enabled = false,
                BaseUrl = "http://localhost:11434",
                Model = "qwen3:4b",
                Timeout = TimeSpan.FromSeconds(10),
                MaxOutputLength = 500,
                PromptTemplateVersion = "m09-v1",
            }));
        var request = new MessageWordingRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Maya", "Player", "calm", "concise", "Hello", 500, Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), []);

        var result = await generator.GenerateAsync(request, CancellationToken.None);

        Assert.True(result.FallbackUsed);
        Assert.Equal("provider_disabled", result.FailureCode);
        Assert.Equal(0, provider.CallCount);
        Assert.False(string.IsNullOrWhiteSpace(result.Text));
    }

    [Fact]
    public async Task MessageWording_UsesOnlyProvidedMemoryContextAsUntrustedWordingData()
    {
        var provider = new CapturingProvider();
        var generator = new MessageWordingGenerator(provider, new AiPromptBuilder(),
            new DeterministicAiFallbackRenderer(), new AiOutputValidator(), Options.Create(new AiGenerationOptions
            {
                Enabled = true,
                BaseUrl = "http://localhost:11434",
                Model = "qwen3:4b",
                Timeout = TimeSpan.FromSeconds(10),
                MaxOutputLength = 500,
                PromptTemplateVersion = "m09-v1",
            }));
        var request = new MessageWordingRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Maya", "Player", "calm", "concise", "Hello", 500, Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), ["Met at the library."]);

        var result = await generator.GenerateAsync(request, CancellationToken.None);

        Assert.False(result.FallbackUsed);
        Assert.NotNull(provider.Request);
        Assert.Contains("Met at the library.", provider.Request!.Prompt.UserPrompt, StringComparison.Ordinal);
        Assert.Contains("UNTRUSTED_DATA", provider.Request.Prompt.UserPrompt, StringComparison.Ordinal);
    }

    [Fact]
    public void Message_TrimsContentAndRejectsOverLimit()
    {
        var now = DateTimeOffset.UtcNow;
        var message = new Message(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), null, Guid.NewGuid(), "  hello  ", MessageDeliveryStatus.Delivered, now);
        Assert.Equal("hello", message.Content);
        Assert.Throws<ArgumentOutOfRangeException>(() => new Message(Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), new string('x', 2001),
            MessageDeliveryStatus.Delivered, now));
    }

    private sealed class RejectingProvider : IAiTextProvider
    {
        public int CallCount { get; private set; }
        public Task<AiProviderResult> GenerateAsync(AiProviderRequest request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            throw new InvalidOperationException("Provider should not be called.");
        }
    }

    private sealed class CapturingProvider : IAiTextProvider
    {
        public AiProviderRequest? Request { get; private set; }

        public Task<AiProviderResult> GenerateAsync(AiProviderRequest request,
            CancellationToken cancellationToken = default)
        {
            Request = request;
            return Task.FromResult(AiProviderResult.Success("Thanks."));
        }
    }
}
