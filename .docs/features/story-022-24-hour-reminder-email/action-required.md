# Action Required: 24-Hour Reminder Email

No manual steps required for this story.

**Prerequisites:**
- STORY-020 SendGrid API key configured (`Email__ApiKey` env var)
- Run new EF migration after task-01 adds `ReminderSent` column: `dotnet ef migrations add AddReminderSentToReservation --project server/src/Migrations`
