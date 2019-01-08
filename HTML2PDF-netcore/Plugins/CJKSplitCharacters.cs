using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using iText.IO.Font.Otf;
using iText.Layout.Splitting;

namespace HTML2PDF_netcore.Plugins
{
    /// <summary>
    /// 
    /// </summary>
    public class CJKSplitCharacters : ISplitCharacters
    {
        // line of text cannot start or end with this character
        static char u2060 = '\u2060';   //       - ZERO WIDTH NO BREAK SPACE

        // a line of text cannot start with any following characters in NOT_BEGIN_CHARACTERS[]
        static char u30fb = '\u30fb';   //  ・   - KATAKANA MIDDLE DOT
        static char u2022 = '\u2022';   //  •    - BLACK SMALL CIRCLE (BULLET)
        static char uff65 = '\uff65';   //  ･    - HALFWIDTH KATAKANA MIDDLE DOT
        static char u300d = '\u300d';   //  」   - RIGHT CORNER BRACKET
        static char uff09 = '\uff09';   //  ）   - FULLWIDTH RIGHT PARENTHESIS
        static char u0021 = '\u0021';   //  !    - EXCLAMATION MARK
        static char u0025 = '\u0025';   //  %    - PERCENT SIGN
        static char u0029 = '\u0029';   //  )    - RIGHT PARENTHESIS
        static char u002c = '\u002c';   //  ,    - COMMA
        static char u002e = '\u002e';   //  .    - FULL STOP
        static char u003f = '\u003f';   //  ?    - QUESTION MARK
        static char u005d = '\u005d';   //  ]    - RIGHT SQUARE BRACKET
        static char u007d = '\u007d';   //  }    - RIGHT CURLY BRACKET
        static char uff61 = '\uff61';   //  ｡    - HALFWIDTH IDEOGRAPHIC FULL STOP
        static char uff63 = '\uff63';   //  ｣    - HALFWIDTH RIGHT CORNER BRACKET
        static char uff64 = '\uff64';   //  ､    - HALFWIDTH IDEOGRAPHIC COMMA
        static char uff67 = '\uff67';   //  ｧ    - HALFWIDTH KATAKANA LETTER SMALL A
        static char uff68 = '\uff68';   //  ｨ    - HALFWIDTH KATAKANA LETTER SMALL I
        static char uff69 = '\uff69';   //  ｩ    - HALFWIDTH KATAKANA LETTER SMALL U
        static char uff6a = '\uff6a';   //  ｪ    - HALFWIDTH KATAKANA LETTER SMALL E
        static char uff6b = '\uff6b';   //  ｫ    - HALFWIDTH KATAKANA LETTER SMALL O
        static char uff6c = '\uff6c';   //  ｬ    - HALFWIDTH KATAKANA LETTER SMALL YA
        static char uff6d = '\uff6d';   //  ｭ    - HALFWIDTH KATAKANA LETTER SMALL YU
        static char uff6e = '\uff6e';   //  ｮ    - HALFWIDTH KATAKANA LETTER SMALL YO
        static char uff6f = '\uff6f';   //  ｯ    - HALFWIDTH KATAKANA LETTER SMALL TU
        static char uff70 = '\uff70';   //  ｰ    - HALFWIDTH KATAKANA-HIRAGANA PROLONGED SOUND MARK
        static char uff9e = '\uff9e';   //  ﾞ    - HALFWIDTH KATAKANA VOICED SOUND MARK
        static char uff9f = '\uff9f';   //  ﾟ    - HALFWIDTH KATAKANA SEMI-VOICED SOUND MARK
        static char u3001 = '\u3001';   //  、    - IDEOGRAPHIC COMMA
        static char u3002 = '\u3002';   //  。    - IDEOGRAPHIC FULL STOP
        static char uff0c = '\uff0c';   //  ，    - FULLWIDTH COMMA
        static char uff0e = '\uff0e';   //  ．    - FULLWIDTH FULL STOP
        static char uff1a = '\uff1a';   //  ：    - FULLWIDTH COLON
        static char uff1b = '\uff1b';   //  ；    - FULLWIDTH SEMICOLON
        static char uff1f = '\uff1f';   //  ？    - FULLWIDTH QUESTION MARK
        static char uff01 = '\uff01';   //  ！    - FULLWIDTH EXCLAMATION MARK
        static char u309b = '\u309b';   //  ゛    - KATAKANA-HIRAGANA VOICED SOUND MARK
        static char u309c = '\u309c';   //  ゜    - KATAKANA-HIRAGANA SEMI-VOICED SOUND MARK
        static char u30fd = '\u30fd';   //  ヽ    - KATAKANA ITERATION MARK
        static char u30fe = '\u30fe';   //  ヾ    - KATAKANA VOICED ITERATION MARK
        static char u309d = '\u309d';   //  ゝ    - HIRAGANA ITERATION MARK
        static char u309e = '\u309e';   //  ゞ    - HIRAGANA VOICED ITERATION MARK
        static char u3005 = '\u3005';   //  々    - IDEOGRAPHIC ITERATION MARK
        static char u30fc = '\u30fc';   //  ー    - KATAKANA-HIRAGANA PROLONGED SOUND MARK
        static char u2019 = '\u2019';   //  ’    - RIGHT SINGLE QUOTATION MARK
        static char u201d = '\u201d';   //  ”    - RIGHT DOUBLE QUOTATION MARK
        static char u3015 = '\u3015';   //  〕    - RIGHT TORTOISE SHELL BRACKET
        static char uff3d = '\uff3d';   //  ］    - FULLWIDTH RIGHT SQUARE BRACKET
        static char uff5d = '\uff5d';   //  ｝    - FULLWIDTH RIGHT CURLY BRACKET
        static char u3009 = '\u3009';   //  〉    - RIGHT ANGLE BRACKET
        static char u300b = '\u300b';   //  》    - RIGHT DOUBLE ANGLE BRACKET
        static char u300f = '\u300f';   //  』    - RIGHT WHITE CORNER BRACKET
        static char u3011 = '\u3011';   //  】    - RIGHT BLACK LENTICULAR BRACKET
        static char u00b0 = '\u00b0';   //  °    - DEGREE SIGN
        static char u2032 = '\u2032';   //  ′    - PRIME
        static char u2033 = '\u2033';   //  ″    - DOUBLE PRIME
        static char u2103 = '\u2103';   //  ℃    - DEGREE CELSIUS
        static char u00a2 = '\u00a2';   //  ¢    - CENT SIGN
        static char uff05 = '\uff05';   //  ％    - FULLWIDTH PERCENT SIGN
        static char u2030 = '\u2030';   //  ‰    - PER MILLE SIGN
        static char u3041 = '\u3041';   //  ぁ    - HIRAGANA LETTER SMALL A
        static char u3043 = '\u3043';   //  ぃ    - HIRAGANA LETTER SMALL I
        static char u3045 = '\u3045';   //  ぅ    - HIRAGANA LETTER SMALL U
        static char u3047 = '\u3047';   //  ぇ    - HIRAGANA LETTER SMALL E
        static char u3049 = '\u3049';   //  ぉ    - HIRAGANA LETTER SMALL O
        static char u3063 = '\u3063';   //  っ    - HIRAGANA LETTER SMALL TU
        static char u3083 = '\u3083';   //  ゃ    - HIRAGANA LETTER SMALL YA
        static char u3085 = '\u3085';   //  ゅ    - HIRAGANA LETTER SMALL YU
        static char u3087 = '\u3087';   //  ょ    - HIRAGANA LETTER SMALL YO
        static char u308e = '\u308e';   //  ゎ    - HIRAGANA LETTER SMALL WA
        static char u30a1 = '\u30a1';   //  ァ    - KATAKANA LETTER SMALL A
        static char u30a3 = '\u30a3';   //  ィ    - KATAKANA LETTER SMALL I
        static char u30a5 = '\u30a5';   //  ゥ    - KATAKANA LETTER SMALL U
        static char u30a7 = '\u30a7';   //  ェ    - KATAKANA LETTER SMALL E
        static char u30a9 = '\u30a9';   //  ォ    - KATAKANA LETTER SMALL O
        static char u30c3 = '\u30c3';   //  ッ    - KATAKANA LETTER SMALL TU
        static char u30e3 = '\u30e3';   //  ャ    - KATAKANA LETTER SMALL YA
        static char u30e5 = '\u30e5';   //  ュ    - KATAKANA LETTER SMALL YU
        static char u30e7 = '\u30e7';   //  ョ    - KATAKANA LETTER SMALL YO
        static char u30ee = '\u30ee';   //  ヮ    - KATAKANA LETTER SMALL WA
        static char u30f5 = '\u30f5';   //  ヵ    - KATAKANA LETTER SMALL KA
        static char u30f6 = '\u30f6';   //  ヶ    - KATAKANA LETTER SMALL KE

