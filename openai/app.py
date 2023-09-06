import os
import openai
import json
from dotenv import load_dotenv
import random
import uuid

load_dotenv('.env')

# Set up your Cosmos DB client
openai.organization = os.environ.get('OPENAI_ORG')
openai.api_key = os.environ.get('OPENAI_API_KEY')

with open('example.json', 'r') as file:
    # Step 2: Load the JSON data
    example = json.dumps(json.load(file), indent=4)

with open('previous.json', 'r') as file:
    # Step 2: Load the JSON data
    previous = json.dumps(json.load(file), indent=4)

#print("{},".format(json_document))

response = openai.ChatCompletion.create(
    model="gpt-3.5-turbo",
    messages=[
        {"role": "system", "content": "Pretend you are an expert game content generator"},
        {"role": "user", "content": "Each game entry is named by the 'name' property, and the next options are used to feed the next query to advance to story. If the user selects an option then the query will lookup the next entry with the name matching the 'next' property value, and this new game entry has its own set of options to choose from recursively until there are no more options to choose from. When there are no more options then that branch of the story ends with game over. Prefix each name's value with gpt1- and each next's value with gpt1-. Only provide game entries for each option provided. The previous game entry is the following \n " + "{}".format(previous) + "\nDo not include any explanations in your output, only provide a RFC8259 compliant JSON response following this format without deviation. \n" + "{}".format(example)},
    ]
)

print("{}".format(response.choices[0].message.content))
