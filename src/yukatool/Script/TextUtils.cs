using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Yuka.Script {
	class TextUtils {
		public static int defaultLineWidth = 600;
		public static int defaultCharWidth = 24;

		public static bool IsFullWidthCharacter(char ch) {
			return ch >= 0x3000 && ch < 0x3040 // Japanese-style punctuation
				|| ch >= 0x3040 && ch < 0x30A0 // Hiragana 
				|| ch >= 0x30A0 && ch < 0x3100 // Katakana
				|| ch >= 0xFF00 && ch < 0xFF70 // Full-width roman characters
				|| ch >= 0x4E00 && ch < 0x9FB0 // CJK unified ideographs - Common and uncommon kanji
				|| ch >= 0x3400 && ch < 0x4DC0 // CJK unified ideographs Extension A - Rare kanji
				|| ch >= 0x2600 && ch < 0x2700 // Miscellaneous Symbols
				|| "─".Contains(ch);
		}

		public static int StringWidth(string str, FontMetrics metrics) {
			return str.Aggregate(0, (int l, char ch) => l + metrics.GetCharacterWidth(ch));
		}

		public static string[] WrapWords(string source, int lineWidth, FontMetrics metrics) {
			int pos = 0;
			int lastWrap = 0;
			int lastPossibleWrap = 0;
			int curWidth = 0;
			List<string> lines = new List<string>();

			while(pos < source.Length) {
				char ch = source[pos];
				int charWidth = metrics.GetCharacterWidth(ch);

				if(ch == '\n') {
					// wrap here
					string line = source.Substring(lastWrap, pos - lastWrap);
					lastWrap = lastPossibleWrap = pos;
					curWidth = 0;

					lines.Add(line.PadRight(line.Length + (lineWidth - StringWidth(line, metrics)) / metrics.HalfWidthHorizontalSpacing));
				}
				else if(curWidth + charWidth > lineWidth) {
					if(lastPossibleWrap > lastWrap) {
						// check length of current word
						int end = pos;
						while(end < source.Length && !char.IsWhiteSpace(source[end]) && !IsFullWidthCharacter(source[end])) end++;
						string curWord = source.Substring(lastPossibleWrap, end - lastPossibleWrap).Trim();

						if(StringWidth(curWord, metrics) > lineWidth) {
							// word wouldn't fit in its own line, so we keep its start on the current one and force a hard wrap (no padding)
							lines.Add(source.Substring(lastWrap, pos - lastWrap));
							lastWrap = lastPossibleWrap = pos;
							curWidth = 0;
						}
						else {
							// push the current word to the next line
							string line = source.Substring(lastWrap, lastPossibleWrap - lastWrap);
							int curLineWidth = StringWidth(line, metrics);
							lastWrap = lastPossibleWrap;
							curWidth -= curLineWidth;

							// pad and add line
							lines.Add(line.PadRight(line.Length + (lineWidth - curLineWidth) / metrics.HalfWidthHorizontalSpacing));
						}
					}
					else {
						// force a hard wrap here (no padding)
						lines.Add(source.Substring(lastWrap, pos - lastWrap));
						lastWrap = lastPossibleWrap = pos;
						curWidth = 0;
					}
				}

				curWidth += charWidth;

				pos++;

				// allow soft wraps after each full-width character
				if(char.IsWhiteSpace(ch) || IsFullWidthCharacter(ch)) {
					lastPossibleWrap = pos;
				}
			}

			// add the last line
			if(pos > lastWrap) {
				string line = source.Substring(lastWrap, pos - lastWrap);
				if(line.Trim().Length > 0) {
					lines.Add(line.PadRight(line.Length + (lineWidth - StringWidth(line, metrics)) / metrics.HalfWidthHorizontalSpacing));
				}
			}

			return lines.ToArray();
		}

		public static string ReplaceSpecialChars(string source) {
			StringBuilder sb = new StringBuilder(source);

			// use ─ for seamless lines
			// supported special characters: ★☆♀♂♪♭♯

			sb.Replace('（', '(');
			sb.Replace('）', ')');
			sb.Replace('『', '"');
			sb.Replace('』', '"');
			sb.Replace("「", "");
			sb.Replace("」", "");
			sb.Replace("、", ", ");
			sb.Replace("。", ". ");
			sb.Replace("…", "...");
			sb.Replace('！', '!');
			sb.Replace('？', '?');
			sb.Replace('~', '～');
			sb.Replace('⁓', '～');
			sb.Replace('–', '—');
			sb.Replace('—', '—');
			sb.Replace('ー', '—');
			sb.Replace('ｰ', '—');
			sb.Replace('―', '—');
            sb.Replace('─', '—');
            sb.Replace('’', '\'');

			return sb.ToString();
		}

		public static string CorrectPunctuation(string value) {
			// replace 2 or more dots by exactly 3
			value = Regex.Replace(value, @"…|[…\.]{2,}", "...");
			// remove redundant exclamation points
			value = Regex.Replace(value, @"!{2,}", "!");
			// remove redundant question marks
			value = Regex.Replace(value, @"\?{2,}", "?");
			// remove redundant commas
			value = Regex.Replace(value, @",{2,}", ",");
			// ensure incorrect interrobang order (-.-)
			value = Regex.Replace(value, @"\?!", "!?");
			// add a space after ellipses that are surrounded by letters
			value = Regex.Replace(value, @"(?<=\w)\.\.\.(?=\w)", "... ");
			// replace multiple dashes by "em" dash
			value = Regex.Replace(value, @"-{2,}", "—");
			// add a space after comma
			value = Regex.Replace(value, @",(?=\w)", ", ");
			// remove space before and add space after semicolon
			value = Regex.Replace(value, @"\s*;\s*", "; ");
			// add a full stop at the end, if missing
			value = Regex.Replace(value, @"(?=\w)$", ".");
			// remove redundant spaces
			value = Regex.Replace(value, @"\s{2,}", " ");
			value = value.Trim();
			return value;
		}

		public static string ProgressBar(int width, double progress) {
			string line = new string('=', (int)(width * progress)) + new string('.', width - (int)(width * progress));
			string text = progress.ToString("p");
			return line.Substring(0, (width - text.Length) / 2 - 1) + ' ' + text + ' ' + line.Substring((width + text.Length) / 2 + 2);
		}

		public class FontMetrics {
			public int FullWidthHorizontalSpacing, HalfWidthHorizontalSpacing, VerticalSpacing;

			public int GetCharacterWidth(char ch) {
				return IsFullWidthCharacter(ch) ? FullWidthHorizontalSpacing : HalfWidthHorizontalSpacing;
			}

			public FontMetrics(int horizontal) {
				FullWidthHorizontalSpacing = horizontal;
				HalfWidthHorizontalSpacing = horizontal / 2;
				VerticalSpacing = horizontal;
			}

			public FontMetrics(int horizontal, int vertical) {
				FullWidthHorizontalSpacing = horizontal;
				HalfWidthHorizontalSpacing = horizontal / 2;
				VerticalSpacing = vertical;
			}
		}

		private static readonly Encoding AsciiEncoding = Encoding.GetEncoding(
			"ASCII",
			EncoderFallback.ExceptionFallback,
			DecoderFallback.ExceptionFallback);
		
		public static string EncodeYukaString(string str) {
			var result = new StringBuilder(str.Length);
			var buffer = new byte[2];

			for(int i = 0; i < str.Length; i++) {
				try {
					AsciiEncoding.GetBytes(str, i, 1, buffer, 0);
					result.Append(str[i]);
				}
				catch {
					result.Append($"@u({(int)str[i]:X4})");
				}
			}
			return result.ToString();
		}
	}
}
