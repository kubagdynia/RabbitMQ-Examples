docker exec -it rabbitmq-example rabbitmqctl version

docker exec -it rabbitmq-cluster-node-1 rabbitmqctl hash_password adminadmin # Hqm0iyKF8znYkmJIBNUnje5EUKvvTeK2H6v6viny3ujaZhEZ

docker exec -it rabbitmq-cluster-node-1 rabbitmqctl list_users
docker exec -it rabbitmq-cluster-node-1 rabbitmqctl list_vhosts
docker exec -it rabbitmq-cluster-node-1 rabbitmqctl environment
