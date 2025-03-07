docker-compose -p rabbitmq-cluster -f rabbitmq-cluster.yaml down
docker volume rm rabbitmq-cluster_rabbitmq-cluster-node-1-data
docker volume rm rabbitmq-cluster_rabbitmq-cluster-node-2-data
docker volume rm rabbitmq-cluster_rabbitmq-cluster-node-3-data