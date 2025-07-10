# Bingo Configuration

Croupier will load all JSON files under the `config/bingo` directory as bingo configuration files. Every bingo configuration file has the same options available, so they are organised primarily by filename.

Each bingo configuration file can be used to define tiles, groups and/or areas. Croupier will always load all tile definitions last, no matter where or how they are defined, so these can safely be defined however and wherever you please.

## Philosophy: Tiles &amp; Triggers

### Tiles

The major concepts for Croupier's Bingo configuration are tiles and triggers. A square on a bingo grid (henceforth a 'tile') is linked to a small piece of configuration that sets the display properties of the tile and, most importantly, defines the *trigger* for that tile.

There are two tile types: objective and complication. These are linked to the Bingo mode and affect how the trigger behaves.

An 'objective' tile will get marked as completed the moment its trigger is achieved.

A 'complication' tile will get marked as completed at the end of the mission, so long as it has not been failed. It will be marked as failed the moment its trigger is achieved.

Essentially, these two types have opposite behaviours when their triggers are achieved.

### Triggers

A trigger is a piece of logic that defines what game event and parameters should trigger the tile's win state to change.

Internally, the mod will send event messages to the app on certain game actions, then the app will test all the triggers of the tiles on the current board with that event message.

Triggers can have a `Count` property, meaning the trigger will not change the state of the tile straight away, but rather on the nth time that the trigger is achieved.

A trigger only has access to parameters that are sent as part of the event message, which are mostly specific to each kind of event/trigger. In many cases, there are a common set of parameters that Croupier automatically adds to the usual game events. If you already have some knowledge of the game's events, you may recognise many of the available parameters, though some have been renamed for consistency and/or clarity.

## Area Configuration

In order to be able set up tile triggers that are based on the player or some other entity's location, we can define our own areas which will be passed to the mod in order for it to determine an entity's area based on its position. This is done with a simple box volume check where when a position is between two opposite corners of the box, it is considered as being within the area.

To define areas, the JSON file must have an object as the root node and define an 'Areas' array property.

```json
{
	"Areas": [ ... ]
}
```

Each area is defined as an object within this array.

```json
{
	"Areas": [
		{
			"ID": "Helipad",
			"Missions": ["The Showstopper", "Holiday Hoarders"],
			"From": [-279.825, -2.929, -4.935],
			"To": [-328.375, 32.669, 0.215]
		},
		{
			"ID": "TopFloor",
			"Missions": ["The Showstopper", "Holiday Hoarders"],
			"From": [-261.0, -24.0, 15.3],
			"To": [-124.0, 55.0, 20.0]
		}
	]
}
```

The applicable properties for each object in the array are as follows:

- (Required) `ID` - A name which will be used to uniquely identify and refer to this area in bingo tile configs.
- `Missions` - An array of mission name strings this area applies to. Omitting this or leaving it empty will cause the area to be used in all missions (which probably won't ever make sense and will result in extra unnecessary computation). Be sure each string matches the name of the mission as defined in the `config/missions/*.json` files.
- `From` - The position of the corner of the bounding box to check.
- `To` - Same as `From` but on the opposite corner.

## Group Configuration

Groups exist to help organise the tiles by what kind of objective or complication they are. These are defined per-tile with the `Group` property, however by default this has no visible effect.

You can further configure each unique group in a `Groups` configuration:

```json
{
	"Groups": {
		"Collect": {
			"Name": "Collect",
			"Color": "#77AACC",
			"Tip": "Collect {0} of this item on the map."
		},
		"Disguise": {
			"Name": "Disguise",
			"Color": "#66BB66",
			"Tip": "Change into this disguise."
		}
		// etc...
	}
}
```

The property must be assigned an *object* with each sub-property being the ID of the group (as given to the tile's `Group` property) and given a sub-object as a value.

The applicable properties for the sub-objects are as follows:

- (Required) `Name` - The display name for the group.
- `Color` - The associated color of this group. This should be provided as an RGB hex string (as seen in HTML).
- `Hidden` - A boolean indicating whether the group should actually be displayed on the tile.
- `Tip` - A default tooltip text giving the user a more detailed explanation of tiles in this group, if the tile does not have one of its own.

Using these configurations, you can add coloured headers to the tiles based on their group. In future, Croupier may add the ability for the user to toggle certain groups of tiles on/off or there may be bingo rulesets which need to refer to the groups.

## Tile Configuration

To define tiles, the JSON file can either have a plain array as its root or, similarly to the areas configuration, an object with a 'Tiles' array as a property...

```json
// This...
[
	{
		"Name": "Auction Staff",
		"Group": "Disguise",
		"Missions": ["The Showstopper", "Holiday Hoarders"],
		"Tags": ["Disguise", "Civilian"],
		"Disguise": ["b5664bed-462a-417c-bc07-6d9d3f666e2d"]
	}
]
```

```json
// .. is the same as this
{
	"Tiles": [
		{
			"Name": "Auction Staff",
			"Group": "Disguise",
			"Missions": ["The Showstopper", "Holiday Hoarders"],
			"Tags": ["Disguise", "Civilian"],
			"Disguise": ["b5664bed-462a-417c-bc07-6d9d3f666e2d"]
		}
	]
}
```

You can therefore define areas and tiles in the same file, but only when using the object root node syntax. The array root node syntax is purely for convenience.

The applicable properties for each object in the array are as follows:

- (Required) `Name` - The name of the tile, to be displayed in the grid cell.
- `Group` - The ID of the group this tile belongs to.
- `Type` - The type of the bingo tile ('Complication' or 'Objective').
- `Disabled` - A boolean which if `true` will cause this tile to be disabled within Croupier. This is for convenient removal of the tile from the pool without having to erase it from the file.
- `Missions` - A list of the mission names this tile is valid for. If this property is omitted or empty, the tile can appear for any mission.

**Unused**
- `Tags` - You may find this in existing configurations, however this is currently unused. It may be used as a more flexible alternative to groups for some future functionality
- `Difficulty` - This unused property is for assigning difficulty levels (Low, Medium, High, VeryHigh) to tiles for a potential future difficulty selection feature or possibly for automatic difficulty balancing during board generation.

Any additional properties are expected to provide the trigger logic definitions for the tile. The trigger logic is also *required* for the tile, so omitting it will result in a config loading error. In the above example, the trigger is defined with the `Disguise` property.

## Trigger Definitions

A trigger definition is part of a tile's configuration.

```json
{
	"Name": "Disguise as Auction Staff",
	"Type": "Objective",
	// ...
	"Disguise": ["b5664bed-462a-417c-bc07-6d9d3f666e2d"]
}
```

As this is an Objective tile, it will be marked as completed when a 'Disguise' event is received with the given repository ID. This syntax is a shorthand for:

```json
"Disguise": {
	"RepositoryId": "d2c76544-3a12-43a8-abc3-c7ce51830c1e"
}
```