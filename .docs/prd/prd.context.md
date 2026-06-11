# PRD Creation Context

## Original Product Idea

Source: `.docs/prd/pr-ideas.md`

> I have a basic idea for an application that will allow users to find the restaurant of their choice and book a reservation for dining with that particular restaurant. At some point, the user will have to specify how many are in the party and the date of the reservation for the restaurant in question.

## Clarifying Questions & Answers

| Question | Answer |
|---|---|
| Target platform | Web (browser only) |
| Primary audience | Both equally — dual-sided marketplace (diners + restaurant operators) |
| Restaurant data management | Seeded/mock data for MVP; self-service restaurant management deferred to a later phase |
| Phase 2 notification channels | Email only (confirmation, reminders, .ics calendar attachment) |
| Authentication method | Email + password |
| Tech stack | Angular 20 (frontend) + .NET backend |
| Phase 3 review eligibility | Open to all registered users (not restricted to verified diners) |

## Phased Roadmap (as stated by user)

- **Phase 1** — Sign-in, party size selection, reserve open time from restaurant listings
- **Phase 2** — Email notifications (booking confirmation + reminders), calendar event (.ics attachment), directions link
- **Phase 3** — User reviews, restaurant can upload menu/entree photos tied to a reservation time
- **Phase 4** — Favorites section highlighting most-booked restaurants by week/month/locale
