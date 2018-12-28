using Sandbox.Game;
using VRage.Game;
using VRage.Game.ModAPI;

public class SpawnToolsRemover {

    Logger logger = Logger.getLogger("ToolRemover");

    public void Remove(IMyCharacter character) {
        var characterInventory = character.GetInventory();
        if (characterInventory == null) {
            logger.WriteLine(character.Name + " has no Inventory");
            return;
        }

        MyInventory inventory = characterInventory as MyInventory;
        if (inventory == null) {
            logger.WriteLine(character.Name + " has no Inventory");
            return;
        }

        inventory.ClearItems();
    }
}