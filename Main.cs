using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using SpaceEngineers.Game.ModAPI;
using Sandbox.ModAPI;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.Components;
using VRage.Utils;
using VRageMath;

[MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
public class Main : MySessionComponentBase {

    private SpawnToolsRemover spawnToolsRemover;
    private Logger logger;

    public override void Init(MyObjectBuilder_SessionComponent sessionComponent) {
        base.Init(sessionComponent);

        spawnToolsRemover = new SpawnToolsRemover();

        logger = Logger.getLogger("ToolRemover");
        logger.WriteLine("Initialized");

        /* Add Listener */
        MyVisualScriptLogicProvider.PlayerSpawned += PlayerSpawned;
    }

    protected override void UnloadData() {

        base.UnloadData();

        MyVisualScriptLogicProvider.PlayerSpawned -= PlayerSpawned;

        if (logger != null) {
            logger.WriteLine("Unloaded");
            logger.Close();
        }
    }

    private void PlayerSpawned(long playerId) {
        //logger.WriteLine("Request of Player "+ playerId);

        IMyIdentity playerIdentity = Player(playerId);

        //logger.WriteLine("Found Identity " + playerId);

        if (playerIdentity != null) {
            //logger.WriteLine("Player is " + playerIdentity.DisplayName);

            var playerList = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(playerList, p => p != null && p.IdentityId == playerIdentity.IdentityId);

            var player = playerList.FirstOrDefault();
            if (player != null) {

                if (!MustBeExecuted(player)) {
                    logger.WriteLine("Player " + playerIdentity.DisplayName + " did not spawn near special Medbay!");
                    return;
                }

                MyVisualScriptLogicProvider.SetPlayersHydrogenLevel(playerIdentity.IdentityId, 0);

                IMyCharacter character = player.Character;

                if (character == null)
                    logger.WriteLine("Player " + playerIdentity.DisplayName + " has no Character yet!");

                spawnToolsRemover.Remove(character);
            } else {
                logger.WriteLine("Player " + playerIdentity.DisplayName + " not Found!");
            }
        }
    }

    private bool MustBeExecuted(IMyPlayer player) {

        Vector3D position = player.GetPosition();

        BoundingSphereD sphere = new BoundingSphereD(position, 3.0);

        List<MyEntity> entities = MyEntities.GetEntitiesInSphere(ref sphere);

        double minDist = int.MaxValue;
        IMyMedicalRoom nearest = null;

        foreach (MyEntity entity in entities) {
            if (!(entity is IMyMedicalRoom))
                continue;

            IMyMedicalRoom medicalRoom = entity as IMyMedicalRoom;

            var dist = Vector3D.DistanceSquared(medicalRoom.GetPosition(), position);

            if (dist < minDist) {
                minDist = dist;
                nearest = medicalRoom;
            }
        }

        if (nearest == null)
            return false;

        /* Only affect Medbays that have the following customData */
        if (nearest.CustomData.Contains("spawnPointMedbay")) {
            logger.WriteLine(player.DisplayName + " spawned at " + nearest.CustomName + " on grid " + nearest.CubeGrid.Name);
            return true;
        }

        return false;
    }

    private IMyIdentity Player(long entityId) {
        try {
            List<IMyIdentity> listIdentities = new List<IMyIdentity>();

            MyAPIGateway.Players.GetAllIdentites(listIdentities,
                p => p != null && p.DisplayName != "" && p.IdentityId == entityId);

            if (listIdentities.Count == 1)
                return listIdentities[0];

            return null;
        } catch (Exception e) {
            logger.WriteLine("Error on getting Player Identity " + e);
            return null;
        }
    }
}