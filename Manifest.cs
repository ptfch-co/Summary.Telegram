using Core.Modules.Manifest;
using Summary.Telegram;

[assembly: Feature(
    Id = Telegram.Features.Telegram,
    Name = Telegram.Localize.SubjectOfTelegram,
    Description =Telegram.Localize.DescriptionOfTelegram,
    Category = Telegram.Public.Category,
    Dependencies = new[] { "Core.Workflows" },
    Version = "1.0.0"
)]