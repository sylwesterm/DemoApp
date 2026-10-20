using CurrencyRates.Application.Behaviors;
using FluentValidation;
using MediatR;

namespace CurrencyRates.Application.Tests.Behaviors;

public class ValidationBehaviorTests
{
    public sealed record TestRequest(string Value) : IRequest<string>;

    private bool nextCalled;

    private Task<string> Next(CancellationToken cancellationToken)
    {
        nextCalled = true;
        return Task.FromResult("handled");
    }

    private static InlineValidator<TestRequest> Validator(Action<InlineValidator<TestRequest>> rules)
    {
        var validator = new InlineValidator<TestRequest>();
        rules(validator);
        return validator;
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsNextHandler()
    {
        var behavior = new ValidationBehavior<TestRequest, string>(
            [Validator(v => v.RuleFor(r => r.Value).NotEmpty())]);

        var result = await behavior.Handle(new TestRequest("ok"), Next, CancellationToken.None);

        result.ShouldBe("handled");
        nextCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_NoValidators_CallsNextHandler()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([]);

        await behavior.Handle(new TestRequest(""), Next, CancellationToken.None);

        nextCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_InvalidRequest_ThrowsWithFailuresFromAllValidatorsAndSkipsHandler()
    {
        var behavior = new ValidationBehavior<TestRequest, string>(
        [
            Validator(v => v.RuleFor(r => r.Value).NotEmpty().WithMessage("Pierwszy błąd.")),
            Validator(v => v.RuleFor(r => r.Value).MinimumLength(3).WithMessage("Drugi błąd.")),
        ]);

        var exception = await Should.ThrowAsync<ValidationException>(
            () => behavior.Handle(new TestRequest(""), Next, CancellationToken.None));

        exception.Errors.Select(e => e.ErrorMessage).ShouldBe(["Pierwszy błąd.", "Drugi błąd."], ignoreOrder: true);
        nextCalled.ShouldBeFalse();
    }
}
