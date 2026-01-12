# Shift Stone ~~Randomizer~~ Manager
Adds more buttons to the shift stone quick swapper to allow to save loadouts and settings for shift stones. Also allows for automatic management and selection of stones depending on client/host status and maps.


## Shiftstone Case:
The buttons on the shift stone case are pretty self explanatory. 

-  Blacklist: Blacklists stones from the randomizer. Blacklisted stones can still be manually equipped.
-  Hand Lock: Choose to only randomize one hand. 


## Shiftstone Quick Swapper

The four buttons on top by default are loadout apply buttons. Clicking them applies the loadout stored in them to your hands

- Front button: The tablets are now replaced with floating shift stones. The loadout buttons are still there but they are now for saving shift stones and will store the stones or commands you have equipped into the loadout
    - Commands: Four additional objects are on the bottom. When you click them, they will be displayed on your shift stone socket allowing you to save them to the loadout and executed when you apply a loadout that contains them.
        - Mirror: Copies the opponent stone on the same hand
        - None: Does not make any changes when applied
        - Random: Picks a random stone that isn't blacklisted.
        - Empty: Clears equipped shift stone for that hand when applied. 
- Right Button: Dedicated randomize button
- Left Button: Rotates between having left, right, or both hands being randomized by the dedicated random button
- Rear Button: Unequips shift stones from both hands


## Automation

When automation is enabled, the four loadout buttons now represent four combinations of host status and maps.  They are automatically applied between matches depending on the host/client status of the next match. Clicking on any of the loadout apply buttons will disable automation until after the next match. Automation can be toggled to a permanent status with the automation enable button 

~~TODO: Create automation enable button~~

## Config
### Preferences
| Option | Default | Description |
|---|---|---|
| Enable Debug Mode | false | Enable for more verbose logging|
| Randomized Hand | Both | Hand where randomization is enabled |
| Auto Equip Mode | Random | Random: Randomize every match, Auto: Based on map automation config None: No action |

### Enabled Stones
| Option | Default | Description |
|---|---|---|
| Shift Stone | true | Include in the randomization choices |


