import json

with open('game.json', 'r') as file:
    # Step 2: Load the JSON data
    data = json.load(file)

# Example usage to create a CYOA story
current_state = "gpt1-begin-bessy-game"
# Initialize an empty list to store chosen game entries
chosen_entries = []

while current_state:
    # Get the current state's data
    current_state_data = next((item for item in data if item['name'] == current_state), None)

    if current_state_data:
        # Append the current state data to the chosen_entries list
        chosen_entries.append(current_state_data)

        print(current_state_data['description'][0])  # Print the current state's description

        if current_state_data['options']:
            print("Options:")
            for idx, option in enumerate(current_state_data['options']):
                print(f"{idx + 1}. {option['description']}")

            # Prompt the user for their choice number
            try:
                user_choice_num = int(input("Enter the number of your choice: "))
                if 1 <= user_choice_num <= len(current_state_data['options']):
                    user_choice = current_state_data['options'][user_choice_num - 1]['description']
                else:
                    print("Invalid choice number. Please choose a valid option.")
                    continue
            except ValueError:
                print("Invalid input. Please enter a number.")
                continue

            # Find the next state based on user's choice
            next_state = None
            for option in current_state_data['options']:
                if option['description'] == user_choice:
                    next_state = option['next']
                    break

            if next_state:
                current_state = next_state  # Update the current state
            else:
                print("Invalid choice. Please choose a valid option.")
        else:
            print("End of the story.")
            break
    else:
        print("Current state not found.")
        break

# Print the JSON array of chosen game entries at the end
import json
print(json.dumps(chosen_entries, indent=2))
