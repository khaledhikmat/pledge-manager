docker buildx build --platform linux/amd64 -t khaledhikmat/pledgemanager-be-campaigns:1.5 . -f Dockerfile-backend-campaigns
docker buildx build --platform linux/amd64 -t khaledhikmat/pledgemanager-be-users:1.5 . -f Dockerfile-backend-users
docker buildx build --platform linux/amd64 -t khaledhikmat/pledgemanager-fe-functions:1.5 . -f Dockerfile-frontend-functions
docker buildx build --platform linux/amd64 -t khaledhikmat/pledgemanager-fe-client:1.5 . -f Dockerfile-frontend-client
