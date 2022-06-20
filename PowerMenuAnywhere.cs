using Terraria;
using Terraria.ModLoader;

namespace PowerMenuAnywhere
{
	public class PowerMenuAnywhere : Mod
	{
		public override void Load()
		{
			base.Load();
			On.Terraria.GameContent.Creative.CreativeUI.Draw += CreativeUI_Draw;
			On.Terraria.Main.MouseText_DrawItemTooltip_GetLinesInfo += Main_MouseText_DrawItemTooltip_GetLinesInfo;
		}

		private void Main_MouseText_DrawItemTooltip_GetLinesInfo(On.Terraria.Main.orig_MouseText_DrawItemTooltip_GetLinesInfo orig, Item item, ref int yoyoLogo, ref int researchLine, float oldKB, ref int numLines, string[] toolTipLine, bool[] preFixLine, bool[] badPreFixLine, string[] toolTipNames)
		{
			var ogDifficulty = Main.LocalPlayer.difficulty;
			Main.LocalPlayer.difficulty = 3;
			orig(item, ref yoyoLogo, ref researchLine, oldKB, ref numLines, toolTipLine, preFixLine, badPreFixLine, toolTipNames);
			Main.LocalPlayer.difficulty = ogDifficulty;
		}

		private void CreativeUI_Draw(On.Terraria.GameContent.Creative.CreativeUI.orig_Draw orig, Terraria.GameContent.Creative.CreativeUI self, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
		{
			var ogDifficulty = Main.LocalPlayer.difficulty;
			Main.LocalPlayer.difficulty = 3;
			orig(self, spriteBatch);
			Main.LocalPlayer.difficulty = ogDifficulty;
		}
	}
}