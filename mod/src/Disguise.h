#pragma once
#include "util.h"
#include <string>
#include <map>

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

inline static std::map<std::string, std::string, InsensitiveCompareLexicographic> disguiseVariants = {
	// Whittleton Creek Garbage Man Undercover Variants
	{"e3256178-ce59-4796-bc5b-800cd6120b28", "4912d30a-80cb-41d8-8137-7b4727e76e4e"},
	// Whittleton Creek Gardener Undercover Variants
	{"8b162546-0eab-40a0-a66b-a08e8ddf2ea4", "78fc9e38-cade-42c3-958c-c7d8edf43713"},

	// Agent Montgomery disguise -> Club Security
	{"6ca35f8b-b244-44a0-9813-dc050a565ac2", "590629f7-19a3-4eb8-88a6-94e550cd1c07"},
	// Agent Green disguise -> Club Security
	{"acb7695a-a5eb-420a-8455-d409d08d53e2", "590629f7-19a3-4eb8-88a6-94e550cd1c07"},
	// Agent Chamberlin disguise -> Club Security
	{"d9e1d24c-c9cc-48d6-bfd4-821d3e742b64", "590629f7-19a3-4eb8-88a6-94e550cd1c07"},
	// Agent Banner disguise -> Technician
	{"61545feb-9594-4231-8fa2-f98307ac796f", "f724d6b9-a45b-425f-84f1-c27dedd1fd07"},

	// Agent Rhodes disguise -> Biker
	{"034d5a11-1e5c-4f57-99d9-233443e42caf", "95918f14-fa9f-4315-be95-bf4b9efe6ee6"},
	// Agent Tremaine disguise -> Biker
	{"d222f4c4-708a-42c7-9433-f8c5cfd72706", "95918f14-fa9f-4315-be95-bf4b9efe6ee6"},
	// Agent Lowenthal disguise -> Biker
	{"e65fb964-a5e3-45b6-99d6-75d3c539ae92", "95918f14-fa9f-4315-be95-bf4b9efe6ee6"},

	// Agent Thames disguise -> Club Crew
	{"107c35d7-7300-417c-832a-4f36cd3071b9", "6e84215c-28b7-44b2-9d15-83e9be490965"},
};

inline static std::string transformDisguiseVariantRepoId(const std::string& repoId) {
	auto const it = disguiseVariants.find(repoId);
	return it != end(disguiseVariants) ? it->second : repoId;
}
