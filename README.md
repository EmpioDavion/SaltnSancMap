Map for visually organising and tracking checks for the Salt and Sanctuary Archipelago, both inside and outside the game.
All controls and elements other than left clicking the map will be disabled when running a Release build.

# Controls

## Map

### Left Mouse Click
Select the clicked check and go to and select region that contains it.
Hold the left mouse button down and move the mouse to pan the map.

### Middle Mouse Click
Toggle check as collected/not collected.

### Right Mouse Click
Move the clicked check into the current region selected in the region list.
Shows context menu if there are multiple checks stacked that are not in the current region.

## Regions List
Contains the list of regions.
Note that the highlighted region is not necessarily the selected region.

### Left Mouse Click
Select the clicked region. Checks will be filtered, showing checks in the region as green.
Hold the left mouse button down and move the mouse to reorder the current selected region in the regions list.
Checks outside the region will be marked red.

### Right Mouse Click
Create a new connection from the current region to the clicked region.
Double left click to edit the name of a region.
Region names must be unique.

### Delete Key
Removes the current selected region from the regions list.
Note that currently, other regions that connect to the deleted region will not have those connections deleted.

## Connections List
Contains the list of connections for the current selected region.
Note that the highlighted connection is not necessarily the selected connection.

### Left Mouse Click
Select the clicked connection, checks will be filtered and shown as blue to indicate checks in the connected region.
Double left click a connection to show a list for changing which region it connects to.

### Delete Key
Removes the current selected connection from the connections list and current selected region.

## Checks List
Contains the list of checks for the current selected region.
Note that the highlighted check is not necessarily the selected check.

### Left Mouse Click
Select the clicked check, the current selected check will be shown with a dark purple border.
The map will snap to the selected check when it is selected.
The type of the current selected check will be shown below the checks list.

## Items List
Contains the list of items required to traverse the current selected connection.
Note that the highlighted connection is not necessarily the selected item.

### Left Mouse Click
Select the clicked item.
Double left click to edit the clicked item name.

### Delete Key
Deletes the current selected item from the items list and the current selected connection.

## Progress List
Contains the list of received items.
Note that the highlighted progress item is not necessarily the selected progress item

### Left Mouse Button
Left click a progress item to select it.
Used to test check availability based on received items.
Double left click to edit the clicked progress item name.

### Delete Key
Deletes the current selected progress item from the progress list.

## Clear Filter
Clears the filter for the current selected region, connection, and check.
Used to show check availability on the map.

## Add Region
Adds a new region to the region list, name defaults to "new_region[#]".

## Add Connection
Adds a new connection to the current selected region, defaults to first region "Menu".

## Add Item
Adds a new item to the current selected connection, defaults to "new_item".

## Add Progress
Adds a new progress item for testing check availability, defaults to "new_progress".

## Check Name
Used to edit the display name of the current selected check.
The display name is only shown when hovered when running on a Release build.
The display name is also the name shown for the check in AP trackers.

## Check Description
Used to edit the description/hint text of the current selected check.

## Save
Save the current changes to the SaltMapEdit/Shared folder.

## Undo
Undo the last data-editing action.
Only actions that affect save data are tracked.

## Redo
Redo the last undone action.
Only actions that affect save data are tracked.
