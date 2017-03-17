using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using StardewValley;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace dotNetMT
{
    [PluginTag("ManualSprinklers", "d1", @"Add clickable sprinklers ...")]
    public class ManualSprinklers : PluginBase
    {
        public override void OnUpdateControlInput(KeyboardState stateKeyboard, MouseState stateMouse, GamePadState stateGamePad)
        {
            if(stateMouse.RightButton == ButtonState.Pressed && Game1.oldMouseState.RightButton != ButtonState.Pressed)
            {
                int x = Game1.getMouseX() + Game1.viewport.X;
                int y = Game1.getMouseY() + Game1.viewport.Y;
                Vector2 key = new Vector2((float)(x / Game1.tileSize), (float)(y / Game1.tileSize));
                GameLocation currentLocation = Game1.currentLocation;
                if (currentLocation.objects.ContainsKey(key) && currentLocation.objects[key].name.Contains("Sprinkler"))
                {
                    currentLocation.objects[key].DayUpdate(currentLocation);
                    DelayedAction.playSoundAfterDelay("coin", 260);
                }
            }
        }
    }
}