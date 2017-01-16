# VR Game Launcher

A Categorized VR Game Launcher. Useful when demoing your Vive to other people.

Instructions on how to use this:

When running the program for the first time, it will ask for your SteamID and Steam Web API Key

**To Look up your steamID:**

1) You must login your account in a browser
2) Click your profile picture on the top right corner
3) Copy the URL in the address bar (Example: https://steamcommunity.com/id/MDuh/)
4) Go here: https://steamid.eu/ and paste your profile url on the blue searchbox
5) Copy the bold string in "Community ID" (Example: "76561198046980920")
6) Paste the string in the program in the SteamID textbox

**To Look up your Steam Web API Key:**

1) You must login your account in a browser
2) Go here: https://steamcommunity.com/dev/apikey and generate/request a new API Key
3) Get the Key string and paste it in the Steam Web API Key textbox

Press the Save Information and everytime you run the program, it will remember your steamID and steam web api key.

If you want to use a different steamID or steam Web API Key, delete the filename settings.txt

------------------------

![Root with level 1 children](http://puu.sh/tnZrr/880d2ae96b.jpg)
![Nodes with associated games with it](http://puu.sh/tnZsl/229ef0decd.jpg)

Current limitation:
- Game Title must not have a comma on it
- Only detect owned games in steam
- Don't have implementation of command arguments for now