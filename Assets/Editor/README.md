# Football Goalpost Prefab Generator

## How to use

1. In your Unity project, create this folder if it does not exist:

   `Assets/Editor`

2. Put `CreateFootballGoalpostPrefab.cs` inside `Assets/Editor`.

3. In Unity, run:

   `Tools > Create Football Goalpost Prefab`

4. The prefab will be created here:

   `Assets/Prefabs/Football_Goalpost_Prefab.prefab`

## Notes

- This creates a simple American football goalpost using Unity primitive cylinders.
- It includes:
  - yellow goalpost material
  - black base pad
  - left/right uprights
  - crossbar
  - optional trigger collider named `Goal_Area_Trigger`
- You can scale the prefab after creation depending on your field size.