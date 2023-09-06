import json
import uuid

# Load the original JSON data from a file or any other source
# For this example, we'll assume you have your data in a file named "original_data.json"
with open("game.json", "r") as json_file:
    original_data = json.load(json_file)

# Function to generate new GUIDs for each entry
def generate_new_guids(data):
    new_data = []
    for entry in data:
        new_entry = entry.copy()  # Create a copy of the original entry
        new_entry["id"] = str(uuid.uuid4())  # Generate a new GUID for the ID
        new_data.append(new_entry)
    return new_data

# Generate new GUIDs for the original data
new_data = generate_new_guids(original_data)

# Print the updated data with new GUIDs
print(json.dumps(new_data, indent=2))

# Optionally, you can save the updated data to a new file
with open("updated_game.json", "w") as updated_json_file:
    json.dump(new_data, updated_json_file, indent=2)
