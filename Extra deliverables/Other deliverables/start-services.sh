echo "Starting ElasticSearch service..."
systemctl start elasticsearch.service
echo "Starting Kibana service... (takes a few minutes to be ready)"
systemctl start kibana.service
