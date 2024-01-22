#pragma once
#include <string>

struct RouletteDisguise {
public:
	RouletteDisguise(std::string name, std::string image, bool suit = false) : name(name), image(image), suit(suit)
	{}

public:
	std::string name;
	std::string image;
	bool suit = false;
};
