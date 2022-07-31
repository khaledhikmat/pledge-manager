docker image build -t khaledhikmat/pledgemanager-be-campaigns:latest . -f Dockerfile-backend-campaigns
docker image build -t khaledhikmat/pledgemanager-fe-functions:latest . -f Dockerfile-frontend-functions
docker image build -t khaledhikmat/pledgemanager-fe-client:latest . -f Dockerfile-frontend-client