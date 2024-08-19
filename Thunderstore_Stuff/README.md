## Installation Instructions
1. Install MelonLoader
2. Run Game
3. Drop Mods Folder in to Rumble Source Directory
4. Run the Game and enjoy the mod

## Overview
The Mod tracks which poses are being done by the player and which structures are being affected by poses

## Logging
- Every Pose done by the mod user will be logged 
	- v2.0+: 
	- Optional Setting for logging every Player in your lobby
	- Only Poses that actually affect something are being logged
- After variable amount of time (base: 5s) of inactivity a spacer will be created in the log File 
	- This will also reset the board/onScreen Logs
- The log files are located in UserData/PoseLogger/Logs/ 

## Board Logging
- There are 2 Boards in the gym and 1 in the park
	- At Howard's Place
	- At the ring at the top of the Gym
	- At the central ring in the park
- The Board itself has no collision to prevent getting stuck on it in the Park

## Controls
- The Logging can be turned via ModUI
- All other Settings are in ModUI

## Known "Issues"
- The Logging does only work in the park and in the gym
	- This will not change.
- The Logging to File always logs the previous move. 
	- This is intended because of the way that combo tracking works

## Known Issues
- Sprint Poses are not tracked.

## Help And Other Resources
Get help and find other resources in the Modding Discord:
https://discord.gg/fsbcnZgzfa