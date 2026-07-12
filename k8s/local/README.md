# SteamApp Local Kubernetes Smoke Test

This directory targets the local `docker-desktop` Kubernetes context.

Build the local images:

```powershell
docker build -f .\SteamApp.Server\SteamApp.WebAPI\Dockerfile -t steamapp-webapi:local .\SteamApp.Server
docker build -f .\SteamApp.Client\Dockerfile -t steamapp-client:local .\SteamApp.Client
```

Apply the stack:

```powershell
kubectl apply -f .\k8s\local\
```

Wait for rollouts:

```powershell
kubectl -n steamapp-local rollout status statefulset/steamapp-sql
kubectl -n steamapp-local wait --for=condition=complete job/steamapp-sql-bootstrap --timeout=180s
kubectl -n steamapp-local rollout status deployment/steamapp-redis
kubectl -n steamapp-local rollout status deployment/steamapp-rabbitmq
kubectl -n steamapp-local rollout status deployment/steamapp-webapi
kubectl -n steamapp-local rollout status deployment/steamapp-client
```

Run an in-cluster API smoke check:

```powershell
kubectl -n steamapp-local run smoke-curl --rm -i --restart=Never --image=curlimages/curl -- curl -fsS http://steamapp-webapi:8080/health/ready
```

Open the client:

```powershell
kubectl -n steamapp-local port-forward svc/steamapp-client 8088:80
```

Then browse to `http://localhost:8088/`.

Clean up the local stack:

```powershell
kubectl delete namespace steamapp-local
```
