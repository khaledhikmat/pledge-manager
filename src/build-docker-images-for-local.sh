docker image build -t pledgemanager-be-campaigns:latest . -f Dockerfile-backend-campaigns
docker image build -t pledgemanager-fe-functions:latest . -f Dockerfile-frontend-functions
docker image build -t pledgemanager-fe-client:latest . -f Dockerfile-frontend-client