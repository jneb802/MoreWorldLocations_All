# Optional location checks

Run each state in a fresh process because the location database initializes once:

```sh
dotnet run --project tests/OptionalLocations -- absent
dotnet run --project tests/OptionalLocations -- present
```

These checks compile the production location definitions, database, registration
flow, and Upgrade World commands against substitutes for external APIs. They cover
dependency filtering, unchanged existing locations, and matching Minddrip placement
defaults. They do not test BepInEx load order, bundle loading, mock resolution, or
teleport travel in Valheim.
