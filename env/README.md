# Docker Compose Environment Files

Docker Compose expects local environment files in this folder:

- `env/server.env`
- `env/sqlserver.env`
- `env/full-stack.env` for `docker-compose.full.yml`

Use the `*.example` files as templates. The real `*.env` files are ignored by git so local secrets and machine-specific connection strings stay out of source control.

The full-stack file is a separate production-like staging workflow. Copy
`full-stack.env.example` to `full-stack.env`, replace every required blank value,
and keep the populated file only on the deployment host.
