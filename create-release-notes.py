from notion.client import NotionClient
print("page title fetching, x: $(x)")

# Obtain the `token_v2` value by inspecting your browser cookies on a logged-in session on Notion.so
client = NotionClient(token_v2="$(token_v2)")
page = client.get_block("https://www.notion.so/xlentlink/OpenMW-7964d5ffb51744c39f84b6662f125b07")
print("The old title is:", page.title)
