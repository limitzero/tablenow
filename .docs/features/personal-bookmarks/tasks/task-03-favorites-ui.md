# Task 03: Save/Unsave UI & My Favorites Dashboard Tab

## Status

pending

## Wave

3

## Description

Adds a heart/bookmark icon to restaurant cards showing saved state. Clicking toggles the favorite. Adds a "My Favorites" tab to the My Reservations dashboard (or a separate route). Unauthenticated users are redirected to login.

## Dependencies

**Depends on:** task-02-favorites-store.md
**Blocks:** Nothing

**Context from dependencies:**
- task-02: `FavoritesStore.isFavorite()` is a function returning a boolean for a given restaurantId
- `FavoritesService.add(id)` / `remove(id)` toggle saved state
- `RestaurantCardComponent` (STORY-012) needs a save button added

## Files to Create

- `client/src/app/features/restaurants/components/my-favorites/my-favorites.component.ts`
- `client/src/app/features/restaurants/components/my-favorites/my-favorites.component.html`

## Files to Modify

- `client/src/app/features/restaurants/components/restaurant-card/restaurant-card.component.ts` — Add save/unsave button
- `client/src/app/features/restaurants/components/restaurant-card/restaurant-card.component.html` — Heart icon

## Technical Details

Add to `RestaurantCardComponent`:
```typescript
private readonly favStore = inject(FavoritesStore);
private readonly favService = inject(FavoritesService);
private readonly authService = inject(AuthService);
private readonly router = inject(Router);

isSaved = computed(() => this.favStore.isFavorite()(this.restaurant().id));

toggleSave(event: MouseEvent): void {
  event.stopPropagation();
  if (!this.authService.isAuthenticated()) {
    this.router.navigateByUrl('/auth/login');
    return;
  }
  const id = this.restaurant().id;
  const action = this.isSaved() ? this.favService.remove(id) : this.favService.add(id);
  action.subscribe();
}
```

In card HTML:
```html
<button class="save-btn" (click)="toggleSave($event)" [attr.aria-label]="isSaved() ? 'Unsave' : 'Save'">
  {{ isSaved() ? '♥' : '♡' }}
</button>
```

My Favorites page:
```typescript
@Component({ ... })
export class MyFavoritesComponent {
  private readonly favStore = inject(FavoritesStore);
  private readonly restaurantsStore = inject(RestaurantsStore);

  readonly favorites = computed(() =>
    restaurantsStore.restaurants().filter(r =>
      favStore.isFavorite()(r.id)));
}
```

## Acceptance Criteria

- [ ] Heart icon on restaurant cards shows filled/unfilled based on saved state
- [ ] Clicking heart toggles saved state
- [ ] Unauthenticated heart click redirects to `/auth/login`
- [ ] My Favorites page shows all saved restaurants
- [ ] `npm run build` exits with code 0
