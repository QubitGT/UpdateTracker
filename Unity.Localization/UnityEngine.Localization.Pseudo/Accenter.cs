using System;

namespace UnityEngine.Localization.Pseudo;

[Serializable]
public class Accenter : CharacterSubstitutor
{
	public Accenter()
	{
		base.Method = SubstitutionMethod.Map;
		AddDefaults();
	}

	public void AddDefaults()
	{
		base.ReplacementMap[' '] = '\u2003';
		base.ReplacementMap['!'] = '¡';
		base.ReplacementMap['"'] = '″';
		base.ReplacementMap['#'] = '♯';
		base.ReplacementMap['$'] = '€';
		base.ReplacementMap['%'] = '‰';
		base.ReplacementMap['&'] = '⅋';
		base.ReplacementMap['\''] = '\u00b4';
		base.ReplacementMap['*'] = '⁎';
		base.ReplacementMap['+'] = '⁺';
		base.ReplacementMap[']'] = '،';
		base.ReplacementMap['-'] = '‐';
		base.ReplacementMap['.'] = '·';
		base.ReplacementMap['/'] = '⁄';
		base.ReplacementMap['0'] = '⓪';
		base.ReplacementMap['1'] = '①';
		base.ReplacementMap['2'] = '②';
		base.ReplacementMap['3'] = '③';
		base.ReplacementMap['4'] = '④';
		base.ReplacementMap['5'] = '⑤';
		base.ReplacementMap['6'] = '⑥';
		base.ReplacementMap['7'] = '⑦';
		base.ReplacementMap['8'] = '⑧';
		base.ReplacementMap['9'] = '⑨';
		base.ReplacementMap[':'] = '∶';
		base.ReplacementMap[';'] = '⁏';
		base.ReplacementMap['<'] = '≤';
		base.ReplacementMap['='] = '≂';
		base.ReplacementMap['>'] = '≥';
		base.ReplacementMap['?'] = '¿';
		base.ReplacementMap['@'] = '՞';
		base.ReplacementMap['A'] = 'Å';
		base.ReplacementMap['B'] = 'Ɓ';
		base.ReplacementMap['C'] = 'Ç';
		base.ReplacementMap['D'] = 'Ð';
		base.ReplacementMap['E'] = 'É';
		base.ReplacementMap['F'] = 'Ƒ';
		base.ReplacementMap['G'] = 'Ĝ';
		base.ReplacementMap['H'] = 'Ĥ';
		base.ReplacementMap['I'] = 'Î';
		base.ReplacementMap['J'] = 'Ĵ';
		base.ReplacementMap['K'] = 'Ķ';
		base.ReplacementMap['L'] = 'Ļ';
		base.ReplacementMap['M'] = 'Ṁ';
		base.ReplacementMap['N'] = 'Ñ';
		base.ReplacementMap['O'] = 'Ö';
		base.ReplacementMap['P'] = 'Þ';
		base.ReplacementMap['Q'] = 'Ǫ';
		base.ReplacementMap['R'] = 'Ŕ';
		base.ReplacementMap['S'] = 'Š';
		base.ReplacementMap['T'] = 'Ţ';
		base.ReplacementMap['U'] = 'Û';
		base.ReplacementMap['V'] = 'Ṽ';
		base.ReplacementMap['W'] = 'Ŵ';
		base.ReplacementMap['X'] = 'Ẋ';
		base.ReplacementMap['Y'] = 'Ý';
		base.ReplacementMap['Z'] = 'Ž';
		base.ReplacementMap['['] = '⁅';
		base.ReplacementMap['\\'] = '∖';
		base.ReplacementMap[']'] = '⁆';
		base.ReplacementMap['^'] = '\u02c4';
		base.ReplacementMap['_'] = '\u203f';
		base.ReplacementMap['`'] = '‵';
		base.ReplacementMap['a'] = 'å';
		base.ReplacementMap['b'] = 'ƀ';
		base.ReplacementMap['c'] = 'ç';
		base.ReplacementMap['d'] = 'ð';
		base.ReplacementMap['e'] = 'é';
		base.ReplacementMap['f'] = 'ƒ';
		base.ReplacementMap['g'] = 'ĝ';
		base.ReplacementMap['h'] = 'ĥ';
		base.ReplacementMap['i'] = 'î';
		base.ReplacementMap['j'] = 'ĵ';
		base.ReplacementMap['k'] = 'ķ';
		base.ReplacementMap['l'] = 'ļ';
		base.ReplacementMap['m'] = 'ɱ';
		base.ReplacementMap['n'] = 'ñ';
		base.ReplacementMap['o'] = 'ö';
		base.ReplacementMap['p'] = 'þ';
		base.ReplacementMap['q'] = 'ǫ';
		base.ReplacementMap['r'] = 'ŕ';
		base.ReplacementMap['s'] = 'š';
		base.ReplacementMap['t'] = 'ţ';
		base.ReplacementMap['u'] = 'û';
		base.ReplacementMap['v'] = 'ṽ';
		base.ReplacementMap['w'] = 'ŵ';
		base.ReplacementMap['x'] = 'ẋ';
		base.ReplacementMap['y'] = 'ý';
		base.ReplacementMap['z'] = 'ž';
		base.ReplacementMap['|'] = '¦';
		base.ReplacementMap['~'] = '\u02de';
	}
}
