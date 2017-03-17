using System;
using Microsoft.Xna.Framework;
using Terraria;
using GTRPlugins.UI;
using dotNetMT;

namespace GTRPlugins
{
    [PluginTag("Swap Loadout", "GTRPlugins", @"
        |Add new button into inventory screen.
    ")]
    public class LoadoutSwap : PluginBase
    {
        private Button _btnLoadoutSwap;

        public LoadoutSwap()
        {
            _btnLoadoutSwap = new Button("Swap Loadout", new Vector2(502f, 298f), BtnLoadoutSwapClick);
            _btnLoadoutSwap.Scale = 0.9f;
        }

        private void BtnLoadoutSwapClick(object sender, EventArgs e)
        {
            Player player = Main.player[Main.myPlayer];
            for (int i = 0; i < 10; i++)
            {
                Item item = player.armor[i].Clone();
                player.armor[i] = player.armor[i + 10];
                player.armor[i + 10] = item;
            }
        }

        public override void OnDrawInventory()
        {
            if (Main.player[Main.myPlayer].chest == -1 && Main.npcShop == 0)
            {
                _btnLoadoutSwap.Draw();
            }
        }
    }
}
