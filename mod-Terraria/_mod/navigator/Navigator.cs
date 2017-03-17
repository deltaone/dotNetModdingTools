using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

using Terraria;
using Terraria.Map;

namespace dotNetMT
{
    [PluginTag("Navigator", "de1ta0ne", @"
        |Navigate to valuable tiles:
        |
        |   N - switch mode 
        |           0 - off - disable navigator
        |           1 - navigate to nearest 'metal detector' item 
        |           2 - navigate to nearest manually selected tile type
        |                 [ - previous tile type
        |                 ] - next tile type
    ")]
    public class Navigator : PluginBase
    {
        int[] _valuableTiles;
        int _valuableIndex = 0;
        int _navigatorMode = 0;
        Texture2D _navigationArrow;

        public Navigator()
        {
            string section = GetType().Name;

            List<int> items = new List<int>();
            Dictionary<int, int> valuables = new Dictionary<int, int>();
            for (int i = 0; i < Main.tileValue.Length; i++)
                if (Main.tileValue[i] > 0) valuables.Add(i, Main.tileValue[i]);
            foreach (var item in valuables.OrderBy(key => key.Value)) items.Add(item.Key);
            _valuableTiles = items.ToArray();

            HotkeyCore.RegisterHotkey(() =>
            {
                _navigatorMode++;
                if (_navigatorMode > 2) _navigatorMode = 0;
                CORE.Print("Navigator mode - " + (_navigatorMode == 0 ? "off" : (_navigatorMode == 1 ? "metal detector" : "manual")));
                if (_navigatorMode == 1 && !Main.player[Main.myPlayer].accOreFinder)
                    CORE.Print("   Warning, 'Metal Detector' not activated!");
            }, DNMT.Config.Get(section, "Mode", new Hotkey(Keys.N), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                _valuableIndex--;
                if (_valuableIndex < 0) _valuableIndex = _valuableTiles.Length - 1;
                CORE.Print("Navigator - " + Lang.mapLegend[MapHelper.TileToLookup(_valuableTiles[_valuableIndex], 0)]);
            }, DNMT.Config.Get(section, "PrevItem", new Hotkey(Keys.OemOpenBrackets), true));
            HotkeyCore.RegisterHotkey(() =>
            {
                _valuableIndex++;
                if (_valuableIndex >= _valuableTiles.Length) _valuableIndex = 0;
                CORE.Print("Navigator - " + Lang.mapLegend[MapHelper.TileToLookup(_valuableTiles[_valuableIndex], 0)]);
            }, DNMT.Config.Get(section, "NextItem", new Hotkey(Keys.OemCloseBrackets), true));

            _navigationArrow = CORE.GetTexture(@"navigator\NavigatorArrow");
        }

        private string GetDirectionAndDistance(int x0, int y0, int x1, int y1, out int distance)
        {   // distance check - if((x1-x2)*(x1-x2)+(y1-y2)*(y1-y2) < d*d) {}  where d - distance / avoid sqrt by d*d
            int dx = x0 - x1;
            int dy = y0 - y1;
            
            distance = (int)Math.Sqrt(dx * dx + dy * dy);
            if (distance < 2) return ("HERE");

            string result = distance.ToString() + " on ";

            if (x0 == x1) // ((dx > 0 ? dx : -dx) < 2)
            {
                if (y1 < y0) return (result + "'N'");
                else return (result + "'S'");
            }
            if (y0 == y1) // ((dy > 0 ? dy : -dy) < 2)
            {
                if (x1 < x0) return (result + "'W'");
                else return (result + "'E'");
            }
            if (y1 < y0)
            {
                if (x1 < x0) return (result + "'NW'");
                else return (result + "'NE'");
            }
            else
            {
                if (x1 < x0) return (result + "'SW'");
                else return (result + "'SE'");
            }
        }
        
        private bool TryGetNearestTileV0(int tileID, int x0, int y0, int radius, out int x1, out int y1)
        {
            int left = x0, right = x0, up = y0, down = y0,
                maxWidthIndex = Main.tile.GetLength(0) - 1, maxHeightIndex = Main.tile.GetLength(1) - 1;

            x1 = x0; y1 = y0;
            if (Main.tile[x0, y0].type == tileID)
                return (true);

            for (int i = 0; i < radius; i++)
            {
                left--; if (left < 0) left = 0;
                right++; if (right > maxWidthIndex) right = maxWidthIndex;
                up--; if (up < 0) up = 0;
                down++; if (down > maxHeightIndex) down = maxHeightIndex;
                for (int x2 = left; x2 <= right; x2++)
                {
                    if (Main.tile[x2, up].type == tileID)
                    {
                        x1 = x2; y1 = up;
                        return (true);
                    }
                    if (Main.tile[x2, down].type == tileID)
                    {
                        x1 = x2; y1 = down;
                        return (true);
                    }
                }
                for (int y2 = up + 1; y2 < down; y2++)
                {
                    if (Main.tile[left, y2].type == tileID)
                    {
                        x1 = left; y1 = y2;
                        return (true);
                    }
                    if (Main.tile[right, y2].type == tileID)
                    {
                        x1 = right; y1 = y2;
                        return (true);
                    }
                }
            }
            return (false);
        }

        private bool TryGetNearestTileV1(int tileID, int x0, int y0, int radius, out int x1, out int y1)
        {
            int range = radius * 2 + 1,
                maxWidth = Main.tile.GetLength(0), maxHeight = Main.tile.GetLength(1),
                x = 0, y = 0,
                dx = 0, dy = -1;

            for (int i = range * range; i > 0; i--)
            {
                x1 = x0 + x;
                y1 = y0 + y;
                // may be try/catch for ArrayIndexOutOfBoundsException
                if ((0 <= x1) && (x1 < maxWidth) && (0 <= y1) && (y1 < maxHeight) && Main.tile[x1, y1].type == tileID)
                    return (true); 
                if ((x == y) || ((x < 0) && (x == -y)) || ((x > 0) && (x == 1 - y)))
                {
                    int t = dx;
                    dx = -dy;
                    dy = t;
                }
                x += dx;
                y += dy;
            }
            x1 = x0; y1 = y0;
            return (false);
        }

        public override void OnDrawInterface()
        {
            if (Main.gameMenu || Main.ingameOptionsWindow || Main.playerInventory || Main.mapFullscreen) return;

            if (_navigatorMode == 0) return;

            int tileID;
            Player player = Main.player[Main.myPlayer];

            if (_navigatorMode == 1 && player.accOreFinder)
                tileID = player.bestOre;
            else if (_navigatorMode == 2)
                tileID = _valuableTiles[_valuableIndex];
            else
                tileID = -1;
            
            bool found = false;
            string text = "Navigator: ";
            int x0 = (int)player.Center.X / 16; // int x = (int)Main.MouseWorld.X / 16;
            int y0 = (int)player.Center.Y / 16; // int y = (int)Main.MouseWorld.Y / 16;
            int x1 = 0, y1 = 0;
            int distance = 0;

            if (tileID != -1)
            {
                found = TryGetNearestTileV0(tileID, x0, y0, 80, out x1, out y1);
                if (found)
                    text += GetDirectionAndDistance(x0, y0, x1, y1, out distance) + " / " + Lang.mapLegend[MapHelper.TileToLookup(tileID, 0)];
                else
                    text += "--- / " + Lang.mapLegend[MapHelper.TileToLookup(tileID, 0)];
            }
            else
                text += "---";

            Vector2 size, position;
            size = Main.fontMouseText.MeasureString(text);
            position = new Vector2((float)(Main.screenWidth - size.X - 4), (float)Main.screenHeight - 24);
            Main.spriteBatch.DrawString(Main.fontMouseText, text, position, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            if (!found || distance < 2 || _navigationArrow == null) return;

            float angle = (float) (180 - (Math.Atan2(y0 - y1, x0 - x1)) * 180 / Math.PI);
            angle %= 360;
            angle = (float) (Math.Round(angle / 5, MidpointRounding.AwayFromZero) * 5);

            text = "angle: " + angle.ToString();
            size = Main.fontMouseText.MeasureString(text);
            position = new Vector2((float)(Main.screenWidth - size.X - 4), (float)Main.screenHeight - 48);
            Main.spriteBatch.DrawString(Main.fontMouseText, text, position, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            Texture2D texture = _navigationArrow;
            Main.spriteBatch.Draw(texture,
                new Vector2((float)(Main.screenWidth - 26), (float)Main.screenHeight - 72),
                new Rectangle?(new Rectangle(0, 0, texture.Width, texture.Height)),
                Color.White,
                -(float)(Math.PI * angle / 180.0),
                new Vector2(texture.Width / 2, texture.Height / 2), 1f, SpriteEffects.None, 0f);
        }
    }
}