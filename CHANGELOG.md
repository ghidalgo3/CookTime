# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 2026-02-14

### Fixed

- Fix crash when switching ingredient units caused by reading the event target inside a deferred React state updater
- Fix UI hang on ingredient views by deferring accordion body rendering until expanded via `mountOnEnter`

### Changed

- Add `ESLint` to CI/CD for frontend code
- More prompt tuning

## 2026-02-13

### Added

- Metric to imperial conversion for recipe ingredient units

### Changed

- Tuned prompt to remove ingredient preparation from recipe ingredients

## 2026-02-07

### Fixed

- Fix resource path for embedded prompts

### Changed

- Normalize namespace named to `CookTime`.

## 2026-01-01

### Added

- Public documentation.
- User custom lists feature: users can now create, manage, and delete their own named recipe lists
- Tests!

### Changed

- Made the repository public.
- Made authentication cookie persist for longer, helps mobile Safari
- Reorganized the source code so it's all under `src`
