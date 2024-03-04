#pragma once
#include <string>

struct RouletteDisguise {
public:
	RouletteDisguise(std::string name, std::string image, std::string repoId, bool suit = false) : name(name), image(image), repoId(repoId), suit(suit)
	{}

public:
	std::string name;
	std::string image;
	std::string repoId;
	bool suit = false;
};
