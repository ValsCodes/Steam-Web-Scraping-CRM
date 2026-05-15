# Docker Compose Environment Files

Docker Compose expects local environment files in this folder:

- `env/server.env`
- `env/sqlserver.env`

Use the `*.example` files as templates. The real `*.env` files are ignored by git so local secrets and machine-specific connection strings stay out of source control.
