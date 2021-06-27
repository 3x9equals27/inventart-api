# inventart-api
publish and copy build to /var/www/inventart/ on Digital Ocean using filezilla
login to digital ocean droplet
cd /var/www/inventart/
nohup dotnet inventart-api.dll &

if instance already running:
ps -aux | grep dotnet
kill running instance
