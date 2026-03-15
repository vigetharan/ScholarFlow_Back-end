using FluentValidation;

namespace ScholarFlow.Application.Features.Questions.Commands.CreateQuestion;

/// <summary>
/// Validator for CreateQuestionCommand
/// </summary>
public class CreateQuestionCommandValidator : AbstractValidator<CreateQuestionCommand>
{
    public CreateQuestionCommandValidator()
    {
        RuleFor(x => x.PaperId)
            .NotEmpty().WithMessage("Paper is required");

        RuleFor(x => x.SubTopicId)
            .NotEmpty().WithMessage("SubTopic is required");

        RuleFor(x => x.QuestionText)
            .NotEmpty().WithMessage("Question text is required")
            .MinimumLength(5).WithMessage("Question text must be at least 5 characters")
            .MaximumLength(2000).WithMessage("Question text must not exceed 2000 characters");

        RuleFor(x => x.Difficulty)
            .InclusiveBetween(1, 10).WithMessage("Difficulty must be between 1 and 10");

        RuleFor(x => x.QuestionImageUrl)
            .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.QuestionImageUrl));

        RuleFor(x => x.Explanation)
            .MaximumLength(2000).WithMessage("Explanation must not exceed 2000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Explanation));

        // Require exactly five options for each question and exactly one correct option
        RuleFor(x => x.Options)
            .Must(options => options != null && options.Count == 5)
            .WithMessage("Exactly five options are required for each question");

        RuleFor(x => x.Options)
            .Must(options => options != null && options.Count(o => o.IsCorrect) == 1)
            .WithMessage("Exactly one option must be marked as correct");

        RuleForEach(x => x.Options)
            .ChildRules(option =>
            {
                option.RuleFor(o => o.OptionText)
                    .NotEmpty().WithMessage("Option text is required")
                    .MaximumLength(500).WithMessage("Option text must not exceed 500 characters");
            })
            .When(x => x.Options != null && x.Options.Count > 0);
    }
}
