# Self-Hosted Mailserver

This project can run a production mailserver for application notifications with
Docker Mailserver and Certbot.

## Prerequisites

- A VPS with a static public IPv4 address.
- DNS control for your mail domain.
- Port 80 available temporarily for Certbot HTTP-01.
- Inbound ports 25, 465, 587, and 993 open on the VPS firewall.
- Reverse DNS/PTR for the VPS IP set to `mail.<your-domain>`.

## Configure

Copy the example environment file and replace every placeholder:

```bash
cp env/mailserver.env.example env/mailserver.env
```

Expected values:

- `MAIL_DOMAIN`: bare domain, for example `example.com`.
- `MAIL_HOSTNAME`: mail FQDN, for example `mail.example.com`.
- `LETSENCRYPT_EMAIL`: certificate renewal contact.
- `POSTMASTER_ADDRESS`: mailbox such as `postmaster@example.com`.
- `SSL_TYPE`: TLS mode for Docker Mailserver. Use `letsencrypt` for a real
  domain and `self-signed` for local smoke tests.

## Local Smoke-Test Domain

For local container startup tests, use the reserved example domain from
`env/mailserver.test.env.example`:

```text
MAIL_DOMAIN=steamapp.test
MAIL_HOSTNAME=mail.steamapp.test
POSTMASTER_ADDRESS=postmaster@steamapp.test
SSL_TYPE=self-signed
```

If you want to connect to this name from the host machine, map it to localhost:

```text
127.0.0.1 mail.steamapp.test
```

On Windows this goes in `C:\Windows\System32\drivers\etc\hosts` from an
administrator editor. This domain is only for local smoke tests. It cannot get
a Let's Encrypt certificate and will not deliver public internet mail.

The checked-in Development backend settings use `localhost` for `Email:Host`,
so Visual Studio can reach the published SMTP port without a hosts-file entry.
Use `mail.steamapp.test` only when you have added the hosts-file mapping or
when another Docker service resolves the mailserver through the Compose network
alias.

## DNS

Create or verify these records:

```text
mail.<domain>.  A    <VPS public IPv4>
<domain>.       MX   10 mail.<domain>.
<domain>.       TXT  "v=spf1 mx -all"
_dmarc.<domain> TXT  "v=DMARC1; p=none; rua=mailto:postmaster@<domain>"
```

After DKIM keys are generated, publish the generated DKIM TXT record from:

```text
docker-data/dms/config/opendkim/keys/<domain>/mail.txt
```

## Certificates

Run Certbot once with port 80 free:

```bash
docker compose --env-file env/mailserver.env -f docker-compose.mail.yml --profile certbot run --rm certbot
```

For a staging certificate test first, set this in `env/mailserver.env`:

```text
CERTBOT_EXTRA_ARGS=--staging
```

Remove `CERTBOT_EXTRA_ARGS` before issuing the production certificate.

## Start Mailserver

Create the app sender and postmaster accounts before relying on SMTP. These
commands write account data into the mounted Docker Mailserver config volume:

```bash
docker compose --env-file env/mailserver.env -f docker-compose.mail.yml run --rm mailserver setup email add postmaster@<domain> '<strong-password>'
docker compose --env-file env/mailserver.env -f docker-compose.mail.yml run --rm mailserver setup email add notifications@<domain> '<strong-password>'
docker compose --env-file env/mailserver.env -f docker-compose.mail.yml run --rm mailserver setup config dkim
```

For the local smoke-test domain, the concrete commands are:

```bash
docker compose --env-file env/mailserver.test.env.example -f docker-compose.mail.yml run --rm mailserver setup email add postmaster@steamapp.test 'Password1!'
docker compose --env-file env/mailserver.test.env.example -f docker-compose.mail.yml run --rm mailserver setup email add notifications@steamapp.test 'Password1!'
docker compose --env-file env/mailserver.test.env.example -f docker-compose.mail.yml run --rm mailserver setup config dkim
```

Then start or restart the mailserver:

```bash
docker compose --env-file env/mailserver.env -f docker-compose.mail.yml up -d mailserver
```

For the local smoke-test domain:

```bash
docker compose --env-file env/mailserver.test.env.example -f docker-compose.mail.yml up -d mailserver
```

If the container is already running and still inside Docker Mailserver's
first-start grace period, `exec` also works:

```bash
docker compose --env-file env/mailserver.env -f docker-compose.mail.yml exec mailserver setup email add notifications@<domain> '<strong-password>'
```

The warning `You need at least one mail account to start Dovecot` means the
mailserver has no accounts yet and will shut down if one is not created.

## Backend Configuration

Set these values through environment variables or user secrets:

```text
Email__Host=mail.<domain>
Email__Port=587
Email__UserName=notifications@<domain>
Email__Password=<strong-password>
Email__FromAddress=notifications@<domain>
Email__FromName=SteamApp
Email__UseStartTls=true
```

For the local smoke-test domain:

```text
Email__Host=localhost
Email__Port=587
Email__UserName=notifications@steamapp.test
Email__Password=Password1!
Email__FromAddress=notifications@steamapp.test
Email__FromName=SteamApp
Email__UseStartTls=true
Email__AllowInvalidCertificate=true
```

Use `Email__Host=mail.steamapp.test` instead of `localhost` only if the machine
running the backend can resolve that name. In Docker, the `mailserver` service
also has a network alias for `mail.steamapp.test` when the services share the
same Compose project network.

Because the local smoke-test env uses a self-signed certificate,
`Email__AllowInvalidCertificate=true` is allowed in Development only. For
end-to-end app email tests without certificate exceptions, use a real domain
with a valid Let's Encrypt certificate.

## Smoke Checks

```bash
docker compose --env-file env/mailserver.env -f docker-compose.mail.yml config
openssl s_client -starttls smtp -connect mail.<domain>:587
```

For the local smoke-test domain:

```bash
docker compose --env-file env/mailserver.test.env.example -f docker-compose.mail.yml config
openssl s_client -starttls smtp -connect localhost:587 -servername mail.steamapp.test
```

If `mail.steamapp.test` reports `No such host is known`, DNS is the failing
part. Either add the hosts-file mapping above or connect to `localhost` from the
host machine.

Then send a test notification from the backend and check SPF, DKIM, and DMARC
alignment with your DNS/mail testing tool of choice.