        static char[] NOT_BEGIN_CHARACTERS = new char[]{u30fb, u2022, uff65, u300d, uff09, u0021, u0025, u0029, u002c,
          u002e, u003f, u005d, u007d, uff61, uff63, uff64, uff67, uff68, uff69, uff6a, uff6b, uff6c, uff6d, uff6e,
          uff6f, uff70, uff9e, uff9f, u3001, u3002, uff0c, uff0e, uff1a, uff1b, uff1f, uff01, u309b, u309c, u30fd,
          u30fe, u309d, u309e, u3005, u30fc, u2019, u201d, u3015, uff3d, uff5d, u3009, u300b, u300f, u3011, u00b0,
          u2032, u2033, u2103, u00a2, uff05, u2030, u3041, u3043, u3045, u3047, u3049, u3063, u3083, u3085, u3087,
          u308e, u30a1, u30a3, u30a5, u30a7, u30a9, u30c3, u30e3, u30e5, u30e7, u30ee, u30f5, u30f6, u2060};

        // a line of text cannot end with any following characters in NOT_ENDING_CHARACTERS[]
        static char u0024 = '\u0024';   //  $   - DOLLAR SIGN
        static char u0028 = '\u0028';   //  (   - LEFT PARENTHESIS
        static char u005b = '\u005b';   //  [   - LEFT SQUARE BRACKET
        static char u007b = '\u007b';   //  {   - LEFT CURLY BRACKET
        static char u00a3 = '\u00a3';   //  £   - POUND SIGN
        static char u00a5 = '\u00a5';   //  ¥   - YEN SIGN
        static char u201c = '\u201c';   //  “   - LEFT DOUBLE QUOTATION MARK
        static char u2018 = '\u2018';   //   ‘  - LEFT SINGLE QUOTATION MARK
        static char u300a = '\u300a';   //  《  - LEFT DOUBLE ANGLE BRACKET
        static char u3008 = '\u3008';   //  〈  - LEFT ANGLE BRACKET
        static char u300c = '\u300c';   //  「  - LEFT CORNER BRACKET
        static char u300e = '\u300e';   //  『  - LEFT WHITE CORNER BRACKET
        static char u3010 = '\u3010';   //  【  - LEFT BLACK LENTICULAR BRACKET
        static char u3014 = '\u3014';   //  〔  - LEFT TORTOISE SHELL BRACKET
        static char uff62 = '\uff62';   //  ｢   - HALFWIDTH LEFT CORNER BRACKET
        static char uff08 = '\uff08';   //  （  - FULLWIDTH LEFT PARENTHESIS
        static char uff3b = '\uff3b';   //  ［  - FULLWIDTH LEFT SQUARE BRACKET
        static char uff5b = '\uff5b';   //  ｛  - FULLWIDTH LEFT CURLY BRACKET
        static char uffe5 = '\uffe5';   //  ￥  - FULLWIDTH YEN SIGN
        static char uff04 = '\uff04';   //  ＄  - FULLWIDTH DOLLAR SIGN

        static char[] NOT_ENDING_CHARACTERS = new char[]{u0024, u0028, u005b, u007b, u00a3, u00a5, u201c, u2018, u3008,
          u300a, u300c, u300e, u3010, u3014, uff62, uff08, uff3b, uff5b, uffe5, uff04, u2060};

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="glyphPos"></param>
        /// <returns></returns>
        public bool IsSplitCharacter(GlyphLine text, int glyphPos)
        {
            if (!text.Get(glyphPos).HasValidUnicode())
            {
                return false;
            }
            int charCode = text.Get(glyphPos).GetUnicode();

            if (NOT_BEGIN_CHARACTERS.Contains((char)charCode))
            {
                return false;
            }
            return new DefaultSplitCharacters().IsSplitCharacter(text, glyphPos);
        }
    }
}
