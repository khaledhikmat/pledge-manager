docker image build -t khaledhikmat/pledgemanager-be-campaigns:1.5 . -f Dockerfile-backend-campaigns
docker image build -t khaledhikmat/pledgemanager-be-users:1.5 . -f Dockerfile-backend-users
docker image build -t khaledhikmat/pledgemanager-fe-functions:1.5 . -f Dockerfile-frontend-functions
docker image build -t khaledhikmat/pledgemanager-fe-client:1.5 . -f Dockerfile-frontend-client