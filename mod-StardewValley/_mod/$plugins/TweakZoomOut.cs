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
    [PluginTag("Tweak-ZoomOut", "d1", @"
        |   Add 'Zoom out' tweak! (default 0.75 / changed to 0.5)
        |
        |   Note: enable screen zoom buttons in options, use these buttons to adjust zoom rate!
    ")]
    public class TweakZoomOut
    {
        [PluginPatch("Stardew Valley.exe")]
        public static void Patch(ModuleDefinition module, List<string> log)
        {
            float minZoom = DNMT.Config.Get<float>("Tweak-ZoomOut", "minZoom", 0.5f, true);

            var type = IL.GetTypeDefinition(module, "DayTimeMoneyBox");

            var fn = IL.GetMethodDefinition(type, "draw");
            int spot = IL.ScanForOpcodePattern(fn,
                (i, instruction) =>
                {
                    var fieldReference = fn.Body.Instructions[i].Operand as FieldReference;
                    return (fieldReference != null && fieldReference.Name == "zoomLevel");
                },
                OpCodes.Ldfld,
                OpCodes.Ldc_R4,
                OpCodes.Ble_S
            );

            if (spot == -1)
            {
                log.Add("Can't find patch location!");
                return;
            }

            fn.Body.Instructions[spot + 1].Operand = minZoom;

            fn = IL.GetMethodDefinition(type, "receiveLeftClick");
            spot = IL.ScanForOpcodePattern(fn,
                            (i, instruction) =>
                            {
                                var fieldReference = fn.Body.Instructions[i].Operand as FieldReference;
                                return (fieldReference != null && fieldReference.Name == "zoomLevel");
                            },
                            OpCodes.Ldfld,
                            OpCodes.Ldc_R4,
                            OpCodes.Ble_Un_S
                        );

            if (spot == -1)
            {
                log.Add("Can't find patch location!");
                return;
            }

            fn.Body.Instructions[spot + 1].Operand = minZoom;

            spot = IL.ScanForOpcodePattern(fn,
                            (i, instruction) =>
                            {
                                var fieldReference = fn.Body.Instructions[i].Operand as FieldReference;                                
                                return (fieldReference != null && fieldReference.Name == "options");
                            },
                            OpCodes.Ldsfld,
                            OpCodes.Ldc_R4,
                            OpCodes.Ldloc_1
                        );

            if (spot == -1)
            {
                log.Add("Can't find patch location!");
                return;
            }

            fn.Body.Instructions[spot + 1].Operand = minZoom;
        }
    }
}