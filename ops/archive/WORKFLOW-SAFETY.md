# DASH2A Workflow Safety

This document records what GitHub Actions can run for DASH2A and what happens on merge.

## Active Root Workflows

GitHub only treats workflows under root `.github/workflows/` as active.

### `.github/workflows/firebase-hosting-pull-request.yml`

Trigger:
- `pull_request`

Behavior:
- Runs `npm ci && npm run build` in `frontend`.
- Sets `VITE_API_BASE_URL=http://51.83.159.175`.
- Does not deploy Firebase.

Current PR check:
- `build` passed.

Risk:
- This validates frontend only. It does not validate backend build, EF migrations, or server readiness.

### `.github/workflows/firebase-hosting-merge.yml`

Trigger:
- `push` to `main`

Behavior:
- Runs `npm ci && npm run build` in `frontend`.
- Sets `VITE_API_BASE_URL=http://51.83.159.175`.
- Deploys Firebase Hosting live with:
  - `firebaseServiceAccount: ${{ secrets.FIREBASE_SERVICE_ACCOUNT_EUGENIO_DASHBOARD_2 }}`
  - `channelId: live`
  - `projectId: eugenio-dashboard-2`
  - `entryPoint: ./frontend`

Risk:
- Merge to `main` can publish frontend live automatically if the Firebase secret exists.
- Frontend may point users to backend `http://51.83.159.175` before backend readiness is complete.

## Inactive Nested Workflows

These paths are not active GitHub workflows unless moved to root `.github/workflows/`:
- `frontend/.github/workflows/firebase-hosting-merge.yml`
- `frontend/.github/workflows/firebase-hosting-pull-request.yml`
- `backend/.github/workflows/dotnet.yml`
- `backend/.github/workflows/main_eugenioapi.yml`

Important:
- `backend/.github/workflows/main_eugenioapi.yml` describes an Azure Web App deploy on push to `main`, but because it is under `backend/.github/workflows/`, it is documentation/inactive from GitHub Actions' root workflow perspective.
- If moved to root, it would become a backend deploy workflow and must be reviewed before activation.

## Firebase Safety Checklist

Before merge:
- [ ] Confirm `.github/workflows/firebase-hosting-merge.yml` is intentionally allowed to deploy live.
- [ ] Confirm `FIREBASE_SERVICE_ACCOUNT_EUGENIO_DASHBOARD_2` exists only if live deploy is approved.
- [ ] Confirm `projectId` is `eugenio-dashboard-2`.
- [ ] Confirm `frontend/.firebaserc` default is `eugenio-dashboard-2`.
- [ ] Confirm no `eugenio-dashboard-1` or Dashboard 1 Firebase references are operational.
- [ ] Confirm `VITE_API_BASE_URL` is the intended production API.
- [ ] Confirm backend API is ready for the frontend version being published.

## Backend Safety Checklist

Before backend deployment:
- [ ] No active backend deploy workflow exists without explicit approval.
- [ ] Backend build has been run and passed.
- [ ] EF migration script is reviewed.
- [ ] Server folder/process/App Pool/service is known.
- [ ] App backup and DB backup are verified.
- [ ] Rollback is ready.
- [ ] Server env/secrets are configured outside repo.

## Merge Gate

Do not merge while any of these are true:
- Firebase live deploy is not approved.
- Backend API at `http://51.83.159.175` is not ready.
- Backend migration validation is incomplete.
- Versioned secrets remain in tracked config.
- Backup/rollback readiness is incomplete.

Safe merge options:
1. Disable or gate Firebase live workflow before merge, then merge code only.
2. Keep workflow active only after explicit approval that frontend live deploy may happen on merge.
3. Merge only after backend readiness is complete and the frontend can safely point to the configured API.
