namespace EventService.Application.Authorization;

public abstract record BaseCommand
{
    public CurrentUser CurrentUser { get; set; } = null!;
}
