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
3) Put any domain name and click "I agree" checkbox    
3) Get the Key string and paste it in the Steam Web API Key textbox    

Press the Save Information and everytime you run the program, it will remember your steamID and steam web api key.

If you want to use a different steamID or steam Web API Key, delete the filename settings.txt

------------------------

![In Action](http://puu.sh/trodS/c5193accce.gif)
![Root with level 1 children](http://puu.sh/tnZrr/880d2ae96b.jpg)

Space in the game names are added with a Newline in the button. If you want to group 2 or more words in a line, replace the space with a special space character by doing ALT+255 in numpad

Current limitation:
- SteamID profiles must be public
- Game Title must not have a comma on it
- Only detects owned games in steam (no non-steam executables atm)
- Don't have implementation of command arguments

Planned features:
- Add multiple steamID that you are family sharing
- Ability to hide games you own by Right click
- Game description panel
- Add non-steam games
- Add page 2 or more when games+nodes exceed 32 being out of bounds of program
- Change tooltip font to be bigger