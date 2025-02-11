using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;

namespace PowerMenuAnywhere
{
    public class PowerMenuAnywhere : Mod
    {

        public override void Load()
        {
            base.Load();
            On_Main.UpdateCreativeGameModeOverride += Main_UpdateCreativeGameModeOverride;
            On_CreativeUI.Draw += CreativeUI_Draw;
            On_NPC.ScaleStats += NPC_ScaleStats;
            On_NPC.SpawnNPC += NPC_SpawnNPC;
            On_NPC.SlimeRainSpawns += NPC_SlimeRainSpawns;
            On_Player.GrabItems += Player_GrabItems;
            On_Player.ResetEffects += Player_ResetEffects;
            On_Player.GetItemGrabRange += Player_GetItemGrabRange;
            On_Player.Hurt_PlayerDeathReason_int_int_refHurtInfo_bool_bool_int_bool_float_float_float += Player_Hurt;
            On_CreativePowers.DifficultySliderPower.Load += DifficultySliderPower_Load;
        }

        public override void Unload()
        {
            base.Unload();
            On_Main.UpdateCreativeGameModeOverride -= Main_UpdateCreativeGameModeOverride;
            On_CreativeUI.Draw -= CreativeUI_Draw;
            On_NPC.ScaleStats -= NPC_ScaleStats;
            On_NPC.SpawnNPC -= NPC_SpawnNPC;
            On_NPC.SlimeRainSpawns -= NPC_SlimeRainSpawns;
            On_Player.GrabItems -= Player_GrabItems;
            On_Player.ResetEffects -= Player_ResetEffects;
            On_Player.GetItemGrabRange -= Player_GetItemGrabRange;
            On_Player.Hurt_PlayerDeathReason_int_int_refHurtInfo_bool_bool_int_bool_float_float_float -= Player_Hurt;
            On_CreativePowers.DifficultySliderPower.Load -= DifficultySliderPower_Load;
        }

        /// <summary>
        /// Simplified detour for injecting Creative difficulty.<br/>
        /// Opted for local scope variables with deep copies of difficulty since managing state globally was too clunky.
        /// </summary>
        private void CreativeDetour(Action action)
        {
            var ogDifficulty = GetDifficultySettings();
            SetCreativeDifficulty();
            action.Invoke();
            SetDifficulty(ogDifficulty);
        }

        /// <summary>
        /// Applies difficulty overrides based on Difficulty Slider.
        /// </summary>
        private void Main_UpdateCreativeGameModeOverride(On_Main.orig_UpdateCreativeGameModeOverride orig) => CreativeDetour(() => orig());
        
        /// <summary>
        /// Forces the Power Menu to show even if it's not a Journey character or world.
        /// </summary>
        private void CreativeUI_Draw(On_CreativeUI.orig_Draw orig, CreativeUI self, SpriteBatch spriteBatch) => CreativeDetour(() => orig(self, spriteBatch));

        /// <summary>
        /// Enables StrengthMultiplierToGiveNPCs.
        /// </summary>
        private void NPC_ScaleStats(On_NPC.orig_ScaleStats orig, NPC self, int? activePlayersCount, GameModeData gameModeData, float? strengthOverride) => CreativeDetour(() => orig(self, activePlayersCount, gameModeData, strengthOverride));

        /// <summary>
        /// Enables SpawnRateSliderPerPlayerPower.
        /// </summary>
        private void NPC_SpawnNPC(On_NPC.orig_SpawnNPC orig) => CreativeDetour(() => orig());

        /// <summary>
        /// Enables SpawnRateSliderPerPlayerPower for Slime Rain.
        /// </summary>
        private void NPC_SlimeRainSpawns(On_NPC.orig_SlimeRainSpawns orig, int plr) => CreativeDetour(() => orig(plr));

        /// <summary>
        /// Enables FarPlacementRangePower.
        /// </summary>
        private void Player_GrabItems(On_Player.orig_GrabItems orig, Player self, int i) => CreativeDetour(() => orig(self, i));

        /// <summary>
        /// Enables FarPlacementRangePower.
        /// </summary>
        private void Player_ResetEffects(On_Player.orig_ResetEffects orig, Player self) => CreativeDetour(() => orig(self));

        /// <summary>
        /// Enables FarPlacementRangePower.
        /// </summary>
        private int Player_GetItemGrabRange(On_Player.orig_GetItemGrabRange orig, Player self, Item item)
        {
            var ogDifficulty = GetDifficultySettings();
            SetCreativeDifficulty();
            var payload = orig(self, item);
            SetDifficulty(ogDifficulty);
            return payload;
        }

        /// <summary>
        /// Enables damage scaling from Enemy Difficulty.
        /// </summary>
        private double Player_Hurt(On_Player.orig_Hurt_PlayerDeathReason_int_int_refHurtInfo_bool_bool_int_bool_float_float_float orig, Player self, Terraria.DataStructures.PlayerDeathReason damageSource, int Damage, int hitDirection, out Player.HurtInfo info, bool pvp, bool quiet, int cooldownCounter, bool dodgeable, float armorPenetration, float scalingArmorPenetration, float knockback)
        {
            var ogDifficulty = GetDifficultySettings();
            SetCreativeDifficulty();
            var payload = orig(self, damageSource, Damage, hitDirection, out info, pvp, quiet, cooldownCounter, dodgeable, armorPenetration, scalingArmorPenetration, knockback);
            SetDifficulty(ogDifficulty);
            return payload;
        }

        public DifficultySettings GetDifficultySettings()
        {
            return new DifficultySettings()
            {
                OGWorldDifficulty = Main.ActiveWorldFileData.GameMode,
                OGPlayerDifficulty = Main.LocalPlayer.difficulty,
                OGGameMode = Main.GameMode
            };
        }

        public void SetDifficulty(DifficultySettings settings)
        {
            Main.ActiveWorldFileData.GameMode = settings.OGWorldDifficulty;
            Main.LocalPlayer.difficulty = settings.OGPlayerDifficulty;
            Main.GameMode = settings.OGGameMode;
        }

        public void SetCreativeDifficulty()
        {
            Main.ActiveWorldFileData.GameMode = GameModeID.Creative;
            Main.LocalPlayer.difficulty = (byte)PlayerDifficultyID.Creative;
            Main.GameMode = GameModeID.Creative;
        }

        /// <summary>
        /// Sets the Difficulty Slider to the game mode of the world when loading.
        /// </summary>
        private void DifficultySliderPower_Load(On_CreativePowers.DifficultySliderPower.orig_Load orig, CreativePowers.DifficultySliderPower self, System.IO.BinaryReader reader, int gameVersionSaveWasMadeOn)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            // The slider is set in terms of "slider position"
            // The difficulties are spaced out by quarters.
            // So use 3rds, and have 0 be the 4th.
            switch (Main.GameMode) {
                case 0: // Normal
                    writer.Write(1f / 3f);
                    break;
                case 1: // Expert
                    writer.Write(2f / 3f);
                    break;
                case 2: 
                    writer.Write(1f);
                    break;
                default:
                    writer.Write(0f);
                    break;
            }
            writer.Flush();
            stream.Position = 0;

            reader.ReadSingle();
            var myreader = new BinaryReader(stream);
            orig(self, myreader, gameVersionSaveWasMadeOn);
            writer.Dispose();
        }
    }

    public class DifficultySettings
    {
        public int OGWorldDifficulty { get; set; }
        public byte OGPlayerDifficulty { get; set; }
        public int OGGameMode { get; set; }
    }
}