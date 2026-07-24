#!/bin/bash

echo "Bat dau qua trinh Zero Downtime Deployment (No-Config-Change Edition)..."

# Lay ID cua container API cu
OLD_CONTAINER=$(docker compose ps -q api | head -n 1)

if [ -z "$OLD_CONTAINER" ]; then
    echo "Khong tim thay container cu. Dang khoi dong moi hoan toan..."
    docker compose up -d --build
    exit 0
fi

echo "Dang build image moi cho API..."
docker compose build api

# Tinh toan so luong container can scale
CURRENT_COUNT=$(docker compose ps -q api | wc -l)
NEW_COUNT=$((CURRENT_COUNT + 1))

echo "Dang khoi chay phien ban moi song song voi ban cu (Scale to $NEW_COUNT)..."
docker compose up -d --scale api=$NEW_COUNT --no-recreate api

# Tim ID cua container moi vua duoc tao
NEW_CONTAINER=$(docker compose ps -q api | grep -v "^$OLD_CONTAINER$")

if [ -z "$NEW_CONTAINER" ]; then
    echo "Loi: Khong the tao container moi."
    exit 1
fi

# Lay IP noi bo cua container moi
NEW_IP=$(docker inspect -f '{{range.NetworkSettings.Networks}}{{.IPAddress}}{{end}}' $NEW_CONTAINER)

echo "Cho API moi san sang tai IP $NEW_IP..."
NGINX_CONTAINER=$(docker compose ps -q nginx)

MAX_RETRIES=30
RETRY_COUNT=0

# Dung wget vi nginx:alpine co san wget
while [ $RETRY_COUNT -lt $MAX_RETRIES ]; do
    STATUS=$(docker exec $NGINX_CONTAINER wget -q -S -O /dev/null http://${NEW_IP}:8080/swagger/index.html 2>&1 | grep "HTTP/" | awk '{print $2}')
    
    if [ "$STATUS" == "200" ]; then
       echo "API moi da san sang!"
       break
    fi
    echo "Dang cho... (Status: $STATUS)"
    sleep 2
    RETRY_COUNT=$((RETRY_COUNT+1))
done

if [ $RETRY_COUNT -eq $MAX_RETRIES ]; then
    echo "Timeout. Huy deploy."
    docker stop $NEW_CONTAINER
    docker rm $NEW_CONTAINER
    exit 1
fi

echo "Dang reload Nginx de nhan dien ca 2 phien ban (cu va moi)..."
# Khi Nginx reload, no se resolve lai DNS cua service 'api' va thay ca 2 IP
docker exec $NGINX_CONTAINER nginx -s reload

# Cho mot chut de Nginx phan bo traffic
sleep 2

echo "Dang tat phien ban cu..."
# Khi tat ban cu, proxy_next_upstream cua Nginx se tu dong day request fail sang ban moi
docker stop $OLD_CONTAINER

echo "Reload Nginx lan cuoi de go bo IP cu..."
docker exec $NGINX_CONTAINER nginx -s reload

echo "Xoa phien ban cu..."
docker rm $OLD_CONTAINER

echo "Zero Downtime Deployment Hoan Tat!"
