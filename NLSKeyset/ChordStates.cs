namespace NLSKeyset;

public static class ChordStates {
	public static Dictionary<byte, char> States = new() {
		//No mouse buttons held
		{ 0b00000001, 'a' },
		{ 0b00000010, 'b' },
		{ 0b00000011, 'c' },
		{ 0b00000100, 'd' },
		{ 0b00000101, 'e' },
		{ 0b00000110, 'f' },
		{ 0b00000111, 'g' },
		{ 0b00001000, 'h' },
		{ 0b00001001, 'i' },
		{ 0b00001010, 'j' },
		{ 0b00001011, 'k' },
		{ 0b00001100, 'l' },
		{ 0b00001101, 'm' },
		{ 0b00001110, 'n' },
		{ 0b00001111, 'o' },
		{ 0b00010000, 'p' },
		{ 0b00010001, 'q' },
		{ 0b00010010, 'r' },
		{ 0b00010011, 's' },
		{ 0b00010100, 't' },
		{ 0b00010101, 'u' },
		{ 0b00010110, 'v' },
		{ 0b00010111, 'w' },
		{ 0b00011000, 'x' },
		{ 0b00011001, 'y' },
		{ 0b00011010, 'z' },
		{ 0b00011011, ',' },
		{ 0b00011100, '.' },
		{ 0b00011101, ';' },
		{ 0b00011110, '?' },
		{ 0b00011111, ' ' },
		
		//Middle mouse held
		{ 0b01000001, 'A' },
		{ 0b01000010, 'B' },
		{ 0b01000011, 'C' },
		{ 0b01000100, 'D' },
		{ 0b01000101, 'E' },
		{ 0b01000110, 'F' },
		{ 0b01000111, 'G' },
		{ 0b01001000, 'H' },
		{ 0b01001001, 'I' },
		{ 0b01001010, 'J' },
		{ 0b01001011, 'K' },
		{ 0b01001100, 'L' },
		{ 0b01001101, 'M' },
		{ 0b01001110, 'N' },
		{ 0b01001111, 'O' },
		{ 0b01010000, 'P' },
		{ 0b01010001, 'Q' },
		{ 0b01010010, 'R' },
		{ 0b01010011, 'S' },
		{ 0b01010100, 'T' },
		{ 0b01010101, 'U' },
		{ 0b01010110, 'V' },
		{ 0b01010111, 'W' },
		{ 0b01011000, 'X' },
		{ 0b01011001, 'Y' },
		{ 0b01011010, 'Z' },
		{ 0b01011011, '<' }, 
		{ 0b01011100, '>' }, 
		{ 0b01011101, ':' }, 
		{ 0b01011110, '\\' },
		{ 0b01011111, '\t' },
		
		//Left mouse held
		{ 0b10000001, '!' }, 
		{ 0b10000010, '"' }, 
		{ 0b10000011, '#' }, 
		{ 0b10000100, '$' }, 
		{ 0b10000101, '%' }, 
		{ 0b10000110, '&' }, 
		{ 0b10000111, '`' },  
		{ 0b10001000, '(' },
		{ 0b10001001, ')' },
		{ 0b10001010, '@' },
		{ 0b10001011, '+' },
		{ 0b10001100, '-' },
		{ 0b10001101, '*' },
		{ 0b10001110, '/' },
		{ 0b10001111, '↑' },
		{ 0b10010000, '0' },
		{ 0b10010001, '1' },
		{ 0b10010010, '2' },
		{ 0b10010011, '3' },
		{ 0b10010100, '4' },
		{ 0b10010101, '5' },
		{ 0b10010110, '6' },
		{ 0b10010111, '7' },
		{ 0b10011000, '8' },
		{ 0b10011001, '9' },
		{ 0b10011010, '=' },
		{ 0b10011011, '[' },
		{ 0b10011100, ']' },
		{ 0b10011101, '←' },
		{ 0b10011110, '\u001B' },
		{ 0b10011111, '\n' },
	};
}
