# Contributing

Thanks for your interest in contributing.

## Branching and PRs

- Create a feature branch from your main development branch.
- Keep commits small and focused.
- Open a PR with:
  - What changed
  - Why it changed
  - How it was tested
  - Screenshots for UI changes

## Local checks

### Frontend

```bash
cd SteamApp.Client
npm install
npm run build -- --configuration development
npm test
```

### Backend

```bash
cd SteamApp.Server/SteamApp.WebAPI
dotnet restore
dotnet build
dotnet test
```

## Coding guidelines

- Prefer readable, explicit naming over clever shorthand.
- Keep component/service responsibilities focused.
- For API changes:
  - update DTOs/contracts
  - update client services
  - update docs (`docs/API_REFERENCE.md`)
- For UI changes:
  - keep behavior consistent with existing UX patterns
  - include screenshot evidence in PR

## Commit message examples

- `feat(products): add export action to products table`
- `fix(game-url-form): preserve selected relations when editing`
- `docs: expand getting-started and architecture guides`

## Reporting issues

Please include:

- exact steps to reproduce
- expected vs actual behavior
- logs/error messages
- runtime versions (`node -v`, `dotnet --version`)
