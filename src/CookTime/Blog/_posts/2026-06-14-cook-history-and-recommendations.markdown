---
layout: post
title: "Cook history and recipe recommendations"
date: 2026-06-14 00:00:00 -0400
categories: features, recommendations
---

Two new features are live on CookTime: cook history and recipe recommendations.

# Cook History

If you are signed in, every recipe page now shows a "Cooked" section with an "I cooked this" button.
Clicking it logs today's date as a cook event for that recipe.
The section shows how many times you have cooked a recipe and when you last made it.

You can also visit your personal History page from the user menu.
It lists every recipe you have ever cooked and lets you delete entries if you made a mistake.

Cook events are stored one per user, recipe, and day, so clicking "I cooked this" multiple times on the same day is safe.

# Recipe Recommendations

At the bottom of every recipe page you will now find a "You might also like" section with up to six recipe suggestions.
These are visible to all users, signed in or not.

## How the scoring works

The backbone of the recommendation is ingredient similarity.
We use [Jaccard similarity](https://en.wikipedia.org/wiki/Jaccard_index) over the distinct set of ingredient IDs shared between two recipes:

```
score = |shared ingredients| / |union of all ingredients|
```

A recipe with no overlapping ingredients scores zero and is never shown.

For signed-in users we add three more signals on top of the similarity score:

| Signal | Weight | Meaning |
|---|---|---|
| Ingredient similarity | 60% | Jaccard similarity |
| Owned by you | 15% | You created the recipe |
| In your Favorites list | 15% | You have favorited it |
| Novelty | 10% | Not cooked in the last 7 days |

All of this runs as a single PostgreSQL function using CTEs, so there is no round-tripping between the API and the database to build the ranked list.

## Score breakdowns for admins

If you have an Administrator account, each recommendation card shows the total score and an expandable breakdown of exactly how much each signal contributed.
This made it a lot easier to tune weights during development — we could see at a glance whether a card ranked highly because of ingredient overlap or because we happened to own the recipe.

## What we ruled out

An early version of the filter included a "novelty" clause that surfaced recipes you had never cooked before, regardless of ingredient overlap.
That turned out to bring in completely unrelated recipes from the database, so we removed it.
Novelty is now only a tiebreaker boost, never a reason to include a recipe on its own.

# What's next

The scoring model has a deferred "diet match" slot already wired into the score breakdown.
Once CookTime has diet preference tracking, that signal can be filled in without changing the API contract.
We would also like to add edit support on the History page so you can correct the date of a cook event without deleting and re-adding it.
