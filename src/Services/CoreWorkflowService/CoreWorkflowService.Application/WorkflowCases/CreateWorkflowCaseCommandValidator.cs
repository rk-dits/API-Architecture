using FluentValidation;

namespace CoreWorkflowService.Application.WorkflowCases;

public class CreateWorkflowCaseCommandValidator : AbstractValidator<CreateWorkflowCaseCommand>
{
    public CreateWorkflowCaseCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
    }
}