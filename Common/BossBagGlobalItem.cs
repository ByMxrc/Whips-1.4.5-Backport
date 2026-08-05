using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Whips145.Items;

namespace Whips145.Common
{
    public class BossBagGlobalItem : GlobalItem
    {
        public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
        {
            // Vulgar Display of Flower -> Plantera Boss Bag (25% de probabilidad)
            if (item.type == ItemID.PlanteraBossBag)
            {
                itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<VulgarDisplayOfFlower>(), 4));
            }

            // Electric Eel -> Duke Fishron Boss Bag (25%)
            if (item.type == ItemID.FishronBossBag)
            {
                itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<ElectricEel>(), 4));
            }

            // Possession -> Moon Lord Boss Bag (25%)
            if (item.type == ItemID.MoonLordBossBag)
            {
                itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Possession>(), 4));
            }
        }
    }
}
