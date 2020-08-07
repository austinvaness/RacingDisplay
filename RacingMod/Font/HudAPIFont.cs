using Draygo.API;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.Components;
using VRage.Game;
using VRage.Utils;
using System.Text.RegularExpressions;

namespace avaness.RacingMod.Font
{
	public class HudAPIFont
	{
		private readonly string id;
		private fontdata font = new fontdata();
		private readonly List<glyph> glyphset = new List<glyph>();
		private readonly Dictionary<int, bitmap> bitmapset = new Dictionary<int, bitmap>();
		private readonly List<kernpair> kernpairset = new List<kernpair>();

		public HudAPIFont(string data, string id)
		{
			this.id = id;

			var strings = data.Split('\n');
			try
			{
				foreach (var str in strings)
					ParseLine(str);
			}
			catch (Exception ex)
			{
				MyLog.Default.WriteLine(ex);
			}
		}

		public void CreateFont()
		{
			HudAPIv2.FontDefinition Font = HudAPIv2.APIinfo.GetFontDefinition(MyStringId.GetOrCompute(id));
			MyLog.Default.WriteLine($"Loaded {id} Glyphs:{glyphset.Count} Kerning pairs:{kernpairset.Count}");
			Font.DefineFont(font.fontbase, font.fontheight, font.fontsize);
			foreach (glyph gl in glyphset)
			{
				bitmap bm;
				if (bitmapset.TryGetValue(gl.bm, out bm))
				{
					Font.AddCharacter(gl.ch, bm.material, bm.materialsize, gl.code, gl.originx, gl.originy, gl.sizex, gl.sizey, gl.aw, gl.lsb, gl.forcewhite);
				}


			}
			foreach (kernpair kp in kernpairset)
			{
				Font.AddKerning(kp.adjust, kp.right, kp.left);
			}
		}

		public void ParseLine(string Line)
		{
			Line = Line.TrimStart(' ', '\t');
			if (Line.StartsWith("name"))
			{
				ParseBaseHeight(Line);
				return;
			}
			if (Line.StartsWith("face"))
			{
				ParseFontNameSize(Line);
				return;
			}
			if (Line.StartsWith("<bitmap"))
			{
				ParseBitmapLine(Line);
				return;
			}
			if (Line.StartsWith("<glyph"))
			{
				ParseGlyphLine(Line);
				return;
			}
			if (Line.StartsWith("<kernpair"))
			{
				ParseKernpair(Line);
				return;
			}
		}

		Regex regKernpair = new Regex("<kernpair left\\s*=\\s*\"(?'leftchar'[^\"]*)\" right\\s*=\\s*\"(?'rightchar'[^\"]*)\" adjust\\s*=\\s*\"(?'adjust'[\\-0-9]*)\"", RegexOptions.ExplicitCapture | RegexOptions.Compiled);
		private void ParseKernpair(string line)
		{

			var data = regKernpair.Match(line);
			if (!data.Success)
				return;
			string leftstring = data.Groups["leftchar"].Value;
			string rightstring = data.Groups["rightchar"].Value;
			kernpair kp = new kernpair();
			int.TryParse(data.Groups["adjust"].Value, out kp.adjust);
			kp.left = getChar(leftstring);
			kp.right = getChar(rightstring);
			kernpairset.Add(kp);
		}

		private char getChar(string str)
		{
			if (str == "&quot;")
				return '\"';
			if (str == "&amp;")
				return '&';
			if (str == "&gt;")
				return '>';
			if (str == "&lt;")
				return '<';
			return str[0];
		}

