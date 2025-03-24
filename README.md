# IF2211_TB1_K03_G41_BJB

This project implements a bot for Robocode Tank Royale using various greedy algorithms to optimize its behavior in battle.

## Greedy Algorithms Implementation

### Main Bot (BJB)
The main bot implements a **Zone-Based Positioning Greedy Strategy**:
- Divides the arena into strategic zones and focuses on maintaining position in safe zones
- Uses heuristics to evaluate risks and moves to safer zones when threatened
- Implements emergency escape maneuvers when stuck or when hit by enemies
- Adjusts firing power based on distance to optimize damage

### Alternative Bot 1 (Kenan)
The alternative bot 1 implements a **Corner Movement and Distance-Based Firing Strategy**:
- Targets corner positions for strategic advantage
- Adjusts gun turn range based on position in the arena
- Uses distance-based firing power to optimize damage
- Switches corners when hit to confuse enemies

### Alternative Bot 2 (Nelson)
The alternative bot 2 implements a **Bullet Dodging and Adaptive Movement Strategy**
- Enters dodge mode when hit to make evasive maneuvers
- Maintains a safe distance from walls
- Performs random movements to be unpredictable
- Adjusts radar scanning to maximize enemy detection

### Alternative Bot 3 (Bryan)
The alternative bot 3 implements a **Center-Seeking and Safe Positioning Strategy**
- Prioritizes moving to center when near walls
- Makes randomized movements when in safe areas
- Implements smart firing based on distance
- Takes emergency measures when cornered

## Requirements
- .NET SDK 6.0 or higher
- Robocode Tank Royale 0.30.0 or higher

## Installation and Running
1. Clone the repository
```
git clone https://github.com/nathangalung/IF2211_TB1_K03_G41_BJB.git
```
2. Make sure you have the Robocode Tank Royale GUI jar file in the lib directory:
```
lib/robocode-tankroyale-gui-0.30.0.jar
```
3. To start the Robocode Tank Royale GUI
```
java -jar lib/robocode-tankroyale-gui-0.30.0.jar
```
4. To compile the bots:
- Open the solution in Visual Studio or Visual Studio Code
- Build the solution
- Alternatively, use the .NET CLI
```
dotnet build IF2211_TB1_BJB.sln
```
5. In the Robocode Tank Royale GUI:
- Connect to a running server or start a local server
- Add the bots from the main-bot and alternative-bots directories
- Start a new game with the selected bots

## Authors
- Jonathan Kenan Budianto (13522129)
- Sebastian Nelson 
- Bryan P. Hutagalung (18222130)