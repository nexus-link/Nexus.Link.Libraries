from notion.client import NotionClient
from notion.block import PageBlock
from notion.block import CalloutBlock
from xml.dom import minidom
import os
import re
import urllib.request
import time

tic = time.perf_counter()

BUG_ICON = "🐞"
DEFAULT_ICON = "🔸"
NOTES_ICON = "📒"

# Obtain the `token_v2` value by inspecting your browser cookies on a logged-in session on Notion.so
client = NotionClient(token_v2="7932fc7c07fce2028b38e34d47f69b099947a8256f3558ca8cf5734af8d545134a58ad8926969ed6075db4f639853082c88775e721bc1e4696b6e8169928909c9b41dd9327abf489d527624fc4c1")

# Main release notes page for Libraries
page = client.get_block("https://www.notion.so/xlentlink/Release-notes-Nexus-Link-Libraries-7779ee0765a649e78ad94878c7f859a0")
print("We're at Notion page", page.title)

# Remove all sub pages and build it all up again
for child in page.children:
    if child.type == "page":
        print("Removing", child)
        child.remove(permanently=True)

# Search for csproj files
for r, d, f in os.walk("./src"):
    for file in f:
        if file.endswith(".csproj"):
            filepath = f"{r + '/' + file}"
            print("Handling", filepath)
            
            filedoc = minidom.parse(filepath)
            packageId = filedoc.getElementsByTagName("PackageId")
            if len(packageId) != 1: continue
            
            # The package identifier
            name = packageId[0].firstChild.data
            
            # Create in Notion
            rnPage = page.children.add_new(PageBlock, title = name)
            rnPage.icon = NOTES_ICON

            # The release notes text
            releaseNotes = filedoc.getElementsByTagName("PackageReleaseNotes")
            if len(releaseNotes) != 1: continue

            # Split into individual notes per version
            matches = re.split("(\d+\.\d+\.\d+)", releaseNotes[0].firstChild.data)
            for i in range(len(matches)):
                try:
                    match = matches[i]
                    version = ""
                    description = ""
                    # The regex matches will gives an array like [ "1.0.0", "description...", "1.0.1", "description...", -...]
                    if match[0].isdigit() and i + 1 < len(matches):
                        version = match.strip()
                        description = matches[i+1].strip()

                        # Fetch release date from nuget server
                        try:
                            url = "http://fulcrum-nuget.azurewebsites.net/nuget/Packages(Id='" + name + "',Version='" + version + "')"
                            date = ""
                            with urllib.request.urlopen(url) as response:
                                r = response.read()
                                xml = minidom.parseString(r)
                                date = xml.getElementsByTagName("updated")[0].firstChild.data[:10]
                        except:
                            # It's ok, we skip the date for this version
                            print("Could not find DATE for", url, ". It's ok")

                    if len(version) == 0: continue

                    # Put it togheter
                    text = "**" + version + "**"
                    if len(date) != 0: text += " (" + date + ")"
                    text += "\n" + description
                    
                    # Create in Notion
                    item = rnPage.children.add_new(CalloutBlock, title = text)
                    icon = DEFAULT_ICON
                    if "fix" in description.lower() or "bug" in description.lower(): icon = BUG_ICON
                    item.icon = icon
                            
                except Exception as e:
                    print("ERROR", e)
            
            

print("\nDONE")
toc = time.perf_counter()
print(f"Finished in {toc - tic:0.4f} seconds")
