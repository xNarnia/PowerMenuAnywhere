using Newtonsoft.Json.Serialization;
using System.IO;
using System.Windows.Markup;
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
            On_CreativePowers.DifficultySliderPower.Load += DifficultySliderPower_Load;
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

    public class PWAModSystem : ModSystem
    {
        public override void OnWorldLoad()
        {
            PowerMenuAnywhere.SaveDifficultyState();
            PowerMenuAnywhere.SetOverrideDifficultyWithCreative();
        }
    }
}