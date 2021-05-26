﻿// ToolkitUtils
// Copyright (C) 2021  SirRandoo
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace SirRandoo.ToolkitUtils
{
    [UsedImplicitly]
    public class Coordinator : GameComponent
    {
        private readonly List<Thing> portals = new List<Thing>();

        public Coordinator(Game game) { }

        internal bool HasActivePortals => portals.Count > 0;

        [ContractAnnotation("map:notnull => true,portal:notnull; map:notnull => false,portal:null")]
        internal bool TryGetRandomPortal(Map map, out Thing portal)
        {
            portals.Where(p => p.Map.Equals(map)).TryRandomElement(out portal);
            return portal != null;
        }

        internal bool TrySpawnItem(Map map, Thing item)
        {
            if (!TryGetRandomPortal(map, out Thing portal))
            {
                return false;
            }

            GenPlace.TryPlaceThing(
                item,
                portal.Position,
                portal.Map,
                ThingPlaceMode.Near,
                (thing, count) => MoteMaker.ThrowSmoke(
                    thing.Position.ToVector3(),
                    thing.Map,
                    thing.Graphic.drawSize.magnitude
                )
            );
            return true;
        }

        internal bool TrySpawnPawn(Map map, Pawn pawn)
        {
            if (!TryGetRandomPortal(map, out Thing portal))
            {
                return false;
            }

            GenSpawn.Spawn(pawn, portal.Position, portal.Map, WipeMode.VanishOrMoveAside);
            return true;
        }

        internal void RemovePortal(Thing thing)
        {
            portals.Remove(thing);
        }

        internal void RegisterPortal(Thing thing)
        {
            portals.Add(thing);
        }
    }
}
