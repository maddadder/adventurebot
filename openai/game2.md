Pretend you are an expert game content generator. Each game entry is named by the 'name' property, and the next options are used to feed the next query to advance to story. If the user selects an option then the query will lookup the next entry with the name matching the 'next' property value, and this new game entry has its own set of options to choose from recursively until there are no more options to choose from. When there are no more options then that branch of the story ends with game over. Prefix each name's value with gpt2- and each next's value with gpt2-. Only provide game entries for the following game entry options in this game entry:
{
    "name": "gpt2-birmingshaw-mountain-pass",
    "description": [
      "After school, you meet with some of your classmates behind the school. One of the older boys dares you to go to the troll caves at the base of the Birmingshaw Mountain Pass and bring back an item from the cave to prove you were there. You reluctantly accept the challenge as you cannot bear the thought of being seen as a chicken, yellow, or a coward for eternity.",
      "You go home to grab your backpack and a lantern.",
      "You head south out of town as fast as you can so you're not seen by any family; your sister would for sure rat you out! Although that could be a good reason you didn't go... what if your sister did rat you out, and you did get grounded? Would you still be a chicken, or is it worse to get caught? You decide to keep moving.",
      "As you make your way to the edge of the forest, you begin to see off in the distance a low ridge rising before you, beyond which you see the Birmingshaw Mountain Pass scraping the sky. The closer in you get, there is something strewn across the rocky ground ahead. Moving closer, you realize that they are skeletons of the trolls that once inhabited the cave. You follow the stone wall mountains to the south, where you find hidden amongst the brush a tunnel that burrows into the foot of the soaring snow-capped mountain."
    ],
    "options": [
        {
            "description": "Go home and be a chicken",
            "next": "gpt2-chicken"
        },
        {
            "description": "Just go into the first room of the cave; maybe there will be an item you can take back",
            "next": "gpt2-first-room"
        },
        {
            "description": "Go back to town and see if the general goods merchant has an item to help you on your quest",
            "next": "gpt2-general-goods-merchant"
        }
    ],
    "id": "34426b01-8970-46b3-b708-8c09c68de981",
    "__T": "ge",
    "Created": "2022-06-01T09:30:48.9876543Z",
    "Modified": "2022-06-01T09:30:48.9876543Z",
    "createdBy": null,
    "modifiedBy": "charlie@leenet.link"
  }

Do not include any explanations in your output, only provide a RFC8259 compliant JSON response following this format without deviation. 
[
    {
        "name": "gpt2-game-entry",
        "description": [
            "The choose your adventure content goes here"
        ],
        "options": [
            {
                "description": "this is choice one",
                "next": "gpt2-this-is-choice-one"
            },
            {
                "description": "this is choice two",
                "next": "gpt2-this-is-choice-two"
            },
            {
                "description": "this is choice three",
                "next": "gpt2-this-is-choice-three"
            }
        ],
        "id": "bef35ec2-abcb-4961-9f25-20cc0769ee75",
        "__T": "ge",
        "Created": "2022-05-30T01:11:44.5456615Z",
        "Modified": "2023-03-12T21:38:57.1205151Z",
        "createdBy": null,
        "modifiedBy": "charlie@leenet.link"
    }
]