using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;

namespace PowerMenuAnywhere
{
	public class PowerMenuAnywhere : Mod
    {
        public static int OGWorldDifficulty { get; set; }
        public static byte OGPlayerDifficulty { get; set; }
        public static int OGGameMode { get; set; }

        public override void Load()
        {
            base.Load();
            On_CreativeUI.Draw += CreativeUI_Draw;
            On_WorldFile.SaveWorld += this.On_WorldFile_SaveWorld;
            On_Player.SavePlayer += this.On_Player_SavePlayer;
        }

        public override void Unload()
        {
            base.Unload();
            On_CreativeUI.Draw -= CreativeUI_Draw;
            On_WorldFile.SaveWorld -= this.On_WorldFile_SaveWorld;
            On_Player.SavePlayer -= this.On_Player_SavePlayer;
        }

        private void CreativeUI_Draw(On_CreativeUI.orig_Draw orig, CreativeUI self, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            SetOverrideDifficultyWithCreative();
            orig(self, spriteBatch);
        }

        private void On_Player_SavePlayer(On_Player.orig_SavePlayer orig, PlayerFileData playerFile, bool skipMapSave)
        {
            SetOriginalDifficulty();
            orig(playerFile, skipMapSave);
            SetOverrideDifficultyWithCreative();
        }

        private void On_WorldFile_SaveWorld(On_WorldFile.orig_SaveWorld orig)
        {
            SetOriginalDifficulty();
            orig();
            SetOverrideDifficultyWithCreative();
        }

        public static void SaveDifficultyState()
        {
            OGWorldDifficulty = Main.ActiveWorldFileData.GameMode;
            OGPlayerDifficulty = Main.LocalPlayer.difficulty;
            OGGameMode = Main.GameMode;
        }

        public static void SetOriginalDifficulty()
        {
            Main.ActiveWorldFileData.GameMode = OGWorldDifficulty;
            Main.LocalPlayer.difficulty = OGPlayerDifficulty;
            Main.GameMode = OGGameMode;
        }

        public static void SetOverrideDifficultyWithCreative()
        {
            Main.ActiveWorldFileData.GameMode = GameModeID.Creative;
            Main.LocalPlayer.difficulty = (byte)GameModeID.Creative;
            Main.GameMode = GameModeID.Creative;
        }
    }

    public class PWAModSystem : ModSystem
    {
        public override void OnWorldLoad()
        {
            PowerMenuAnywhere.SaveDifficultyState();
            PowerMenuAnywhere.SetOverrideDifficultyWithCreative();
        }
    }
}