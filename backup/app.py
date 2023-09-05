import os
import json
from azure.cosmos import CosmosClient

from dotenv import load_dotenv

load_dotenv('.env')

# Set up your Cosmos DB client
endpoint = os.environ.get('COSMOSDB_ENDPOINT')
key = os.environ.get('COSMOSDB_KEY')
client = CosmosClient(endpoint, key)

# Select the database and container
database_name = os.environ.get('DATABASE_NAME')
container_name = os.environ.get('CONTAINER_NAME')
database = client.get_database_client(database_name)
container = database.get_container_client(container_name)

# Define your query
query = 'SELECT * FROM c'  # Select all documents, adjust as needed

# Execute the query and retrieve documents
for item in container.query_items(query, enable_cross_partition_query=True):
    json_document = json.dumps(item, indent=4)
    print("{},".format(json_document))
