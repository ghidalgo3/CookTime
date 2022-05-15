---
layout: post
title:  "Cumulative update"
date:   2022-05-15 00:36:58 -0400
categories: 
---

Almost 2 weeks without a blog post, lots of things have gotten done.
Get ready!

# New recipes
Any recipe created in the last week will be shown in the `New Recipes` list of the home page.
Usually these recipes are "in-development" so showing them first makes the author's job easier.

# Feature recipes
For the featured recipes lists, we query our recipe database for recipes with photos, take `N` of them, and then display them on the page.
It is preferable to ask the database to randomly sort and take the top `N` recipes after sorting, otherwise we have to load all the recipes client-side, randomize, and take `N` of them.
This can take an unnacceptable amount of time if the number of recipes is large.
CookTime uses PostgreSQL as its database, Entity Framework as the ORM, and the npsql as the bridge between LINQ queries and PostgreSQL statements. 
PostgreSQL can certainl do what we want, I found a stack overflow question asking for exactly this: https://stackoverflow.com/questions/654906/linq-to-entities-random-order

The only missing piece was to enable an extension on the database to allow it to generate UUIDs on-demand to create random sort orders. The query-command is this:

`CREATE EXTENSION IF NOT EXISTS "uuid-ossp";`

With all that database infrastructure in place, we now show featured recipes on the home page :).

# Pagination
The home page and query results are now paginated.
This should reduce the amount of scrolling needed on mobile devices to view recipes.
We observed that users get lost if the page is too long to scroll through; pages should address this issue.

# Ratings and Reviews
If you have a CookTime account, you can now rate and review recipes.
Your ratings will be aggregated and averaged.
Over time, we will come to find which recipes are the best recipes :) 

# Authentication improvements
You can now use your email OR your username to sign in to CookTime.
We now also display validation errors during the sign up process.
We have simplified the password requirements, basically you need atleast 2 unique characters and a minimum length of 6 total characters.

# Categories in the navigation bar
During user testing, we found that users wanted to know what were all of the categories of reicpes that CookTime had recipes for.
We did not have a way to view the list of categories, but now we do!
You can click on `Categories` in the navigation bar and then click on a category you're interested in to view all recieps for that category.

# Ingredient aliases
This feature is not user-facing but it did involve a big change in our backend so I'd like to describe it.

Early in CookTime's development we identified a need for ingredient aliases.
For example, "milk" and "whole milk" are practically the same ingredient, so for the purposes of auto-completion and building a correct recipe database we should treat them as the same ingredient.
Initially this was _not_ the case, recipes with "milk" and "whole milk" would be referencing unique ingredients and this would create several kinds of problems for us.

We could not associate different variants of an ingredient name with unique nutrition facts.

We could not properly highlight ingredient names in the recipe text if the user used one name in the ingredients list but another name in the text.
This is extremely common: for example you might say a recipe need 6 galic cloves but then in the text of the recipe you will write "peel the garlic."
Since you did not write exactly "garlic clove", we would not highlight the ingredient and thats not a good user experience.

Another problem was that we have a power user that creates recipes in portuguese so we had to associate the portuguese words for milk if we wanted nutrition facts and all the other features to work.

To implement aliases, we basically store a list of names for an ingredient along with a single "canonical" name.
In the case of milk, this looks like:
`Milk; Leite; Whole Milk`
Where `Milk` is the canonical name, and `Leite` (milk in portugues) and `Whole Milk` are aliases for this ingredient.

The bulk of the work of this change was to hide from the user the fact that an ingredient's name is now a semi-colon separated list of possible aliases.