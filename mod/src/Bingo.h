#pragma once
#include <string>
#include <vector>
#include <imgui.h>

namespace Croupier {
	struct BingoDimensions {
		BingoDimensions(int rows, int cols, bool remainder = false) : rows(rows), columns(cols), hasRemainder(remainder)
		{ }

		int rows = 0;
		int columns = 0;
		bool hasRemainder = 0;
	};

	inline auto isPerfectSquare(long long n) -> bool {
		double guess = sqrt(n);
		long long r = floor(guess);
		if (r * r == n) return true;
		r = ceil(guess);
		return r * r == n;
	}

	inline auto GetBingoDimensions(int numTiles) -> BingoDimensions {
		if (numTiles <= 5) return {numTiles, 1};
		switch (numTiles) {
			case 6: return { 3, 2 };
			case 8: return { 4, 2 };
			case 10: return { 5, 2 };
			case 12: return { 4, 3 };
			case 15: return { 5, 3 };
			case 20: return { 5, 4 };
			case 30: return { 6, 5 };
		}

		if (isPerfectSquare(numTiles)) {
			auto sqr = (int)sqrt(numTiles);
			return {sqr, sqr};
		}
		
		if (numTiles < 36) {
			auto const rem = numTiles % 5 != 0;
			return {5, (numTiles / 5) + (rem ? 1 : 0), rem};
		}
		auto const rem2 = numTiles % 6 != 0;
		return {6, (numTiles / 6) + (rem2 ? 1 : 0), rem2};
	}

	struct BingoTile {
		std::string text;
		std::string group;
		ImU32 groupColour;
		bool achieved = false;
		bool failed = false;
	};

	class BingoCard {
	public:
		std::vector<BingoTile> tiles;
	};
}
