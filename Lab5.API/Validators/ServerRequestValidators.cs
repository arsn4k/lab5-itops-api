using FluentValidation;
using Lab5.Services;

namespace Lab5.API.Validators;

public sealed class CreateServerRequestValidator : AbstractValidator<CreateServerRequest>
{
    public CreateServerRequestValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessage("Назва сервера є обов’язковою.")
            .MaximumLength(100).WithMessage("Назва сервера не може перевищувати 100 символів.");

        RuleFor(r => r.Type)
            .IsInEnum().WithMessage("Вказано недопустимий тип сервера.");

        RuleFor(r => r.Host)
            .NotEmpty().WithMessage("Адреса сервера (host) є обов’язковою.")
            .MaximumLength(255).WithMessage("Адреса сервера не може перевищувати 255 символів.");

        RuleFor(r => r.CpuUsage)
            .Must(v => !v.HasValue || (v.Value >= 0m && v.Value <= 100m))
            .WithMessage("Завантаження CPU має бути в діапазоні від 0 до 100.");

        RuleFor(r => r.MemoryUsage)
            .Must(v => !v.HasValue || (v.Value >= 0m && v.Value <= 100m))
            .WithMessage("Використання пам’яті має бути в діапазоні від 0 до 100.");
    }
}

public sealed class UpdateServerRequestValidator : AbstractValidator<UpdateServerRequest>
{
    public UpdateServerRequestValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessage("Назва сервера є обов’язковою.")
            .MaximumLength(100).WithMessage("Назва сервера не може перевищувати 100 символів.");

        RuleFor(r => r.Type)
            .IsInEnum().WithMessage("Вказано недопустимий тип сервера.");

        RuleFor(r => r.Host)
            .NotEmpty().WithMessage("Адреса сервера (host) є обов’язковою.")
            .MaximumLength(255).WithMessage("Адреса сервера не може перевищувати 255 символів.");

        RuleFor(r => r.CpuUsage)
            .Must(v => !v.HasValue || (v.Value >= 0m && v.Value <= 100m))
            .WithMessage("Завантаження CPU має бути в діапазоні від 0 до 100.");

        RuleFor(r => r.MemoryUsage)
            .Must(v => !v.HasValue || (v.Value >= 0m && v.Value <= 100m))
            .WithMessage("Використання пам’яті має бути в діапазоні від 0 до 100.");
    }
}

public sealed class SetServerStatusRequestValidator : AbstractValidator<SetServerStatusRequest>
{
    // Допустимі стани беруться з доменного enum ServerStatus (Online, Offline, Maintenance).
    private static readonly string[] AllowedStatuses = Enum.GetNames<ServerStatus>();

    public SetServerStatusRequestValidator()
    {
        RuleFor(r => r.Status)
            .NotEmpty().WithMessage("Стан сервера є обов’язковим.")
            .Must(status => AllowedStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Вказано недопустимий стан сервера. Допустимі: Online, Offline, Maintenance.");
    }
}
