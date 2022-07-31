docker buildx build --platform linux/amd64 -t khaledhikmat/pledgemanager-be-campaigns:latest . -f Dockerfile-backend-campaigns
docker buildx build --platform linux/amd64 -t khaledhikmat/pledgemanager-fe-functions:latest . -f Dockerfile-frontend-functions
docker buildx build --platform linux/amd64 -t khaledhikmat/pledgemanager-fe-client:latest . -f Dockerfile-frontend-client