		Regex regglyph = new Regex("<glyph ch\\s*=\\s*\"(?'character'[^\"]*)\" code\\s*=\\s*\"(?'charactercode'[^\"]*)\" bm\\s*=\\s*\"(?'bitmapid'[0-9]*)\" origin\\s*=\\s*\"(?'originx'[0-9]*),(?'originy'[0-9]*)\" size\\s*=\\s*\"(?'sizex'[0-9]*)x(?'sizey'[0-9]*)\" aw\\s*=\\s*\"(?'aw'[0-9]*)\" lsb\\s*=\\s*\"(?'lsb'[\\-0-9]*)\"(?'forcewhite' forcewhite\\s*=\\s*\"true\")*", RegexOptions.ExplicitCapture | RegexOptions.Compiled);
		private void ParseGlyphLine(string line)
		{

			try
			{
				var data = regglyph.Match(line);
				if (!data.Success)
					return;
				var glyph = new glyph();
				glyph.code = data.Groups["charactercode"].Value;
				glyph.ch = getChar(data.Groups["character"].Value);

				int.TryParse(data.Groups["bitmapid"].Value, out glyph.bm);
				int.TryParse(data.Groups["originx"].Value, out glyph.originx);
				int.TryParse(data.Groups["originy"].Value, out glyph.originy);
				int.TryParse(data.Groups["sizex"].Value, out glyph.sizex);
				int.TryParse(data.Groups["sizey"].Value, out glyph.sizey);
				int.TryParse(data.Groups["aw"].Value, out glyph.aw);
				int.TryParse(data.Groups["lsb"].Value, out glyph.lsb);
				if (data.Groups["forcewhite"].Success)
				{
					glyph.forcewhite = true;
				}
				else
					glyph.forcewhite = false;
				glyphset.Add(glyph);
			}
			catch
			{
				MyLog.Default.WriteLine($"Failed to parse glyph on line: {line}");
			}



		}

		Regex regbitmap = new Regex("<bitmap id\\s*=\\s*\"(?'bitmapid'[0-9]*)\" name\\s*=\\s*\"(?'bitmapmaterial'[^\"]*)\" size\\s*=\\s*\"(?'bitmapsize'[0-9]*)", RegexOptions.ExplicitCapture | RegexOptions.Compiled);
		private void ParseBitmapLine(string line)
		{

			var data = regbitmap.Match(line);
			if (!data.Success)
				return;
			bitmap bp = new bitmap();
			int.TryParse(data.Groups["bitmapid"].Value, out bp.bm);
			bp.material = MyStringId.GetOrCompute(data.Groups["bitmapmaterial"].Value.Substring(0, data.Groups["bitmapmaterial"].Value.Length - 4));
			int.TryParse(data.Groups["bitmapsize"].Value, out bp.materialsize);
			if (bitmapset.ContainsKey(bp.bm))
				bitmapset.Remove(bp.bm);
			bitmapset.Add(bp.bm, bp);
		}

		Regex regfontnamesize = new Regex("face =\"(?'fontname'[a-zA-Z]*)\" size=\"(?'fontsize'[0-9]*)\"", RegexOptions.ExplicitCapture | RegexOptions.Compiled);
		private void ParseFontNameSize(string line)
		{
			var data = regfontnamesize.Match(line);
			if (!data.Success)
				return;
			font.FontName = MyStringId.GetOrCompute(data.Groups["fontname"].Value);
			int.TryParse(data.Groups["fontsize"].Value, out font.fontsize);
		}

		Regex regbaseheight = new Regex("base\\s*=\\s*\"(?'base'[0-9]+)\" height\\s*=\\s*\"(?'height'[0-9]+)\"", RegexOptions.ExplicitCapture | RegexOptions.Compiled);
		private void ParseBaseHeight(string line)
		{
			var data = regbaseheight.Match(line);
			if (!data.Success)
				return;
			int.TryParse(data.Groups["base"].Value, out font.fontbase);
			int.TryParse(data.Groups["height"].Value, out font.fontheight);
		}

		struct bitmap
		{
			public int bm;
			public MyStringId material;
			public int materialsize;
		}
		struct glyph
		{
			public char ch;
			public string code;
			public int bm;
			public int originx;
			public int originy;
			public int sizex;
			public int sizey;
			public int aw;
			public int lsb;
			public bool forcewhite;
		}
		public struct fontdata
		{
			public int fontbase;
			public int fontsize;
			public int fontheight;
			public MyStringId FontName;
		}
		public struct kernpair
		{
			public char left;
			public char right;
			public int adjust;
		}
	}
}
