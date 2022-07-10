docker image build -t pledgemanager-be-campaigns:1.5 . -f Dockerfile-backend-campaigns
docker image build -t pledgemanager-be-users:1.5 . -f Dockerfile-backend-users
docker image build -t pledgemanager-fe-functions:1.5 . -f Dockerfile-frontend-functions
docker image build -t pledgemanager-fe-client:1.5 . -f Dockerfile-frontend-client