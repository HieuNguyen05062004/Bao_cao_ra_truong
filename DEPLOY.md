# Deploy QUAN_LY_THU_VIEN

## Docker Compose

1. Copy `.env.example` to `.env` and update secrets.
2. Run:

```powershell
docker compose up -d --build
```

Client web UI opens at `http://localhost:8080/` and points directly to `Fontend/Client/index.html`.
Admin web UI opens at `http://localhost:8081/` and points directly to `Fontend/Admin/login.html`.

The apps run database migrations on startup. Uploads are stored in the shared `uploads-data` Docker volume.

## Production Notes

Use a reverse proxy or hosting platform to point your public domain to the Client service. Keep Admin on a separate private subdomain or protected port.

Example:

- `https://thu-vien.example.com` -> `client:8080`
- `https://admin.thu-vien.example.com` -> `admin:8080`

Do not commit real database passwords or API keys. Put them in `.env` or your hosting provider's secret manager.
